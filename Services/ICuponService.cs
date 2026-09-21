using SelenneApi.Models.Entities;

namespace SelenneApi.Services;

public record ResultadoValidacionCupon(bool Valido, string? Error, Cupon? Cupon, decimal MontoDescuento);

public interface ICuponService
{
    Task<ResultadoValidacionCupon> ValidarAsync(string codigo, decimal subtotal);
}
