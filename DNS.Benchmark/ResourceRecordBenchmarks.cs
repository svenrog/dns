using BenchmarkDotNet.Attributes;
using DNS.Benchmark.Baseline.Protocol.ResourceRecords;
using DNS.Protocol.ResourceRecords;
using DNS.Tests;

namespace DNS.Benchmark;

[MemoryDiagnoser]
[ShortRunJob]
public class ResourceRecordBenchmarks
{
    private byte[] _all = [];
    private byte[] _empty = [];

    private ResourceRecord _a;
    private BaselineResourceRecord _b;

    [GlobalSetup]
    public void Setup()
    {
        _all = Helper.ReadFixture("ResourceRecord", "www.google.com_all");
        _empty = Helper.ReadFixture("ResourceRecord", "empty-domain_any");

        _a = ResourceRecord.FromArray(_all, 0, out int _);
        _b = BaselineResourceRecord.FromArray(_all, 0, out int _);
    }

    [Benchmark]
    public void ResourceRecordParseArrayBaseline()
    {
        BaselineResourceRecord.FromArray(_all, 0, out int _);
        BaselineResourceRecord.FromArray(_empty, 0, out int _);
    }

    [Benchmark]
    public void ResourceRecordParseArray()
    {
        ResourceRecord.FromArray(_all, 0, out int _);
        ResourceRecord.FromArray(_empty, 0, out int _);
    }

    [Benchmark]
    public void ResourceRecordToArrayBaseline()
    {
        _b.ToArray();
    }

    [Benchmark]
    public void ResourceRecordToArray()
    {
        _a.ToArray();
    }
}