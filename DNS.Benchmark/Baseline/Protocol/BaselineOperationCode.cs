namespace DNS.Benchmark.Baseline.Protocol;

public enum BaselineOperationCode
{
    Query = 0,
    IQuery,
    Status,
    // Reserved = 3
    Notify = 4,
    Update,
}
