namespace Raster.Graphics;

public enum ClearMask
{
    None = 0b0000,
    Color = 0b0001,
    Depth = 0b0010,
    Stencil = 0b0100,

    All = Color | Depth | Stencil,
}