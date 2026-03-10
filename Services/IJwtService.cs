using SelenneApi.Models.Entities;
using System.Security.Claims;

namespace SelenneApi.Services;

public interface IJwtService
{
    string GenerateAccessToken(Usuario usuario, List<string> permisos);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    int? GetUserIdFromToken(string token);
}
