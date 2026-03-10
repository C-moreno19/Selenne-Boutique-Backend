using Microsoft.EntityFrameworkCore;
using SelenneApi.Models.Entities;

namespace SelenneApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Auth & Security
    public DbSet<Rol> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<EmailVerification> EmailVerifications { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<LoginAttempt> LoginAttempts { get; set; }

    // Catalog
    public DbSet<CategoriaPrincipal> CategoriasPrincipales { get; set; }
    public DbSet<TipoProducto> TiposProducto { get; set; }
    public DbSet<Marca> Marcas { get; set; }
    public DbSet<Talla> Tallas { get; set; }
    public DbSet<Color> Colores { get; set; }
    public DbSet<Material> Materiales { get; set; }

    // Products
    public DbSet<Producto> Productos { get; set; }
    public DbSet<ProductoImagen> ProductoImagenes { get; set; }
    public DbSet<ProductoTalla> ProductoTallas { get; set; }
    public DbSet<ProductoColor> ProductoColores { get; set; }
    public DbSet<ProductoMaterial> ProductoMateriales { get; set; }
    public DbSet<ProductoStockVariante> ProductoStockVariantes { get; set; }

    // Orders
    public DbSet<Carrito> Carrito { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<PedidoDetalle> PedidoDetalles { get; set; }
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<VentaDetalle> VentaDetalles { get; set; }

    // User features
    public DbSet<Favorito> Favoritos { get; set; }
    public DbSet<Valoracion> Valoraciones { get; set; }
    public DbSet<Notificacion> Notificaciones { get; set; }

    // Suppliers & Stock
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<Compra> Compras { get; set; }
    public DbSet<CompraDetalle> CompraDetalles { get; set; }
    public DbSet<StockMovimiento> StockMovimientos { get; set; }

    // Audit
    public DbSet<AuditoriaPerfil> AuditoriaPerfil { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // IMPORTANTE: Indica a EF que Usuarios tiene trigger
        // Sin esto EF falla con INSERT/UPDATE por el OUTPUT clause
        modelBuilder.Entity<Usuario>()
            .ToTable(tb => tb.HasTrigger("trg_Usuarios_Auditar"));

        // Unique constraints
        modelBuilder.Entity<RolePermission>()
            .HasIndex(rp => new { rp.RoleID, rp.PermissionID }).IsUnique();

        modelBuilder.Entity<ProductoTalla>()
            .HasIndex(pt => new { pt.ProductoID, pt.TallaID }).IsUnique();

        modelBuilder.Entity<ProductoColor>()
            .HasIndex(pc => new { pc.ProductoID, pc.ColorID }).IsUnique();

        modelBuilder.Entity<ProductoMaterial>()
            .HasIndex(pm => new { pm.ProductoID, pm.MaterialID }).IsUnique();

        modelBuilder.Entity<Favorito>()
            .HasIndex(f => new { f.UsuarioID, f.ProductoID }).IsUnique();

        // Pedido -> Cliente (restrict delete cycle)
        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Cliente)
            .WithMany(u => u.Pedidos)
            .HasForeignKey(p => p.ClienteID)
            .OnDelete(DeleteBehavior.Restrict);

        // Venta -> Usuario
        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Usuario)
            .WithMany()
            .HasForeignKey(v => v.UsuarioID)
            .OnDelete(DeleteBehavior.Restrict);

        // Valoracion -> Pedido (no cascade)
        modelBuilder.Entity<Valoracion>()
            .HasOne(v => v.Pedido)
            .WithMany()
            .HasForeignKey(v => v.PedidoID)
            .OnDelete(DeleteBehavior.Restrict);

        // UserSession -> RefreshToken
        modelBuilder.Entity<UserSession>()
            .HasOne(us => us.RefreshToken)
            .WithMany()
            .HasForeignKey(us => us.RefreshTokenID)
            .OnDelete(DeleteBehavior.SetNull);

        // LoginAttempt -> Usuario
        modelBuilder.Entity<LoginAttempt>()
            .HasOne(la => la.Usuario)
            .WithMany()
            .HasForeignKey(la => la.UsuarioID)
            .OnDelete(DeleteBehavior.SetNull);

        // Producto -> StockVariantes
        modelBuilder.Entity<ProductoStockVariante>()
            .HasOne(v => v.Producto)
            .WithMany(p => p.StockVariantes)
            .HasForeignKey(v => v.ProductoID)
            .OnDelete(DeleteBehavior.Cascade);

        // PedidoDetalle -> Talla, Color (no cascade)
        modelBuilder.Entity<PedidoDetalle>()
            .HasOne(pd => pd.Talla)
            .WithMany()
            .HasForeignKey(pd => pd.TallaID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PedidoDetalle>()
            .HasOne(pd => pd.Color)
            .WithMany()
            .HasForeignKey(pd => pd.ColorID)
            .OnDelete(DeleteBehavior.Restrict);

        // VentaDetalle -> Talla, Color
        modelBuilder.Entity<VentaDetalle>()
            .HasOne(vd => vd.Talla)
            .WithMany()
            .HasForeignKey(vd => vd.TallaID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VentaDetalle>()
            .HasOne(vd => vd.Color)
            .WithMany()
            .HasForeignKey(vd => vd.ColorID)
            .OnDelete(DeleteBehavior.Restrict);

        // StockMovimiento -> Usuario
        modelBuilder.Entity<StockMovimiento>()
            .HasOne(sm => sm.Usuario)
            .WithMany()
            .HasForeignKey(sm => sm.UsuarioID)
            .OnDelete(DeleteBehavior.SetNull);
    }
}