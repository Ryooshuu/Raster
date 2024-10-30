using System.Runtime.InteropServices;

namespace Raster.Memory;

public interface IHandleMemoryResources : IDisposable
{
    internal HashSet<GCHandle> Resources { get; }
    
    internal void AddResourceReference(GCHandle resourceReference);
    internal void RemoveResourceReference(GCHandle resourceReference);
}
