using SGCN.Application.Interfaces;

namespace SGCN.Infrastructure.Services;

public sealed class EmailService : IEmailService
{
    public Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
