namespace Amplitude.Net;

public record TransactionInfo(Optional<decimal> Price = default, Optional<int> Quantity = default, Optional<decimal> Revenue = default, Optional<string> ProductId = default, Optional<string> RevenueType = default);