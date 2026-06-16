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
/// Pruebas unitarias para ComprasController (definido en CatalogosController.cs).
/// Cubre los métodos Create (POST /api/compras) y Update (PUT /api/compras/{id}).
/// </summary>
public class ComprasControllerTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly ComprasController _controller;

    public ComprasControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _db = new AppDbContext(options);
        _controller = new ComprasController(_db);
    }

    // ── Helpers de datos de prueba ────────────────────────────────────

    private async Task<Proveedor> SeedProveedorAsync(int id = 1)
    {
        var proveedor = new Proveedor
        {
            ProveedorID = id,
            Nombre = $"Proveedor de Prueba {id}",
            Estado = "activo"
        };
        _db.Proveedores.Add(proveedor);
        await _db.SaveChangesAsync();
        return proveedor;
    }

    private async Task<Compra> SeedCompraAsync(int compraId, int proveedorId)
    {
        await SeedProveedorAsync(proveedorId);
        var compra = new Compra
        {
            CompraID = compraId,
            ProveedorID = proveedorId,
            OrdenFactura = $"FAC-{compraId:D4}",
            Fecha = DateTime.Now,
            Total = 500000m,
            Estado = "Pendiente"
        };
        _db.Compras.Add(compra);
        await _db.SaveChangesAsync();
        return compra;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRUEBAS PARA EL MÉTODO CREATE (POST /api/compras)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Prueba 1: Proveedor no existe → 400 Bad Request.
    /// No se puede registrar una compra sin un proveedor válido.
    /// </summary>
    [Fact]
    public async Task Create_ProveedorNoExiste_RetornaBadRequest()
    {
        // Arrange: no se registra ningún proveedor en la BD
        var dto = new CompraDto
        {
            ProveedorID = 9999,  // ID inexistente
            OrdenFactura = "FAC-0001",
            Total = 200000m
        };

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Contains("Proveedor", response.Message);
    }

    /// <summary>
    /// Prueba 2: Creación exitosa → 200 OK + compra guardada en BD.
    /// Verifica que la compra queda registrada con estado "Pendiente".
    /// </summary>
    [Fact]
    public async Task Create_ConProveedorValido_RetornaOkYGuardaCompra()
    {
        // Arrange: proveedor registrado
        await SeedProveedorAsync(id: 1);
        var dto = new CompraDto
        {
            ProveedorID = 1,
            OrdenFactura = "FAC-2024-001",
            Fecha = DateTime.Now,
            Total = 350000m,
            Notas = "Compra de reposición de stock"
        };

        // Act
        var result = await _controller.Create(dto);

        // Assert - respuesta HTTP 200
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.True(response.Success);

        // Assert - compra guardada en BD con estado "Pendiente"
        var compra = await _db.Compras.FirstOrDefaultAsync(c => c.ProveedorID == 1);
        Assert.NotNull(compra);
        Assert.Equal("Pendiente", compra.Estado);
        Assert.Equal(350000m, compra.Total);
        Assert.Equal("FAC-2024-001", compra.OrdenFactura);
    }

    /// <summary>
    /// Prueba 3: Creación con detalles → detalles guardados en BD.
    /// Verifica que los ítems de la compra se persisten correctamente.
    /// </summary>
    [Fact]
    public async Task Create_ConDetalles_GuardaDetallesEnBD()
    {
        // Arrange: proveedor y dos productos
        await SeedProveedorAsync(id: 2);
        _db.Productos.AddRange(
            new Producto { ProductoID = 10, Codigo = "P010", Nombre = "Blusa", PrecioVenta = 40000m, Stock = 0 },
            new Producto { ProductoID = 11, Codigo = "P011", Nombre = "Pantalon", PrecioVenta = 70000m, Stock = 0 }
        );
        await _db.SaveChangesAsync();

        var dto = new CompraDto
        {
            ProveedorID = 2,
            OrdenFactura = "FAC-2024-002",
            Total = 220000m,
            Detalles = new List<CompraDetalleDto>
            {
                new CompraDetalleDto { ProductoID = 10, Cantidad = 3, PrecioUnitario = 30000m, Total = 90000m },
                new CompraDetalleDto { ProductoID = 11, Cantidad = 2, PrecioUnitario = 65000m, Total = 130000m }
            }
        };

        // Act
        await _controller.Create(dto);

        // Assert - se guardaron 2 detalles en la BD
        var compra = await _db.Compras.FirstOrDefaultAsync(c => c.ProveedorID == 2);
        Assert.NotNull(compra);
        var detalles = await _db.CompraDetalles.Where(d => d.CompraID == compra.CompraID).ToListAsync();
        Assert.Equal(2, detalles.Count);
        Assert.Contains(detalles, d => d.ProductoID == 10 && d.Cantidad == 3);
        Assert.Contains(detalles, d => d.ProductoID == 11 && d.Cantidad == 2);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRUEBAS PARA EL MÉTODO UPDATE (PUT /api/compras/{id})
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Prueba 4: Compra no existe → 404 Not Found.
    /// </summary>
    [Fact]
    public async Task Update_CompraNoExiste_RetornaNotFound()
    {
        // Arrange: BD vacía
        var dto = new CompraDto { ProveedorID = 1, OrdenFactura = "X", Total = 0m };

        // Act
        var result = await _controller.Update(9999, dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    /// Prueba 5: Actualización exitosa → 200 OK + datos actualizados en BD.
    /// Verifica que OrdenFactura, Total y Notas se actualizan correctamente.
    /// </summary>
    [Fact]
    public async Task Update_ConDatosValidos_ActualizaCompraEnBD()
    {
        // Arrange: compra existente con datos originales
        await SeedCompraAsync(compraId: 1, proveedorId: 3);

        var dto = new CompraDto
        {
            ProveedorID = 3,
            OrdenFactura = "FAC-CORREGIDA-001",
            Total = 750000m,
            Notas = "Orden corregida por error de digitación"
        };

        // Act
        var result = await _controller.Update(1, dto);

        // Assert - respuesta HTTP 200
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.True(Assert.IsType<ApiResponse<object>>(ok.Value).Success);

        // Assert - datos actualizados en BD
        var compra = await _db.Compras.FindAsync(1);
        Assert.Equal("FAC-CORREGIDA-001", compra!.OrdenFactura);
        Assert.Equal(750000m, compra.Total);
        Assert.Equal("Orden corregida por error de digitación", compra.Notas);
    }

    /// <summary>
    /// Prueba 6: UpdateEstado — compra no existe → 404.
    /// </summary>
    [Fact]
    public async Task UpdateEstado_CompraNoExiste_RetornaNotFound()
    {
        var result = await _controller.UpdateEstado(9999, new EstadoDto { Estado = "Completada" });
        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    /// Prueba 7: UpdateEstado exitoso → 200 OK + estado persistido.
    /// </summary>
    [Fact]
    public async Task UpdateEstado_CompraExistente_ActualizaEstadoYRetornaOk()
    {
        // Arrange: compra en "Pendiente"
        await SeedCompraAsync(compraId: 2, proveedorId: 4);

        // Act
        var result = await _controller.UpdateEstado(2, new EstadoDto { Estado = "Completada" });

        // Assert - respuesta HTTP 200
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.True(Assert.IsType<ApiResponse<object>>(ok.Value).Success);

        // Assert - estado actualizado en BD
        var compra = await _db.Compras.FindAsync(2);
        Assert.Equal("Completada", compra!.Estado);
    }

    public void Dispose() => _db.Dispose();
}
