using System.Net.Http.Headers;
using Application.Configurations;
using Application.GigaChatModels;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Application.Services.Implementations;

public class GigaChatAiAssistant : IAiAssistant
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1);
    private int _queueCount = 0;
    private string _accessToken = "";
    private DateTime _tokenExpireTime = DateTime.Now;
    public int QueueCount => _queueCount;
    private readonly IOptions<GigaChatData> _options;
    private readonly ILogger<GigaChatAiAssistant> _logger;


    public GigaChatAiAssistant(IOptions<GigaChatData> options, ILogger<GigaChatAiAssistant> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
        _logger = logger;
    }

    private async Task GetAccess(HttpClient httpClient, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Value.AuthUrl);
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("RqUID", new Guid().ToString());
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _options.Value.Base64AuthData);
        var content = new StringContent("scope=GIGACHAT_API_PERS");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        request.Content = content;
        using var response = await httpClient.SendAsync(request, ct);
        var responseContent = await response.Content.ReadFromJsonAsync<AiAccessToken>(ct);
        if (responseContent == null) throw new InvalidOperationException("Access token is null");
        _accessToken = responseContent.Token;
        _tokenExpireTime = DateTimeOffset.FromUnixTimeMilliseconds(responseContent.ExpireAt).UtcDateTime;
        _tokenExpireTime = _tokenExpireTime.AddMinutes(-1);
    }

    public async Task<T> ExecuteRequest<T>(string requestData, HttpClient httpClient, CancellationToken ct)
    {
        _logger.LogInformation("@{requestData}", requestData);
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestData);
        _queueCount = Interlocked.Increment(ref _queueCount);
        await _semaphoreSlim.WaitAsync(ct);
        try
        {
            if (string.IsNullOrWhiteSpace(_accessToken) || _tokenExpireTime <= DateTime.Now)
            {
                await GetAccess(httpClient, ct);
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, _options.Value.ChatUrl);
            request.Content = new StringContent(requestData);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            using var response = await httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("aiMessage: {@responseContent}",  await response.Content.ReadAsStringAsync(ct));
            var responseContent = await response.Content.ReadFromJsonAsync<T>(ct);
            if (responseContent == null) throw new InvalidOperationException("Invalid response message");
            _logger.LogInformation("aiMessage: {@responseContent}", responseContent);
            return responseContent;
        }
        finally
        {
            _semaphoreSlim.Release();
            _queueCount = Interlocked.Decrement(ref _queueCount);
        }
    }

    public async Task<byte[]> DownloadImage(string id, HttpClient httpClient, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        _queueCount = Interlocked.Increment(ref _queueCount);
        await _semaphoreSlim.WaitAsync(ct);
        try
        {
            if (string.IsNullOrWhiteSpace(_accessToken) || _tokenExpireTime <= DateTime.Now)
            {
                await GetAccess(httpClient, ct);
            }

            var url = $"https://gigachat.devices.sberbank.ru/api/v1/files/{id}/content";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            using var response = await httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync(ct);
            stream.Seek(0, SeekOrigin.Begin);
            var result = new byte[stream.Length];
            await stream.ReadAsync(result, ct);
            return result;
        }
        finally
        {
            _semaphoreSlim.Release();
            _queueCount = Interlocked.Decrement(ref _queueCount);
        }
    }
}