using System.Security.Claims;
using Fei.Is.Api.Data.Contexts;
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
        var device = await dbContext.Devices.AsNoTracking().Where(d => d.Id == deviceId).SingleOrDefaultAsync();
        if (device == null)
        {
            return;
        }
        if (device.OwnerId != GetUserId())
        {
            return;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, deviceId.ToString());
    }

    public async Task UnsubscribeFromDevice(Guid deviceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, deviceId.ToString());
    }

    private Guid GetUserId()
    {
        var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not authenticated");
        return Guid.Parse(userIdString);
    }
}
