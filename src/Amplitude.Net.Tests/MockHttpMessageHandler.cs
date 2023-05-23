using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Amplitude.Net.Tests;

public class MockHttpMessageHandler : HttpClientHandler
{
    private readonly Counters _counters;
    private Exception? _exception;

    public MockHttpMessageHandler(Counters counters)
    {
        _counters = counters;
    }

    public Exception? Exception => _exception;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _counters.Calls);
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                Interlocked.Increment(ref _counters.Successes);
            }

            return response;
        }
        catch (Exception ex)
        {
            _exception = ex;
            throw;
        }
    }
}