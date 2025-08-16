using System.Text.Json;
using System.Text.Json.Serialization;

namespace VeritasX.Api.Extensions;

public class TimeSpanConverter : JsonConverter<TimeSpan>
{
	private const string FormatString = "c";

	public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return TimeSpan.Parse(reader.GetString()!);
	}

	public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString(FormatString));
	}
}
