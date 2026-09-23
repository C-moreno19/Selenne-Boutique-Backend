namespace SelenneApi.Services;

public interface INotificationService
{
    Task CreateAsync(int usuarioId, string titulo, string mensaje, string tipo = "info", string? referencia = null);
    Task CreateBulkAsync(List<int> usuarioIds, string titulo, string mensaje, string tipo = "info");
    Task CreateForAllAsync(string titulo, string mensaje, string tipo = "info");
    Task CreateForRoleAsync(string rolNombre, string titulo, string mensaje, string tipo = "info");
    // Administradores + cualquier usuario cuyo rol tenga el permiso dado (mismo criterio que ya usaba NotificarStockBajoAsync).
    Task CreateForPermissionAsync(string permiso, string titulo, string mensaje, string tipo = "info", string? referencia = null);
    Task MarkAsReadAsync(int notificacionId, int usuarioId);
    Task MarkAllAsReadAsync(int usuarioId);
}
