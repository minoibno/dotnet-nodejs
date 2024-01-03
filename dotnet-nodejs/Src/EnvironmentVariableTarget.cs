namespace Minoibno.Dotnet.NodeJs;

public enum EnvironmentVariableTarget { User, Machine }

public static class ScopeExtensions {
    public static System.EnvironmentVariableTarget ToSystemEnvironmentVariableTarget(this EnvironmentVariableTarget environmentVariableTarget) =>
        environmentVariableTarget switch {
            EnvironmentVariableTarget.User => System.EnvironmentVariableTarget.User,
            EnvironmentVariableTarget.Machine => System.EnvironmentVariableTarget.Machine,
            _ => throw new ArgumentOutOfRangeException(nameof(environmentVariableTarget), environmentVariableTarget, null)
        };
}
