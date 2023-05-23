using Microsoft.Extensions.Logging;

namespace Amplitude.Net;

public class AmplitudeForDevice: AmplitudeFor
{
    public AmplitudeForDevice(ILogger logger, IAmplitudeSender amplitudeSender, string deviceId) : base(logger, amplitudeSender, new [] {new KeyValuePair<string, object?>("device_id", deviceId) })
    {
    }
}