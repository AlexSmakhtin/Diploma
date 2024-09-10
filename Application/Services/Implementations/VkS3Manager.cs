using System.Net.Sockets;
using System.Text.Json;
using Amazon.S3;
using Application.Services.Interfaces;
using AmazonS3Config = Amazon.S3.AmazonS3Config;

namespace Application.Services.Implementations;

public class VkS3Manager : IS3Manager, IDisposable
{
    private readonly string _vkUrl1;
    private readonly string _vkUrl2;
    private readonly string _accessKey;
    private readonly string _secretKey;

    public VkS3Manager(string vkUrl1, string accessKey, string secretKey, string vkUrl2 = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vkUrl1);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);
        _vkUrl2 = vkUrl2;
        _vkUrl1 = vkUrl1;
        _accessKey = accessKey;
        _secretKey = secretKey;
    }

    public async Task<T> GetFileAsObject<T>(string fileName, string bucketName, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(bucketName);

        var s3Client = new AmazonS3Client(_accessKey, _secretKey, new AmazonS3Config()
        {
            ServiceURL = _vkUrl1,
            ForcePathStyle = true
        });
        try
        {
            return await GetObject<T>(s3Client, fileName, bucketName, ct);
        }
        catch (HttpRequestException)
        {
            s3Client = new AmazonS3Client(_accessKey, _secretKey, new AmazonS3Config()
            {
                ServiceURL = _vkUrl2,
                ForcePathStyle = true
            });
            return await GetObject<T>(s3Client, fileName, bucketName, ct);
        }
    }

    private async Task<T> GetObject<T>(
        AmazonS3Client s3Client,
        string fileName,
        string bucketName,
        CancellationToken ct)
    {
        using var response = await s3Client.GetObjectAsync(bucketName, fileName, ct);
        var jsonObject = await JsonSerializer.DeserializeAsync<T>(response.ResponseStream, cancellationToken: ct);
        if (jsonObject == null)
            throw new InvalidOperationException($"Deserialized object {typeof(T)} equals null");
        return jsonObject;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}