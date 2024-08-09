using Fei.Is.Api.MqttClient.Commands;
using MediatR;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;

namespace Fei.Is.Api.MqttClient;

public class MqttClientService : IHostedService
{
    private readonly IManagedMqttClient _mqttClient;
    private readonly ManagedMqttClientOptions _mqttOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly ILogger<MqttClientService> _logger;

    public MqttClientService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ILogger<MqttClientService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;

        var factory = new MqttFactory();
        _mqttClient = factory.CreateManagedMqttClient();

        var clientId = Guid.NewGuid().ToString();

        var connectionString = configuration.GetConnectionString("MqttConnection");

        var username = configuration.GetSection("MqttConnection:Username").Value;
        var password = configuration.GetSection("MqttConnection:Password").Value;

        var clientOptions = new MqttClientOptionsBuilder()
            .WithClientId("Backend-" + clientId)
            .WithCredentials(username, password)
            .WithTcpServer(connectionString)
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .Build();

        _mqttOptions = new ManagedMqttClientOptionsBuilder().WithAutoReconnectDelay(TimeSpan.FromSeconds(5)).WithClientOptions(clientOptions).Build();
        ConfigureMqttClient();
    }

    private void ConfigureMqttClient()
    {
        _mqttClient.ApplicationMessageReceivedAsync += ProcessMessageAsync;
    }

    public async Task ProcessMessageAsync(MqttApplicationMessageReceivedEventArgs args)
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MQTT message");
            }
        });
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _mqttClient.StartAsync(_mqttOptions);

        await _mqttClient.SubscribeAsync("$SYS/brokers/+/clients/+/connected", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
        await _mqttClient.SubscribeAsync("$SYS/brokers/+/clients/+/disconnected", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);

        await _mqttClient.SubscribeAsync("$queue/devices/+/data", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _mqttClient.StopAsync();
    }
}
