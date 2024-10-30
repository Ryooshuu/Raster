namespace Raster.Graphics.Resources;

public abstract class Shader : GraphicsResource
{
    protected Shader(IGraphicsDevice device)
        : base(device)
    {
    }
}
