namespace Amplitude.Net;

public record AmplitudeOptions
{
    public const string Section = "Amplitude";
    public string? ApiKey { get; set; }
}