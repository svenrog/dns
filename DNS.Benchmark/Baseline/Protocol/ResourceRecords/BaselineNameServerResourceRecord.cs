using DNS.Protocol.ResourceRecords;

namespace DNS.Benchmark.Baseline.Protocol.ResourceRecords
{
    public class BaselineNameServerResourceRecord : BaselineBaseResourceRecord
    {
        public BaselineNameServerResourceRecord(IBaselineResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            NSDomainName = BaselineDomain.FromArray(message, dataOffset);
        }

        public BaselineNameServerResourceRecord(BaselineDomain domain, BaselineDomain nsDomain, TimeSpan ttl = default) :
            base(new BaselineResourceRecord(domain, nsDomain.ToArray(), BaselineRecordType.NS, BaselineRecordClass.IN, ttl))
        {
            NSDomainName = nsDomain;
        }

        public BaselineDomain NSDomainName { get; }
    }
}
