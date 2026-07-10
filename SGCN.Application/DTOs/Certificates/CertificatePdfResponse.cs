namespace SGCN.Application.DTOs.Certificates;

public sealed record CertificatePdfResponse(
    byte[] Content,
    string FileName,
    string ContentType);
