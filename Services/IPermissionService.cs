namespace SelenneApi.Services;

public interface IPermissionService
{
    Task<List<string>> GetUserPermissionsAsync(int usuarioId);
    Task<bool> HasPermissionAsync(int usuarioId, string permiso);
}
