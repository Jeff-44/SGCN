namespace SGCN.Application.Interfaces;

public interface IPdfService
{
    Task<byte[]> GeneratePdfAsync(
        string templateName,
        object model,
        CancellationToken cancellationToken = default);
}
