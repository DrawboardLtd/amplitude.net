namespace Amplitude.Net;

public interface IAmplitude
{
    IAmplitudeForTarget ForUserId(string userId);
    IAmplitudeForTarget ForDeviceId(string deviceId);
}