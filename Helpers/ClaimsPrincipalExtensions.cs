using System.Security.Claims;
namespace SelenneApi.Helpers;
public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var val = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return val != null && int.TryParse(val, out var id) ? id : 0;
    }
    public static bool HasPermission(this ClaimsPrincipal user, string permission)
        => user.Claims.Any(c => c.Type == "permission" && c.Value == permission);
}
