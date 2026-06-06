using BenchmarkDotNet.Attributes;
using DNS.Benchmark.Baseline.Protocol;
using DNS.Protocol;
using DNS.Tests;

namespace DNS.Benchmark;

[MemoryDiagnoser]
[ShortRunJob]
public class HeaderBenchmarks
{
    private byte[] _all = [];
    private byte[] _empty = [];

    private Header _a;
    private BaselineHeader _b;

    [GlobalSetup]
    public void Setup()
    {
        _all = Helper.ReadFixture("Header", "all");
        _empty = Helper.ReadFixture("Header", "empty");

        _a = Header.FromArray(_all);
        _b = BaselineHeader.FromArray(_all);
    }

    [Benchmark]
    public void HeaderParseBaseline()
    {
        BaselineHeader.FromArray(_all);
        BaselineHeader.FromArray(_empty);
    }

    [Benchmark]
    public void HeaderParse()
    {
        Header.FromArray(_all);
        Header.FromArray(_empty);
    }

    [Benchmark(Baseline = true)]
    public void HeaderToArrayBaseline()
    {
        _b.ToArray();
    }

    [Benchmark]
    public void HeaderToArray()
    {
        _a.ToArray();
    }
}