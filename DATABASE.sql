-- ============================================================
-- SELENNE BOUTIQUE - DATABASE SCRIPT
-- Ejecutar en SQL Server LocalDB
-- ============================================================

USE master;
GO
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SelenneDb')
    CREATE DATABASE SelenneDb;
GO
USE SelenneDb;
GO

-- AUTH & SECURITY
CREATE TABLE Roles (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    Descripcion NVARCHAR(255),
    Estado NVARCHAR(20) DEFAULT 'activo',
    CONSTRAINT CK_Roles_Estado CHECK (Estado IN ('activo','inactivo'))
);

CREATE TABLE Permissions (
    PermissionID INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion NVARCHAR(255),
    Estado NVARCHAR(20) DEFAULT 'activo',
    CONSTRAINT CK_Permissions_Estado CHECK (Estado IN ('activo','inactivo'))
);

CREATE TABLE RolePermissions (
    RolePermissionID INT PRIMARY KEY IDENTITY(1,1),
    RoleID INT NOT NULL,
    PermissionID INT NOT NULL,
    CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID) ON DELETE CASCADE,
    CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionID) REFERENCES Permissions(PermissionID) ON DELETE CASCADE,
    CONSTRAINT UQ_RolePermissions UNIQUE (RoleID, PermissionID)
);

CREATE TABLE Usuarios (
    UsuarioID INT PRIMARY KEY IDENTITY(1,1),
    NombreCompleto NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Telefono NVARCHAR(20),
    Documento NVARCHAR(20),
    Direccion NVARCHAR(255),
    PasswordHash NVARCHAR(255) NOT NULL,
    RoleID INT,
    Estado NVARCHAR(20) DEFAULT 'activo',
    EmailVerificado BIT DEFAULT 0,
    FechaRegistro DATETIME DEFAULT GETDATE(),
    FechaUltimoLogin DATETIME,
    CONSTRAINT CK_Usuarios_Estado CHECK (Estado IN ('activo','inactivo')),
    CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID) ON DELETE SET NULL
);

CREATE TABLE Notificaciones (
    NotificacionID INT PRIMARY KEY IDENTITY(1,1),
    UsuarioID INT NOT NULL,
    Titulo NVARCHAR(200) NOT NULL,
    Mensaje NVARCHAR(MAX) NOT NULL,
    Tipo NVARCHAR(50) DEFAULT 'info',
    Leida BIT DEFAULT 0,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    FechaLeida DATETIME,
    Referencia NVARCHAR(100),
    CONSTRAINT FK_Notificaciones_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID) ON DELETE CASCADE
);

CREATE TABLE RefreshTokens (
    RefreshTokenID INT PRIMARY KEY IDENTITY(1,1),
    UsuarioID INT NOT NULL,
    Token NVARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt DATETIME NOT NULL,
    Revoked BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    RevokedAt DATETIME,
    IPAddress NVARCHAR(50),
    CONSTRAINT FK_RefreshTokens_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID) ON DELETE CASCADE
);

CREATE TABLE EmailVerifications (
    VerificationID INT PRIMARY KEY IDENTITY(1,1),
    UsuarioID INT NOT NULL,
    Token NVARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt DATETIME NOT NULL,
    Verified BIT DEFAULT 0,
    VerifiedAt DATETIME,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_EmailVerifications_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID) ON DELETE CASCADE
);

CREATE TABLE PasswordResetTokens (
    ResetTokenID INT PRIMARY KEY IDENTITY(1,1),
    UsuarioID INT NOT NULL,
    Token NVARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt DATETIME NOT NULL,
    Used BIT DEFAULT 0,
    UsedAt DATETIME,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_PasswordResetTokens_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID) ON DELETE CASCADE
);

