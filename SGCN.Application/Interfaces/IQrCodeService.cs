namespace SGCN.Application.Interfaces;

public interface IQrCodeService
{
    Task<byte[]> GenerateQrCodeAsync(
        string content,
        CancellationToken cancellationToken = default);
}
