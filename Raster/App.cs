using System.Diagnostics;
using System.Drawing;
using Raster.Graphics;
using Raster.Windowing;

namespace Raster;

public class App : IDisposable
{
    private readonly Stopwatch timer;
    private TimeSpan lastTime;
    private TimeSpan accumulator;

    public bool Running { get; private set; }
    public IWindow Window { get; private set; }
    public IGraphicsDevice GraphicsDevice => Window.GraphicsDevice;
    public IRenderer Renderer => GraphicsDevice.Renderer;

    public App(WindowCreateInfo info)
    {
        lastTime = TimeSpan.Zero;
        timer = new Stopwatch();

        Window = new SDL3Window();
        Window.Init(info);
    }

    private Game game = null!;
    private bool firstDraw = true;

    public void Run(Game game)
    {
        this.game = game;

        game.Load(GraphicsDevice);
        timer.Start();
        Running = Window.Alive;

        while (Running)
        {
            Running = Window.Alive;
            Window.Poll();
            Tick();

            Renderer.BeginFrame(Window.Size);
            Renderer.Clear(Color.Red);
            game.Draw(Renderer);
            Renderer.EndFrame();

            if (firstDraw)
            {
                firstDraw = false;
                Window.Show();
            }
        }

        Window.Close();
    }

    private void Tick()
    {
        void Update(TimeSpan delta)
        {
            Time.Frame++;
            Time.Advance(delta);

            game.Update(delta);
        }

        TimeSpan target;
        TimeSpan maxElapsedTime;

        if (!Time.FixedStep)
        {
            target = TimeSpan.FromSeconds(1.0 / 1000.0);
            maxElapsedTime = TimeSpan.FromSeconds(5.0 / 1000.0);
        }
        else
        {
            target = Window.Focused ? Time.FixedStepTarget : Time.UnfocusedStepTarget;
            maxElapsedTime = Window.Focused ? Time.FixedStepMaxTarget : Time.UnfocusedStepMaxTarget;
        }

        TimeSpan currentTime = timer.Elapsed;
        TimeSpan deltaTime = currentTime - lastTime;
        lastTime = currentTime;

        accumulator += deltaTime;

        // don't run too fast
        while (accumulator < target)
        {
            i32 milliseconds = (i32)(target - accumulator).TotalMilliseconds;
            Thread.Sleep(milliseconds);

            currentTime = timer.Elapsed;
            deltaTime = currentTime - lastTime;
            lastTime = currentTime;
            accumulator += deltaTime;
        }

        // don't allow any update to take longer than our maximum
        if (accumulator > maxElapsedTime)
        {
            Time.Advance(accumulator - maxElapsedTime);
            accumulator = maxElapsedTime;
        }

        // do as many fixed updates as we can
        while (accumulator >= target)
        {
            accumulator -= target;
            Update(target);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        game.Dispose();
        Window.Dispose();
    }
}
