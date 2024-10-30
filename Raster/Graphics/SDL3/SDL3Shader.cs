using System.Runtime.InteropServices;
using Raster.Graphics.Resources;
using SDL;
using Silk.NET.Core.Native;
using static SDL.SDL3;

namespace Raster.Graphics.SDL3;

public unsafe class SDL3Shader : Shader
{
    public SDL3Shader(IGraphicsDevice device)
        : base(device)
    {
    }

    public static Shader Create(SDL3GraphicsDevice device, Stream stream, ShaderCreateInfo info)
    {
        var bytecodeBuffer = NativeMemory.Alloc((nuint) stream.Length);
        var bytecodeSpan = new Span<u8>(bytecodeBuffer, (i32)stream.Length);
        stream.ReadExactly(bytecodeSpan);

        SDL_GPUShaderCreateInfo createInfo;
        createInfo.code_size = (nuint)stream.Length;
        createInfo.code = (u8*) bytecodeBuffer;
        createInfo.entrypoint = (u8*) SilkMarshal.StringToPtr(info.EntryPoint, NativeStringEncoding.LPUTF8Str);
        createInfo.stage = info.Stage.ToShaderStage();
        createInfo.format = info.Format.ToShaderFormat();

        var shaderModule = SDL_CreateGPUShader(device.GPUDevice, &createInfo);
        
        NativeMemory.Free(bytecodeBuffer);

        if (shaderModule == null)
            throw new InvalidOperationException($"Failed to create shader. SDL Error: {SDL_GetError()}");

        return new SDL3Shader(device)
        {
            Handle = (IntPtr)shaderModule
        };
    }

    protected override void ReleaseHandle(IGraphicsDevice device)
    {
        if (Handle == IntPtr.Zero)
            return;
        
        var sdlDevice = (SDL3GraphicsDevice)device;
        SDL_ReleaseGPUShader(sdlDevice.GPUDevice, (SDL_GPUShader*)Handle);
    }
}
