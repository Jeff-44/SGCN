using System.Text.Json.Serialization;

namespace SGCN.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CertificateRequestStatus
{
    Pending = 1,
    InProgress = 2,
    CertificateCreated = 3,
    Rejected = 4,
    Cancelled = 5
}
