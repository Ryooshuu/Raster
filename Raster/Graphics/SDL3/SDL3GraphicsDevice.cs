using Raster.Graphics.Resources;
using Raster.Memory;
using Raster.Windowing;
using SDL;
using static SDL.SDL3;

namespace Raster.Graphics.SDL3;

public unsafe class SDL3GraphicsDevice : ResourceManager, IGraphicsDevice
{
    public IRenderer Renderer { get; private set; }
    public IWindow Window { get; internal init; } = null!;

    private SDL3Window sdlWindow => (SDL3Window)Window;
    
    internal SDL_GPUDevice* GPUDevice;

    public void Init()
    {
        GPUDevice = SDL_CreateGPUDevice(SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV, true, (u8*)null);

        if (!SDL_ClaimWindowForGPUDevice(GPUDevice, sdlWindow.SDLWindowHandle))
        {
            throw new InvalidOperationException($"Failed to claim window for GPU device. SDL Error: {SDL_GetError()}");
        }

        Renderer = new SDL3Renderer
        {
            GraphicsDevice = this,
            Window = Window
        };
        
        Renderer.Init();
    }

    public Shader CreateShader(string filePath, ShaderCreateInfo info)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return CreateShader(stream, info);
    }
    
    public Shader CreateShader(u8[] data, ShaderCreateInfo info)
    {
        using var stream = new MemoryStream(data);
        return CreateShader(stream, info);
    }
    
    public Shader CreateShader(Stream stream, ShaderCreateInfo info)
        => SDL3Shader.Create(this, stream, info);

    protected override void DisposeResources()
    {
        Renderer.Dispose();
        
        SDL_ReleaseWindowFromGPUDevice(GPUDevice, sdlWindow.SDLWindowHandle);
        SDL_DestroyGPUDevice(GPUDevice);
    }
}
