using System.Net;
using System.Net.Mail;
using Application.Configurations;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Application.Services.Implementations;

public class EmailSender : IEmailSender
{
    private readonly int _port;
    private readonly string _login;
    private readonly string _password;
    private readonly string _host;

    public EmailSender(IOptions<EmailSenderConfig> config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _port = config.Value.Port;
        _login = config.Value.Login;
        _password = config.Value.Password;
        _host = config.Value.Host;
    }

    public async Task SendEmail(string to, string subject, string body, CancellationToken ct)
    {
        var fromAddress = new MailAddress(_login);
        var toAddress = new MailAddress(to);
        using var mailMessage = new MailMessage(fromAddress, toAddress);
        mailMessage.Subject = subject;
        mailMessage.IsBodyHtml = true;
        mailMessage.Body = body;
        using var smtpClient = new SmtpClient(_host, _port);
        smtpClient.Credentials = new NetworkCredential(_login, _password);
        smtpClient.EnableSsl = true;
        await smtpClient.SendMailAsync(mailMessage, ct);
    }
}