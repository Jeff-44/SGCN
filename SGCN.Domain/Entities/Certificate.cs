using SGCN.Domain.Common;
using SGCN.Domain.Enums;
using SGCN.Domain.Identity;

namespace SGCN.Domain.Entities;

public sealed class Certificate : BaseEntity
{
    public string CertificateNumber { get; set; } = string.Empty;
    public Guid? CertificateRequestId { get; set; }
    public CertificateRequest? CertificateRequest { get; set; }
    public Guid BirthRecordId { get; set; }
    public BirthRecord BirthRecord { get; set; } = default!;
    public string ChildFirstName { get; set; } = string.Empty;
    public string ChildLastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateOnly BirthDate { get; set; }
    public TimeOnly BirthTime { get; set; }
    public string BirthPlace { get; set; } = string.Empty;
    public string HospitalName { get; set; } = string.Empty;
    public string CommuneName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string MotherFullName { get; set; } = string.Empty;
    public string? FatherFullName { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public ApplicationUser CreatedByUser { get; set; } = default!;
    public CertificateStatus Status { get; set; } = CertificateStatus.Active;
    public string VerificationCode { get; set; } = string.Empty;
    public string? PdfPath { get; set; }
    public string? QrCodePath { get; set; }
    public DateTime? AnnulledAt { get; set; }
    public string? AnnulledReason { get; set; }
}
