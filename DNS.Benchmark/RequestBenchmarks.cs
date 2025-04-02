using BenchmarkDotNet.Attributes;
using DNS.Benchmark.Baseline.Protocol;
using DNS.Protocol;
using DNS.Tests;

namespace DNS.Benchmark;

[MemoryDiagnoser]
[ShortRunJob]
public class RequestBenchmarks
{
    private byte[] _multiple = [];
    private byte[] _empty = [];

    private Request _a;
    private BaselineRequest _b;

    [GlobalSetup]
    public void Setup()
    {
        _multiple = Helper.ReadFixture("Request", "multiple-questions");
        _empty = Helper.ReadFixture("Request", "empty-header_basic-question");

        _a = Request.FromArray(_multiple);
        _b = BaselineRequest.FromArray(_multiple);
    }

    [Benchmark]
    public void RequestParseArrayBaseline()
    {
        BaselineRequest.FromArray(_multiple);
        BaselineRequest.FromArray(_empty);
    }

    [Benchmark]
    public void RequestParseArray()
    {
        Request.FromArray(_multiple);
        Request.FromArray(_empty);
    }

    [Benchmark]
    public void RequestToArrayBaseline()
    {
        _b.ToArray();
    }

    [Benchmark]
    public void RequestToArray()
    {
        _a.ToArray();
    }
}