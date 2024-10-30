using Raster.Memory;

namespace Raster.Graphics.Resources;

public abstract class GraphicsResource : MemoryResource
{
    private IntPtr handle;

    public IntPtr Handle
    {
        get => handle;
        set => handle = value;
    }

    private readonly IGraphicsDevice device;
    
    protected GraphicsResource(IGraphicsDevice device)
        : base(device)
    {
        this.device = device;
    }

    protected abstract void ReleaseHandle(IGraphicsDevice device);

    public static implicit operator IntPtr(GraphicsResource resource)
        => resource.Handle;

    protected override void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            var toDispose = Interlocked.Exchange(ref handle, IntPtr.Zero);
            if (toDispose != IntPtr.Zero)
                ReleaseHandle(device);
        }
        
        base.Dispose(disposing);
    }
}
