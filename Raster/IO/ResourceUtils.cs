using System.Reflection;

namespace Raster.IO;

public static class ResourceUtils
{
    public static Stream OpenResourceFromAssembly(string resourceName, Assembly assembly)
    {
        var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.{resourceName.Replace('/', '.')}");
        if (stream == null)
            throw new InvalidOperationException($"Failed to open resource '{resourceName}' from assembly '{assembly.GetName().Name}'.");
        return stream;
    }
}
