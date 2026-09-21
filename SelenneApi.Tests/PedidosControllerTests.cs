using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Configuration;
using Moq;
using SelenneApi.Controllers;
using SelenneApi.Data;
using SelenneApi.Models.DTOs;
using SelenneApi.Models.DTOs.Pedidos;
using SelenneApi.Models.Entities;
using SelenneApi.Services;
using Xunit;

namespace SelenneApi.Tests;

/// <summary>
/// Pruebas unitarias para PedidosController.
/// Cubre los métodos Create (POST /api/pedidos) y UpdateEstado (PUT /api/pedidos/{id}/estado).
/// </summary>
public class PedidosControllerTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly Mock<IEmailService> _emailMock;
    private readonly Mock<INotificationService> _notifMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly PedidosController _controller;

    public PedidosControllerTests()
    {
        // Base de datos en memoria aislada por prueba (GUID único evita colisiones).
        // Se ignora TransactionIgnoredWarning porque InMemory no soporta transacciones reales,
        // pero el código de producción las usa para garantizar atomicidad con SQL Server.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _db = new AppDbContext(options);

        _emailMock = new Mock<IEmailService>();
        _notifMock = new Mock<INotificationService>();
        _configMock = new Mock<IConfiguration>();
        _envMock = new Mock<IWebHostEnvironment>();

        _configMock.Setup(c => c["AppSettings:BaseUrl"]).Returns("http://localhost:5000");
        // GetValue<T>() (usado por NotificarStockBajoAsync) llama a GetSection() internamente;
        // sin este stub devuelve null y explota con NullReferenceException.
        _configMock.Setup(c => c.GetSection(It.IsAny<string>())).Returns(Mock.Of<IConfigurationSection>());

        // Se usa la implementacion real de ICuponService (no un mock) porque
        // valida contra el mismo _db en memoria -- asi las pruebas ejercitan
        // la logica real de validacion de cupones, no un doble simulado.
        _controller = new PedidosController(
            _db, _emailMock.Object, _notifMock.Object, _configMock.Object, _envMock.Object, new CuponService(_db));
    }

    // ── Helpers de contexto HTTP ──────────────────────────────────────

    private void SetAuthenticatedUser(int userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private void SetAnonymousUser()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // ── Helpers de datos de prueba ────────────────────────────────────

    private async Task<Usuario> SeedUsuarioAsync(int id = 1, string email = "user@test.com")
    {
        var usuario = new Usuario
        {
            UsuarioID = id,
            NombreCompleto = "Usuario de Prueba",
            Email = email,
            PasswordHash = "hash_de_prueba"
        };
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
        return usuario;
    }

    private async Task<Producto> SeedProductoAsync(int id = 10, decimal precio = 50000m, int stock = 10)
    {
        var producto = new Producto
        {
            ProductoID = id,
            Codigo = $"PROD{id:D3}",
            Nombre = $"Producto de Prueba {id}",
            PrecioVenta = precio,
            Stock = stock
        };
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();
        return producto;
    }

    private async Task<Pedido> SeedPedidoAsync(int pedidoId, int clienteId)
    {
        await SeedUsuarioAsync(clienteId, $"cliente{clienteId}@test.com");
        var pedido = new Pedido
        {
            PedidoID = pedidoId,
            ClienteID = clienteId,
            NombreCliente = "Cliente de Prueba",
            EmailCliente = $"cliente{clienteId}@test.com",
            TelefonoCliente = "3001234567",
            DireccionEnvio = "Calle 1 # 2-3",
            Ciudad = "Bogotá",
            MetodoPago = "Efectivo",
            Estado = "Pendiente"
        };
        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();
        return pedido;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRUEBAS PARA EL MÉTODO CREATE (POST /api/pedidos)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Prueba 1: Checkout de invitado (sin sesión) sin email → 400 Bad Request.
    /// El checkout de invitado esta permitido, pero exige nombre y correo
    /// para poder identificar/crear la cuenta minima que recibe el pedido.
    /// </summary>
    [Fact]
    public async Task Create_SinAutenticacionYSinEmail_RetornaBadRequest()
    {
        // Arrange: usuario anónimo, sin email en el DTO
        SetAnonymousUser();
        var dto = new CrearPedidoRequestDto
        {
            NombreCliente = "Juan Pérez",
            EmailCliente = "",
            TelefonoCliente = "3001234567",
            DireccionEnvio = "Calle 1 # 2-3",
            Ciudad = "Bogotá",
            MetodoPago = "Transferencia",
            Items = new List<PedidoItemDto>()
        };

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Contains("correo", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Prueba 2: Pedido sin productos → 400 Bad Request.
    /// El sistema debe exigir al menos un producto en el pedido.
    /// </summary>
    [Fact]
    public async Task Create_SinItems_RetornaBadRequest()
    {
        // Arrange: usuario autenticado, pero lista de items vacía
        await SeedUsuarioAsync(id: 1);
        SetAuthenticatedUser(userId: 1);
        var dto = new CrearPedidoRequestDto
        {
            NombreCliente = "Ana López",
            EmailCliente = "ana@test.com",
            TelefonoCliente = "3009876543",
            DireccionEnvio = "Carrera 5 # 10-20",
            Ciudad = "Medellín",
            MetodoPago = "Efectivo",
            Items = new List<PedidoItemDto>()   // sin productos
        };

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Contains("producto", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Prueba 3: Stock insuficiente → 400 Bad Request.
    /// Si el cliente pide más unidades de las disponibles, se rechaza el pedido.
    /// </summary>
    [Fact]
    public async Task Create_StockInsuficiente_RetornaBadRequest()
    {
        // Arrange: producto con stock=1, pedido solicita cantidad=5
        await SeedUsuarioAsync(id: 2);
        await SeedProductoAsync(id: 10, precio: 60000m, stock: 1);
        SetAuthenticatedUser(userId: 2);
        var dto = new CrearPedidoRequestDto
        {
            NombreCliente = "Pedro Gómez",
            EmailCliente = "pedro@test.com",
            TelefonoCliente = "3001112233",
            DireccionEnvio = "Av. Principal 100",
            Ciudad = "Cali",
            MetodoPago = "Transferencia",
            Items = new List<PedidoItemDto>
            {
                new PedidoItemDto { ProductoID = 10, Cantidad = 5 }  // solicita 5, solo hay 1
            }
        };

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Contains("Stock insuficiente", response.Message);
    }

    /// <summary>
    /// Prueba 4: Creación exitosa → 201 Created + stock descontado.
    /// Verifica el flujo completo: se crea el pedido y el stock del producto se reduce.
    /// </summary>
    [Fact]
    public async Task Create_ConDatosValidos_RetornaCreatedYDescontaStock()
    {
        // Arrange: usuario autenticado, producto con stock suficiente
        await SeedUsuarioAsync(id: 3, email: "carlos@test.com");
        await SeedProductoAsync(id: 20, precio: 80000m, stock: 10);
        SetAuthenticatedUser(userId: 3);

        _notifMock
            .Setup(n => n.CreateAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        var dto = new CrearPedidoRequestDto
        {
            NombreCliente = "Carlos Ruiz",
            EmailCliente = "carlos@test.com",
            TelefonoCliente = "3205554444",
            DireccionEnvio = "Calle 100 # 15-40",
            Ciudad = "Barranquilla",
            MetodoPago = "Efectivo",
            Items = new List<PedidoItemDto>
            {
                new PedidoItemDto { ProductoID = 20, Cantidad = 3 }  // 3 unidades a $80.000 = $240.000
            }
        };

        // Act
        var result = await _controller.Create(dto);

        // Assert - respuesta HTTP 201
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(created.Value);
        Assert.True(response.Success);

        // Assert - el stock se descontó correctamente (10 - 3 = 7)
        var producto = await _db.Productos.FindAsync(20);
        Assert.Equal(7, producto!.Stock);

        // Assert - el pedido quedó guardado con los valores correctos
        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.ClienteID == 3);
        Assert.NotNull(pedido);
        Assert.Equal("Pendiente", pedido.Estado);
        Assert.Equal(240000m, pedido.Total);  // 80.000 × 3 unidades
    }

    /// <summary>
    /// Prueba: un cupón de porcentaje válido descuenta el Total (no el Subtotal),
    /// queda registrado en el pedido, y su contador de usos se incrementa.
    /// </summary>
    [Fact]
    public async Task Create_ConCuponValido_AplicaDescuentoEIncrementaUso()
    {
        await SeedUsuarioAsync(id: 5, email: "descuento@test.com");
        await SeedProductoAsync(id: 25, precio: 100000m, stock: 10);
        SetAuthenticatedUser(userId: 5);

        var cupon = new Cupon
        {
            Codigo = "BIENVENIDA10",
            TipoDescuento = "porcentaje",
            ValorDescuento = 10,
            Activo = true,
            UsosActuales = 0
        };
        _db.Cupones.Add(cupon);
        await _db.SaveChangesAsync();

        _notifMock
            .Setup(n => n.CreateAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        var dto = new CrearPedidoRequestDto
        {
            NombreCliente = "Cliente Descuento",
            EmailCliente = "descuento@test.com",
            TelefonoCliente = "3001234567",
            DireccionEnvio = "Calle 8 # 9-10",
            Ciudad = "Bogotá",
            MetodoPago = "Efectivo",
            CuponCodigo = "bienvenida10",  // minusculas a proposito: la validacion no es case-sensitive
            Items = new List<PedidoItemDto>
            {
                new PedidoItemDto { ProductoID = 25, Cantidad = 2 }  // subtotal 200.000
            }
        };

        var result = await _controller.Create(dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);

        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.ClienteID == 5);
        Assert.NotNull(pedido);
        Assert.Equal(200000m, pedido!.Subtotal);
        Assert.Equal(20000m, pedido.Descuento);   // 10% de 200.000
        Assert.Equal(180000m, pedido.Total);
        Assert.Equal(cupon.CuponID, pedido.CuponID);

        var cuponActualizado = await _db.Cupones.FindAsync(cupon.CuponID);
        Assert.Equal(1, cuponActualizado!.UsosActuales);
    }

    /// <summary>
    /// Prueba: un código de cupón inexistente rechaza todo el pedido con 400,
    /// sin descontar stock ni crear el pedido a medias.
    /// </summary>
    [Fact]
    public async Task Create_ConCuponInexistente_RetornaBadRequestYNoCreaPedido()
    {
        await SeedUsuarioAsync(id: 6, email: "sincupon@test.com");
        await SeedProductoAsync(id: 26, precio: 50000m, stock: 5);
        SetAuthenticatedUser(userId: 6);

        var dto = new CrearPedidoRequestDto
        {
            NombreCliente = "Cliente Sin Cupon",
            EmailCliente = "sincupon@test.com",
            TelefonoCliente = "3009998888",
            DireccionEnvio = "Calle 1 # 1-1",
            Ciudad = "Cali",
            MetodoPago = "Efectivo",
            CuponCodigo = "NOEXISTE",
            Items = new List<PedidoItemDto>
            {
                new PedidoItemDto { ProductoID = 26, Cantidad = 1 }
            }
        };

        var result = await _controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.False(await _db.Pedidos.AnyAsync(p => p.ClienteID == 6));
        var producto = await _db.Productos.FindAsync(26);
        Assert.Equal(5, producto!.Stock);  // stock intacto
    }

    /// <summary>
    /// Prueba: checkout de invitado (sin sesión) con nombre/correo completos →
    /// crea el pedido y una cuenta mínima nueva asociada a ese email, sin
    /// exigir login previo.
    /// </summary>
    [Fact]
    public async Task Create_ComoInvitado_CreaPedidoYCuentaMinima()
    {
        // Arrange: sin usuario autenticado, producto con stock suficiente
        await SeedProductoAsync(id: 30, precio: 40000m, stock: 5);
        SetAnonymousUser();

        _notifMock
            .Setup(n => n.CreateAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        var dto = new CrearPedidoRequestDto
        {
            NombreCliente = "Invitada Anonima",
            EmailCliente = "invitada@test.com",
            TelefonoCliente = "3001112222",
            DireccionEnvio = "Calle 5 # 6-7",
            Ciudad = "Cali",
            MetodoPago = "Efectivo",
            Items = new List<PedidoItemDto>
            {
                new PedidoItemDto { ProductoID = 30, Cantidad = 2 }
            }
        };

        // Act
        var result = await _controller.Create(dto);

        // Assert - respuesta HTTP 201
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(created.Value);
        Assert.True(response.Success);

        // Assert - se creó una cuenta mínima para el invitado
        var usuarioCreado = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == "invitada@test.com");
        Assert.NotNull(usuarioCreado);
        Assert.Equal("Invitada Anonima", usuarioCreado!.NombreCompleto);

        // Assert - el pedido quedó asociado a esa cuenta
        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.EmailCliente == "invitada@test.com");
        Assert.NotNull(pedido);
        Assert.Equal(usuarioCreado.UsuarioID, pedido!.ClienteID);
    }

    /// <summary>
    /// Prueba: checkout de invitado con un email que ya tiene cuenta → el
    /// pedido se asocia a la cuenta existente en vez de crear una duplicada.
    /// </summary>
    [Fact]
    public async Task Create_ComoInvitadoConEmailExistente_ReutilizaLaCuenta()
    {
        var existente = await SeedUsuarioAsync(id: 40, email: "yaexiste@test.com");
        await SeedProductoAsync(id: 31, precio: 30000m, stock: 5);
        SetAnonymousUser();

        _notifMock
            .Setup(n => n.CreateAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        var dto = new CrearPedidoRequestDto
        {
            NombreCliente = "Nombre Distinto En El Form",
            EmailCliente = "yaexiste@test.com",
            TelefonoCliente = "3009998888",
            DireccionEnvio = "Carrera 9 # 10-11",
            Ciudad = "Medellín",
            MetodoPago = "Efectivo",
            Items = new List<PedidoItemDto>
            {
                new PedidoItemDto { ProductoID = 31, Cantidad = 1 }
            }
        };

        var result = await _controller.Create(dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(1, await _db.Usuarios.CountAsync(u => u.Email == "yaexiste@test.com"));
        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.EmailCliente == "yaexiste@test.com");
        Assert.NotNull(pedido);
        Assert.Equal(existente.UsuarioID, pedido!.ClienteID);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRUEBAS PARA EL MÉTODO UPDATE ESTADO (PUT /api/pedidos/{id}/estado)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Prueba 5: Pedido no encontrado → 404 Not Found.
    /// Al intentar actualizar un pedido que no existe, el sistema responde 404.
    /// </summary>
    [Fact]
    public async Task UpdateEstado_PedidoNoExiste_RetornaNotFound()
    {
        // Arrange: no hay pedidos en la base de datos
        SetAnonymousUser();

        // Act
        var result = await _controller.UpdateEstado(9999, new ActualizarEstadoPedidoDto
        {
            NuevoEstado = "Aprobado"
        });

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    /// <summary>
    /// Prueba 6: Estado inválido → 400 Bad Request.
    /// El sistema solo acepta estados del conjunto: Pendiente, Aprobado, En Proceso,
    /// Completado, Cancelado, Rechazado. Cualquier otro valor es rechazado.
    /// </summary>
    [Fact]
    public async Task UpdateEstado_EstadoInvalido_RetornaBadRequest()
    {
        // Arrange: pedido existente, se intenta asignar un estado no permitido
        SetAnonymousUser();
        await SeedPedidoAsync(pedidoId: 1, clienteId: 4);

        // Act
        var result = await _controller.UpdateEstado(1, new ActualizarEstadoPedidoDto
        {
            NuevoEstado = "EstadoInventado"   // no existe en la lista de estados válidos
        });

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Contains("Estado invalido", response.Message);
    }

    /// <summary>
    /// Prueba 7: Actualización exitosa → 200 OK + estado persistido.
    /// Verifica que el estado del pedido se actualiza correctamente en la base de datos.
    /// </summary>
    [Fact]
    public async Task UpdateEstado_EstadoValido_ActualizaEstadoYRetornaOk()
    {
        // Arrange: pedido en estado "Pendiente", se cambia a "Aprobado"
        SetAnonymousUser();
        await SeedPedidoAsync(pedidoId: 2, clienteId: 5);

        _notifMock
            .Setup(n => n.CreateAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateEstado(2, new ActualizarEstadoPedidoDto
        {
            NuevoEstado = "Aprobado"
        });

        // Assert - respuesta HTTP 200
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.True(response.Success);

        // Assert - el estado quedó actualizado en la base de datos
        var pedidoActualizado = await _db.Pedidos.FindAsync(2);
        Assert.Equal("Aprobado", pedidoActualizado!.Estado);
    }

    public void Dispose() => _db.Dispose();
}
