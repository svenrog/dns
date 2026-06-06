using DNS.Protocol.ResourceRecords;

namespace DNS.Benchmark.Baseline.Protocol;

public interface IBaselineResponse : IBaselineMessage
{
    int Id { get; set; }
    IList<IBaselineResourceRecord> AnswerRecords { get; }
    IList<IBaselineResourceRecord> AuthorityRecords { get; }
    IList<IBaselineResourceRecord> AdditionalRecords { get; }
    bool RecursionAvailable { get; set; }
    bool AuthenticData { get; set; }
    bool CheckingDisabled { get; set; }
    bool AuthorativeServer { get; set; }
    bool Truncated { get; set; }
    BaselineOperationCode OperationCode { get; set; }
    BaselineResponseCode ResponseCode { get; set; }
}
