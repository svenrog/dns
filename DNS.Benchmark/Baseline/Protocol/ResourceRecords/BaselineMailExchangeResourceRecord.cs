using DNS.Protocol.ResourceRecords;

namespace DNS.Benchmark.Baseline.Protocol.ResourceRecords;

public class BaselineMailExchangeResourceRecord : BaselineBaseResourceRecord
{
    private const int _preferenceSize = 2;

    private static BaselineResourceRecord Create(BaselineDomain domain, int preference, BaselineDomain exchange, TimeSpan ttl)
    {
        byte[] buffer = BitConverter.GetBytes((ushort)preference);
        byte[] data = new byte[buffer.Length + exchange.Size];

        if (BitConverter.IsLittleEndian)
            Array.Reverse(buffer);

        buffer.CopyTo(data, 0);
        exchange.ToArray().CopyTo(data, buffer.Length);

        return new BaselineResourceRecord(domain, data, BaselineRecordType.MX, BaselineRecordClass.IN, ttl);
    }

    public BaselineMailExchangeResourceRecord(IBaselineResourceRecord record, byte[] message, int dataOffset)
        : base(record)
    {
        byte[] preference = new byte[_preferenceSize];
        Array.Copy(message, dataOffset, preference, 0, preference.Length);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(preference);

        dataOffset += _preferenceSize;

        Preference = BitConverter.ToUInt16(preference, 0);
        ExchangeDomainName = BaselineDomain.FromArray(message, dataOffset);
    }

    public BaselineMailExchangeResourceRecord(BaselineDomain domain, int preference, BaselineDomain exchange, TimeSpan ttl = default) :
        base(Create(domain, preference, exchange, ttl))
    {
        Preference = preference;
        ExchangeDomainName = exchange;
    }

    public int Preference { get; }
    public BaselineDomain ExchangeDomainName { get; }
}
