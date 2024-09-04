using Fei.Is.Api.Data.Models;
using Fei.Is.Api.MqttClient.Publish;
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
        var publishJobStatus = scope.ServiceProvider.GetRequiredService<PublishJobStatus>();
        await publishJobStatus.Execute(notification.Job.DeviceId, notification.Job.Device.AccessToken, notification.Job);
    }
}
