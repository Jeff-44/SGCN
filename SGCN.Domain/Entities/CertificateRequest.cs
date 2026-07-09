using SGCN.Domain.Common;
using SGCN.Domain.Enums;
using SGCN.Domain.Identity;

namespace SGCN.Domain.Entities;

public sealed class CertificateRequest : BaseEntity
{
    public string RequestNumber { get; set; } = string.Empty;
    public string RequestedByUserId { get; set; } = string.Empty;
    public ApplicationUser RequestedByUser { get; set; } = default!;
    public string TargetFirstName { get; set; } = string.Empty;
    public string TargetLastName { get; set; } = string.Empty;
    public Gender TargetGender { get; set; }
    public DateOnly TargetBirthDate { get; set; }
    public string MotherFullName { get; set; } = string.Empty;
    public string? FatherFullName { get; set; }
    public string BirthPlace { get; set; } = string.Empty;
    public string? HospitalFileNumber { get; set; }
    public string RelationshipToTarget { get; set; } = string.Empty;
    public CertificateRequestStatus Status { get; set; } = CertificateRequestStatus.Pending;
    public string? RejectionReason { get; set; }
    public Guid? LinkedBirthRecordId { get; set; }
    public BirthRecord? LinkedBirthRecord { get; set; }
}
