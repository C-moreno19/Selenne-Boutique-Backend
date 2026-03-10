using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.DTOs.Auth;
using SelenneApi.Models.Entities;

namespace SelenneApi.Services;

public interface IAuthService
{
    Task<ApiResponse<TokenResponseDto>> SignupAsync(SignupRequestDto dto, string? ip);
    Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginRequestDto dto, string? ip);
    Task<ApiResponse<string>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<bool>> LogoutAsync(string refreshToken);
    Task<ApiResponse<bool>> VerifyEmailAsync(string token);
    Task<ApiResponse<bool>> ForgotPasswordAsync(string email);
    Task<ApiResponse<bool>> ResetPasswordAsync(string token, string newPassword);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;
    private readonly IEmailService _email;
    private readonly INotificationService _notif;
    private readonly IPermissionService _perms;

    public AuthService(AppDbContext db, IJwtService jwt, IEmailService email, INotificationService notif, IPermissionService perms)
    { _db = db; _jwt = jwt; _email = email; _notif = notif; _perms = perms; }

    public async Task<ApiResponse<TokenResponseDto>> SignupAsync(SignupRequestDto dto, string? ip)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
            return ApiResponse<TokenResponseDto>.Fail("El email ya esta registrado");

        var clienteRole = await _db.Roles.FirstOrDefaultAsync(r => r.Nombre == "Cliente");
        var usuario = new Usuario {
            NombreCompleto = dto.NombreCompleto, Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena),
            Telefono = dto.Telefono, RoleID = clienteRole?.RoleID, Estado = "activo"
        };
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        var vToken = Guid.NewGuid().ToString("N");
        _db.EmailVerifications.Add(new EmailVerification {
            UsuarioID = usuario.UsuarioID, Token = vToken, ExpiresAt = DateTime.Now.AddHours(24)
        });
        await _db.SaveChangesAsync();

        _ = Task.Run(async () => {
            await _email.SendWelcomeEmailAsync(usuario.Email, usuario.NombreCompleto);
            await _email.SendVerificationEmailAsync(usuario.Email, usuario.NombreCompleto, vToken);
        });

        return await BuildTokenResponse(usuario, ip);
    }

    public async Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginRequestDto dto, string? ip)
    {
        var usuario = await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Email == dto.Email);
        var ok = usuario != null && BCrypt.Net.BCrypt.Verify(dto.Contrasena, usuario.PasswordHash);

        _db.LoginAttempts.Add(new LoginAttempt {
            UsuarioID = usuario?.UsuarioID, Email = dto.Email, Successful = ok, IPAddress = ip
        });
        await _db.SaveChangesAsync();

        if (!ok) return ApiResponse<TokenResponseDto>.Fail("Credenciales invalidas");
        if (usuario!.Estado != "activo") return ApiResponse<TokenResponseDto>.Fail("Cuenta bloqueada o inactiva");

        usuario.FechaUltimoLogin = DateTime.Now;
        await _db.SaveChangesAsync();

        var response = await BuildTokenResponse(usuario, ip);

        _ = Task.Run(() => _notif.CreateAsync(usuario.UsuarioID, "Inicio de sesion", "Nuevo inicio de sesion detectado.", "info"));

        return response;
    }

    public async Task<ApiResponse<string>> RefreshTokenAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens.Include(rt => rt.Usuario).ThenInclude(u => u.Rol)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.Revoked && rt.ExpiresAt > DateTime.Now);
        if (token == null) return ApiResponse<string>.Fail("Refresh token invalido o expirado");
        var permisos = await _perms.GetUserPermissionsAsync(token.UsuarioID);
        return ApiResponse<string>.Ok(_jwt.GenerateAccessToken(token.Usuario, permisos));
    }

    public async Task<ApiResponse<bool>> LogoutAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (token != null) { token.Revoked = true; token.RevokedAt = DateTime.Now; await _db.SaveChangesAsync(); }
        return ApiResponse<bool>.Ok(true, "Sesion cerrada");
    }

    public async Task<ApiResponse<bool>> VerifyEmailAsync(string token)
    {
        var ver = await _db.EmailVerifications.Include(ev => ev.Usuario)
            .FirstOrDefaultAsync(ev => ev.Token == token && !ev.Verified && ev.ExpiresAt > DateTime.Now);
        if (ver == null) return ApiResponse<bool>.Fail("Token invalido o expirado");
        ver.Verified = true; ver.VerifiedAt = DateTime.Now; ver.Usuario.EmailVerificado = true;
        await _db.SaveChangesAsync();
        _ = Task.Run(() => _notif.CreateAsync(ver.UsuarioID, "Email verificado", "Email verificado exitosamente.", "success"));
        return ApiResponse<bool>.Ok(true, "Email verificado");
    }

    public async Task<ApiResponse<bool>> ForgotPasswordAsync(string email)
    {
        var u = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        if (u != null) {
            var tok = Guid.NewGuid().ToString("N");
            _db.PasswordResetTokens.Add(new PasswordResetToken { UsuarioID = u.UsuarioID, Token = tok, ExpiresAt = DateTime.Now.AddHours(1) });
            await _db.SaveChangesAsync();
            _ = Task.Run(() => _email.SendPasswordResetEmailAsync(email, u.NombreCompleto, tok));
        }
        return ApiResponse<bool>.Ok(true, "Si el email existe, recibiras instrucciones");
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(string token, string newPassword)
    {
        var reset = await _db.PasswordResetTokens.Include(rt => rt.Usuario)
            .FirstOrDefaultAsync(rt => rt.Token == token && !rt.Used && rt.ExpiresAt > DateTime.Now);
        if (reset == null) return ApiResponse<bool>.Fail("Token invalido o expirado");
        reset.Usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        reset.Used = true; reset.UsedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        _ = Task.Run(() => _email.SendPasswordChangedEmailAsync(reset.Usuario.Email, reset.Usuario.NombreCompleto));
        return ApiResponse<bool>.Ok(true, "Contrasena actualizada");
    }

    private async Task<ApiResponse<TokenResponseDto>> BuildTokenResponse(Usuario usuario, string? ip)
    {
        if (usuario.Rol == null && usuario.RoleID.HasValue)
            await _db.Entry(usuario).Reference(u => u.Rol).LoadAsync();
        var permisos = await _perms.GetUserPermissionsAsync(usuario.UsuarioID);
        var access = _jwt.GenerateAccessToken(usuario, permisos);
        var refresh = _jwt.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken {
            UsuarioID = usuario.UsuarioID, Token = refresh,
            ExpiresAt = DateTime.Now.AddDays(7), IPAddress = ip
        });
        await _db.SaveChangesAsync();
        return ApiResponse<TokenResponseDto>.Ok(new TokenResponseDto {
            AccessToken = access, RefreshToken = refresh,
            Usuario = new UsuarioTokenDto {
                UsuarioID = usuario.UsuarioID, NombreCompleto = usuario.NombreCompleto,
                Email = usuario.Email, Rol = usuario.Rol?.Nombre, Permisos = permisos
            }
        });
    }
}
