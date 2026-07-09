using Microsoft.EntityFrameworkCore;
using SGCN.Application.Interfaces;
using SGCN.Infrastructure.Persistence;

namespace SGCN.Infrastructure.Services;

public sealed class VerificationCodeGenerator : IVerificationCodeGenerator
{
    private readonly ApplicationDbContext _dbContext;

    public VerificationCodeGenerator(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        string verificationCode;
        do
        {
            verificationCode = Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();
        }
        while (await _dbContext.Certificates.AnyAsync(certificate => certificate.VerificationCode == verificationCode, cancellationToken));

        return verificationCode;
    }
}
