using System.Security.Claims;
namespace SelenneApi.Helpers;
public static class PermissionHelper
{
    public static int GetUserId(ClaimsPrincipal user)
    {
        var val = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return val != null && int.TryParse(val, out var id) ? id : 0;
    }

    public static bool HasPermission(ClaimsPrincipal user, string permission)
        => user.Claims.Any(c => c.Type == "permission" && c.Value == permission);
}
