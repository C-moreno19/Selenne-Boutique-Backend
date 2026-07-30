using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SelenneApi.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var cloudName = configuration["CLOUDINARY_CLOUD_NAME"]
            ?? throw new InvalidOperationException("Falta la variable de entorno CLOUDINARY_CLOUD_NAME");
        var apiKey = configuration["CLOUDINARY_API_KEY"]
            ?? throw new InvalidOperationException("Falta la variable de entorno CLOUDINARY_API_KEY");
        var apiSecret = configuration["CLOUDINARY_API_SECRET"]
            ?? throw new InvalidOperationException("Falta la variable de entorno CLOUDINARY_API_SECRET");

        _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
    }

    public async Task<string> SubirImagenAsync(Stream fileStream, string fileName)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "selenne-boutique",
            Format = "webp",
            Transformation = new Transformation().Width(1200).Crop("limit").Quality(82)
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new Exception($"Error subiendo imagen a Cloudinary: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }
}
