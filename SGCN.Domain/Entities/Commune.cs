using SGCN.Domain.Common;

namespace SGCN.Domain.Entities;

public sealed class Commune : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    public ICollection<Hospital> Hospitals { get; set; } = new List<Hospital>();
}
