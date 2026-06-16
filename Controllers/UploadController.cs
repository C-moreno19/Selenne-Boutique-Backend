using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SelenneApi.Models.DTOs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<UploadController> _logger;

    public UploadController(IWebHostEnvironment env, ILogger<UploadController> logger)
    {
        _env = env;
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

        // Crear carpeta si no existe
        var uploadsFolder = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // Guardar como WebP optimizado
        var fileName = $"{Guid.NewGuid()}.webp";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using var inputStream = file.OpenReadStream();
        using var image = await Image.LoadAsync(inputStream);

        // Redimensionar si supera 1200px de ancho manteniendo proporción
        if (image.Width > 1200)
            image.Mutate(x => x.Resize(1200, 0));

        var encoder = new WebpEncoder { Quality = 82 };
        await image.SaveAsync(filePath, encoder);

        // URL pública del archivo
        var request = HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var fileUrl = $"{baseUrl}/uploads/{fileName}";

        _logger.LogInformation("Imagen subida: {FileName} → {Url}", fileName, fileUrl);

        return Ok(ApiResponse<object>.Ok(new { url = fileUrl }, "Imagen subida correctamente"));
    }
}