CREATE TABLE UserSessions (
    SessionID INT PRIMARY KEY IDENTITY(1,1),
    UsuarioID INT NOT NULL,
    RefreshTokenID INT,
    UserAgent NVARCHAR(500),
    IPAddress NVARCHAR(50),
    CreatedAt DATETIME DEFAULT GETDATE(),
    LastSeenAt DATETIME DEFAULT GETDATE(),
    Revoked BIT DEFAULT 0,
    CONSTRAINT FK_UserSessions_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID) ON DELETE CASCADE,
    CONSTRAINT FK_UserSessions_RefreshTokens FOREIGN KEY (RefreshTokenID) REFERENCES RefreshTokens(RefreshTokenID) ON DELETE SET NULL
);

CREATE TABLE LoginAttempts (
    AttemptID INT PRIMARY KEY IDENTITY(1,1),
    UsuarioID INT,
    Email NVARCHAR(100),
    AttemptAt DATETIME DEFAULT GETDATE(),
    Successful BIT DEFAULT 0,
    IPAddress NVARCHAR(50),
    CONSTRAINT FK_LoginAttempts_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID) ON DELETE SET NULL
);

-- CATALOG
CREATE TABLE CategoriasPrincipales (
    CategoriaPrincipalID INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion NVARCHAR(500),
    Imagen NVARCHAR(500),
    Estado NVARCHAR(20) DEFAULT 'activo',
    CONSTRAINT CK_Categorias_Estado CHECK (Estado IN ('activo','inactivo'))
);

CREATE TABLE TiposProducto (
    TipoProductoID INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion NVARCHAR(500),
    Estado NVARCHAR(20) DEFAULT 'activo',
    CONSTRAINT CK_TiposProducto_Estado CHECK (Estado IN ('activo','inactivo'))
);

CREATE TABLE Marcas (
    MarcaID INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    Logo NVARCHAR(500),
    SitioWeb NVARCHAR(200),
    Estado NVARCHAR(20) DEFAULT 'activo',
    CONSTRAINT CK_Marcas_Estado CHECK (Estado IN ('activo','inactivo'))
);

CREATE TABLE Tallas (
    TallaID INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(10) NOT NULL UNIQUE,
    Orden INT DEFAULT 0
);

CREATE TABLE Colores (
    ColorID INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    CodigoHex NVARCHAR(7),
    Estado NVARCHAR(20) DEFAULT 'activo',
    CONSTRAINT CK_Colores_Estado CHECK (Estado IN ('activo','inactivo'))
);

CREATE TABLE Materiales (
    MaterialID INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion NVARCHAR(500),
    Estado NVARCHAR(20) DEFAULT 'activo',
    CONSTRAINT CK_Materiales_Estado CHECK (Estado IN ('activo','inactivo'))
);

CREATE TABLE Productos (
    ProductoID INT PRIMARY KEY IDENTITY(1,1),
    Codigo NVARCHAR(50) NOT NULL UNIQUE,
    Nombre NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(MAX),
    DescripcionCorta NVARCHAR(500),
    CategoriaPrincipalID INT NOT NULL,
    TipoProductoID INT NOT NULL,
    MarcaID INT NOT NULL,
    PrecioCompra DECIMAL(18,2),
    PrecioVenta DECIMAL(18,2) NOT NULL,
    PrecioOferta DECIMAL(18,2),
    Stock INT DEFAULT 0,
    ImagenPrincipal NVARCHAR(500),
    Estado NVARCHAR(20) DEFAULT 'activo',
    FechaCreacion DATETIME DEFAULT GETDATE(),
    FechaActualizacion DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Productos_Categoria FOREIGN KEY (CategoriaPrincipalID) REFERENCES CategoriasPrincipales(CategoriaPrincipalID),
    CONSTRAINT FK_Productos_Tipo FOREIGN KEY (TipoProductoID) REFERENCES TiposProducto(TipoProductoID),
    CONSTRAINT FK_Productos_Marca FOREIGN KEY (MarcaID) REFERENCES Marcas(MarcaID),
    CONSTRAINT CK_Productos_Estado CHECK (Estado IN ('activo','inactivo'))
);

