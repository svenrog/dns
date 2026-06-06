using System.Text.Json;

namespace DNS.Protocol.Serialization;

public sealed class DomainJsonConverter : StringConverter<Domain>
{
    public override Domain Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new Domain(reader.GetString());
    }
}
