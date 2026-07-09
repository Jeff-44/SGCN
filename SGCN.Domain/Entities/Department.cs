using SGCN.Domain.Common;

namespace SGCN.Domain.Entities;

public sealed class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Commune> Communes { get; set; } = new List<Commune>();
}