CREATE TABLE ProductoImagenes (
    ProductoImagenID INT PRIMARY KEY IDENTITY(1,1),
    ProductoID INT NOT NULL,
    URL NVARCHAR(500) NOT NULL,
    Orden INT DEFAULT 0,
    CONSTRAINT FK_ProductoImagenes_Productos FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID) ON DELETE CASCADE
);

CREATE TABLE ProductoTallas (
    ProductoTallaID INT PRIMARY KEY IDENTITY(1,1),
    ProductoID INT NOT NULL,
    TallaID INT NOT NULL,
    StockTalla INT DEFAULT 0,
    CONSTRAINT FK_ProductoTallas_Productos FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID) ON DELETE CASCADE,
    CONSTRAINT FK_ProductoTallas_Tallas FOREIGN KEY (TallaID) REFERENCES Tallas(TallaID),
    CONSTRAINT UQ_ProductoTallas UNIQUE (ProductoID, TallaID)
);

CREATE TABLE ProductoColores (
    ProductoColorID INT PRIMARY KEY IDENTITY(1,1),
    ProductoID INT NOT NULL,
    ColorID INT NOT NULL,
    CONSTRAINT FK_ProductoColores_Productos FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID) ON DELETE CASCADE,
    CONSTRAINT FK_ProductoColores_Colores FOREIGN KEY (ColorID) REFERENCES Colores(ColorID),
    CONSTRAINT UQ_ProductoColores UNIQUE (ProductoID, ColorID)
);

CREATE TABLE ProductoMateriales (
    ProductoMaterialID INT PRIMARY KEY IDENTITY(1,1),
    ProductoID INT NOT NULL,
    MaterialID INT NOT NULL,
    Porcentaje DECIMAL(5,2),
    CONSTRAINT FK_ProductoMateriales_Productos FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID) ON DELETE CASCADE,
    CONSTRAINT FK_ProductoMateriales_Materiales FOREIGN KEY (MaterialID) REFERENCES Materiales(MaterialID),
    CONSTRAINT UQ_ProductoMateriales UNIQUE (ProductoID, MaterialID)
);

-- ORDERS
CREATE TABLE Carrito (
    CarritoID INT PRIMARY KEY IDENTITY(1,1),
    UsuarioID INT NOT NULL,
    ProductoID INT NOT NULL,
    Cantidad INT NOT NULL DEFAULT 1,
    TallaSeleccionada NVARCHAR(10),
    ColorSeleccionado NVARCHAR(50),
    FechaAgregado DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Carrito_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID) ON DELETE CASCADE,
    CONSTRAINT FK_Carrito_Productos FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID),
    CONSTRAINT CK_Carrito_Cantidad CHECK (Cantidad > 0)
);

CREATE TABLE Pedidos (
    PedidoID INT PRIMARY KEY IDENTITY(1,1),
    ClienteID INT NOT NULL,
    FechaPedido DATETIME DEFAULT GETDATE(),
    NombreCliente NVARCHAR(100) NOT NULL,
    DocumentoCliente NVARCHAR(20),
    EmailCliente NVARCHAR(100) NOT NULL,
    TelefonoCliente NVARCHAR(20) NOT NULL,
    DireccionEnvio NVARCHAR(255) NOT NULL,
    Ciudad NVARCHAR(100) NOT NULL,
    CodigoPostal NVARCHAR(10),
    MetodoPago NVARCHAR(50) NOT NULL,
    NumeroCuenta NVARCHAR(50),
    NombreTitular NVARCHAR(100),
    Banco NVARCHAR(100),
    TipoCuenta NVARCHAR(50),
    Subtotal DECIMAL(18,2) NOT NULL,
    Descuento DECIMAL(18,2) DEFAULT 0,
    Envio DECIMAL(18,2) DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL,
    Estado NVARCHAR(20) DEFAULT 'Pendiente',
    NumeroGuia NVARCHAR(50),
    Transportadora NVARCHAR(100),
    FechaEnvio DATETIME,
    FechaEntrega DATETIME,
    Notas NVARCHAR(MAX),
    FechaActualizacion DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Pedidos_Clientes FOREIGN KEY (ClienteID) REFERENCES Usuarios(UsuarioID)
);

