using Raster.Graphics.Resources;
using Raster.Memory;
using Raster.Windowing;

namespace Raster.Graphics;

public interface IGraphicsDevice : IHandleMemoryResources
{
    internal IRenderer Renderer { get; }
    internal IWindow Window { get; }
    
    void Init();
    
    Shader CreateShader(Stream stream, ShaderCreateInfo info);
    Shader CreateShader(u8[] data, ShaderCreateInfo info);
    Shader CreateShader(string filePath, ShaderCreateInfo info);
}
