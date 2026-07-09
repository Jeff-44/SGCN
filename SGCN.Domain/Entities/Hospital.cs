using SGCN.Domain.Common;

namespace SGCN.Domain.Entities;

public sealed class Hospital : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid CommuneId { get; set; }
    public Commune Commune { get; set; } = default!;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}
