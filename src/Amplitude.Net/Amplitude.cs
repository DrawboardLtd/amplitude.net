using Microsoft.Extensions.Logging;

namespace Amplitude.Net;

/// <summary>
/// Handle for sending requests to Amplitude. Should be a scoped service.
/// </summary>
public class Amplitude : IAmplitude
{
    private readonly ILogger<Amplitude> _logger;
    private readonly IAmplitudeSender _sender;

    public Amplitude(ILogger<Amplitude> logger, IAmplitudeSender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    public IAmplitudeForTarget ForUserId(string userId, string? deviceId = default)
    {
        return new AmplitudeForUser(_logger, _sender, userId, deviceId);
    }

    public IAmplitudeForTarget ForDeviceId(string deviceId)
    {
        return new AmplitudeForDevice(_logger, _sender, deviceId);
    }
}