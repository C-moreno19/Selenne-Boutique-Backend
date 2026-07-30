namespace SelenneApi.Services;

public interface ICloudinaryService
{
    Task<string> SubirImagenAsync(Stream fileStream, string fileName);
}
