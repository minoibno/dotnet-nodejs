using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Minoibno.Dotnet.NodeJs.Publisher;

[PublicAPI]
internal record NodeVersionResource(
    [property: JsonPropertyName("version")]
    string Version,
    [property: JsonPropertyName("files")] string[] Files,
    [property: JsonPropertyName("npm")] string Npm
);
