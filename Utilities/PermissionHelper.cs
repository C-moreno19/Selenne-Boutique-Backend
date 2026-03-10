using System.Security.Claims;

namespace SelenneApi.Utilities;

public static class PermissionHelper
{ public static int GetUserId(System.Security.Claims.ClaimsPrincipal user) { var c = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier); return int.TryParse(c?.Value, out var id) ? id : 0; } }
