
using Raster.Graphics;
using Raster.Graphics.Resources;
using Raster.IO;

namespace Raster.Explorer;

public class Explorer : Game
{
    private Shader vertexShader = null!;
    private Shader fragmentShader = null!;
    
    public override void Load(IGraphicsDevice device)
    {
        vertexShader = ShaderUtils.CreateShader(device, "Shaders/rawTriangle.vert", typeof(App).Assembly, ShaderCreateInfo.VertexShader);
        fragmentShader = ShaderUtils.CreateShader(device, "Shaders/solidColor.frag", typeof(App).Assembly, ShaderCreateInfo.FragmentShader);
    }

    public override void Update(TimeSpan delta)
    {
        if (Time.OnInterval(1.0f))
        {
            Console.WriteLine($"FPS: {Time.FramesPerSecond}");
        }
    }

    public override void Draw(IRenderer renderer)
    {
        renderer.BindShader(vertexShader, fragmentShader);
    }
}