CREATE TABLE PedidoDetalles (
    PedidoDetalleID INT PRIMARY KEY IDENTITY(1,1),
    PedidoID INT NOT NULL,
    ProductoID INT NOT NULL,
    TallaID INT,
    ColorID INT,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_PedidoDetalles_Pedidos FOREIGN KEY (PedidoID) REFERENCES Pedidos(PedidoID) ON DELETE CASCADE,
    CONSTRAINT FK_PedidoDetalles_Productos FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID),
    CONSTRAINT FK_PedidoDetalles_Tallas FOREIGN KEY (TallaID) REFERENCES Tallas(TallaID),
    CONSTRAINT FK_PedidoDetalles_Colores FOREIGN KEY (ColorID) REFERENCES Colores(ColorID),
    CONSTRAINT CK_PedidoDetalles_Cantidad CHECK (Cantidad > 0)
);

CREATE TABLE Ventas (
    VentaID INT PRIMARY KEY IDENTITY(1,1),
    UsuarioID INT,
    ClienteID INT,
    FechaVenta DATETIME DEFAULT GETDATE(),
    Subtotal DECIMAL(18,2) NOT NULL,
    Descuento DECIMAL(18,2) DEFAULT 0,
    Envio DECIMAL(18,2) DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL,
    Estado NVARCHAR(20) DEFAULT 'Pendiente',
    MetodoPago NVARCHAR(50),
    Notas NVARCHAR(MAX),
    CONSTRAINT FK_Ventas_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID),
    CONSTRAINT FK_Ventas_Clientes FOREIGN KEY (ClienteID) REFERENCES Usuarios(UsuarioID)
);

CREATE TABLE VentaDetalles (
    VentaDetalleID INT PRIMARY KEY IDENTITY(1,1),
    VentaID INT NOT NULL,
    ProductoID INT NOT NULL,
    TallaID INT,
    ColorID INT,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_VentaDetalles_Ventas FOREIGN KEY (VentaID) REFERENCES Ventas(VentaID) ON DELETE CASCADE,
    CONSTRAINT FK_VentaDetalles_Productos FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID),
    CONSTRAINT FK_VentaDetalles_Tallas FOREIGN KEY (TallaID) REFERENCES Tallas(TallaID),
    CONSTRAINT FK_VentaDetalles_Colores FOREIGN KEY (ColorID) REFERENCES Colores(ColorID),
    CONSTRAINT CK_VentaDetalles_Cantidad CHECK (Cantidad > 0)
);

CREATE TABLE Favoritos (
    FavoritoID INT PRIMARY KEY IDENTITY(1,1),
    UsuarioID INT NOT NULL,
    ProductoID INT NOT NULL,
    Nota NVARCHAR(500),
    FechaAgregado DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Favoritos_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID) ON DELETE CASCADE,
    CONSTRAINT FK_Favoritos_Productos FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID) ON DELETE CASCADE,
    CONSTRAINT UQ_Favoritos UNIQUE (UsuarioID, ProductoID)
);

CREATE TABLE Valoraciones (
    ValoracionID INT PRIMARY KEY IDENTITY(1,1),
    ProductoID INT NOT NULL,
    UsuarioID INT NOT NULL,
    PedidoID INT,
    Puntuacion INT NOT NULL CHECK (Puntuacion BETWEEN 1 AND 5),
    Comentario NVARCHAR(MAX),
    Util INT DEFAULT 0,
    NoUtil INT DEFAULT 0,
    VerificadoCompra BIT DEFAULT 0,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    Estado NVARCHAR(20) DEFAULT 'pendiente',
    CONSTRAINT FK_Valoraciones_Productos FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID) ON DELETE CASCADE,
    CONSTRAINT FK_Valoraciones_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID),
    CONSTRAINT FK_Valoraciones_Pedidos FOREIGN KEY (PedidoID) REFERENCES Pedidos(PedidoID),
    CONSTRAINT CK_Valoraciones_Estado CHECK (Estado IN ('pendiente','aprobada','rechazada'))
);

