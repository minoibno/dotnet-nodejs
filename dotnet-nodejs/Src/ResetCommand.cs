using System.CommandLine;

namespace Minoibno.Dotnet.NodeJs;

internal static class ResetCommand {
    private static readonly Option<EnvironmentVariableTarget> target = new(
        ["-t", "--target"],
        () => EnvironmentVariableTarget.User,
        "The environment variable target to adjust"
    );

    internal static readonly Command Command =
        new("reset", "Deletes the NodeJs binaries and removes the location from the PATH environment variable") { target };

    static ResetCommand() {
        Command.SetHandler(
            target => {
                RuntimeUtils.ExecuteEnvironmentDependent(
                    () => {
                        string[] currentPath =
                            (Environment.GetEnvironmentVariable("PATH", target.ToSystemEnvironmentVariableTarget()) ?? string.Empty).Split(
                                NodeJsPathUtils.EnvVariableSeparator
                            );

                        if (!currentPath.Contains(NodeJsPathUtils.NodeJsBinaryDirectory, StringComparer.OrdinalIgnoreCase)) {
                            Console.WriteLine($"'{NodeJsPathUtils.NodeJsBinaryDirectory}' is not contained in PATH environment variable");
                            return;
                        }

                        Environment.SetEnvironmentVariable(
                            "PATH",
                            string.Join(
                                NodeJsPathUtils.EnvVariableSeparator,
                                currentPath.Where(val => !val.Equals(NodeJsPathUtils.NodeJsBinaryDirectory, StringComparison.OrdinalIgnoreCase))
                            ),
                            target.ToSystemEnvironmentVariableTarget()
                        );

                        Console.WriteLine($"'{NodeJsPathUtils.NodeJsBinaryDirectory}' successfully removed from PATH environment variable");

                        if (!Directory.Exists(NodeJsPathUtils.NodeJsPackageDirectory))
                            return;

                        Directory.Delete(NodeJsPathUtils.NodeJsPackageDirectory, true);
                        Console.WriteLine($"'{NodeJsPathUtils.NodeJsPackageDirectory}' deleted");
                    },
                    () => {
                        if (Directory.Exists(NodeJsPathUtils.NodeJsPackageDirectory)) {
                            Directory.Delete(NodeJsPathUtils.NodeJsPackageDirectory, true);
                            Console.WriteLine($"'{NodeJsPathUtils.NodeJsPackageDirectory}' deleted");
                        }

                        Console.WriteLine(
                            "Cleanup your environment variable configuration files and remove the reference to the following location inside the PATH variable:\n\n" +
                            NodeJsPathUtils.NodeJsBinaryDirectory
                        );
                    }
                );
            },
            target
        );
    }
}
