using System.Security.Claims;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Devices.Extensions;

public static class DeviceExtensions
{
    public static bool IsOwner(this Device device, ClaimsPrincipal user)
    {
        return device.OwnerId == user.GetUserId();
    }

    public static bool IsSharedWithUser(this Device device, ClaimsPrincipal user)
    {
        if (device.SharedWithUsers == null)
        {
            return false;
        }

        return device.SharedWithUsers.Any(share => share.SharedToUserId == user.GetUserId());
    }

    public static bool IsEditor(this Device device, ClaimsPrincipal user)
    {
        if (user.IsAdmin())
        {
            return true;
        }
        if (device.SharedWithUsers == null)
        {
            return false;
        }

        return device.SharedWithUsers.Any(share => share.SharedToUserId == user.GetUserId() && share.Permission == DeviceSharePermission.Editor);
    }

    public static bool CanView(this Device device, ClaimsPrincipal user)
    {
        if (user.IsAdmin())
        {
            return true;
        }

        return device.IsOwner(user) || device.IsSharedWithUser(user);
    }

    public static bool CanEdit(this Device device, ClaimsPrincipal user)
    {
        if (user.IsAdmin())
        {
            return true;
        }

        return device.IsOwner(user) || device.IsEditor(user);
    }

    public static DevicePermission GetPermission(this Device device, ClaimsPrincipal user)
    {
        if (device.IsOwner(user))
        {
            return DevicePermission.Owner;
        }

        if (device.IsEditor(user))
        {
            return DevicePermission.Editor;
        }

        return DevicePermission.Viewer;
    }

    public static DeviceConnectionState GetConnectionState(this Device device, bool mqttConnected, DateTimeOffset? lastSeen, DateTimeOffset nowUtc)
    {
        return GetConnectionState(device.Protocol, device.SampleRateSeconds, mqttConnected, lastSeen, nowUtc);
    }

    public static DeviceConnectionState GetConnectionState(
        DeviceConnectionProtocol protocol,
        float? sampleRateSeconds,
        bool mqttConnected,
        DateTimeOffset? lastSeen,
        DateTimeOffset nowUtc
    )
    {
        if (protocol == DeviceConnectionProtocol.MQTT && !mqttConnected)
        {
            return DeviceConnectionState.Offline;
        }

        if (protocol == DeviceConnectionProtocol.HTTP && (!sampleRateSeconds.HasValue || sampleRateSeconds <= 0))
        {
            sampleRateSeconds = 60;
        }

        if (!lastSeen.HasValue)
        {
            return DeviceConnectionState.Offline;
        }

        if (!sampleRateSeconds.HasValue || sampleRateSeconds <= 0)
        {
            return DeviceConnectionState.Online;
        }

        var elapsed = nowUtc - lastSeen.Value;
        var window = TimeSpan.FromSeconds(sampleRateSeconds.Value);

        if (elapsed > window + window)
        {
            return DeviceConnectionState.Offline;
        }

        if (elapsed > window)
        {
            return DeviceConnectionState.Degraded;
        }

        return DeviceConnectionState.Online;
    }

    public static IQueryable<Device> WhereOwned(this IQueryable<Device> devices, Guid ownerId)
    {
        return devices.Where(d => d.OwnerId == ownerId);
    }

    public static IQueryable<Device> WhereOwnedOrShared(this IQueryable<Device> devices, Guid userId)
    {
        return devices
            .Include(d => d.SharedWithUsers)
            .Where(d => d.OwnerId == userId || d.SharedWithUsers.Any(share => share.SharedToUserId == userId));
    }
}
