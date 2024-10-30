using Raster.Memory;

namespace Raster.Windowing;

public class WindowHandle : MemoryResource
{
    public IntPtr NativeDisplayTypeHandle;
    public IntPtr NativeWindowHandle;

    public WindowHandle(IHandleMemoryResources memoryManager, IntPtr nativeDisplayTypeHandle, IntPtr nativeWindowHandle) : base(memoryManager)
    {
        NativeDisplayTypeHandle = nativeDisplayTypeHandle;
        NativeWindowHandle = nativeWindowHandle;
    }
}
