using SGCN.Domain.Common;
using SGCN.Domain.Enums;
using SGCN.Domain.Identity;

namespace SGCN.Domain.Entities;

public sealed class BirthRecord : BaseEntity
{
    public string SgcnId { get; set; } = string.Empty;
    public string ChildFirstName { get; set; } = string.Empty;
    public string ChildLastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateOnly BirthDate { get; set; }
    public TimeOnly BirthTime { get; set; }
    public string BirthPlace { get; set; } = string.Empty;
    public Guid HospitalId { get; set; }
    public Hospital Hospital { get; set; } = default!;
    public string MotherFullName { get; set; } = string.Empty;
    public string? FatherFullName { get; set; }
    public string? HospitalFileNumber { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public ApplicationUser CreatedByUser { get; set; } = default!;
    public bool IsLocked { get; set; }
    public bool IsActive { get; set; } = true;
}
