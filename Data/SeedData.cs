using SelenneApi.Models.Entities;

namespace SelenneApi.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Seed permissions if none exist
        if (!context.Permissions.Any())
        {
            var perms = new[]
            {
                "productos:ver","productos:crear","productos:editar","productos:eliminar","productos:descuento",
                "ventas:ver","ventas:crear","ventas:editar","ventas:eliminar","ventas:devoluciones","ventas:reportes",
                "clientes:ver","clientes:crear","clientes:editar","clientes:eliminar","clientes:bloquear","clientes:historial",
                "inventario:ver","inventario:actualizar","inventario:ajustes","inventario:alertas","inventario:reportes",
                "usuarios:ver","usuarios:crear","usuarios:editar","usuarios:eliminar","usuarios:bloquear","usuarios:resetear_pass",
                "roles:ver","roles:crear","roles:editar","roles:eliminar","roles:permisos","roles:asignar",
                "reportes:ventas","reportes:inventario","reportes:clientes","reportes:financiero","reportes:descargar","reportes:customizar",
                "notif:ver","notif:enviar","notif:templates","notif:historial",
                "config:sistema","config:empresa","config:email","config:integraciones","config:auditoria","config:backup",
                "tienda:ver","tienda:comprar","tienda:carrito","tienda:pedidos","tienda:ofertas"
            };
            foreach (var p in perms)
                context.Permissions.Add(new Permission { Nombre = p, Descripcion = p });
            await context.SaveChangesAsync();
        }

        // Seed roles
        if (!context.Roles.Any())
        {
            context.Roles.Add(new Rol { Nombre = "Administrador", Descripcion = "Acceso total al sistema" });
            context.Roles.Add(new Rol { Nombre = "Empleado", Descripcion = "Gestión de productos, ventas, clientes e inventario" });
            context.Roles.Add(new Rol { Nombre = "Cliente", Descripcion = "Acceso a la tienda" });
            await context.SaveChangesAsync();
        }

        // Assign permissions to roles
        if (!context.RolePermissions.Any())
        {
            var adminRole = context.Roles.First(r => r.Nombre == "Administrador");
            var empleadoRole = context.Roles.First(r => r.Nombre == "Empleado");
            var clienteRole = context.Roles.First(r => r.Nombre == "Cliente");
            var allPerms = context.Permissions.ToList();

            // Admin gets all
            foreach (var p in allPerms)
                context.RolePermissions.Add(new RolePermission { RoleID = adminRole.RoleID, PermissionID = p.PermissionID });

            // Empleado gets specific modules
            var empPerms = allPerms.Where(p =>
                p.Nombre.StartsWith("productos:") || p.Nombre.StartsWith("ventas:") ||
                p.Nombre.StartsWith("clientes:") || p.Nombre.StartsWith("inventario:") ||
                p.Nombre.StartsWith("tienda:")).ToList();
            foreach (var p in empPerms)
                context.RolePermissions.Add(new RolePermission { RoleID = empleadoRole.RoleID, PermissionID = p.PermissionID });

            // Cliente gets tienda & notif:ver
            var clientePerms = allPerms.Where(p => p.Nombre.StartsWith("tienda:") || p.Nombre == "notif:ver").ToList();
            foreach (var p in clientePerms)
                context.RolePermissions.Add(new RolePermission { RoleID = clienteRole.RoleID, PermissionID = p.PermissionID });

            await context.SaveChangesAsync();
        }

        // Seed admin user
        if (!context.Usuarios.Any(u => u.Email == "admin@selenne.com"))
        {
            var adminRole = context.Roles.First(r => r.Nombre == "Administrador");
            context.Usuarios.Add(new Usuario
            {
                NombreCompleto = "Administrador Selenne",
                Email = "admin@selenne.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1234!"),
                RoleID = adminRole.RoleID,
                Estado = "activo",
                EmailVerificado = true
            });
            await context.SaveChangesAsync();
        }
    }
}
