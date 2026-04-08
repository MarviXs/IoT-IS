using System.Threading.Channels;
using Fei.Is.Api.MqttClient.Subscribe;
using FluentResults;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

namespace Fei.Is.Api.MqttClient;

public class MqttClientService : IHostedService
{
    private const int WorkerCount = 8;
    private const int MaxMessageBatchSize = 256;
    private const string DeviceTopicPrefix = "devices/";
    private const string DataTopicSuffix = "/data";
    private const string JobFromDeviceTopicSuffix = "/job_from_device";
    private const string SysConnectedTopicSuffix = "/connected";
    private const string SysDisconnectedTopicSuffix = "/disconnected";

    private readonly IMqttClient? _mqttClient;
    private readonly MqttClientOptions? _mqttOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Channel<QueuedMqttMessage> _messageChannel = Channel.CreateUnbounded<QueuedMqttMessage>(
        new UnboundedChannelOptions { SingleWriter = false, SingleReader = false }
    );

    private readonly ILogger<MqttClientService> _logger;

    private readonly bool _isMqttEnabled = true;
    private readonly TimeSpan _reconnectDelay = TimeSpan.FromSeconds(5);

    private CancellationTokenSource? _reconnectCancellationTokenSource;
    private CancellationTokenSource? _processingCancellationTokenSource;
    private Task? _reconnectTask;
    private Task[] _processingTasks = [];

    public MqttClientService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ILogger<MqttClientService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;

        bool isMqttEnabled = configuration.GetValue<bool>("MqttSettings:Enabled");
        if (!isMqttEnabled)
        {
            _isMqttEnabled = false;
            _logger.LogWarning("MQTT is disabled in the configuration.");
            return;
        }

        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        var clientId = Guid.NewGuid().ToString();

        var connectionString = configuration.GetSection("MqttSettings:Host").Value;

        var username = configuration.GetSection("MqttSettings:Username").Value;
        var password = configuration.GetSection("MqttSettings:Password").Value;

        _mqttOptions = new MqttClientOptionsBuilder()
            .WithClientId("Backend-" + clientId)
            .WithCredentials(username, password)
            .WithTcpServer(connectionString)
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .Build();

