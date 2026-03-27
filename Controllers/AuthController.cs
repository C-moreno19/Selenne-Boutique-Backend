using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;
using SelenneApi.Helpers;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.DTOs.Auth;
using SelenneApi.Services;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly AppDbContext _db;

    public AuthController(IAuthService auth, AppDbContext db)
    {
        _auth = auth;
        _db = db;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequestDto dto)
    {
        var result = await _auth.SignupAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var result = await _auth.LoginAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString());
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        // Se añade verificación de nulo para evitar advertencia CS8602
        if (string.IsNullOrEmpty(dto.RefreshToken))
            return BadRequest(ApiResponse<object>.Fail("Token requerido"));

        var result = await _auth.RefreshTokenAsync(dto.RefreshToken);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        if (string.IsNullOrEmpty(dto.RefreshToken))
            return BadRequest(ApiResponse<object>.Fail("Token requerido"));

        return Ok(await _auth.LogoutAsync(dto.RefreshToken));
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
    {
        if (string.IsNullOrEmpty(dto.Token))
            return BadRequest(ApiResponse<object>.Fail("Token requerido"));

        var result = await _auth.VerifyEmailAsync(dto.Token);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (string.IsNullOrEmpty(dto.Email))
            return BadRequest(ApiResponse<object>.Fail("Email requerido"));

        return Ok(await _auth.ForgotPasswordAsync(dto.Email));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (string.IsNullOrEmpty(dto.Token) || string.IsNullOrEmpty(dto.NuevaContrasena))
            return BadRequest(ApiResponse<object>.Fail("Datos incompletos"));

        var result = await _auth.ResetPasswordAsync(dto.Token, dto.NuevaContrasena);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("gen-hash")]
    [AllowAnonymous]
    public IActionResult GenHash([FromQuery] string pass)
    {
        if (string.IsNullOrEmpty(pass)) return BadRequest("Password requerido");
        return Ok(BCrypt.Net.BCrypt.HashPassword(pass));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var uid = User.GetUserId();
        var usuario = await _db.Usuarios.FindAsync(uid);

        if (usuario == null)
            return NotFound(ApiResponse<object>.Fail("Usuario no encontrado"));

        // Verificamos que PasswordHash no sea nulo antes de validar
        if (string.IsNullOrEmpty(usuario.PasswordHash) ||
            !BCrypt.Net.BCrypt.Verify(dto.ContrasenaActual, usuario.PasswordHash))
        {
            return BadRequest(ApiResponse<object>.Fail("La contraseña actual es incorrecta"));
        }

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaContrasena);
        await _db.SaveChangesAsync();

        // Se usa un objeto anónimo vacío { } en lugar de null para evitar advertencia CS8625
        return Ok(ApiResponse<object>.Ok(new { }, "Contraseña actualizada exitosamente"));
    }
}