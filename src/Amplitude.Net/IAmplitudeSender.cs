using Microsoft.Extensions.Logging;
namespace Amplitude.Net;

#if (NETSTANDARD2_0)
using ReturnType = Task;
#else
using ReturnType = ValueTask;
#endif

public interface IAmplitudeSender
{
    ReturnType Identify(IDictionary<string, object?> payload, ILogger logger);
    ReturnType Event(IDictionary<string, object?> payload, ILogger logger);
}