using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SelenneApi.Models.DTOs;

namespace SelenneApi.Controllers;

[ApiController]
[Route("api/config")]
public class ConfigController : ControllerBase
{
    private readonly IConfiguration _config;

    public ConfigController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("banco")]
    [AllowAnonymous]
    public IActionResult GetBanco()
    {
        return Ok(ApiResponse<object>.Ok(new
        {
            banco        = _config["BankAccount:Banco"]        ?? "Nequi",
            numeroCuenta = _config["BankAccount:NumeroCuenta"] ?? "",
            titular      = _config["BankAccount:Titular"]      ?? "Cristian Cordoba",
            tipoCuenta   = _config["BankAccount:TipoCuenta"]   ?? "Nequi",
            whatsapp     = _config["BankAccount:WhatsApp"]     ?? "",
        }));
    }
}
