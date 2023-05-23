using Microsoft.Extensions.Logging;

namespace Amplitude.Net;

public class AmplitudeForUser: AmplitudeFor
{
    public AmplitudeForUser(ILogger logger, IAmplitudeSender amplitudeSender, string userId, string? deviceId) : base(logger, amplitudeSender, 
        new [] {new KeyValuePair<string, object?>("user_id", userId), new KeyValuePair<string, object?>("device_id", deviceId)}
        )
    {
    }
}