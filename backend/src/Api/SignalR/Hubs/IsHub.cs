using System.Security.Claims;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Features.Devices.Extensions;
using Fei.Is.Api.SignalR.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.SignalR.Hubs;

[Authorize]
public class IsHub(ILogger<IsHub> logger, AppDbContext dbContext) : Hub<INotificationsClient>
{
    public async Task SubscribeToDevice(Guid deviceId)
    {
        var device = await dbContext.Devices.Include(d => d.SharedWithUsers).AsNoTracking().Where(d => d.Id == deviceId).SingleOrDefaultAsync();
        if (device == null)
        {
            return;
        }
        if (Context.User == null || !device.CanView(Context.User))
        {
            return;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, deviceId.ToString());
    }

    public async Task UnsubscribeFromDevice(Guid deviceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, deviceId.ToString());
    }
}
