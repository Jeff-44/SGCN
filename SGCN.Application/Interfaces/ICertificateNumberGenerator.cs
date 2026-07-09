namespace SGCN.Application.Interfaces;

public interface ICertificateNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
