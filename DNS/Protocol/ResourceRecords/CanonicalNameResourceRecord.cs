using DNS.Protocol.Serialization;
using System.Text.Json;

namespace DNS.Protocol.ResourceRecords;

public class CanonicalNameResourceRecord : BaseResourceRecord
{
    public CanonicalNameResourceRecord(IResourceRecord record, byte[] message, int dataOffset)
        : base(record)
    {
        CanonicalDomainName = Domain.FromArray(message, dataOffset);
    }

    public CanonicalNameResourceRecord(Domain domain, Domain cname, TimeSpan ttl = default) :
        base(new ResourceRecord(domain, cname.ToArray(), RecordType.CNAME, RecordClass.IN, ttl))
    {
        CanonicalDomainName = cname;
    }

    public Domain CanonicalDomainName { get; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, StringifierContext.Default.CanonicalNameResourceRecord);
    }
}
