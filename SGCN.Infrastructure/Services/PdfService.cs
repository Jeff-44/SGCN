using SGCN.Application.Interfaces;

namespace SGCN.Infrastructure.Services;

public sealed class PdfService : IPdfService
{
    public Task<byte[]> GeneratePdfAsync(
        string templateName,
        object model,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Array.Empty<byte>());
    }
}
