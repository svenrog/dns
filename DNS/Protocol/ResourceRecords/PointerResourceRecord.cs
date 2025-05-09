﻿using DNS.Protocol.Serialization;
using System.Net;
using System.Text.Json;

namespace DNS.Protocol.ResourceRecords;

public class PointerResourceRecord : BaseResourceRecord
{
    public PointerResourceRecord(IResourceRecord record, byte[] message, int dataOffset)
        : base(record)
    {
        PointerDomainName = Domain.FromArray(message, dataOffset);
    }

    public PointerResourceRecord(IPAddress ip, Domain pointer, TimeSpan ttl = default) :
        base(new ResourceRecord(Domain.PointerName(ip), pointer.ToArray(), RecordType.PTR, RecordClass.IN, ttl))
    {
        PointerDomainName = pointer;
    }

    public Domain PointerDomainName { get; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, StringifierContext.Default.PointerResourceRecord);
    }
}
