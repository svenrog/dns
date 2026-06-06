using DNS.Benchmark.Baseline.Protocol;

namespace DNS.Protocol.ResourceRecords;

public interface IBaselineResourceRecord : IBaselineMessageEntry
{
    TimeSpan TimeToLive { get; }
    int DataLength { get; }
    byte[] Data { get; }
}
