using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Raster.Graphics.Resources;
using Raster.Windowing;
using SDL;
using static SDL.SDL3;

namespace Raster.Graphics.SDL3;

public unsafe class SDL3RendererOld : IRenderer
{
    private SDL_GPUDevice* gpuDevice;

    public IGraphicsDevice GraphicsDevice { get; }
    public IWindow Window { get; }

    public void Init()
    {
        // create GPU device
        gpuDevice = SDL_CreateGPUDevice(SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV, true, (byte*)null);

        /*if (!SDL_ClaimWindowForGPUDevice(gpuDevice, SDL3Window.SdlWindowHandle))
        {
            throw new InvalidOperationException($"Failed to claim window for GPU device. SDL Error: {SDL_GetError()}");
        }*/

        // create shaders
        SDL3ShaderOld vertex = new(gpuDevice, "rawTriangle.vert", SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_VERTEX);
        SDL3ShaderOld fragment = new(gpuDevice, "solidColor.frag", SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT);

        vertex.CompileSourceToSPIRV();
        fragment.CompileSourceToSPIRV();

        SDL_GPUShader* vertexShader = vertex.CompileSPIRVToNative();
        SDL_GPUShader* fragmentShader = fragment.CompileSPIRVToNative();

        // create pipelines
        SDL_GPUColorTargetDescription* colorAttachmentDescriptions = stackalloc SDL_GPUColorTargetDescription[1];
        //colorAttachmentDescriptions[0].format = SDL_GetGPUSwapchainTextureFormat(gpuDevice, SDL3Window.SdlWindowHandle);

        SDL_GPUGraphicsPipelineCreateInfo pipelineCreateInfo = new()
        {
            target_info = new()
            {
                num_color_targets = 1,
                color_target_descriptions = colorAttachmentDescriptions
            },
            rasterizer_state = new()
            {
                fill_mode = SDL_GPUFillMode.SDL_GPU_FILLMODE_FILL
            },
            primitive_type = SDL_GPUPrimitiveType.SDL_GPU_PRIMITIVETYPE_TRIANGLELIST,
            vertex_shader = vertexShader,
            fragment_shader = fragmentShader,
        };

        fillPipeline = SDL_CreateGPUGraphicsPipeline(gpuDevice, &pipelineCreateInfo);

        pipelineCreateInfo.rasterizer_state.fill_mode = SDL_GPUFillMode.SDL_GPU_FILLMODE_LINE;
        linePipeline = SDL_CreateGPUGraphicsPipeline(gpuDevice, &pipelineCreateInfo);
            
        SDL_ReleaseGPUShader(gpuDevice, vertexShader);
        SDL_ReleaseGPUShader(gpuDevice, fragmentShader);
    }

    private SDL_GPUGraphicsPipeline* fillPipeline;
    private SDL_GPUGraphicsPipeline* linePipeline;

    private SDL_GPUCommandBuffer* commandBuffer;
    private SDL_GPUTexture* swapchainTexture;

    public void BeginFrame(Vector2 size)
    {
        /*if (SDL3Window.SdlWindowHandle == null)
        {
            return;
        }*/

        commandBuffer = SDL_AcquireGPUCommandBuffer(gpuDevice);

        if (commandBuffer == null)
        {
            throw new InvalidOperationException($"Failed to claim command buffer for GPU device. SDL Error: {SDL_GetError()}");
        }

        fixed (SDL_GPUTexture** targetTexture = &swapchainTexture)
        {
            /*if (!SDL_AcquireGPUSwapchainTexture(commandBuffer, SDL3Window.SdlWindowHandle, targetTexture, null, null))
            {
                throw new InvalidOperationException($"Failed to claim swapchain texture. SDL Error: {SDL_GetError()}");
            }*/
        }
    }

    public void EndFrame()
    {
        /*if (SDL3Window.SdlWindowHandle == null)
        {
            return;
        }*/
        _ = SDL_SubmitGPUCommandBuffer(commandBuffer);
    }

    public void Clear(Color color)
    {
        Clear(color, 1.0f, 0, ClearMask.All);
    }

    public void Clear(Color color, float depth, u8 stencil, ClearMask mask)
    {
        /*if (SDL3Window.SdlWindowHandle == null)
        {
            return;
        }*/
        SDL_GPUColorTargetInfo colorTargetInfo = new()
        {
            texture = swapchainTexture,
            clear_color = new() { r = color.R, g = color.G, b = color.B, a = color.A },
            load_op = SDL_GPULoadOp.SDL_GPU_LOADOP_CLEAR,
            store_op = SDL_GPUStoreOp.SDL_GPU_STOREOP_STORE
        };

        SDL_GPURenderPass* renderPass = SDL_BeginGPURenderPass(commandBuffer, &colorTargetInfo, 1, null);
        SDL_BindGPUGraphicsPipeline(renderPass, fillPipeline);
        SDL_DrawGPUPrimitives(renderPass, 3, 1, 0, 0);
        SDL_EndGPURenderPass(renderPass);
    }

    public void BindShader(Shader vertex, Shader fragment)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        SDL_ReleaseGPUGraphicsPipeline(gpuDevice, fillPipeline);
        //SDL_ReleaseWindowFromGPUDevice(gpuDevice, SDL3Window.SdlWindowHandle);
        SDL_DestroyGPUDevice(gpuDevice);

        gpuDevice = null!;
    }

    public HashSet<GCHandle> Resources { get; }
    public void AddResourceReference(GCHandle resourceReference)
    {
        throw new NotImplementedException();
    }

    public void RemoveResourceReference(GCHandle resourceReference)
    {
        throw new NotImplementedException();
    }
}
