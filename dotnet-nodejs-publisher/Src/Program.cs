using Microsoft.Extensions.Logging.Console;
using Minoibno.Dotnet.NodeJs.Publisher;

await new HostBuilder().ConfigureDefaults(args)
    .ConfigureLogging(
        builder => builder.ClearProviders()
            .AddConsoleFormatter<PublishingConsoleFormatter, ConsoleFormatterOptions>()
            .AddConsole(config => { config.FormatterName = PublishingConsoleFormatter.FormatterName; })
            .AddFilter("Microsoft.Extensions.Hosting.Internal.Host", LogLevel.Information)
            .SetMinimumLevel(LogLevel.Information)
    )
    .ConfigureServices(
        services => services.AddHostedService<PublishingWorker>()
            .AddHttpClient()
            .AddSingleton<NugetLogger>()
            .AddOptions<PublisherOptions>()
            .BindConfiguration(PublisherOptions.ConfigSection)
            .Validate(
                (PublisherOptions options, ILogger<PublishingWorker> log) => {
                    if (string.IsNullOrWhiteSpace(options.NuGetApiKey)) {
                        log.LogError($"{PublisherOptions.ConfigSection}:{nameof(PublisherOptions.NuGetApiKey)} is is null or empty");
                        return false;
                    }

                    return true;
                }
            )
    )
    .ConfigureHostOptions(options => options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost)
    .UseConsoleLifetime(options => options.SuppressStatusMessages = true)
    .Build()
    .RunAsync();
