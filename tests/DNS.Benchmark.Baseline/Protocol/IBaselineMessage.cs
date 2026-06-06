namespace DNS.Benchmark.Baseline.Protocol;

public interface IBaselineMessage
{
    IList<BaselineQuestion> Questions { get; }

    int Size { get; }
    byte[] ToArray();
}
