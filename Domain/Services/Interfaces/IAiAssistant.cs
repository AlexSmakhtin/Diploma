namespace Domain.Services.Interfaces;

public interface IAiAssistant
{
    public int QueueCount { get; }
    Task<T> ExecuteRequest<T>(string prompt, HttpClient httpClient, CancellationToken ct);
    Task<byte[]> DownloadImage(string id, HttpClient httpClient, CancellationToken ct);
}