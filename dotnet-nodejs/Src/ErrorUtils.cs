namespace Minoibno.Dotnet.NodeJs;

public static class ErrorUtils {
    public static void WriteErrorAndSetExitCode(string message, int exitCode = -1) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(message);
        Console.ResetColor();
        RootCommand.ExitCode = exitCode;
    }
}
