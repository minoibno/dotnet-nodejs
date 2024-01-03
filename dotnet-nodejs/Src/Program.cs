using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Minoibno.Dotnet.NodeJs;

return await RootCommand.InvokeAsync(args);

public static class RootCommand {
    public static int ExitCode = 0;

    public static async Task<int> InvokeAsync(string[] args) {
        System.CommandLine.RootCommand rootCommand = new("Configure NuGet based NodeJs setup");
        rootCommand.AddCommand(InitCommand.Command);
        rootCommand.AddCommand(GetLocationCommand.Command);
        rootCommand.AddCommand(ResetCommand.Command);

        await new CommandLineBuilder(rootCommand).AddMiddleware(
                async (context, next) => {
                    try {
                        await next(context);
                    } catch (ExitSilentlyException e) {
                        ErrorUtils.WriteErrorAndSetExitCode(e.Message);
                    }
                }
            )
            .UseDefaults()
            .Build()
            .InvokeAsync(args);

        return ExitCode;
    }
}

public sealed class ExitSilentlyException : InvalidOperationException {
    /// <inheritdoc />
    public ExitSilentlyException(string message) : base(message) { }
}
