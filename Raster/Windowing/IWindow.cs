using System.Numerics;
using Raster.Graphics;
using Raster.Memory;

namespace Raster.Windowing;

public interface IWindow : IHandleMemoryResources
{
    bool Visible { get; set; }
    bool Focused { get; set; }
    bool IsMinimized { get; set; }
    bool IsFullscreen { get; set; }
    i32 X { get; set; }
    i32 Y { get; set; }
    i32 Width { get; set; }
    i32 Height { get; set; }
    Vector2 Position { get; set; }
    Vector2 Size { get; set; }
    string Title { get; set; }

    bool Alive { get; }
    WindowHandle Handle { get; }
    IGraphicsDevice GraphicsDevice { get; }

    void Init(WindowCreateInfo info);
    void Show();
    void Hide();
    void ToggleFullscreen();
    void Poll();
    void Close();
}
