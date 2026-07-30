using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SelenneApi.Models.DTOs;
using SelenneApi.Services;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<UploadController> _logger;

    public UploadController(ICloudinaryService cloudinaryService, ILogger<UploadController> logger)
    {
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    [HttpPost("imagen"), Authorize]
    public async Task<IActionResult> SubirImagen(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("No se recibió ningún archivo"));

        // Validar tipo de archivo
        var allowedTypes = new[] { "image/png", "image/jpeg", "image/jpg", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(ApiResponse<object>.Fail("Solo se permiten imágenes PNG, JPG o WebP"));

        // Validar tamaño (máx 5MB)
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(ApiResponse<object>.Fail("La imagen no puede superar 5MB"));

        using var inputStream = file.OpenReadStream();
        var fileUrl = await _cloudinaryService.SubirImagenAsync(inputStream, file.FileName);

        _logger.LogInformation("Imagen subida a Cloudinary: {Url}", fileUrl);

        return Ok(ApiResponse<object>.Ok(new { url = fileUrl }, "Imagen subida correctamente"));
    }
}
