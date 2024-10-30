using Raster.Windowing;

namespace Raster.Explorer;

public static class Program
{
    public static void Main()
    {
        using App app = new(new WindowCreateInfo
        {
            Title = "Raster",
            Width = 1280,
            Height = 720,
            Resizable = true
        });
        app.Run(new Explorer());
    }
}
