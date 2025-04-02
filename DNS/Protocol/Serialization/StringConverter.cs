using System.Text.Json;
using System.Text.Json.Serialization;

namespace DNS.Protocol.Serialization;

public abstract class StringConverter<T> : JsonConverter<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        var stringValue = value?.ToString();
        if (stringValue == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(stringValue);
        }
    }
}
