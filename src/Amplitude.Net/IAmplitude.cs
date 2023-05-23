namespace Amplitude.Net;

public interface IAmplitude
{
    IAmplitudeForTarget ForUserId(string userId, string? deviceId = default);
    IAmplitudeForTarget ForDeviceId(string deviceId);
}