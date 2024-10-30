using System.Runtime.InteropServices;

namespace Raster.Memory;

public abstract class ResourceManager : IHandleMemoryResources
{
    private readonly HashSet<GCHandle> resources = new();
    
    internal void AddResourceReference(GCHandle resourceReference)
    {
        lock (resources)
            resources.Add(resourceReference);
    }

    internal void RemoveResourceReference(GCHandle resourceReference)
    {
        lock (resources)
            resources.Remove(resourceReference);
    }
    
    ~ResourceManager()
    {
        Dispose(false);
    }

    public bool IsDisposed { get; private set; }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        if (disposing)
        {
            lock (resources)
            {
                foreach (var resource in resources)
                {
                    if (resource.Target is IDisposable disposable)
                        disposable.Dispose();
                }
                
                DisposeResources();
                
                resources.Clear();
            }
        }

        IsDisposed = true;
    }

    protected virtual void DisposeResources()
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    HashSet<GCHandle> IHandleMemoryResources.Resources => resources;

    void IHandleMemoryResources.AddResourceReference(GCHandle resourceReference)
        => AddResourceReference(resourceReference);

    void IHandleMemoryResources.RemoveResourceReference(GCHandle resourceReference)
        => RemoveResourceReference(resourceReference);
}
