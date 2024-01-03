using System.Runtime.InteropServices;
using Minoibno.Dotnet.NodeJs;

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
    ErrorUtils.WriteErrorAndSetExitCode($"Platform<{RuntimeInformation.RuntimeIdentifier}> is not supported");
    return RootCommand.ExitCode;
}

return await RootCommand.InvokeAsync(args);
