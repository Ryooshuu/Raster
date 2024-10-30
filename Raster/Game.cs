using Raster.Graphics;

namespace Raster;

public class Game : IDisposable
{
    public virtual void Load(IGraphicsDevice device)
    {
    }

    public virtual void Update(TimeSpan delta)
    {
    }

    public virtual void Draw(IRenderer renderer)
    {
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