        ConfigureMqttClient();
    }

    private void ConfigureMqttClient()
    {
        if (_mqttClient == null)
        {
            return;
        }

        _mqttClient.ApplicationMessageReceivedAsync += ProcessMessageAsync;
        _mqttClient.DisconnectedAsync += args =>
        {
            if (!args.ClientWasConnected)
            {
                return Task.CompletedTask;
            }

            if (args.Exception != null)
            {
                _logger.LogWarning("MQTT client disconnected from broker. Attempting to reconnect. Reason: {Reason}", args.Exception.Message);
            }
            else
            {
                _logger.LogWarning("MQTT client disconnected from broker. Attempting to reconnect...");
            }

            return Task.CompletedTask;
        };
    }

    public void ReconnectUsingTimer(CancellationToken cancellationToken)
    {
        if (_mqttClient == null || _mqttOptions == null)
        {
            return;
        }

        _reconnectCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _reconnectCancellationTokenSource.Token;

        _reconnectTask = Task.Run(
            async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (!_mqttClient.IsConnected)
                        {
                            await _mqttClient.ConnectAsync(_mqttOptions, token);
                            await SubscribeToTopics(token);
                            _logger.LogInformation("Connected to MQTT broker.");
                        }
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (MqttCommunicationException ex)
                    {
                        _logger.LogWarning(
                            "Failed to connect to MQTT broker. Retrying in {RetryDelaySeconds} seconds. Reason: {Reason}",
                            _reconnectDelay.TotalSeconds,
                            ex.Message
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Unexpected error while connecting to MQTT broker. Retrying in {RetryDelaySeconds} seconds...",
                            _reconnectDelay.TotalSeconds
                        );
                    }

                    try
                    {
                        await Task.Delay(_reconnectDelay, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            },
            token
        );
    }

    public async Task ProcessMessageAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
            if (_processingCancellationTokenSource == null)
            {
                return;
            }

            var topic = args.ApplicationMessage.Topic;
            var payload = args.ApplicationMessage.PayloadSegment;

            await _messageChannel.Writer.WriteAsync(new QueuedMqttMessage(topic, payload), _processingCancellationTokenSource.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queueing MQTT message");
        }
    }

    private void LogHandlerFailure(string handlerName, Result result)
    {
        if (result.IsSuccess)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Message));
        _logger.LogError("MQTT handler {HandlerName} returned failure. Errors: {Errors}", handlerName, errors);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_isMqttEnabled)
        {
            _logger.LogWarning("MQTT is disabled. Skipping MQTT client startup.");
            return Task.CompletedTask;
        }

        _processingCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _processingTasks = Enumerable
            .Range(0, WorkerCount)
            .Select(_ => Task.Run(() => ProcessMessagesLoopAsync(_processingCancellationTokenSource.Token), _processingCancellationTokenSource.Token))
            .ToArray();

        ReconnectUsingTimer(cancellationToken);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_isMqttEnabled || _mqttClient == null)
        {
            return;
        }

        if (_reconnectCancellationTokenSource != null)
        {
            _reconnectCancellationTokenSource.Cancel();
        }
        if (_processingCancellationTokenSource != null)
        {
            _processingCancellationTokenSource.Cancel();
        }

        if (_reconnectTask != null)
        {
            try
            {
                await _reconnectTask;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Reconnect task stopped with an error.");
            }
        }
        if (_processingTasks.Length != 0)
        {
            try
            {
                await Task.WhenAll(_processingTasks);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MQTT processing workers stopped with an error.");
            }
        }

        try
        {
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to disconnect MQTT client cleanly.");
        }
        finally
        {
            _reconnectCancellationTokenSource?.Dispose();
            _processingCancellationTokenSource?.Dispose();
        }
    }

    private async Task ProcessMessagesLoopAsync(CancellationToken cancellationToken)
    {
        var messageBatch = new List<QueuedMqttMessage>(MaxMessageBatchSize);

        while (await _messageChannel.Reader.WaitToReadAsync(cancellationToken))
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                messageBatch.Clear();

                while (messageBatch.Count < MaxMessageBatchSize && _messageChannel.Reader.TryRead(out var message))
                {
                    messageBatch.Add(message);
                }

                if (messageBatch.Count == 0)
                {
                    break;
                }

                try
                {
                    await ProcessQueuedMessagesAsync(messageBatch, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing queued MQTT message");
                }
            }
        }
    }

    private async Task ProcessQueuedMessagesAsync(List<QueuedMqttMessage> messages, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        List<(string DeviceAccessToken, ArraySegment<byte> Payload)>? dataMessages = null;

        foreach (var message in messages)
        {
            if (TryGetDeviceTopicToken(message.Topic, DataTopicSuffix, out var deviceAccessToken))
            {
                dataMessages ??= new List<(string DeviceAccessToken, ArraySegment<byte> Payload)>(messages.Count);
                dataMessages.Add((deviceAccessToken, message.Payload));
            }
            else if (TryGetDeviceTopicToken(message.Topic, JobFromDeviceTopicSuffix, out deviceAccessToken))
            {
                var jobStatusReceived = scope.ServiceProvider.GetRequiredService<JobStatusReceived>();
                var result = await jobStatusReceived.Handle(deviceAccessToken, message.Payload, cancellationToken);
                LogHandlerFailure(nameof(JobStatusReceived), result);
            }
            else if (message.Topic.EndsWith(SysConnectedTopicSuffix, StringComparison.Ordinal))
            {
                var onDeviceConnected = scope.ServiceProvider.GetRequiredService<OnDeviceConnected>();
                var result = await onDeviceConnected.Handle(message.Payload, cancellationToken);
                LogHandlerFailure(nameof(OnDeviceConnected), result);
            }
            else if (message.Topic.EndsWith(SysDisconnectedTopicSuffix, StringComparison.Ordinal))
            {
                var onDeviceDisconnected = scope.ServiceProvider.GetRequiredService<OnDeviceDisconnected>();
                var result = await onDeviceDisconnected.Handle(message.Payload, cancellationToken);
                LogHandlerFailure(nameof(OnDeviceDisconnected), result);
            }
        }

        if (dataMessages is { Count: > 0 })
        {
            var dataPointReceived = scope.ServiceProvider.GetRequiredService<DataPointReceived>();
            var result = await dataPointReceived.HandleBatch(dataMessages, cancellationToken);
            LogHandlerFailure(nameof(DataPointReceived), result);
        }
    }

    private static bool TryGetDeviceTopicToken(string topic, string suffix, out string deviceAccessToken)
    {
        deviceAccessToken = string.Empty;

        if (!topic.StartsWith(DeviceTopicPrefix, StringComparison.Ordinal) || !topic.EndsWith(suffix, StringComparison.Ordinal))
        {
            return false;
        }

        var tokenLength = topic.Length - DeviceTopicPrefix.Length - suffix.Length;
        if (tokenLength <= 0)
        {
            return false;
        }

        var tokenStart = DeviceTopicPrefix.Length;
        if (topic.AsSpan(tokenStart, tokenLength).Contains('/'))
        {
            return false;
        }

        deviceAccessToken = topic.Substring(tokenStart, tokenLength);
        return true;
    }

    private async Task SubscribeToTopics(CancellationToken cancellationToken)
    {
        if (_mqttClient == null)
        {
            return;
        }

        var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter("devices/+/data", MqttQualityOfServiceLevel.AtLeastOnce)
            .WithTopicFilter("devices/+/job_from_device", MqttQualityOfServiceLevel.AtLeastOnce)
            .WithTopicFilter("$SYS/brokers/+/clients/+/connected", MqttQualityOfServiceLevel.AtLeastOnce)
            .WithTopicFilter("$SYS/brokers/+/clients/+/disconnected", MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.SubscribeAsync(subscribeOptions, cancellationToken);
        _logger.LogInformation("Subscribed to MQTT topics");
    }

    public async Task<bool> TryPublishAsync(
        string topic,
        byte[] payload,
        MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce,
        bool retain = false
    )
    {
        if (!_isMqttEnabled || _mqttClient == null)
        {
            _logger.LogWarning("Skipping MQTT publish because MQTT is disabled.");
            return false;
        }

        if (!_mqttClient.IsConnected)
        {
            _logger.LogWarning("Skipping MQTT publish for topic {Topic} because MQTT client is not connected.", topic);
            return false;
        }

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(qos)
            .WithRetainFlag(retain)
            .Build();

        try
        {
            var result = await _mqttClient.PublishAsync(message);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("MQTT publish failed for topic {Topic}.", topic);
            }

            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MQTT publish threw an exception for topic {Topic}.", topic);
            return false;
        }
    }

    private sealed record QueuedMqttMessage(string Topic, ArraySegment<byte> Payload);
}
