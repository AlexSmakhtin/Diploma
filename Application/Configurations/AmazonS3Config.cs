using System.ComponentModel.DataAnnotations;

namespace Application.Configurations;

public class AmazonS3Config
{
    [Required] public string BucketName { get; set; } = null!;
    [Required] public string JwtConfigFileName { get; set; } = null!;
    [Required] public string ConnStrFileName { get; set; } = null!;
    [Required] public string AccessKey { get; set; } = null!;
    [Required] public string SecretKey { get; set; } = null!;
    [Required] public string S3Url1 { get; set; } = null!;
    [Required] public string S3Url2 { get; set; } = null!;
    [Required] public string GigaChatDataFileName { get; set; } = null!;
    
    [Required] public string EmailSenderConfigFileName { get; set; } = null!;
}