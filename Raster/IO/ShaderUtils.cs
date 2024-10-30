using System.Reflection;
using System.Runtime.InteropServices;
using Raster.Graphics;
using Raster.Graphics.Resources;
using Silk.NET.Shaderc;

namespace Raster.IO;

public static unsafe class ShaderUtils
{
    private static readonly Shaderc shaderC = Shaderc.GetApi();

    public static Shader CreateShader(IGraphicsDevice device, string filename, Assembly assembly, ShaderCreateInfo info)
    {
        var stream = CompileShaderToSPIRV(filename, assembly, info.Stage, info.EntryPoint);
        return device.CreateShader(stream, info);
    }
    
    public static Stream CompileShaderToSPIRV(string filename, Assembly assembly, ShaderStage stage, string entryPoint = "main")
    {
        var stream = ResourceUtils.OpenResourceFromAssembly(filename, assembly);
        return CompileShaderToSPIRV(stream, stage, entryPoint);
    }

    public static Stream CompileShaderToSPIRV(Stream stream, ShaderStage stage, string entryPoint = "main")
    {
        var bytecodeBuffer = NativeMemory.Alloc((nuint)stream.Length);
        var bytecodeSpan = new Span<u8>(bytecodeBuffer, (i32)stream.Length);
        stream.ReadExactly(bytecodeSpan);
        
        var compiler = shaderC.CompilerInitialize();
        var compilationResult = shaderC.CompileIntoSpv(compiler, bytecodeSpan, (nuint)stream.Length, stage == ShaderStage.Vertex ? ShaderKind.VertexShader : ShaderKind.FragmentShader, "", entryPoint, null);
        
        var errors = shaderC.ResultGetNumErrors(compilationResult);
        if (errors > 0)
            throw new InvalidOperationException($"Failed to compile shader into SPIRV. Error ({errors}): {shaderC.ResultGetErrorMessageS(compilationResult)}");
        
        var spirvLength = shaderC.ResultGetLength(compilationResult);
        var spirvBuffer = NativeMemory.Alloc(spirvLength);
        var bytes = shaderC.ResultGetBytes(compilationResult);
        NativeMemory.Copy(bytes, spirvBuffer, spirvLength);
        var spirvSpan = new Span<u8>(spirvBuffer, (i32)shaderC.ResultGetLength(compilationResult));
        
        NativeMemory.Free(bytecodeBuffer);
        NativeMemory.Free(spirvBuffer);
        
        return new MemoryStream(spirvSpan.ToArray());
    }
}
