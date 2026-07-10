using System.Net;
using System.Text.RegularExpressions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SGCN.Application.Interfaces;

namespace SGCN.Infrastructure.Services;

public sealed class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var host = GetRequiredValue("Email:Host");
        var fromAddress = GetRequiredValue("Email:FromAddress");
        var fromName = _configuration["Email:FromName"]?.Trim();
        var userName = _configuration["Email:UserName"]?.Trim();
        var password = _configuration["Email:Password"];

        if (!int.TryParse(GetRequiredValue("Email:Port"), out var port) || port is <= 0 or > 65535)
        {
            throw new InvalidOperationException("Email:Port is not valid.");
        }

        if (!bool.TryParse(GetRequiredValue("Email:EnableSsl"), out var enableSsl))
        {
            throw new InvalidOperationException("Email:EnableSsl is not valid.");
        }

        if (string.IsNullOrWhiteSpace(userName) != string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Email SMTP credentials are incomplete.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            string.IsNullOrWhiteSpace(fromName) ? "SGCN" : fromName,
            fromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder
        {
            HtmlBody = body,
            TextBody = ConvertToPlainText(body)
        }.ToMessageBody();

        using var client = new SmtpClient
        {
            Timeout = 30_000
        };

        await client.ConnectAsync(
            host,
            port,
            enableSsl
                ? port == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls
                : SecureSocketOptions.None,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
        {
            await client.AuthenticateAsync(userName, password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    private string GetRequiredValue(string key)
    {
        var value = _configuration[key]?.Trim();
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"{key} is not configured.");
    }

    private static string ConvertToPlainText(string html)
    {
        var withLineBreaks = Regex.Replace(
            html,
            "<(br|/p|/div|/h[1-6])[^>]*>",
            Environment.NewLine,
            RegexOptions.IgnoreCase);
        var withoutTags = Regex.Replace(withLineBreaks, "<[^>]+>", string.Empty);
        return WebUtility.HtmlDecode(withoutTags).Trim();
    }
}
