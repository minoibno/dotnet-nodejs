using System.CommandLine;
using System.Formats.Tar;
using System.IO.Compression;
using System.Security;

namespace Minoibno.Dotnet.NodeJs;

internal static class InitCommand {
    private static readonly Option<EnvironmentVariableTarget> target = new(
        ["-t", "--target"],
        () => EnvironmentVariableTarget.User,
        "The environment variable target to adjust"
    );

    internal static readonly Command Command =
        new("init", "Unpacks the NodeJs binaries and adds the location to the PATH environment variable") { target };

    static InitCommand() {
        Command.SetHandler(
            async target => {
                WarnUserDirectoryIfNecessary(target);

                await UnpackFileIfNecessaryAsync();

                RuntimeUtils.CheckArchitecture();

                RuntimeUtils.ExecuteEnvironmentDependent(
                    () => {
                        HashSet<string> currentPath =
                            (Environment.GetEnvironmentVariable("PATH", target.ToSystemEnvironmentVariableTarget()) ?? string.Empty)
                            .Split(NodeJsPathUtils.EnvVariableSeparator)
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

                        if (currentPath.Contains(NodeJsPathUtils.NodeJsBinaryDirectory)) {
                            Console.WriteLine($"'{NodeJsPathUtils.NodeJsBinaryDirectory}' was already added to PATH environment variable");
                            return;
                        }

                        try {
                            Environment.SetEnvironmentVariable(
                                "PATH",
                                string.Join(NodeJsPathUtils.EnvVariableSeparator, currentPath.Concat([NodeJsPathUtils.NodeJsBinaryDirectory])),
                                target.ToSystemEnvironmentVariableTarget()
                            );
                        } catch (SecurityException) when (target == EnvironmentVariableTarget.Machine) {
                            ErrorUtils.WriteErrorAndSetExitCode("Access denied! Cannot write into environment variables of machine");
                            return;
                        }

                        Console.WriteLine($"'{NodeJsPathUtils.NodeJsBinaryDirectory}' successfully added to PATH environment variable");
                    },
                    () => {
                        if (target == EnvironmentVariableTarget.User)
                            Console.WriteLine(
                                "Add the following line to the startup file for your user of your favorite shell (e.g. ~/.profile or ~/.bashrc):\n\n" +
                                $"PATH=\"$PATH{NodeJsPathUtils.EnvVariableSeparator}{NodeJsPathUtils.NodeJsBinaryDirectory}\""
                            );
                        else
                            Console.WriteLine(
                                "Add the following line to the startup file for your machine of your favorite shell (e.g. /etc/profile):\n\n" +
                                $"PATH=\"$PATH{NodeJsPathUtils.EnvVariableSeparator}{NodeJsPathUtils.NodeJsBinaryDirectory}\""
                            );
                    }
                );
            },
            target
        );
    }

    private static void WarnUserDirectoryIfNecessary(EnvironmentVariableTarget target) {
        if (target == EnvironmentVariableTarget.Machine &&
            Path.GetFullPath(NodeJsPathUtils.NodeJsBaseDirectory.Replace('/', '\\'))
                .StartsWith(
                    Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace('/', '\\')).TrimEnd('\\'),
                    StringComparison.OrdinalIgnoreCase
                )) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                "WARNING: The tool seems to be installed inside a user directory, which may not be desired when NodeJs is added to the PATH variable on machine level"
            );

            Console.ResetColor();
        }
    }

    private static async Task UnpackFileIfNecessaryAsync() {
        if (!Directory.Exists(NodeJsPathUtils.NodeJsPackageDirectory) ||
            !Directory.EnumerateFileSystemEntries(NodeJsPathUtils.NodeJsPackageDirectory).Any()) {
            Console.WriteLine($"Extracting NodeJs to '{NodeJsPathUtils.NodeJsPackageDirectory}'");

            string tmpPath = Path.Combine(NodeJsPathUtils.NodeJsBaseDirectory, "tmp");

            try {
                Directory.CreateDirectory(tmpPath);

                if (NodeJsPathUtils.NodeJsPackageFile.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) {
                    ZipFile.ExtractToDirectory(NodeJsPathUtils.NodeJsPackageFile, tmpPath);
                } else if (NodeJsPathUtils.NodeJsPackageFile.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase)) {
                    await using FileStream fileStream = File.Open(NodeJsPathUtils.NodeJsPackageFile, FileMode.Open, FileAccess.Read, FileShare.Read);

                    await using GZipStream gzipStream = new(fileStream, CompressionMode.Decompress);

                    await TarFile.ExtractToDirectoryAsync(gzipStream, tmpPath, false);
                } else {
                    throw new ExitSilentlyException($"Unpacking of File<{NodeJsPathUtils.NodeJsPackageFile}> is not supported");
                }

                string nodeDirectory = Directory.EnumerateDirectories(tmpPath).Single();

                Directory.Move(nodeDirectory, NodeJsPathUtils.NodeJsPackageDirectory);
            } finally {
                Directory.Delete(tmpPath);
            }
        }
    }
}
