using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SGCN.Api.Json;

public sealed class FlexibleTimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private static readonly string[] Formats =
    [
        "H:mm",
        "HH:mm",
        "H:mm:ss",
        "HH:mm:ss",
        "H:mm:ss.FFFFFFF",
        "HH:mm:ss.FFFFFFF"
    ];

    public override TimeOnly Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Time value must be a string.");
        }

        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException("Time value is required.");
        }

        if (TimeOnly.TryParseExact(
                value,
                Formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var time))
        {
            return time;
        }

        throw new JsonException("Time value must use HH:mm or HH:mm:ss format.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        TimeOnly value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
    }
}
