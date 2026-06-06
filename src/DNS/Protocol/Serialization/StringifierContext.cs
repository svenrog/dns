using DNS.Protocol.ResourceRecords;
using System.Net;
using System.Text.Json.Serialization;

namespace DNS.Protocol.Serialization;

[JsonSerializable(typeof(Header))]
[JsonSerializable(typeof(Question))]
[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(Response))]
[JsonSerializable(typeof(ResourceRecord))]
[JsonSerializable(typeof(BaseResourceRecord))]
[JsonSerializable(typeof(CanonicalNameResourceRecord))]
[JsonSerializable(typeof(IPAddressResourceRecord))]
[JsonSerializable(typeof(MailExchangeResourceRecord))]
[JsonSerializable(typeof(NameServerResourceRecord))]
[JsonSerializable(typeof(PointerResourceRecord))]
[JsonSerializable(typeof(ServiceResourceRecord))]
[JsonSerializable(typeof(StartOfAuthorityResourceRecord))]
[JsonSerializable(typeof(TextResourceRecord))]
[JsonSerializable(typeof(IPAddress))]
[JsonSerializable(typeof(Domain))]
[JsonSourceGenerationOptions(
    WriteIndented = false,
    Converters = [
        typeof(DomainJsonConverter),
    ]
)]

internal partial class StringifierContext : JsonSerializerContext
{
}