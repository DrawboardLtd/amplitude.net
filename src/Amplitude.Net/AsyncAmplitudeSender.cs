using System.Collections.Concurrent;
using System.Text.Json;
using System.Timers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace Amplitude.Net;

/// <summary>
/// Asynchronously delivers requests to Amplitude. Designed to be a singleton.
/// </summary>
public class AsyncAmplitudeSender : IAmplitudeSender, IAsyncDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly ConcurrentQueue<(HttpRequestMessage Request, ILogger Logger)> _queue;
    private readonly Timer _timer;
    private readonly SemaphoreSlim _concurrencySemaphore;

    public AsyncAmplitudeSender(IHttpClientFactory httpClientFactory, string apiKey)
    {
        _httpClientFactory = httpClientFactory;
        _apiKey = apiKey;
        _queue = new ConcurrentQueue<(HttpRequestMessage, ILogger)>();
        _timer = new Timer(TimeSpan.FromMilliseconds(50));
        _timer.AutoReset = true;
        _timer.Elapsed += TimerOnElapsed;
        _timer.Start();
        _concurrencySemaphore = new SemaphoreSlim(10);
    }
    
    public AsyncAmplitudeSender(IHttpClientFactory httpClientFactory, IOptions<AmplitudeOptions> options)
    :this(httpClientFactory, options?.Value?.ApiKey ?? throw new ArgumentException("No api key provided."))
    {
    }

    private async void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_queue.IsEmpty) return;
        
        await _concurrencySemaphore.WaitAsync();
        try
        {
            if (_queue.TryDequeue(out var request))
            {
                try
                {
                    request.Logger.LogTrace("Sending Amplitude request");
                    await SendRequest(request.Request);
                    request.Logger.LogTrace("Amplitude request sent");
                }
                catch(Exception ex)
                {
                    request.Logger.LogError(ex, "Error delivering amplitude request");
                }
            }            
        }
        finally
        {
            _concurrencySemaphore.Release();
        }
    }

    private async Task SendRequest(HttpRequestMessage requestMessage)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();
    }

    public ValueTask Identify(IDictionary<string, object?> payload, ILogger logger)
    {
        const string url = "https://api2.amplitude.com/identify";
        var identificationBody = JsonSerializer.Serialize(payload);
        var requestBody = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            new("api_key", _apiKey),
            new("identification", identificationBody)
        });
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Content = requestBody;
        
        _queue.Enqueue((httpRequest, logger));
        
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _timer.Dispose();
        _concurrencySemaphore.Dispose();

        while (!_queue.IsEmpty)
        {
            if (_queue.TryDequeue(out var request))
            {
                await SendRequest(request.Request);
            }
        }
    }
}