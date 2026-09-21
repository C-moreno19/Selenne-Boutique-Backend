using Microsoft.EntityFrameworkCore;
using SelenneApi.Data;

namespace SelenneApi.Services;

// Regla de validacion centralizada: la usan tanto el endpoint publico de
// "validar cupon" (para mostrarle el descuento al cliente en el checkout)
// como la creacion del pedido (que NUNCA confia en un monto de descuento
// mandado por el cliente — siempre recalcula server-side).
public class CuponService : ICuponService
{
    private readonly AppDbContext _db;
    public CuponService(AppDbContext db) { _db = db; }

    public async Task<ResultadoValidacionCupon> ValidarAsync(string codigo, decimal subtotal)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return new(false, "Ingresa un código de cupón", null, 0);

        var cupon = await _db.Cupones.FirstOrDefaultAsync(c => c.Codigo.ToUpper() == codigo.Trim().ToUpper());
        if (cupon == null)
            return new(false, "El cupón no existe", null, 0);

        if (!cupon.Activo)
            return new(false, "Este cupón ya no está activo", null, 0);

        var ahora = DateTime.UtcNow;
        if (cupon.FechaInicio.HasValue && ahora < cupon.FechaInicio.Value)
            return new(false, "Este cupón todavía no está disponible", null, 0);

        if (cupon.FechaExpiracion.HasValue && ahora > cupon.FechaExpiracion.Value)
            return new(false, "Este cupón ya expiró", null, 0);

        if (cupon.UsosMaximos.HasValue && cupon.UsosActuales >= cupon.UsosMaximos.Value)
            return new(false, "Este cupón alcanzó su límite de usos", null, 0);

        if (cupon.MontoMinimo.HasValue && subtotal < cupon.MontoMinimo.Value)
            return new(false, $"Este cupón requiere una compra mínima de {cupon.MontoMinimo.Value:N0}", null, 0);

        var descuento = cupon.TipoDescuento == "porcentaje"
            ? Math.Round(subtotal * (cupon.ValorDescuento / 100m), 2)
            : Math.Min(cupon.ValorDescuento, subtotal);

        return new(true, null, cupon, descuento);
    }
}
