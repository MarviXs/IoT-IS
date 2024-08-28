using Fei.Is.Api.Data.Models;
using Fei.Is.Api.MqttClient;
using Fei.Is.Api.MqttClient.Commands;
using MediatR;

namespace Fei.Is.Api.Features.Jobs.EventHandlers;

public class JobCreatedEvent(Job job) : INotification
{
    public Job Job { get; } = job;
}

public class JobCreatedEventHandler(IServiceScopeFactory serviceScopeFactory) : INotificationHandler<JobCreatedEvent>
{
    public async Task Handle(JobCreatedEvent notification, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var sendJobStatusCommand = new SendJobStatus.Command(notification.Job.DeviceId, notification.Job.Device.AccessToken, notification.Job);
        await mediator.Send(sendJobStatusCommand, cancellationToken);
    }
}
