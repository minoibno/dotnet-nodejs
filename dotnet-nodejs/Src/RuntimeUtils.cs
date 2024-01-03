using System.Runtime.InteropServices;

namespace Minoibno.Dotnet.NodeJs;

public static class RuntimeUtils {
    public static void CheckArchitecture() {
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new ExitSilentlyException($"ProcessArchitecture<{RuntimeInformation.ProcessArchitecture}> is not supported");
    }

    public static T ExecuteEnvironmentDependent<T>(Func<T> windowsHandler, Func<T> linuxHandler) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return windowsHandler();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return linuxHandler();

        throw new ExitSilentlyException($"Platform<{RuntimeInformation.RuntimeIdentifier}> is not supported");
    }

    public static void ExecuteEnvironmentDependent(Action windowsHandler, Action linuxHandler) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            windowsHandler();
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            linuxHandler();
        else
            throw new ExitSilentlyException($"Platform<{RuntimeInformation.RuntimeIdentifier}> is not supported");
    }
}
