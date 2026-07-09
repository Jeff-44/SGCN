namespace SGCN.Application.Interfaces;

public interface IVerificationCodeGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
