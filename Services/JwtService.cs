using Microsoft.IdentityModel.Tokens;
using SelenneApi.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SelenneApi.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtService(IConfiguration config)
    {
        _config = config;
        _secretKey = config["Jwt:SecretKey"] ?? "SuClaveSecretaMuyLargaAquiAlMenos32CaracteresParaJWT!";
        _issuer = config["Jwt:Issuer"] ?? "SelenneApi";
        _audience = config["Jwt:Audience"] ?? "SelenneClient";
    }

    public string GenerateAccessToken(Usuario usuario, List<string> permisos)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiryMinutes"] ?? "480"));
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.UsuarioID.ToString()),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Name, usuario.NombreCompleto),
            new("roleId", usuario.RoleID?.ToString() ?? ""),
            new("roleName", usuario.Rol?.Nombre ?? "")
        };
        claims.AddRange(permisos.Select(p => new Claim("permission", p)));
        var token = new JwtSecurityToken(issuer: _issuer, audience: _audience,
            claims: claims, expires: expiry, signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tvp = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
            ValidateLifetime = false
        };
        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, tvp, out var sec);
            if (sec is not JwtSecurityToken jwt || !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)) return null;
            return principal;
        }
        catch { return null; }
    }

    public int? GetUserIdFromToken(string token)
    {
        var claim = GetPrincipalFromExpiredToken(token)?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && int.TryParse(claim, out var id) ? id : null;
    }
}