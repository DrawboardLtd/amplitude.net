using Microsoft.Extensions.Logging;

namespace Amplitude.Net;

public interface IAmplitudeSender
{
    ValueTask Identify(IDictionary<string, object?> payload, ILogger logger);
}