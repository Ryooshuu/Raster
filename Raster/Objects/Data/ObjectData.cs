using System.Text.Json.Serialization;

namespace Raster.Objects.Data;

[Serializable]
public class ObjectData
{
    [JsonPropertyName("texture")]
    public string? Texture { get; set; }

    [JsonPropertyName("default_z_layer")]
    public byte? DefaultZLayer { get; set; }

    [JsonPropertyName("default_z_order")]
    public short? DefaultZOrder { get; set; }

    [JsonPropertyName("default_base_color_channel")]
    public ulong? DefaultBaseColorChannel { get; set; }

    [JsonPropertyName("default_detail_color_channel")]
    public ulong? DefaultDetailColorChannel { get; set; }

    [JsonPropertyName("color_type")]
    public string? ColorType { get; set; }

    [JsonPropertyName("swap_base_detail")]
    public bool? SwapBaseDetail { get; set; }

    [JsonPropertyName("opacity")]
    public float? Opacity { get; set; }

    [JsonPropertyName("hitbox")]
    public Hitbox? Hitbox { get; set; }

    [JsonPropertyName("children")]
    public List<ObjectChild>? Children { get; set; }
}