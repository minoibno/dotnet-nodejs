using JetBrains.Annotations;

namespace Minoibno.Dotnet.NodeJs.Publisher;

[PublicAPI]
public sealed class PublisherOptions {
    public const string ConfigSection = "Minoibno:Dotnet:NodeJs:Publisher";

    public string NuGetApiKey { get; set; } = default!;
}
