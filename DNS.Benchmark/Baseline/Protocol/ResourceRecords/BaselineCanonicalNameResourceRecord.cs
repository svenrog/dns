using DNS.Protocol.ResourceRecords;

namespace DNS.Benchmark.Baseline.Protocol.ResourceRecords
{
    public class BaselineCanonicalNameResourceRecord : BaselineBaseResourceRecord
    {
        public BaselineCanonicalNameResourceRecord(IBaselineResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            CanonicalDomainName = BaselineDomain.FromArray(message, dataOffset);
        }

        public BaselineCanonicalNameResourceRecord(BaselineDomain domain, BaselineDomain cname, TimeSpan ttl = default) :
            base(new BaselineResourceRecord(domain, cname.ToArray(), BaselineRecordType.CNAME, BaselineRecordClass.IN, ttl))
        {
            CanonicalDomainName = cname;
        }

        public BaselineDomain CanonicalDomainName { get; }
    }
}
