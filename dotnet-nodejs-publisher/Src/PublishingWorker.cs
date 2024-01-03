using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Minoibno.Dotnet.NodeJs.Publisher;

public sealed class PublishingWorker : BackgroundService {
    private readonly ILogger<PublishingWorker> log;
    private readonly HttpClient httpClient;
    private readonly NugetLogger nugetLogger;
    private readonly IHostApplicationLifetime hostApplicationLifetime;
    private readonly PublisherOptions options;

    public PublishingWorker(
        HttpClient httpClient,
        ILogger<PublishingWorker> log,
        NugetLogger nugetLogger,
        IHostApplicationLifetime hostApplicationLifetime,
        IOptions<PublisherOptions> options
    ) {
        this.httpClient = httpClient;
        this.log = log;
        this.nugetLogger = nugetLogger;
        this.hostApplicationLifetime = hostApplicationLifetime;
        this.options = options.Value;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
        long startTime = Stopwatch.GetTimestamp();

        log.LogInformation("Starting Publishing Worker");

        try {
            List<NodeVersionResource>? nodeJsVersions =
                await httpClient.GetFromJsonAsync<List<NodeVersionResource>>("https://nodejs.org/dist/index.json", cancellationToken);

            if (nodeJsVersions == default)
                throw new InvalidOperationException("Versions is null");

            NuGetVersion minimumNodeJsVersion = NuGetVersion.Parse("18.0.0");

            SourceCacheContext cache = new();
            SourceRepository? repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            FindPackageByIdResource? resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

            HashSet<NuGetVersion> nugetVersions = (await resource.GetAllVersionsAsync("dotnet-nodejs", cache, nugetLogger, default)).ToHashSet();

            await BuildAsync(cancellationToken);

            foreach ((NuGetVersion, NodeVersionResource val) version in nodeJsVersions
                         .Select(val => (NuGetVersion.Parse(val.Version.TrimStart('v')), val))
                         .Where(val => val.Item1 >= minimumNodeJsVersion && !nugetVersions.Contains(val.Item1))
                         .OrderBy(val => val.Item1)) {
                log.LogInformation("Processing NodeJs<{Version}>", version.val.Version);
                cancellationToken.ThrowIfCancellationRequested();

                if (!version.val.Files.Contains("win-x64-zip", StringComparer.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"File<win-x64-zip> is missing in Version<{version.val.Version}>");

                if (!version.val.Files.Contains("linux-x64", StringComparer.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"File<linux-x64> is missing in Version<{version.val.Version}>");

                try {
                    Directory.CreateDirectory(@"..\package\nodejs");

                    await Parallel.ForEachAsync<Func<CancellationToken, ValueTask>>(
                        [
                            async cancellationToken => {
                                HttpResponseMessage win64Response = await httpClient.GetAsync(
                                    $"https://nodejs.org/dist/{version.val.Version}/node-{version.val.Version}-win-x64.zip",
                                    cancellationToken
                                );

                                win64Response.EnsureSuccessStatusCode();

                                await using FileStream winFileStream = File.Open(
                                    @"..\package\nodejs\win-x64.zip",
                                    FileMode.CreateNew,
                                    FileAccess.Write
                                );

                                await using Stream httpStream = await win64Response.Content.ReadAsStreamAsync(cancellationToken);

                                await httpStream.CopyToAsync(winFileStream, cancellationToken);
                            },
                            async cancellationToken => {
                                HttpResponseMessage linux64Response = await httpClient.GetAsync(
                                    $"https://nodejs.org/dist/{version.val.Version}/node-{version.val.Version}-linux-x64.tar.gz",
                                    cancellationToken
                                );

                                linux64Response.EnsureSuccessStatusCode();

                                await using FileStream linuxFileStream = File.Open(
                                    @"..\package\nodejs\linux-x64.tar.gz",
                                    FileMode.CreateNew,
                                    FileAccess.Write
                                );

                                await using Stream httpStream = await linux64Response.Content.ReadAsStreamAsync(cancellationToken);

                                await httpStream.CopyToAsync(linuxFileStream, cancellationToken);
                            }
                        ],
                        new ParallelOptions { MaxDegreeOfParallelism = 2, CancellationToken = cancellationToken },
                        (func, cancellationToken) => func(cancellationToken)
                    );

                    await Parallel.ForEachAsync<Func<CancellationToken, ValueTask>>(
                        [
                            async cancellationToken => { await PackAsync("dotnet-nodejs", version.Item1.ToString(), cancellationToken); },
                            async cancellationToken => { await PackAsync("dotnet-nodejs-linux", version.Item1.ToString(), cancellationToken); },
                            async cancellationToken => { await PackAsync("dotnet-nodejs-win", version.Item1.ToString(), cancellationToken); }
                        ],
                        new ParallelOptions { MaxDegreeOfParallelism = 3, CancellationToken = cancellationToken },
                        async (func, cancellationToken) => await func(cancellationToken)
                    );

                    //Publish
                } finally {
                    if (Directory.Exists(@"..\package\nodejs"))
                        Directory.Delete(@"..\package\nodejs", true);
                }

                break;
            }

            PackageUpdateResource packageUpdateResource = await repository.GetResourceAsync<PackageUpdateResource>(cancellationToken);

            await packageUpdateResource.Push(
                Directory.GetFiles(@"..\packages\"),
                null,
                5 * 60,
                false,
                _ => options.NuGetApiKey,
                _ => null,
                false,
                true,
                null,
                nugetLogger
            );
        } catch (ObjectDisposedException) {
            if (!cancellationToken.IsCancellationRequested)
                throw;
            //ignore object disposed exceptions if cancellation is requested
        } catch (Exception e) {
            Environment.ExitCode = -1;

            log.LogError(e, "Exception during execution of Worker");

            log.LogInformation("Worker finished after {Elapsed}", Stopwatch.GetElapsedTime(startTime));

            throw;
        }

        log.LogInformation("Worker finished after {Elapsed}", Stopwatch.GetElapsedTime(startTime));
        hostApplicationLifetime.StopApplication();
    }

    private static async Task BuildAsync(CancellationToken cancellationToken) {
        Process? build = Process.Start(new ProcessStartInfo("dotnet.exe", "build -c Release") { WorkingDirectory = @"..\" });

        if (build == null)
            throw new InvalidOperationException("Could not start process");

        await build.WaitForExitAsync(cancellationToken);

        if (build.ExitCode != 0)
            throw new InvalidOperationException("dotnet build resulted in exit code: " + build.ExitCode);
    }

    private static async Task PackAsync(string project, string version, CancellationToken cancellationToken) {
        Process? pack = Process.Start(
            new ProcessStartInfo("dotnet.exe", @$"pack --no-build --p:Version={version} --output ..\packages") { WorkingDirectory = $@"..\{project}" }
        );

        if (pack == null)
            throw new InvalidOperationException("Could not start process");

        await pack.WaitForExitAsync(cancellationToken);

        if (pack.ExitCode != 0)
            throw new InvalidOperationException("dotnet pack resulted in exit code: " + pack.ExitCode);
    }
}
