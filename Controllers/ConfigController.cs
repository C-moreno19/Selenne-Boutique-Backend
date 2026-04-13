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
            banco        = _config["BankAccount:Banco"]        ?? "Bancolombia",
            numeroCuenta = _config["BankAccount:NumeroCuenta"] ?? "",
            titular      = _config["BankAccount:Titular"]      ?? "Selenne Boutique",
            tipoCuenta   = _config["BankAccount:TipoCuenta"]   ?? "Ahorros",
            whatsapp     = _config["BankAccount:WhatsApp"]     ?? "",
        }));
    }
}
