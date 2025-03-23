using DNS.Protocol.ResourceRecords;
using System.Net;

namespace DNS.Benchmark.Baseline.Protocol.ResourceRecords
{
    public class BaselinePointerResourceRecord : BaselineBaseResourceRecord
    {
        public BaselinePointerResourceRecord(IBaselineResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            PointerDomainName = BaselineDomain.FromArray(message, dataOffset);
        }

        public BaselinePointerResourceRecord(IPAddress ip, BaselineDomain pointer, TimeSpan ttl = default) :
            base(new BaselineResourceRecord(BaselineDomain.PointerName(ip), pointer.ToArray(), BaselineRecordType.PTR, BaselineRecordClass.IN, ttl))
        {
            PointerDomainName = pointer;
        }

        public BaselineDomain PointerDomainName { get; }
    }
}
