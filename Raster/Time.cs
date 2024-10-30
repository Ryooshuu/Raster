namespace Raster;

public static class Time
{
    public static f32 Delta;

    public static TimeSpan Duration;

    public static u64 Frame;

    public static bool FixedStep = true;

    public static TimeSpan FixedStepTarget = TimeSpan.FromSeconds(1.0f / 60.0f);
    public static TimeSpan FixedStepMaxTarget = TimeSpan.FromSeconds(5.0f / 60.0f);

    public static TimeSpan UnfocusedStepTarget = TimeSpan.FromSeconds(1.0f / 30.0f);
    public static TimeSpan UnfocusedStepMaxTarget = TimeSpan.FromSeconds(5.0f / 30.0f);

    private static f64 tempSecondCounter;
    private static f64 tempFps;

    public static f64 FramesPerSecond;

    public static void Advance(TimeSpan delta)
    {
        Delta = (float)delta.TotalSeconds;
        Duration += delta;

        if (tempSecondCounter <= 1)
        {
            tempSecondCounter += Delta;
            tempFps++;
        }
        else
        {
            FramesPerSecond = tempFps;
            tempSecondCounter = 0;
            tempFps = 0;
        }
    }

    public static bool OnInterval(double time, double delta, double interval, double offset)
    {
        return Math.Floor((time - offset - delta) / interval) < Math.Floor((time - offset) / interval);
    }

    public static bool OnInterval(double delta, double interval, double offset)
    {
        return OnInterval(Duration.TotalSeconds, delta, interval, offset);
    }

    public static bool OnInterval(double interval, double offset = 0.0)
    {
        return OnInterval(Duration.TotalSeconds, Delta, interval, offset);
    }
}