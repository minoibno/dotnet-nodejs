namespace Minoibno.Dotnet.NodeJs;

internal static class NodeJsPathUtils {
    public static readonly string NodeJsBaseDirectory;
    public static readonly string NodeJsPackageFile;
    public static readonly string NodeJsPackageDirectory;
    public static readonly string NodeJsBinaryDirectory;
    public static readonly char EnvVariableSeparator;

    static NodeJsPathUtils() {
#if DEBUG
        NodeJsBaseDirectory = Path.Combine(new DirectoryInfo(AppContext.BaseDirectory).Parent!.Parent!.Parent!.FullName, "nodejs");
#else
        NodeJsBaseDirectory = Path.Combine(new DirectoryInfo(AppContext.BaseDirectory).Parent!.Parent!.Parent!.FullName, "content", "nodejs");
#endif

        (NodeJsPackageFile, NodeJsPackageDirectory, NodeJsBinaryDirectory, EnvVariableSeparator) = RuntimeUtils.ExecuteEnvironmentDependent(
            () => (Path.Combine(NodeJsBaseDirectory, "win-x64.zip"), Path.Combine(NodeJsBaseDirectory, "win-x64"),
                Path.Combine(NodeJsBaseDirectory, "win-x64"), ';'),
            () => (Path.Combine(NodeJsBaseDirectory, "linux-x64.tar.gz"), Path.Combine(NodeJsBaseDirectory, "linux-x64"),
                Path.Combine(NodeJsBaseDirectory, "linux-x64", "bin"), ':')
        );
    }
}
