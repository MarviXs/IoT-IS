using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.MqttClient.Publish;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Jobs.EventHandlers;

public class JobCreatedEvent(Job job) : INotification
{
    public Job Job { get; } = job;
}

public class JobCreatedEventHandler(IServiceProvider serviceProvider) : INotificationHandler<JobCreatedEvent>
{
    public async Task Handle(JobCreatedEvent notification, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var device = await dbContext.Devices.AsNoTracking().Where(d => d.Id == notification.Job.DeviceId).FirstOrDefaultAsync(cancellationToken);
        if (device == null)
        {
            return;
        }

        // Publish job status to MQTT
        if (device.AccessToken != null && device.Protocol == DeviceConnectionProtocol.MQTT)
        {
            var publishJobStatus = scope.ServiceProvider.GetRequiredService<PublishJobStatus>();
            await publishJobStatus.Execute(notification.Job.DeviceId, device.AccessToken, notification.Job);
        }
    }
}
