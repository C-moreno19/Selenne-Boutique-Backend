using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SelenneApi.Controllers;
using SelenneApi.Data;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.Entities;
using Xunit;

namespace SelenneApi.Tests;

/// <summary>
/// Pruebas unitarias para VentasController (definido en CatalogosController.cs).
/// Cubre los métodos Create (POST /api/ventas) y UpdateEstado (PUT /api/ventas/{id}/estado).
/// </summary>
public class VentasControllerTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly VentasController _controller;

    public VentasControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _db = new AppDbContext(options);
        _controller = new VentasController(_db);
    }

    // ── Helper ────────────────────────────────────────────────────────

    private async Task<Venta> SeedVentaAsync(int ventaId, string estado = "Pendiente")
    {
        var venta = new Venta
        {
            VentaID    = ventaId,
            FechaVenta = DateTime.Now,
            Subtotal   = 120000m,
            Total      = 120000m,
            Estado     = estado,
            MetodoPago = "Efectivo"
        };
        _db.Ventas.Add(venta);
        await _db.SaveChangesAsync();
        return venta;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRUEBAS PARA EL MÉTODO CREATE (POST /api/ventas)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Prueba 1: Creación exitosa → 200 OK + venta guardada con estado "Pendiente".
    /// </summary>
    [Fact]
    public async Task Create_ConDatosValidos_RetornaOkYGuardaVenta()
    {
        // Arrange
        var dto = new VentaDto
        {
            Subtotal  = 80000m,
            Descuento = 0m,
            Envio     = 5000m,
            Total     = 85000m
        };

        // Act
        var result = await _controller.Create(dto);

        // Assert - respuesta HTTP 200
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.True(Assert.IsType<ApiResponse<object>>(ok.Value).Success);

        // Assert - venta guardada con estado "Pendiente" por defecto
        var venta = await _db.Ventas.FirstOrDefaultAsync();
        Assert.NotNull(venta);
        Assert.Equal("Pendiente", venta.Estado);
        Assert.Equal(85000m, venta.Total);
    }

    /// <summary>
    /// Prueba 2: El descuento se almacena correctamente en la BD.
    /// </summary>
    [Fact]
    public async Task Create_ConDescuento_GuardaDescuentoCorrectamente()
    {
        // Arrange: subtotal $200.000, descuento $20.000, total $180.000
        var dto = new VentaDto
        {
            Subtotal  = 200000m,
            Descuento = 20000m,
            Envio     = 0m,
            Total     = 180000m
        };

        // Act
        await _controller.Create(dto);

        // Assert - subtotal, descuento y total persistidos
        var venta = await _db.Ventas.FirstOrDefaultAsync();
        Assert.NotNull(venta);
        Assert.Equal(200000m, venta.Subtotal);
        Assert.Equal(20000m, venta.Descuento);
        Assert.Equal(180000m, venta.Total);
    }

    /// <summary>
    /// Prueba 3: Venta con envío → costo de envío guardado correctamente.
    /// </summary>
    [Fact]
    public async Task Create_ConEnvio_GuardaEnvioCorrectamente()
    {
        // Arrange
        var dto = new VentaDto
        {
            Subtotal  = 150000m,
            Descuento = 0m,
            Envio     = 8000m,
            Total     = 158000m
        };

        // Act
        await _controller.Create(dto);

        // Assert
        var venta = await _db.Ventas.FirstOrDefaultAsync();
        Assert.NotNull(venta);
        Assert.Equal(8000m, venta.Envio);
        Assert.Equal(158000m, venta.Total);
    }

    /// <summary>
    /// Prueba 4: Venta con clienteID asignado → referencia guardada.
    /// </summary>
    [Fact]
    public async Task Create_ConClienteID_AsignaClienteCorrectamente()
    {
        // Arrange
        var dto = new VentaDto
        {
            ClienteID = 42,
            Subtotal  = 50000m,
            Total     = 50000m
        };

        // Act
        await _controller.Create(dto);

        // Assert
        var venta = await _db.Ventas.FirstOrDefaultAsync();
        Assert.NotNull(venta);
        Assert.Equal(42, venta.ClienteID);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRUEBAS PARA EL MÉTODO UPDATE ESTADO (PUT /api/ventas/{id}/estado)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Prueba 5: Venta no existe → 404 Not Found.
    /// </summary>
    [Fact]
    public async Task UpdateEstado_VentaNoExiste_RetornaNotFound()
    {
        // Arrange: BD vacía

        // Act
        var result = await _controller.UpdateEstado(9999, new EstadoDto { Estado = "Completada" });

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    /// Prueba 6: Cambio de estado de "Pendiente" a "Completada" → 200 OK + estado persistido.
    /// </summary>
    [Fact]
    public async Task UpdateEstado_CambiaDeEstado_PersisteCambioEnBD()
    {
        // Arrange: venta en "Pendiente"
        await SeedVentaAsync(ventaId: 1, estado: "Pendiente");

        // Act
        var result = await _controller.UpdateEstado(1, new EstadoDto { Estado = "Completada" });

        // Assert - respuesta HTTP 200
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.True(Assert.IsType<ApiResponse<object>>(ok.Value).Success);

        // Assert - estado actualizado en BD
        var venta = await _db.Ventas.FindAsync(1);
        Assert.Equal("Completada", venta!.Estado);
    }

    /// <summary>
    /// Prueba 7: Anular una venta completada → estado cambia a "Anulada".
    /// </summary>
    [Fact]
    public async Task UpdateEstado_AnularVenta_ActualizaAAnulada()
    {
        // Arrange: venta en "Completada"
        await SeedVentaAsync(ventaId: 2, estado: "Completada");

        // Act
        var result = await _controller.UpdateEstado(2, new EstadoDto { Estado = "Anulada" });

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var venta = await _db.Ventas.FindAsync(2);
        Assert.Equal("Anulada", venta!.Estado);
    }

    public void Dispose() => _db.Dispose();
}
