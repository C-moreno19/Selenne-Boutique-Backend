using System.ComponentModel.DataAnnotations;

namespace SelenneApi.Models.DTOs.Productos;

public class SetTallasDto
{
    public List<TallaStockItem> Tallas { get; set; } = new();
}

public class TallaStockItem
{
    public int TallaID { get; set; }
    public int Stock { get; set; } = 0;
}

public class SetColoresDto
{
    public List<int> ColorIDs { get; set; } = new();
}

public class SetImagenesDto
{
    public List<ImagenConColorDto> Imagenes { get; set; } = new();
}

public class ImagenConColorDto
{
    public string URL { get; set; } = string.Empty;
    public string? ColorNombre { get; set; }
}

public class SetMaterialesDto
{
    public List<int> MaterialIDs { get; set; } = new();
}