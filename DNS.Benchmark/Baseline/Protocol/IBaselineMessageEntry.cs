namespace DNS.Benchmark.Baseline.Protocol;

public interface IBaselineMessageEntry
{
    BaselineDomain Name { get; }
    BaselineRecordType Type { get; }
    BaselineRecordClass Class { get; }

    int Size { get; }
    byte[] ToArray();
}
