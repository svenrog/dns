using DNS.Protocol.Serialization;
using System;
using System.Text.Json;

namespace DNS.Protocol.ResourceRecords;

public class MailExchangeResourceRecord : BaseResourceRecord
{
    private const int _preferenceSize = 2;

    private static ResourceRecord Create(Domain domain, int preference, Domain exchange, TimeSpan ttl)
    {
        byte[] buffer = BitConverter.GetBytes((ushort)preference);
        byte[] data = new byte[buffer.Length + exchange.Size];

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(buffer);
        }

        buffer.CopyTo(data, 0);
        exchange.ToArray().CopyTo(data, buffer.Length);

        return new ResourceRecord(domain, data, RecordType.MX, RecordClass.IN, ttl);
    }

    public MailExchangeResourceRecord(IResourceRecord record, byte[] message, int dataOffset)
        : base(record)
    {
        byte[] preference = new byte[_preferenceSize];
        Array.Copy(message, dataOffset, preference, 0, preference.Length);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(preference);
        }

        dataOffset += _preferenceSize;

        Preference = BitConverter.ToUInt16(preference, 0);
        ExchangeDomainName = Domain.FromArray(message, dataOffset);
    }

    public MailExchangeResourceRecord(Domain domain, int preference, Domain exchange, TimeSpan ttl = default) :
        base(Create(domain, preference, exchange, ttl))
    {
        Preference = preference;
        ExchangeDomainName = exchange;
    }

    public int Preference { get; }
    public Domain ExchangeDomainName { get; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, StringifierContext.Default.MailExchangeResourceRecord);
    }
}
