using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SelenneApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasPrincipales",
                columns: table => new
                {
                    CategoriaPrincipalID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Imagen = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasPrincipales", x => x.CategoriaPrincipalID);
                });

            migrationBuilder.CreateTable(
                name: "Colores",
                columns: table => new
                {
                    ColorID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CodigoHex = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colores", x => x.ColorID);
                });

            migrationBuilder.CreateTable(
                name: "Marcas",
                columns: table => new
                {
                    MarcaID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Logo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SitioWeb = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marcas", x => x.MarcaID);
                });

            migrationBuilder.CreateTable(
                name: "Materiales",
                columns: table => new
                {
                    MaterialID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materiales", x => x.MaterialID);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    PermissionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionID);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    ProveedorID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Contacto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Documento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.ProveedorID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "Tallas",
                columns: table => new
                {
                    TallaID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tallas", x => x.TallaID);
                });

            migrationBuilder.CreateTable(
                name: "TiposProducto",
                columns: table => new
                {
                    TipoProductoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposProducto", x => x.TipoProductoID);
                });

            migrationBuilder.CreateTable(
                name: "Compras",
                columns: table => new
                {
                    CompraID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProveedorID = table.Column<int>(type: "integer", nullable: false),
                    OrdenFactura = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notas = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compras", x => x.CompraID);
                    table.ForeignKey(
                        name: "FK_Compras_Proveedores_ProveedorID",
                        column: x => x.ProveedorID,
                        principalTable: "Proveedores",
                        principalColumn: "ProveedorID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RolePermissionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleID = table.Column<int>(type: "integer", nullable: false),
                    PermissionID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.RolePermissionID);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionID",
                        column: x => x.PermissionID,
                        principalTable: "Permissions",
                        principalColumn: "PermissionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreCompleto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Direccion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Ciudad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RoleID = table.Column<int>(type: "integer", nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EmailVerificado = table.Column<bool>(type: "boolean", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaUltimoLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioID);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID");
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    ProductoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    DescripcionCorta = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CategoriaPrincipalID = table.Column<int>(type: "integer", nullable: false),
                    TipoProductoID = table.Column<int>(type: "integer", nullable: false),
                    MarcaID = table.Column<int>(type: "integer", nullable: false),
                    PrecioCompra = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PrecioVenta = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PrecioOferta = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    ImagenPrincipal = table.Column<string>(type: "text", nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.ProductoID);
                    table.ForeignKey(
                        name: "FK_Productos_CategoriasPrincipales_CategoriaPrincipalID",
                        column: x => x.CategoriaPrincipalID,
                        principalTable: "CategoriasPrincipales",
                        principalColumn: "CategoriaPrincipalID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Productos_Marcas_MarcaID",
                        column: x => x.MarcaID,
                        principalTable: "Marcas",
                        principalColumn: "MarcaID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Productos_TiposProducto_TipoProductoID",
                        column: x => x.TipoProductoID,
                        principalTable: "TiposProducto",
                        principalColumn: "TipoProductoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditoriaPerfil",
                columns: table => new
                {
                    AuditoriaID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    TipoCambio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CampoModificado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ValorAnterior = table.Column<string>(type: "text", nullable: true),
                    ValorNuevo = table.Column<string>(type: "text", nullable: true),
                    FechaCambio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Origen = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaPerfil", x => x.AuditoriaID);
                    table.ForeignKey(
                        name: "FK_AuditoriaPerfil_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailVerifications",
                columns: table => new
                {
                    VerificationID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Verified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerifications", x => x.VerificationID);
                    table.ForeignKey(
                        name: "FK_EmailVerifications_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoginAttempts",
                columns: table => new
                {
                    AttemptID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Successful = table.Column<bool>(type: "boolean", nullable: false),
                    IPAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginAttempts", x => x.AttemptID);
                    table.ForeignKey(
                        name: "FK_LoginAttempts_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    NotificacionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Leida = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaLeida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Referencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.NotificacionID);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    ResetTokenID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Used = table.Column<bool>(type: "boolean", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.ResetTokenID);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    PedidoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteID = table.Column<int>(type: "integer", nullable: false),
                    FechaPedido = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NombreCliente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DocumentoCliente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EmailCliente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TelefonoCliente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DireccionEnvio = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Ciudad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CodigoPostal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    MetodoPago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NumeroCuenta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NombreTitular = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Banco = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TipoCuenta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Envio = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NumeroGuia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Transportadora = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaEntrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.PedidoID);
                    table.ForeignKey(
                        name: "FK_Pedidos_Usuarios_ClienteID",
                        column: x => x.ClienteID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    RefreshTokenID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Revoked = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IPAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.RefreshTokenID);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    VentaID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: true),
                    ClienteID = table.Column<int>(type: "integer", nullable: true),
                    FechaVenta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Descuento = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Envio = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MetodoPago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notas = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.VentaID);
                    table.ForeignKey(
                        name: "FK_Ventas_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Carrito",
                columns: table => new
                {
                    CarritoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    ProductoID = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    TallaSeleccionada = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ColorSeleccionado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FechaAgregado = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carrito", x => x.CarritoID);
                    table.ForeignKey(
                        name: "FK_Carrito_Productos_ProductoID",
                        column: x => x.ProductoID,
                        principalTable: "Productos",
                        principalColumn: "ProductoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Carrito_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompraDetalles",
                columns: table => new
                {
                    CompraDetalleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompraID = table.Column<int>(type: "integer", nullable: false),
                    ProductoID = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SKU = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Marca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Talla = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Material = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TipoProducto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompraDetalles", x => x.CompraDetalleID);
                    table.ForeignKey(
                        name: "FK_CompraDetalles_Compras_CompraID",
                        column: x => x.CompraID,
                        principalTable: "Compras",
                        principalColumn: "CompraID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompraDetalles_Productos_ProductoID",
                        column: x => x.ProductoID,
                        principalTable: "Productos",
                        principalColumn: "ProductoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Favoritos",
                columns: table => new
                {
                    FavoritoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    ProductoID = table.Column<int>(type: "integer", nullable: false),
                    Nota = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FechaAgregado = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoritos", x => x.FavoritoID);
                    table.ForeignKey(
                        name: "FK_Favoritos_Productos_ProductoID",
                        column: x => x.ProductoID,
                        principalTable: "Productos",
                        principalColumn: "ProductoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favoritos_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductoColores",
                columns: table => new
                {
                    ProductoColorID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductoID = table.Column<int>(type: "integer", nullable: false),
                    ColorID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoColores", x => x.ProductoColorID);
                    table.ForeignKey(
                        name: "FK_ProductoColores_Colores_ColorID",
                        column: x => x.ColorID,
                        principalTable: "Colores",
                        principalColumn: "ColorID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductoColores_Productos_ProductoID",
                        column: x => x.ProductoID,
                        principalTable: "Productos",
                        principalColumn: "ProductoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductoImagenes",
                columns: table => new
                {
                    ProductoImagenID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductoID = table.Column<int>(type: "integer", nullable: false),
                    URL = table.Column<string>(type: "text", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    ColorNombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoImagenes", x => x.ProductoImagenID);
                    table.ForeignKey(
                        name: "FK_ProductoImagenes_Productos_ProductoID",
                        column: x => x.ProductoID,
                        principalTable: "Productos",
                        principalColumn: "ProductoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductoMateriales",
                columns: table => new
                {
                    ProductoMaterialID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductoID = table.Column<int>(type: "integer", nullable: false),
                    MaterialID = table.Column<int>(type: "integer", nullable: false),
                    Porcentaje = table.Column<decimal>(type: "numeric(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoMateriales", x => x.ProductoMaterialID);
                    table.ForeignKey(
                        name: "FK_ProductoMateriales_Materiales_MaterialID",
                        column: x => x.MaterialID,
                        principalTable: "Materiales",
                        principalColumn: "MaterialID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductoMateriales_Productos_ProductoID",
                        column: x => x.ProductoID,
                        principalTable: "Productos",
                        principalColumn: "ProductoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductoStockVariantes",
                columns: table => new
                {
                    VarianteID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductoID = table.Column<int>(type: "integer", nullable: false),
                    TallaNombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ColorNombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Stock = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoStockVariantes", x => x.VarianteID);
                    table.ForeignKey(
                        name: "FK_ProductoStockVariantes_Productos_ProductoID",
                        column: x => x.ProductoID,
                        principalTable: "Productos",
                        principalColumn: "ProductoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductoTallas",
                columns: table => new
                {
                    ProductoTallaID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductoID = table.Column<int>(type: "integer", nullable: false),
                    TallaID = table.Column<int>(type: "integer", nullable: false),
                    StockTalla = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoTallas", x => x.ProductoTallaID);
                    table.ForeignKey(
                        name: "FK_ProductoTallas_Productos_ProductoID",
                        column: x => x.ProductoID,
                        principalTable: "Productos",
                        principalColumn: "ProductoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductoTallas_Tallas_TallaID",
                        column: x => x.TallaID,
                        principalTable: "Tallas",
                        principalColumn: "TallaID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockMovimientos",
                columns: table => new
                {
                    MovimientoID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductoID = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReferenciaTipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReferenciaID = table.Column<int>(type: "integer", nullable: true),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovimientos", x => x.MovimientoID);
                    table.ForeignKey(
                        name: "FK_StockMovimientos_Productos_ProductoID",
                        column: x => x.ProductoID,
                        principalTable: "Productos",
                        principalColumn: "ProductoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockMovimientos_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PedidoDetalles",
                columns: table => new
                {
                    PedidoDetalleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoID = table.Column<int>(type: "integer", nullable: false),
                    ProductoID = table.Column<int>(type: "integer", nullable: false),
                    TallaID = table.Column<int>(type: "integer", nullable: true),
                    ColorID = table.Column<int>(type: "integer", nullable: true),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoDetalles", x => x.PedidoDetalleID);
                    table.ForeignKey(
                        name: "FK_PedidoDetalles_Colores_ColorID",
                        column: x => x.ColorID,
                        principalTable: "Colores",
                        principalColumn: "ColorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidoDetalles_Pedidos_PedidoID",
                        column: x => x.PedidoID,
                        principalTable: "Pedidos",
                        principalColumn: "PedidoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoDetalles_Productos_ProductoID",
                        column: x => x.ProductoID,
                        principalTable: "Productos",
                        principalColumn: "ProductoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoDetalles_Tallas_TallaID",
                        column: x => x.TallaID,
                        principalTable: "Tallas",
                        principalColumn: "TallaID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Valoraciones",
                columns: table => new
                {
                    ValoracionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductoID = table.Column<int>(type: "integer", nullable: false),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    PedidoID = table.Column<int>(type: "integer", nullable: true),
                    Puntuacion = table.Column<int>(type: "integer", nullable: false),
                    Comentario = table.Column<string>(type: "text", nullable: true),
                    Util = table.Column<int>(type: "integer", nullable: false),
                    NoUtil = table.Column<int>(type: "integer", nullable: false),
                    VerificadoCompra = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Valoraciones", x => x.ValoracionID);
                    table.ForeignKey(
                        name: "FK_Valoraciones_Pedidos_PedidoID",
                        column: x => x.PedidoID,
                        principalTable: "Pedidos",
                        principalColumn: "PedidoID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Valoraciones_Productos_ProductoID",
                        column: x => x.ProductoID,
                        principalTable: "Productos",
                        principalColumn: "ProductoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Valoraciones_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    SessionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioID = table.Column<int>(type: "integer", nullable: false),
                    RefreshTokenID = table.Column<int>(type: "integer", nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IPAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Revoked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.SessionID);
                    table.ForeignKey(
                        name: "FK_UserSessions_RefreshTokens_RefreshTokenID",
                        column: x => x.RefreshTokenID,
                        principalTable: "RefreshTokens",
                        principalColumn: "RefreshTokenID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserSessions_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VentaDetalles",
                columns: table => new
                {
                    VentaDetalleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VentaID = table.Column<int>(type: "integer", nullable: false),
                    ProductoID = table.Column<int>(type: "integer", nullable: false),
                    TallaID = table.Column<int>(type: "integer", nullable: true),
                    ColorID = table.Column<int>(type: "integer", nullable: true),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaDetalles", x => x.VentaDetalleID);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Colores_ColorID",
                        column: x => x.ColorID,
                        principalTable: "Colores",
                        principalColumn: "ColorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Productos_ProductoID",
                        column: x => x.ProductoID,
                        principalTable: "Productos",
                        principalColumn: "ProductoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Tallas_TallaID",
                        column: x => x.TallaID,
                        principalTable: "Tallas",
                        principalColumn: "TallaID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Ventas_VentaID",
                        column: x => x.VentaID,
                        principalTable: "Ventas",
                        principalColumn: "VentaID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaPerfil_UsuarioID",
                table: "AuditoriaPerfil",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Carrito_ProductoID",
                table: "Carrito",
                column: "ProductoID");

            migrationBuilder.CreateIndex(
                name: "IX_Carrito_UsuarioID",
                table: "Carrito",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_CompraDetalles_CompraID",
                table: "CompraDetalles",
                column: "CompraID");

            migrationBuilder.CreateIndex(
                name: "IX_CompraDetalles_ProductoID",
                table: "CompraDetalles",
                column: "ProductoID");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_ProveedorID",
                table: "Compras",
                column: "ProveedorID");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerifications_UsuarioID",
                table: "EmailVerifications",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_ProductoID",
                table: "Favoritos",
                column: "ProductoID");

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_UsuarioID_ProductoID",
                table: "Favoritos",
                columns: new[] { "UsuarioID", "ProductoID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_UsuarioID",
                table: "LoginAttempts",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioID",
                table: "Notificaciones",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UsuarioID",
                table: "PasswordResetTokens",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoDetalles_ColorID",
                table: "PedidoDetalles",
                column: "ColorID");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoDetalles_PedidoID",
                table: "PedidoDetalles",
                column: "PedidoID");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoDetalles_ProductoID",
                table: "PedidoDetalles",
                column: "ProductoID");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoDetalles_TallaID",
                table: "PedidoDetalles",
                column: "TallaID");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ClienteID",
                table: "Pedidos",
                column: "ClienteID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoColores_ColorID",
                table: "ProductoColores",
                column: "ColorID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoColores_ProductoID_ColorID",
                table: "ProductoColores",
                columns: new[] { "ProductoID", "ColorID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductoImagenes_ProductoID",
                table: "ProductoImagenes",
                column: "ProductoID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoMateriales_MaterialID",
                table: "ProductoMateriales",
                column: "MaterialID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoMateriales_ProductoID_MaterialID",
                table: "ProductoMateriales",
                columns: new[] { "ProductoID", "MaterialID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaPrincipalID",
                table: "Productos",
                column: "CategoriaPrincipalID");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_MarcaID",
                table: "Productos",
                column: "MarcaID");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_TipoProductoID",
                table: "Productos",
                column: "TipoProductoID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoStockVariantes_ProductoID",
                table: "ProductoStockVariantes",
                column: "ProductoID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoTallas_ProductoID_TallaID",
                table: "ProductoTallas",
                columns: new[] { "ProductoID", "TallaID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductoTallas_TallaID",
                table: "ProductoTallas",
                column: "TallaID");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UsuarioID",
                table: "RefreshTokens",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionID",
                table: "RolePermissions",
                column: "PermissionID");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleID_PermissionID",
                table: "RolePermissions",
                columns: new[] { "RoleID", "PermissionID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovimientos_ProductoID",
                table: "StockMovimientos",
                column: "ProductoID");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovimientos_UsuarioID",
                table: "StockMovimientos",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_RefreshTokenID",
                table: "UserSessions",
                column: "RefreshTokenID");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UsuarioID",
                table: "UserSessions",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RoleID",
                table: "Usuarios",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_Valoraciones_PedidoID",
                table: "Valoraciones",
                column: "PedidoID");

            migrationBuilder.CreateIndex(
                name: "IX_Valoraciones_ProductoID",
                table: "Valoraciones",
                column: "ProductoID");

            migrationBuilder.CreateIndex(
                name: "IX_Valoraciones_UsuarioID",
                table: "Valoraciones",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_ColorID",
                table: "VentaDetalles",
                column: "ColorID");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_ProductoID",
                table: "VentaDetalles",
                column: "ProductoID");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_TallaID",
                table: "VentaDetalles",
                column: "TallaID");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_VentaID",
                table: "VentaDetalles",
                column: "VentaID");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_UsuarioID",
                table: "Ventas",
                column: "UsuarioID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriaPerfil");

            migrationBuilder.DropTable(
                name: "Carrito");

            migrationBuilder.DropTable(
                name: "CompraDetalles");

            migrationBuilder.DropTable(
                name: "EmailVerifications");

            migrationBuilder.DropTable(
                name: "Favoritos");

            migrationBuilder.DropTable(
                name: "LoginAttempts");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "PedidoDetalles");

            migrationBuilder.DropTable(
                name: "ProductoColores");

            migrationBuilder.DropTable(
                name: "ProductoImagenes");

            migrationBuilder.DropTable(
                name: "ProductoMateriales");

            migrationBuilder.DropTable(
                name: "ProductoStockVariantes");

            migrationBuilder.DropTable(
                name: "ProductoTallas");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "StockMovimientos");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "Valoraciones");

            migrationBuilder.DropTable(
                name: "VentaDetalles");

            migrationBuilder.DropTable(
                name: "Compras");

            migrationBuilder.DropTable(
                name: "Materiales");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Colores");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Tallas");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "CategoriasPrincipales");

            migrationBuilder.DropTable(
                name: "Marcas");

            migrationBuilder.DropTable(
                name: "TiposProducto");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
