using System.Runtime.InteropServices;

namespace Raster.Memory;

public class MemoryResource : IDisposable
{
    public IHandleMemoryResources MemoryManager { get;  }
    
    private GCHandle selfHandle;

    protected MemoryResource(IHandleMemoryResources memoryManager)
    {
        MemoryManager = memoryManager;
        
        selfHandle = GCHandle.Alloc(this, GCHandleType.Weak);
        MemoryManager.AddResourceReference(selfHandle);
    }

    ~MemoryResource()
    {
#if DEBUG
        Console.WriteLine($"WARN: A resource of type {GetType().Name} was not disposed.");
#endif
        
        Dispose(false);
    }

    public bool IsDisposed { get; private set; }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        if (disposing)
        {
            MemoryManager.RemoveResourceReference(selfHandle);
            selfHandle.Free();
        }

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
