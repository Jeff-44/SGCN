using SGCN.Application.Interfaces;

namespace SGCN.Infrastructure.Services;

public sealed class QrCodeService : IQrCodeService
{
    public Task<byte[]> GenerateQrCodeAsync(
        string content,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Array.Empty<byte>());
    }
}
