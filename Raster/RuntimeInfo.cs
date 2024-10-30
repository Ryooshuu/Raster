namespace Raster;

public static class RuntimeInfo
{
    public static Platform OS { get; }

    public static bool IsUnix => OS != Platform.Windows;
    public static bool IsDesktop => OS is Platform.Linux or Platform.MacOS or Platform.Windows;
    public static bool IsMobile => OS is Platform.IOS or Platform.Android;
    public static bool IsApple => OS is Platform.IOS or Platform.MacOS;

    static RuntimeInfo()
    {
        if (OperatingSystem.IsWindows())
            OS = Platform.Windows;
        if (OperatingSystem.IsMacOS())
            OS = Platform.MacOS;
        if (OperatingSystem.IsLinux())
            OS = Platform.Linux;
        if (OperatingSystem.IsIOS())
            OS = Platform.IOS;
        if (OperatingSystem.IsAndroid())
            OS = Platform.Android;
    }

    public enum Platform
    {
        Windows = 1,
        Linux = 2,
        MacOS = 3,
        IOS = 4,
        Android = 5,
    }
}