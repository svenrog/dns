﻿namespace DNS.Protocol.ResourceRecords;

public static class ResourceRecordFactory
{
    public static IList<IResourceRecord> GetAllFromArray(byte[] message, int offset, int count)
    {
        return GetAllFromArray(message, offset, count, out _);
    }

    public static IList<IResourceRecord> GetAllFromArray(byte[] message, int offset, int count, out int endOffset)
    {
        List<IResourceRecord> result = new(count);

        for (int i = 0; i < count; i++)
        {
            result.Add(FromArray(message, offset, out offset));
        }

        endOffset = offset;
        return result;
    }

    public static IResourceRecord FromArray(byte[] message, int offset)
    {
        return FromArray(message, offset, out _);
    }

    public static IResourceRecord FromArray(byte[] message, int offset, out int endOffset)
    {
        ResourceRecord record = ResourceRecord.FromArray(message, offset, out endOffset);
        int dataOffset = endOffset - record.DataLength;

        return record.Type switch
        {
            RecordType.A or RecordType.AAAA => new IPAddressResourceRecord(record),
            RecordType.NS => new NameServerResourceRecord(record, message, dataOffset),
            RecordType.CNAME => new CanonicalNameResourceRecord(record, message, dataOffset),
            RecordType.SOA => new StartOfAuthorityResourceRecord(record, message, dataOffset),
            RecordType.PTR => new PointerResourceRecord(record, message, dataOffset),
            RecordType.MX => new MailExchangeResourceRecord(record, message, dataOffset),
            RecordType.TXT => new TextResourceRecord(record),
            RecordType.SRV => new ServiceResourceRecord(record, message, dataOffset),
            _ => record,
        };
    }
}
