using Raster.Graphics.Resources;
using SDL;

namespace Raster.Graphics.SDL3;

public static class SDL3Utils
{
    public static SDL_GPUShaderFormat ToShaderFormat(this ShaderFormat format)
    {
        return format switch
        {
            ShaderFormat.Private => SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_PRIVATE,
            ShaderFormat.SPIRV => SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV,
            ShaderFormat.DXBC => SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXBC,
            ShaderFormat.DXIL => SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_DXIL,
            ShaderFormat.MSL => SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL,
            ShaderFormat.MetalLib => SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_METALLIB,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
    
    public static SDL_GPUShaderStage ToShaderStage(this ShaderStage stage)
    {
        return stage switch
        {
            ShaderStage.Vertex => SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_VERTEX,
            ShaderStage.Fragment => SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT,
            _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, null)
        };
    }
}
