using System.Collections.Concurrent;
using System.Net;
using System.Text;
#if NETSTANDARD2_0
using Newtonsoft.Json;
#else
using System.Text.Json;
#endif
using System.Timers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace Amplitude.Net;

#if (NETSTANDARD2_0)
using TaskType = Task;
#else
using TaskType = ValueTask;
#endif

/// <summary>
/// Asynchronously delivers requests to Amplitude. Designed to be a singleton.
/// </summary>
public class AsyncAmplitudeSender : IAmplitudeSender, 
#if NETSTANDARD2_0
    IDisposable
    #else
    IAsyncDisposable
    #endif
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly ILogger<AsyncAmplitudeSender> _logger;
    private readonly ConcurrentQueue<object> _identifyQueue, _eventQueue;
    private readonly Timer _identifyTimer, _eventTimer;
    private readonly SemaphoreSlim _identifySemaphore, _eventSemaphore;

    public AsyncAmplitudeSender(IHttpClientFactory httpClientFactory, string apiKey, ILogger<AsyncAmplitudeSender> logger)
    {
        _httpClientFactory = httpClientFactory;
        _apiKey = apiKey;
        _logger = logger;
        _identifyQueue = new ConcurrentQueue<object>();
        _eventQueue = new ConcurrentQueue<object>();
        
#if NET70
        _identifyTimer = new Timer(TimeSpan.FromMilliseconds(50));
        _eventTimer = new Timer(TimeSpan.FromMilliseconds(50));
#else
        _identifyTimer = new Timer(TimeSpan.FromMilliseconds(50).TotalMilliseconds);
        _eventTimer = new Timer(TimeSpan.FromMilliseconds(50).TotalMilliseconds);
#endif

        _identifyTimer.AutoReset = true;
        _identifyTimer.Elapsed += IdentifyTimerOnElapsed;
        _identifyTimer.Start();

        _identifySemaphore = new SemaphoreSlim(2);
        
        _eventTimer.AutoReset = true;
        _eventTimer.Elapsed += EventTimerOnElapsed;
        _eventTimer.Start();

        _eventSemaphore = new SemaphoreSlim(2);
    }
    
    public AsyncAmplitudeSender(IHttpClientFactory httpClientFactory, IOptions<AmplitudeOptions> options,  ILogger<AsyncAmplitudeSender> logger)
        :this(httpClientFactory, options?.Value?.ApiKey ?? throw new ArgumentException("No api key provided."), logger)
    {
    }

    private async void IdentifyTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        await ProcessIdentifyQueue();
    }
    
    private async void EventTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        await ProcessEventQueue();
    }

    private async Task ProcessIdentifyQueue()
    {
        if (_identifyQueue.IsEmpty) return;
        if (!await _identifySemaphore.WaitAsync(TimeSpan.FromMilliseconds(50))) return;
        try
        {
            const int max = 20;
            var counter = 0;
            var payloads = new List<object>();

            while (counter++ < max)
            {
                if (!_identifyQueue.TryDequeue(out var payload)) break;
                payloads.Add(payload);
            }

            if (payloads.Count == 0) return;
            
            _logger.LogTrace("Sending Identify request");
            await SendIdentifyRequest(payloads.ToArray());
            _logger.LogTrace("Identify request sent");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error delivering amplitude identify request");
        }
        finally
        {
            _identifySemaphore.Release();
        }
    }
    
    private async Task ProcessEventQueue()
    {
        if (_eventQueue.IsEmpty) return;
        if (!await _eventSemaphore.WaitAsync(TimeSpan.FromMilliseconds(50))) return;
            
        try
        {
            const int max = 20;
            var counter = 0;
            var payloads = new List<object>();

            while (counter++ < max)
            {
                if (!_eventQueue.TryDequeue(out var payload)) break;
                payloads.Add(payload);
            }

            if (payloads.Count == 0) return;
            
            _logger.LogTrace("Sending Event request");
            await SendEventRequest(payloads.ToArray());
            _logger.LogTrace("Event request sent");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error delivering amplitude event request");
        }
        finally
        {
            _eventSemaphore.Release();
        }
    }

    private async Task SendIdentifyRequest(object[] payloads)
    {
        const string url = "https://api2.amplitude.com/identify";
#if NETSTANDARD2_0
        var identificationBody = JsonConvert.SerializeObject(payloads);
#else
        var identificationBody = JsonSerializer.Serialize(payloads);
#endif
        
        var requestBody = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            new("api_key", _apiKey),
            new("identification", identificationBody)
        });

        var count = 0;
        var delay = TimeSpan.FromSeconds(15);
        const int MaxRetries = 5;
        using var httpClient = _httpClientFactory.CreateClient();

        while (count++ < MaxRetries)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Content = requestBody;

            var response = await httpClient.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                return;
            }
            
            if((int)response.StatusCode == 429)//too many requests
            {
                await Task.Delay(delay);
            }
            else
            {
                response.EnsureSuccessStatusCode();
            }
        }
    }
    
    private async Task SendEventRequest(object[] payloads)
    {
        const string url = "https://api2.amplitude.com/2/httpapi";
        
        var payload = new
        {
            api_key = _apiKey,
            events = payloads
        };
#if NETSTANDARD2_0
        var eventsBody = JsonConvert.SerializeObject(payload);
#else
        var eventsBody = JsonSerializer.Serialize(payload);
#endif

        var requestContent = new StringContent(eventsBody, Encoding.UTF8, "application/json");
        
        var count = 0;
        var delay = TimeSpan.FromSeconds(15);
        const int MaxRetries = 5;
        using var httpClient = _httpClientFactory.CreateClient();

        while (count++ < MaxRetries)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Content = requestContent;

            var response = await httpClient.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                return;
            }
            
            if((int)response.StatusCode == 429)//too many requests
            {
                await Task.Delay(delay);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                
                response.EnsureSuccessStatusCode();
            }
        }
    }

    public TaskType Identify(IDictionary<string, object?> payload)
    {
#if NETSTANDARD2_0
        var identificationBody = JsonConvert.SerializeObject(payload);
#else
        var identificationBody = JsonSerializer.Serialize(payload);
#endif
        
        _identifyQueue.Enqueue(payload);
        
        return TaskType.CompletedTask;
    }
    
    public TaskType Event(IDictionary<string, object?> payload)
    {
#if NETSTANDARD2_0
        var eventBody = JsonConvert.SerializeObject(new [] {payload});
#else
        var eventBody = JsonSerializer.Serialize(new [] {payload});
#endif
        
        _eventQueue.Enqueue(payload);
        
        return TaskType.CompletedTask;
    }

#if NETSTANDARD2_0
    public void Dispose()
    {
        _identifyTimer.Dispose();
        _identifySemaphore.Dispose();
        
        _eventTimer.Dispose();
        _eventSemaphore.Dispose();
        
        while (!_identifyQueue.IsEmpty)
        {
            Task.WaitAll(ProcessIdentifyQueue());
        }

        while (!_eventQueue.IsEmpty)
        {
            Task.WaitAll(ProcessEventQueue());
        }
    }
#else
    public async ValueTask DisposeAsync()
    {
        _identifyTimer.Dispose();
        _identifySemaphore.Dispose();
        
        _eventTimer.Dispose();
        _eventSemaphore.Dispose();

        while (!_identifyQueue.IsEmpty)
        {
            await ProcessIdentifyQueue();
        }
        
        while (!_eventQueue.IsEmpty)
        {
            await ProcessEventQueue();
        }
    }
#endif
}