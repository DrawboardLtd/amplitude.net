using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Amplitude.Net;

public static class ServiceCollectionExtensions
{
    public static void AddAmplitude(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<AmplitudeOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                var ampConfigSection = configuration.GetSection(AmplitudeOptions.Section);
                var ampOptions = ampConfigSection.Get<AmplitudeOptions>();
                if (ampOptions == null)
                {
                    throw new InvalidOperationException(
                        $"Amplitude configuration not found. You must provide a \"{AmplitudeOptions.Section}\" configuration section.");
                }

                options.ApiKey = ampOptions.ApiKey;
            });

        serviceCollection.AddSingleton<IAmplitudeSender, AsyncAmplitudeSender>();
        serviceCollection.AddScoped<IAmplitude, Amplitude>();
    }
}