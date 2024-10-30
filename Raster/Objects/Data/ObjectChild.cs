using System.Text.Json.Serialization;

namespace Raster.Objects.Data;

[Serializable]
public class ObjectChild
{
    [JsonPropertyName("texture")]
    public string Texture { get; set; } = null!;

    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    [JsonPropertyName("z")]
    public short Z { get; set; }

    [JsonPropertyName("rot")]
    public float Rotation { get; set; }

    [JsonPropertyName("anchor_x")]
    public float AnchorX { get; set; }

    [JsonPropertyName("anchor_y")]
    public float AnchorY { get; set; }

    [JsonPropertyName("scale_x")]
    public float ScaleX { get; set; }

    [JsonPropertyName("scale_y")]
    public float ScaleY { get; set; }

    [JsonPropertyName("flip_x")]
    public bool FlipX { get; set; }

    [JsonPropertyName("flip_y")]
    public bool FlipY { get; set; }

    [JsonPropertyName("color_type")]
    public string? ColorType { get; set; }

    [JsonPropertyName("opacity")]
    public float? Opacity { get; set; }

    [JsonPropertyName("children")]
    public List<ObjectChild>? Children { get; set; }
}