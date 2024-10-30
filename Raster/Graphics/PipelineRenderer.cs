using System.Drawing;
using System.Numerics;
using Raster.Graphics.Resources;
using Raster.Memory;
using Raster.Windowing;

namespace Raster.Graphics;

public abstract class PipelineRenderer : ResourceManager, IRenderer
{
    public IGraphicsDevice GraphicsDevice { get; internal init; } = null!;
    public IWindow Window { get; internal init; } = null!;
    
    protected Vector2 FrameBufferSize { get; private set; }
    protected Shader VertexShader { get; private set; } = null!;
    protected Shader FragmentShader { get; private set; } = null!;

    public abstract void Init();

    public virtual void BeginFrame(Vector2 size)
    {
        FrameBufferSize = size;
    }

    public virtual void EndFrame()
    {
        EndRenderPass();
    }

    public void Clear(Color color)
    {
        Clear(color, 1.0f, 0, ClearMask.All);
    }

    public void Clear(Color color, f32 depth, u8 stencil, ClearMask mask)
    {
        EndRenderPass();
        BeginRenderPass(color, depth, stencil, mask);
    }
    
    protected abstract void EndRenderPass();
    
    protected abstract void BeginRenderPass(Color clearColor, f32 clearDepth, u8 clearStencil, ClearMask mask);

    public void BindShader(Shader vertex, Shader fragment)
    {
        VertexShader = vertex;
        FragmentShader = fragment;
    }
}
