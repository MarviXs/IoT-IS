using Fei.Is.Api.MqttClient.Commands;
using MediatR;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

namespace Fei.Is.Api.MqttClient;

public class MqttClientService : IHostedService
{
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _mqttOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly ILogger<MqttClientService> _logger;

    public MqttClientService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ILogger<MqttClientService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;

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
                        }
                    }
                    catch
                    {
                        // Handle the exception properly (logging etc.).
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
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var topic = args.ApplicationMessage.Topic;
                var topicParts = topic.Split('/');

                if (MqttTopicFilterComparer.Compare(topic, "devices/+/data") == MqttTopicFilterCompareResult.IsMatch)
                {
                    var command = new ProcessDataPointMessage.Command(topicParts[1], args.ApplicationMessage.PayloadSegment);
                    await mediator.Send(command);
                }
                else if (MqttTopicFilterComparer.Compare(topic, "devices/+/job_from_device") == MqttTopicFilterCompareResult.IsMatch)
                {
                    var command = new JobStatusReceived.Command(topicParts[1], args.ApplicationMessage.PayloadSegment);
                    await mediator.Send(command);
                }
                else if (MqttTopicFilterComparer.Compare(topic, "$SYS/brokers/+/clients/+/connected") == MqttTopicFilterCompareResult.IsMatch)
                {
                    var command = new OnDeviceConnected.Command(args.ApplicationMessage.PayloadSegment);
                    await mediator.Send(command);
                }
                else if (MqttTopicFilterComparer.Compare(topic, "$SYS/brokers/+/clients/+/disconnected") == MqttTopicFilterCompareResult.IsMatch)
                {
                    var command = new OnDeviceDisconnected.Command(args.ApplicationMessage.PayloadSegment);
                    await mediator.Send(command);
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
        await _mqttClient.ConnectAsync(_mqttOptions, cancellationToken);
        Reconnect_Using_Timer(cancellationToken);

        await _mqttClient.SubscribeAsync("$SYS/brokers/+/clients/+/connected", MqttQualityOfServiceLevel.AtLeastOnce);
        await _mqttClient.SubscribeAsync("$SYS/brokers/+/clients/+/disconnected", MqttQualityOfServiceLevel.AtLeastOnce);

        await _mqttClient.SubscribeAsync("$queue/devices/+/data", MqttQualityOfServiceLevel.AtLeastOnce);
        await _mqttClient.SubscribeAsync("$queue/devices/+/job_from_device", MqttQualityOfServiceLevel.AtLeastOnce);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _mqttClient.DisconnectAsync();
    }

    public async Task PublishAsync(string topic, ArraySegment<byte> payload)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(false)
            .Build();

        await _mqttClient.PublishAsync(message);
    }
}
