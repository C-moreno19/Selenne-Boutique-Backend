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
                "admin:dashboard",
                "productos:ver","productos:crear","productos:editar","productos:eliminar","productos:descuento",
                "pedidos:ver","pedidos:editar",
                "ventas:ver","ventas:crear","ventas:editar","ventas:eliminar",
                "clientes:ver","clientes:historial",
                "compras:ver","compras:crear","compras:editar","compras:eliminar",
                "usuarios:ver","usuarios:crear","usuarios:editar","usuarios:eliminar","usuarios:bloquear","usuarios:resetear_pass",
                "roles:ver","roles:crear","roles:editar","roles:eliminar","roles:permisos","roles:asignar",
                "notif:ver"
            };
            foreach (var p in perms)
                context.Permissions.Add(new Permission { Nombre = p, Descripcion = p });
            await context.SaveChangesAsync();
        }

        // Agregar permisos nuevos de forma segura en bases de datos existentes
        {
            var todosLosPermisos = new[]
            {
                "admin:dashboard",
                "productos:ver","productos:crear","productos:editar","productos:eliminar","productos:descuento",
                "pedidos:ver","pedidos:editar",
                "ventas:ver","ventas:crear","ventas:editar","ventas:eliminar",
                "clientes:ver","clientes:historial",
                "compras:ver","compras:crear","compras:editar","compras:eliminar",
                "usuarios:ver","usuarios:crear","usuarios:editar","usuarios:eliminar","usuarios:bloquear","usuarios:resetear_pass",
                "roles:ver","roles:crear","roles:editar","roles:eliminar","roles:permisos","roles:asignar",
                "notif:ver"
            };
            var existentes = context.Permissions.Select(p => p.Nombre).ToHashSet();
            bool cambios = false;
            foreach (var nombre in todosLosPermisos.Where(n => !existentes.Contains(n)))
            {
                context.Permissions.Add(new Permission { Nombre = nombre, Descripcion = nombre });
                cambios = true;
            }
            if (cambios) await context.SaveChangesAsync();
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
                p.Nombre == "admin:dashboard" ||
                p.Nombre.StartsWith("productos:") || p.Nombre.StartsWith("pedidos:") ||
                p.Nombre.StartsWith("ventas:") || p.Nombre.StartsWith("clientes:") ||
                p.Nombre.StartsWith("compras:") || p.Nombre == "notif:ver").ToList();
            foreach (var p in empPerms)
                context.RolePermissions.Add(new RolePermission { RoleID = empleadoRole.RoleID, PermissionID = p.PermissionID });

            // Cliente solo tiene notif:ver
            var clientePerms = allPerms.Where(p => p.Nombre == "notif:ver").ToList();
            foreach (var p in clientePerms)
                context.RolePermissions.Add(new RolePermission { RoleID = clienteRole.RoleID, PermissionID = p.PermissionID });

            await context.SaveChangesAsync();
        }

        // Corregir permisos y descripciones de roles preexistentes
        {
            var permisosValidos = new[]
            {
                "admin:dashboard",
                "productos:ver","productos:crear","productos:editar","productos:eliminar","productos:descuento",
                "pedidos:ver","pedidos:editar",
                "ventas:ver","ventas:crear","ventas:editar","ventas:eliminar",
                "clientes:ver","clientes:historial",
                "compras:ver","compras:crear","compras:editar","compras:eliminar",
                "usuarios:ver","usuarios:crear","usuarios:editar","usuarios:eliminar","usuarios:bloquear","usuarios:resetear_pass",
                "roles:ver","roles:crear","roles:editar","roles:eliminar","roles:permisos","roles:asignar",
                "notif:ver"
            }.ToHashSet();

            var permisosEmpleado = new[]
            {
                "admin:dashboard",
                "productos:ver","productos:crear","productos:editar","productos:eliminar","productos:descuento",
                "pedidos:ver","pedidos:editar",
                "ventas:ver","ventas:crear","ventas:editar","ventas:eliminar",
                "clientes:ver","clientes:historial",
                "compras:ver","compras:crear","compras:editar","compras:eliminar",
                "notif:ver"
            }.ToHashSet();

            var todosPermisos = context.Permissions.ToList();
            var validosIds = todosPermisos.Where(p => permisosValidos.Contains(p.Nombre)).Select(p => p.PermissionID).ToHashSet();
            var empleadoIds = todosPermisos.Where(p => permisosEmpleado.Contains(p.Nombre)).Select(p => p.PermissionID).ToHashSet();
            var notifId = todosPermisos.Where(p => p.Nombre == "notif:ver").Select(p => p.PermissionID).ToHashSet();

            bool cambios = false;

            var adminRole = context.Roles.FirstOrDefault(r => r.Nombre == "Administrador");
            if (adminRole != null)
            {
                if (adminRole.Descripcion != "Acceso total al sistema")
                { adminRole.Descripcion = "Acceso total al sistema"; cambios = true; }

                var actuales = context.RolePermissions.Where(rp => rp.RoleID == adminRole.RoleID).ToList();
                var sobran = actuales.Where(rp => !validosIds.Contains(rp.PermissionID)).ToList();
                var faltan = validosIds.Except(actuales.Select(rp => rp.PermissionID)).ToList();
                if (sobran.Any()) { context.RolePermissions.RemoveRange(sobran); cambios = true; }
                foreach (var id in faltan)
                { context.RolePermissions.Add(new RolePermission { RoleID = adminRole.RoleID, PermissionID = id }); cambios = true; }
            }

            var empleadoRole = context.Roles.FirstOrDefault(r => r.Nombre == "Empleado");
            if (empleadoRole != null)
            {
                if (empleadoRole.Descripcion != "Gestión de productos, ventas, clientes e inventario")
                { empleadoRole.Descripcion = "Gestión de productos, ventas, clientes e inventario"; cambios = true; }

                var actuales = context.RolePermissions.Where(rp => rp.RoleID == empleadoRole.RoleID).ToList();
                var sobran = actuales.Where(rp => !empleadoIds.Contains(rp.PermissionID)).ToList();
                var faltan = empleadoIds.Except(actuales.Select(rp => rp.PermissionID)).ToList();
                if (sobran.Any()) { context.RolePermissions.RemoveRange(sobran); cambios = true; }
                foreach (var id in faltan)
                { context.RolePermissions.Add(new RolePermission { RoleID = empleadoRole.RoleID, PermissionID = id }); cambios = true; }
            }

            var clienteRole = context.Roles.FirstOrDefault(r => r.Nombre == "Cliente");
            if (clienteRole != null)
            {
                if (clienteRole.Descripcion != "Acceso a la tienda")
                { clienteRole.Descripcion = "Acceso a la tienda"; cambios = true; }

                var actuales = context.RolePermissions.Where(rp => rp.RoleID == clienteRole.RoleID).ToList();
                var sobran = actuales.Where(rp => !notifId.Contains(rp.PermissionID)).ToList();
                var faltan = notifId.Except(actuales.Select(rp => rp.PermissionID)).ToList();
                if (sobran.Any()) { context.RolePermissions.RemoveRange(sobran); cambios = true; }
                foreach (var id in faltan)
                { context.RolePermissions.Add(new RolePermission { RoleID = clienteRole.RoleID, PermissionID = id }); cambios = true; }
            }

            if (cambios) await context.SaveChangesAsync();
        }

        // Nota: el usuario administrador ya no se crea automáticamente.
        // Créalo manualmente desde el dashboard de usuarios.
    }
}
