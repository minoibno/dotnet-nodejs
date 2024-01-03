using System.CommandLine;

namespace Minoibno.Dotnet.NodeJs;

internal static class GetLocationCommand {
    internal static readonly Command Command = new(
        "get-location",
        "Prints the location of the NodeJs binaries that should be added to the PATH environment variable"
    );

    static GetLocationCommand() {
        Command.SetHandler(
            () => {
                if (!Directory.Exists(NodeJsPathUtils.NodeJsBinaryDirectory)) {
                    ErrorUtils.WriteErrorAndSetExitCode("Binaries are not yet unpacked! Execute the 'init' command first");
                    return;
                }

                Console.WriteLine(NodeJsPathUtils.NodeJsBinaryDirectory);
            }
        );
    }
}
