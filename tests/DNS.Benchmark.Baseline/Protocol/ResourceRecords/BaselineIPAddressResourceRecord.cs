using DNS.Protocol.ResourceRecords;
using System.Net;

namespace DNS.Benchmark.Baseline.Protocol.ResourceRecords;

public class BaselineIPAddressResourceRecord : BaselineBaseResourceRecord
{
    private static BaselineResourceRecord Create(BaselineDomain domain, IPAddress ip, TimeSpan ttl)
    {
        byte[] data = ip.GetAddressBytes();
        BaselineRecordType type = data.Length == 4 ? BaselineRecordType.A : BaselineRecordType.AAAA;

        return new BaselineResourceRecord(domain, data, type, BaselineRecordClass.IN, ttl);
    }

    public BaselineIPAddressResourceRecord(IBaselineResourceRecord record) : base(record)
    {
        IPAddress = new IPAddress(Data);
    }

    public BaselineIPAddressResourceRecord(BaselineDomain domain, IPAddress ip, TimeSpan ttl = default) :
        base(Create(domain, ip, ttl))
    {
        IPAddress = ip;
    }

    public IPAddress IPAddress { get; }
}
