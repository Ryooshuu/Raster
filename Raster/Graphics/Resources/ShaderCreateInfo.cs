namespace Raster.Graphics.Resources;

public readonly struct ShaderCreateInfo
{
    public readonly ShaderFormat Format;
    public readonly ShaderStage Stage;
    public readonly string EntryPoint;
    
    public static ShaderCreateInfo VertexShader => new(ShaderStage.Vertex, ShaderFormat.SPIRV);
    public static ShaderCreateInfo FragmentShader => new(ShaderStage.Fragment, ShaderFormat.SPIRV);

    public ShaderCreateInfo(ShaderStage stage, ShaderFormat format, string entryPoint = "main")
    {
        Format = format;
        Stage = stage;
        EntryPoint = entryPoint;
    }
}