CREATE TABLE Proveedores (
    ProveedorID INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(200) NOT NULL,
    Contacto NVARCHAR(100),
    Email NVARCHAR(100),
    Telefono NVARCHAR(50),
    Documento NVARCHAR(50),
    Estado NVARCHAR(20) DEFAULT 'activo',
    FechaRegistro DATETIME DEFAULT GETDATE(),
    CONSTRAINT CK_Proveedores_Estado CHECK (Estado IN ('activo','inactivo'))
);

CREATE TABLE Compras (
    CompraID INT PRIMARY KEY IDENTITY(1,1),
    ProveedorID INT NOT NULL,
    OrdenFactura NVARCHAR(50) NOT NULL UNIQUE,
    Fecha DATETIME DEFAULT GETDATE(),
    Total DECIMAL(18,2) NOT NULL,
    Estado NVARCHAR(20) DEFAULT 'Activa',
    Notas NVARCHAR(MAX),
    CONSTRAINT FK_Compras_Proveedores FOREIGN KEY (ProveedorID) REFERENCES Proveedores(ProveedorID),
    CONSTRAINT CK_Compras_Estado CHECK (Estado IN ('Activa','Anulada','Pendiente'))
);

CREATE TABLE CompraDetalles (
    CompraDetalleID INT PRIMARY KEY IDENTITY(1,1),
    CompraID INT NOT NULL,
    ProductoID INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Total DECIMAL(18,2) NOT NULL,
    SKU NVARCHAR(50),
    Categoria NVARCHAR(100),
    Marca NVARCHAR(100),
    Talla NVARCHAR(10),
    Color NVARCHAR(50),
    Material NVARCHAR(100),
    TipoProducto NVARCHAR(100),
    CONSTRAINT FK_CompraDetalles_Compras FOREIGN KEY (CompraID) REFERENCES Compras(CompraID) ON DELETE CASCADE,
    CONSTRAINT FK_CompraDetalles_Productos FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID),
    CONSTRAINT CK_CompraDetalles_Cantidad CHECK (Cantidad > 0)
);

CREATE TABLE StockMovimientos (
    MovimientoID INT PRIMARY KEY IDENTITY(1,1),
    ProductoID INT NOT NULL,
    Cantidad INT NOT NULL,
    Tipo NVARCHAR(20) NOT NULL,
    ReferenciaTipo NVARCHAR(50) NULL,
    ReferenciaID INT NULL,
    Fecha DATETIME DEFAULT GETDATE(),
    UsuarioID INT NULL,
    CONSTRAINT FK_StockMovimientos_Productos FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID),
    CONSTRAINT CK_StockMovimientos_Tipo CHECK (Tipo IN ('entrada','salida'))
);

CREATE TABLE AuditoriaPerfil (
    AuditoriaID INT PRIMARY KEY IDENTITY(1,1),
    UsuarioID INT NOT NULL,
    TipoCambio NVARCHAR(50) NOT NULL,
    CampoModificado NVARCHAR(100),
    ValorAnterior NVARCHAR(MAX),
    ValorNuevo NVARCHAR(MAX),
    FechaCambio DATETIME DEFAULT GETDATE(),
    Origen NVARCHAR(50),
    CONSTRAINT FK_AuditoriaPerfil_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID) ON DELETE CASCADE,
    CONSTRAINT CK_AuditoriaPerfil_Tipo CHECK (TipoCambio IN ('perfil','email','telefono','direccion','contrasena','pedido','estado'))
);

PRINT 'Database SelenneDb created with all tables successfully!';
GO
