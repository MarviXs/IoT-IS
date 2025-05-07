using Fei.Is.Api.MqttClient.Subscribe;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Org.BouncyCastle.Asn1.IsisMtt;

namespace Fei.Is.Api.MqttClient;

public class MqttClientService : IHostedService
{
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _mqttOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly ILogger<MqttClientService> _logger;

    private readonly bool _isMqttEnabled = true;

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
        _mqttClient.ApplicationMessageReceivedAsync += ProcessMessageAsync;
    }

    public void Reconnect_Using_Timer(CancellationToken cancellationToken)
    {
        _ = Task.Run(
            async () =>
            {
                while (true)
                {
                    try
                    {
                        if (!await _mqttClient.TryPingAsync())
                        {
                            await _mqttClient.ConnectAsync(_mqttOptions, cancellationToken);
                            await SubscribeToTopics();
                        }
                    }
                    catch
                    {
                        _logger.LogError("Failed to connect to MQTT broker. Retrying in 5 seconds...");
                    }
                    finally
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }
            },
            cancellationToken
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
                    await dataPointReceived.Handle(topicParts[1], args.ApplicationMessage.PayloadSegment, CancellationToken.None);
                }
                else if (MqttTopicFilterComparer.Compare(topic, "devices/+/job_from_device") == MqttTopicFilterCompareResult.IsMatch)
                {
                    var jobStatusReceived = scope.ServiceProvider.GetRequiredService<JobStatusReceived>();
                    await jobStatusReceived.Handle(topicParts[1], args.ApplicationMessage.PayloadSegment, CancellationToken.None);
                }
                else if (MqttTopicFilterComparer.Compare(topic, "$SYS/brokers/+/clients/+/connected") == MqttTopicFilterCompareResult.IsMatch)
                {
                    var onDeviceConnected = scope.ServiceProvider.GetRequiredService<OnDeviceConnected>();
                    await onDeviceConnected.Handle(args.ApplicationMessage.PayloadSegment, CancellationToken.None);
                }
                else if (MqttTopicFilterComparer.Compare(topic, "$SYS/brokers/+/clients/+/disconnected") == MqttTopicFilterCompareResult.IsMatch)
                {
                    var onDeviceDisconnected = scope.ServiceProvider.GetRequiredService<OnDeviceDisconnected>();
                    await onDeviceDisconnected.Handle(args.ApplicationMessage.PayloadSegment, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MQTT message");
            }
        });

        return Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_isMqttEnabled)
        {
            _logger.LogWarning("MQTT is disabled. Skipping MQTT client startup.");
            return;
        }
        Reconnect_Using_Timer(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _mqttClient.DisconnectAsync();
    }

    private async Task SubscribeToTopics()
    {
        var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter("devices/+/data", MqttQualityOfServiceLevel.AtLeastOnce)
            .WithTopicFilter("devices/+/job_from_device", MqttQualityOfServiceLevel.AtLeastOnce)
            .WithTopicFilter("$SYS/brokers/+/clients/+/connected", MqttQualityOfServiceLevel.AtLeastOnce)
            .WithTopicFilter("$SYS/brokers/+/clients/+/disconnected", MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.SubscribeAsync(subscribeOptions, CancellationToken.None);
        _logger.LogInformation("Subscribed to MQTT topics");
    }

    public async Task<MqttClientPublishResult> PublishAsync(
        string topic,
        byte[] payload,
        MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce,
        bool retain = false
    )
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(qos)
            .WithRetainFlag(retain)
            .Build();

        return await _mqttClient.PublishAsync(message);
    }
}
