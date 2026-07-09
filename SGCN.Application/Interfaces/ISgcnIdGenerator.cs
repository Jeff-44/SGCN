namespace SGCN.Application.Interfaces;

public interface ISgcnIdGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
