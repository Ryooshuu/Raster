using System.Reflection;
using System.Text;
using SDL;
using Silk.NET.Core.Native;
using Silk.NET.Shaderc;
using Silk.NET.SPIRV.Cross;
using static SDL.SDL3;

namespace Raster.Graphics.SDL3;

public unsafe class SDL3ShaderOld : IDisposable
{
    private readonly Shaderc shaderC;
    private readonly Cross spirvCross;

    private readonly SDL_GPUDevice* gpuDevice;
    private readonly SDL_GPUShaderStage stage;
    private readonly byte[] bytes;

    public SDL3ShaderOld(SDL_GPUDevice* gpuDevice, string filename, SDL_GPUShaderStage stage)
    {
        this.gpuDevice = gpuDevice;
        this.stage = stage;

        shaderC = Shaderc.GetApi();
        spirvCross = Cross.GetApi();

        Stream? stream = Assembly.GetAssembly(typeof(SDL3ShaderOld))!.GetManifestResourceStream($"Raster.Resources.Shaders.{filename}");
        StreamReader reader = new(stream!);
        string source = reader.ReadToEnd();
        bytes = Encoding.UTF8.GetBytes(source);
    }

    private CompilationResult* compilationResult;

    public void CompileSourceToSPIRV()
    {
        var compiler = shaderC.CompilerInitialize();
        var options = shaderC.CompileOptionsInitialize();

        fixed (byte* ptr = &bytes[0])
        {
            shaderC.CompileOptionsSetSourceLanguage(options, SourceLanguage.Glsl);
            shaderC.CompileOptionsSetGenerateDebugInfo(options);
            
            compilationResult = shaderC.CompileIntoSpv(compiler, ptr, (nuint)bytes.Length, stage == SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_VERTEX ? ShaderKind.VertexShader : ShaderKind.FragmentShader, "", "main", options);
        }

        nuint errors = shaderC.ResultGetNumErrors(compilationResult);
        if (errors > 0)
        {
            throw new InvalidOperationException($"Failed to compile shader into SPIRV. Error ({errors}): {shaderC.ResultGetErrorMessageS(compilationResult)}");
        }
    }

    public SDL_GPUShader* CompileSPIRVToNative()
    {
        SDL_GPUShaderCreateInfo shaderInfo = new()
        {
            code = shaderC.ResultGetBytes(compilationResult),
            code_size = shaderC.ResultGetLength(compilationResult),
            entrypoint = (byte*)SilkMarshal.StringToPtr("main", NativeStringEncoding.LPUTF8Str),
            format = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV,
            stage = stage,
        };

        SDL_GPUShader* shader = SDL_CreateGPUShader(gpuDevice, &shaderInfo);

        return shader == null ? throw new InvalidOperationException($"Failed to create shader. Error: {SDL_GetError()}") : shader;

        /*string? driver = SDL_GetGPUDeviceDriver(gpuDevice);*/
        /**/
        /*Context* context;*/
        /*ParsedIr* ir;*/
        /*Silk.NET.SPIRV.Cross.Compiler* compiler;*/
        /*_ = spirvCross.ContextCreate(&context);*/
        /**/
        /*static void errorCallback(void* userdata, byte* message)*/
        /*{*/
        /*    Console.WriteLine(SilkMarshal.PtrToString((nint)message));*/
        /*}*/
        /**/
        /*SDL_GPUShaderFormat shaderFormat = SDL_GetGPUShaderFormats(gpuDevice);*/
        /*Backend backend;*/
        /**/
        /*if ()*/
        /**/
        /*spirvCross.ContextSetErrorCallback(context, PfnErrorCallback.From(errorCallback), null);*/
        /*_ = spirvCross.ContextParseSpirv(context, (uint*)shaderC.ResultGetBytes(compilationResult), shaderC.ResultGetLength(compilationResult), &ir);*/
        /*_ = spirvCross.ContextCreateCompiler(context, Backend.Glsl, ir, CaptureMode.TakeOwnership, &compiler);*/
        /**/
        /*CompilerOptions* compilerOptions;*/
        /*spirvCross.*/
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
