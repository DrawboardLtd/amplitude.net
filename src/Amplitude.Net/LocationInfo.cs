namespace Amplitude.Net;

public record LocationInfo(Optional<string?> Country = default, Optional<string?> Region = default, Optional<string?> City = default, Optional<string?> DMA = default);