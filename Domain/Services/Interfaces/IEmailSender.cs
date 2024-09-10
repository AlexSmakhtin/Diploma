namespace Domain.Services.Interfaces;

public interface IEmailSender
{
    Task SendEmail(string to, string subject, string body, CancellationToken ct);
}