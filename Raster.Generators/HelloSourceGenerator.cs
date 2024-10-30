using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class HelloSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        StringBuilder source = new();

        //TODO: Grab from Resources.
        string jsonString = File.ReadAllText("C:/Users/naoei/Downloads/object.json");
        using JsonDocument document = JsonDocument.Parse(jsonString);

        _ = source.AppendLine(@"using Raster.Objects.Data;

namespace Raster.Generated
{
    public static class ObjectDefaults
    {
        public static Dictionary<string, ObjectData> OBJECT_DEFAULTS = new Dictionary<string, ObjectData>()
        {");

        Dictionary<string, JsonElement> dict = document.Deserialize<Dictionary<string, JsonElement>>() ?? throw new Exception("object.json must exist with valid data.");

        foreach (KeyValuePair<string, JsonElement> entry in dict)
        {
            _ = source.AppendLine("            {");
            _ = source.AppendLine("                \"" + entry.Key + "\",");
            _ = source.AppendLine("                new ObjectData");
            _ = source.AppendLine("                {");

            JsonElement data = entry.Value;

            WriteObject(source, data);

            _ = source.AppendLine("                }");
            _ = source.AppendLine("            },");
        }

        _ = source.AppendLine(@"        };
    }
}");

        Console.WriteLine(source.ToString());
        context.AddSource("ObjectDefaults.g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
    }

    private static void WriteObject(StringBuilder source, JsonElement data)
    {
        _ = data.TryGetProperty("texture", out JsonElement texture)
            ? source.AppendLine($"                    Texture = \"{texture.GetString()}\",")
            : source.AppendLine("                    Texture = \"emptyFrame.png\",");

        _ = data.TryGetProperty("default_z_layer", out JsonElement defaultZLayer)
            ? source.AppendLine($"                    DefaultZLayer = {defaultZLayer.GetByte()},")
            : source.AppendLine($"                    DefaultZLayer = 0,");

        _ = data.TryGetProperty("default_z_order", out JsonElement defaultZOrder)
            ? source.AppendLine($"                    DefaultZOrder = {defaultZOrder.GetInt16()},")
            : source.AppendLine($"                    DefaultZOrder = 0,");

        _ = data.TryGetProperty("default_base_color_channel", out JsonElement defaultBaseColorChannel)
            ? source.AppendLine($"                    DefaultBaseColorChannel = {defaultBaseColorChannel.GetUInt64()},")
            : source.AppendLine($"                    DefaultBaseColorChannel = {ulong.MaxValue},");

        _ = data.TryGetProperty("default_detail_color_channel", out JsonElement defaultDetailColorChannel)
            ? source.AppendLine($"                    DefaultDetailColorChannel = {defaultDetailColorChannel.GetUInt64()},")
            : source.AppendLine($"                    DefaultDetailColorChannel = {ulong.MaxValue},");

        _ = data.TryGetProperty("color_type", out JsonElement colorType)
            ? source.AppendLine($"                    ColorType = \"{colorType.GetString()}\",")
            : source.AppendLine($"                    ColorType = \"None\",");

        _ = data.TryGetProperty("swap_base_detail", out JsonElement swapBaseDetail)
            ? source.AppendLine($"                    SwapBaseDetail = {swapBaseDetail.GetBoolean().ToString().ToLower()},")
            : source.AppendLine($"                    SwapBaseDetail = {false.ToString().ToLower()},");

        _ = data.TryGetProperty("opacity", out JsonElement opacity)
            ? source.AppendLine($"                    Opacity = {opacity.GetSingle()}f,")
            : source.AppendLine($"                    Opacity = 1f,");

        if (data.TryGetProperty("hitbox", out JsonElement hitbox))
        {
            _ = source.AppendLine($"                    Hitbox = new Hitbox");
            _ = source.AppendLine("                    {");
            WriteHitbox(source, hitbox);
            _ = source.AppendLine("                    },");
        }
        else
        {
            _ = source.AppendLine($"                    Hitbox = null,");
        }

        if (data.TryGetProperty("children", out JsonElement children))
        {
            _ = source.AppendLine($"                    Children = new List<ObjectChild>");
            _ = source.AppendLine("                    {");

            foreach (JsonElement child in children.EnumerateArray())
            {
                WriteChild(source, child, "                    ");
            }

            _ = source.AppendLine("                    },");
        }
        else
        {
            _ = source.AppendLine($"                    Children = new List<ObjectChild>(),");
        }
    }

    private static void WriteHitbox(StringBuilder source, JsonElement data)
    {
        _ = data.TryGetProperty("type", out JsonElement type)
            ? source.AppendLine($"                        Type = \"{type.GetString()}\",")
            : source.AppendLine($"                        Type = \"None\",");

        _ = data.TryGetProperty("x", out JsonElement x)
            ? source.AppendLine($"                        X = {x.GetSingle()}f,")
            : source.AppendLine($"                        X = 0f,");

        _ = data.TryGetProperty("y", out JsonElement y)
            ? source.AppendLine($"                        Y = {y.GetSingle()}f,")
            : source.AppendLine($"                        Y = 0f,");

        _ = data.TryGetProperty("width", out JsonElement width)
            ? source.AppendLine($"                        Width = {width.GetSingle()}f,")
            : source.AppendLine($"                        Width = 0f,");

        _ = data.TryGetProperty("height", out JsonElement height)
            ? source.AppendLine($"                        Height = {height.GetSingle()}f,")
            : source.AppendLine($"                        Height = 0f,");

        _ = data.TryGetProperty("radius", out JsonElement radius)
            ? source.AppendLine($"                        Radius = {radius.GetSingle()}f,")
            : source.AppendLine($"                        Radius = 0f,");
    }

    private static void WriteChild(StringBuilder source, JsonElement data, string initialIndent)
    {
        string indent = initialIndent + "    ";

        _ = source.AppendLine($"{indent}new ObjectChild");
        _ = source.AppendLine($"{indent}{{");

        _ = source.AppendLine($"{indent}    Texture = \"{data.GetProperty("texture").GetString()}\",");
        _ = source.AppendLine($"{indent}    X = {data.GetProperty("x").GetSingle()}f,");
        _ = source.AppendLine($"{indent}    Y = {data.GetProperty("y").GetSingle()}f,");
        _ = source.AppendLine($"{indent}    Z = {data.GetProperty("z").GetInt16()},");
        _ = source.AppendLine($"{indent}    Rotation = {data.GetProperty("rot").GetSingle()}f,");
        _ = source.AppendLine($"{indent}    AnchorX = {data.GetProperty("anchor_x").GetSingle()}f,");
        _ = source.AppendLine($"{indent}    AnchorY = {data.GetProperty("anchor_y").GetSingle()}f,");
        _ = source.AppendLine($"{indent}    ScaleX = {data.GetProperty("scale_x").GetSingle()}f,");
        _ = source.AppendLine($"{indent}    ScaleY = {data.GetProperty("scale_y").GetSingle()}f,");
        _ = source.AppendLine($"{indent}    FlipX = {data.GetProperty("flip_x").GetBoolean().ToString().ToLower()},");
        _ = source.AppendLine($"{indent}    FlipY = {data.GetProperty("flip_y").GetBoolean().ToString().ToLower()},");

        _ = data.TryGetProperty("color_type", out JsonElement colorType)
            ? source.AppendLine($"{indent}    ColorType = \"{colorType.GetString()}\",")
            : source.AppendLine($"{indent}    ColorType = \"None\",");

        _ = data.TryGetProperty("opacity", out JsonElement opacity)
            ? source.AppendLine($"{indent}    Opacity = {opacity.GetSingle()}f,")
            : source.AppendLine($"{indent}    Opacity = 1f,");

        if (data.TryGetProperty("children", out JsonElement children))
        {
            _ = source.AppendLine($"{indent}    Children = new List<ObjectChild>");
            _ = source.AppendLine($"{indent}    {{");

            foreach (JsonElement child in children.EnumerateArray())
            {
                WriteChild(source, child, $"{indent}    ");
            }

            _ = source.AppendLine($"{indent}    }},");
        }
        else
        {
            _ = source.AppendLine($"{indent}    Children = new List<ObjectChild>(),");
        }

        _ = source.AppendLine($"{indent}}},");
    }
}
