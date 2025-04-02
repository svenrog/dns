using DNS.Protocol.Serialization;
using System;
using System.Text.Json;

namespace DNS.Protocol.ResourceRecords;

public class NameServerResourceRecord : BaseResourceRecord
{
    public NameServerResourceRecord(IResourceRecord record, byte[] message, int dataOffset)
        : base(record)
    {
        NSDomainName = Domain.FromArray(message, dataOffset);
    }

    public NameServerResourceRecord(Domain domain, Domain nsDomain, TimeSpan ttl = default) :
        base(new ResourceRecord(domain, nsDomain.ToArray(), RecordType.NS, RecordClass.IN, ttl))
    {
        NSDomainName = nsDomain;
    }

    public Domain NSDomainName { get; }

    public override string ToString()
    {

        return JsonSerializer.Serialize(this, StringifierContext.Default.NameServerResourceRecord);
    }
}
