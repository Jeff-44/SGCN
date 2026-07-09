using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SGCN.Application.Interfaces;
using SGCN.Infrastructure.Persistence;

namespace SGCN.Infrastructure.Services;

public sealed class SgcnIdGenerator : ISgcnIdGenerator
{
    private readonly ApplicationDbContext _dbContext;

    public SgcnIdGenerator(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var prefix = $"SGCN-{today}-";

        // TODO: Replace this count-based approach with a concurrency-safe sequence before production use.
        var sequence = await _dbContext.BirthRecords
            .CountAsync(record => record.SgcnId.StartsWith(prefix), cancellationToken) + 1;

        string sgcnId;
        do
        {
            sgcnId = $"{prefix}{sequence:0000}";
            sequence++;
        }
        while (await _dbContext.BirthRecords.AnyAsync(record => record.SgcnId == sgcnId, cancellationToken));

        return sgcnId;
    }
}
