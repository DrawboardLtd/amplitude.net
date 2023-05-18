using Microsoft.Extensions.Logging;

namespace Amplitude.Net;

public class AmplitudeForUser: AmplitudeFor
{
    public AmplitudeForUser(ILogger logger, IAmplitudeSender amplitudeSender, string userId) : base(logger, amplitudeSender, new KeyValuePair<string, object>("user_id", userId))
    {
    }
}