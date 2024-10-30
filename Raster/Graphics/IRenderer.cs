using System.Drawing;
using System.Numerics;
using Raster.Graphics.Resources;
using Raster.Memory;
using Raster.Windowing;

namespace Raster.Graphics;

public interface IRenderer : IHandleMemoryResources 
{
    internal IGraphicsDevice GraphicsDevice { get; }
    internal IWindow Window { get; }
    
    void Init();

    void BeginFrame(Vector2 size);
    void EndFrame();

    void Clear(Color color);
    void Clear(Color color, f32 depth, u8 stencil, ClearMask mask);

    void BindShader(Shader vertex, Shader fragment);
}
