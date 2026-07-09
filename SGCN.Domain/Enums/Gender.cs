using System.Text.Json.Serialization;

namespace SGCN.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Gender
{
    Male = 1,
    Female = 2
}
