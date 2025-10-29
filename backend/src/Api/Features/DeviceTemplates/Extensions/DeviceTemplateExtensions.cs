using System.Security.Claims;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;

namespace Fei.Is.Api.Features.DeviceTemplates.Extensions;

public static class DeviceTemplateExtensions
{
    public static bool IsOwner(this DeviceTemplate template, ClaimsPrincipal user)
    {
        if (user.IsAdmin())
        {
            return true;
        }

        return template.OwnerId == user.GetUserId();
    }

    public static bool CanEdit(this DeviceTemplate template, ClaimsPrincipal user)
    {
        return template.IsOwner(user);
    }
}
