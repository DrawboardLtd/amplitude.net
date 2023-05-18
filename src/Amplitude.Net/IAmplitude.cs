namespace Amplitude.Net;

public interface IAmplitude
{
    const string UNSET = "UNSET";

    IAmplitudeForTarget ForUserId(string userId);
    IAmplitudeForTarget ForDeviceId(string deviceId);
}