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
    private readonly IMqttClient? _mqttClient;
    private readonly MqttClientOptions? _mqttOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly ILogger<MqttClientService> _logger;

    private readonly bool _isMqttEnabled = true;
    private readonly TimeSpan _reconnectDelay = TimeSpan.FromSeconds(5);

    private CancellationTokenSource? _reconnectCancellationTokenSource;
    private Task? _reconnectTask;

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
                        _logger.LogWarning(ex, "Unexpected error while connecting to MQTT broker. Retrying in {RetryDelaySeconds} seconds...", _reconnectDelay.TotalSeconds);
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

    public Task ProcessMessageAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var topic = args.ApplicationMessage.Topic;
                var topicParts = topic.Split('/');

                if (MqttTopicFilterComparer.Compare(topic, "devices/+/data") == MqttTopicFilterCompareResult.IsMatch)
                {
                    var dataPointReceived = scope.ServiceProvider.GetRequiredService<DataPointReceived>();
                    var result = await dataPointReceived.Handle(topicParts[1], args.ApplicationMessage.PayloadSegment, CancellationToken.None);
                    LogHandlerFailure(nameof(DataPointReceived), result);
                }
                else if (MqttTopicFilterComparer.Compare(topic, "devices/+/job_from_device") == MqttTopicFilterCompareResult.IsMatch)
                {
                    var jobStatusReceived = scope.ServiceProvider.GetRequiredService<JobStatusReceived>();
                    var result = await jobStatusReceived.Handle(topicParts[1], args.ApplicationMessage.PayloadSegment, CancellationToken.None);
                    LogHandlerFailure(nameof(JobStatusReceived), result);
                }
                else if (MqttTopicFilterComparer.Compare(topic, "$SYS/brokers/+/clients/+/connected") == MqttTopicFilterCompareResult.IsMatch)
                {
                    var onDeviceConnected = scope.ServiceProvider.GetRequiredService<OnDeviceConnected>();
                    var result = await onDeviceConnected.Handle(args.ApplicationMessage.PayloadSegment, CancellationToken.None);
                    LogHandlerFailure(nameof(OnDeviceConnected), result);
                }
                else if (MqttTopicFilterComparer.Compare(topic, "$SYS/brokers/+/clients/+/disconnected") == MqttTopicFilterCompareResult.IsMatch)
                {
                    var onDeviceDisconnected = scope.ServiceProvider.GetRequiredService<OnDeviceDisconnected>();
                    var result = await onDeviceDisconnected.Handle(args.ApplicationMessage.PayloadSegment, CancellationToken.None);
                    LogHandlerFailure(nameof(OnDeviceDisconnected), result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MQTT message");
            }
        });

        return Task.CompletedTask;
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

        if (_reconnectTask != null)
        {
            try
            {
                await _reconnectTask;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Reconnect task stopped with an error.");
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
        }
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
}
