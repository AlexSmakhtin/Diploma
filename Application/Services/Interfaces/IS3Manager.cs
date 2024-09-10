namespace Application.Services.Interfaces;

public interface IS3Manager
{
    Task<T> GetFileAsObject<T>(string fileName, string bucketName, CancellationToken ct);
}