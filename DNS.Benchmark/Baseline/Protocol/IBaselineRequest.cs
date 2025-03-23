using DNS.Protocol.ResourceRecords;

namespace DNS.Benchmark.Baseline.Protocol;

public interface IBaselineRequest : IBaselineMessage
{
    int Id { get; set; }
    IList<IBaselineResourceRecord> AdditionalRecords { get; }
    BaselineOperationCode OperationCode { get; set; }
    bool RecursionDesired { get; set; }
}
