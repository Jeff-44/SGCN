using System.Text.Json.Serialization;

namespace SGCN.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CertificateStatus
{
    Active = 1,
    Annulled = 2
}
