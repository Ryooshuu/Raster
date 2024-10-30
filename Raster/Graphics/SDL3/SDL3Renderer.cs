using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raster.Windowing;
using SDL;
using static SDL.SDL3;

namespace Raster.Graphics.SDL3;

public unsafe class SDL3Renderer : PipelineRenderer
{
    private SDL3GraphicsDevice sdlGraphicsDevice => (SDL3GraphicsDevice)GraphicsDevice;
    private SDL3Window sdlWindow => (SDL3Window)Window;
    private SDL_GPUDevice* gpuDevice => sdlGraphicsDevice.GPUDevice;

    private SDL_GPUGraphicsPipelineCreateInfo* pipelineCreateInfo;
    private SDL_GPUCommandBuffer* commandBuffer;
    private SDL_GPUTexture* swapchainTexture;

    private GCHandle colorAttachmentDescriptionsPtr;
    
    public override void Init()
    {
        var colorAttachmentDescriptions = new SDL_GPUColorTargetDescription[1];
        colorAttachmentDescriptions[0].format = SDL_GetGPUSwapchainTextureFormat(gpuDevice, sdlWindow.SDLWindowHandle);
        
        colorAttachmentDescriptionsPtr = GCHandle.Alloc(colorAttachmentDescriptions, GCHandleType.Pinned);
        
        pipelineCreateInfo = (SDL_GPUGraphicsPipelineCreateInfo*)Marshal.AllocHGlobal(sizeof(SDL_GPUGraphicsPipelineCreateInfo));
        Unsafe.InitBlockUnaligned(pipelineCreateInfo, 0, (u32)sizeof(SDL_GPUGraphicsPipelineCreateInfo));
        
        pipelineCreateInfo->target_info.num_color_targets = 1;
        pipelineCreateInfo->target_info.color_target_descriptions = (SDL_GPUColorTargetDescription*)colorAttachmentDescriptionsPtr.AddrOfPinnedObject();
        
        pipelineCreateInfo->rasterizer_state.fill_mode = SDL_GPUFillMode.SDL_GPU_FILLMODE_FILL;
        pipelineCreateInfo->primitive_type = SDL_GPUPrimitiveType.SDL_GPU_PRIMITIVETYPE_TRIANGLELIST;
    }

    public override void BeginFrame(Vector2 size)
    {
        base.BeginFrame(size);

        commandBuffer = SDL_AcquireGPUCommandBuffer(gpuDevice);
        
        if (commandBuffer == null)
            throw new InvalidOperationException($"Failed to claim command buffer for GPU device. SDL Error: {SDL_GetError()}");

        fixed (SDL_GPUTexture** targetTexture = &swapchainTexture)
        {
            if (!SDL_AcquireGPUSwapchainTexture(commandBuffer, sdlWindow.SDLWindowHandle, targetTexture, null, null))
                throw new InvalidOperationException($"Failed to claim swapchain texture. SDL Error: {SDL_GetError()}");
        }
    }

    public override void EndFrame()
    {
        base.EndFrame();

        SDL_SubmitGPUCommandBuffer(commandBuffer);
    }

    private SDL_GPURenderPass* currentRenderPass;
    private bool activeRenderPass;

    protected override void BeginRenderPass(Color clearColor, f32 clearDepth, u8 clearStencil, ClearMask mask)
    {
        if (Window.IsMinimized)
            return;
        
        var colorTargetInfo = new SDL_GPUColorTargetInfo
        {
            texture = swapchainTexture,
            clear_color = new() { r = clearColor.R, g = clearColor.G, b = clearColor.B, a = clearColor.A },
            load_op = SDL_GPULoadOp.SDL_GPU_LOADOP_CLEAR,
            store_op = SDL_GPUStoreOp.SDL_GPU_STOREOP_STORE
        };
        
        currentRenderPass = SDL_BeginGPURenderPass(commandBuffer, &colorTargetInfo, 1, null);
        activeRenderPass = true;
    }
    
    protected override void EndRenderPass()
    {
        if (!activeRenderPass)
            return;
        
        // todo: remove
        updatePipelineInfo();
        var pipeline = createPipeline();
        SDL_BindGPUGraphicsPipeline(currentRenderPass, pipeline);
        SDL_DrawGPUPrimitives(currentRenderPass, 3, 1, 0, 0);
        
        SDL_EndGPURenderPass(currentRenderPass);
        activeRenderPass = false;
    }

    private void updatePipelineInfo()
    {
        SDL3ShaderOld vertex = new(gpuDevice, "rawTriangle.vert", SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_VERTEX);
        SDL3ShaderOld fragment = new(gpuDevice, "solidColor.frag", SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT);
        
        vertex.CompileSourceToSPIRV();
        fragment.CompileSourceToSPIRV();
        
        var vertexShader = vertex.CompileSPIRVToNative();
        var fragmentShader = fragment.CompileSPIRVToNative();
        
        pipelineCreateInfo->vertex_shader = (SDL_GPUShader*)VertexShader.Handle;
        pipelineCreateInfo->fragment_shader = (SDL_GPUShader*)FragmentShader.Handle;
        
        // pipelineCreateInfo->vertex_shader = vertexShader;
        // pipelineCreateInfo->fragment_shader = fragmentShader;
    }

    private readonly Dictionary<SDL_GPUGraphicsPipelineCreateInfo, IntPtr> pipelineCache = new();

    private SDL_GPUGraphicsPipeline* createPipeline()
    {
        var descriptor = Marshal.PtrToStructure<SDL_GPUGraphicsPipelineCreateInfo>((IntPtr)pipelineCreateInfo);

        if (pipelineCache.TryGetValue(descriptor, out var instance))
            return (SDL_GPUGraphicsPipeline*)instance;
        
        var pipeline = SDL_CreateGPUGraphicsPipeline(gpuDevice, pipelineCreateInfo);
        if (pipeline == null)
            throw new InvalidOperationException($"Failed to create pipeline. SDL Error: {SDL_GetError()}");
            
        instance = (IntPtr)pipeline;
        pipelineCache.Add(descriptor, instance);

        return (SDL_GPUGraphicsPipeline*)instance;
    }

    protected override void DisposeResources()
    {
        Marshal.FreeHGlobal((IntPtr)pipelineCreateInfo);
        colorAttachmentDescriptionsPtr.Free();
    }
}
