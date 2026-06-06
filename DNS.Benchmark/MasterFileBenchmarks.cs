using BenchmarkDotNet.Attributes;
using DNS.Benchmark.Baseline.Protocol;
using DNS.Benchmark.Baseline.Server;
using DNS.Protocol;
using DNS.Server;

namespace DNS.Benchmark;

[MemoryDiagnoser]
[ShortRunJob]
public class MasterFileBenchmarks
{
    private const int _entryCount = 100;

    private MasterFile _masterFile = new();
    private BaselineMasterFile _baselineMasterFile = new();

    private Request _request = new();
    private BaselineRequest _baselineRequest = new();

    [GlobalSetup]
    public void Setup()
    {
        _masterFile = new MasterFile();
        _baselineMasterFile = new BaselineMasterFile();

        for (int i = 0; i < _entryCount; i++)
        {
            _masterFile.AddIPAddressResourceRecord($"host{i}.example.com", "192.168.0.1");
            _baselineMasterFile.AddIPAddressResourceRecord($"host{i}.example.com", "192.168.0.1");
        }

        // Resolve a name near the end of the zone so the scan does real work.
        _request = new Request();
        _request.Questions.Add(new Question(new Domain($"host{_entryCount - 1}.example.com"), RecordType.A));

        _baselineRequest = new BaselineRequest();
        _baselineRequest.Questions.Add(new BaselineQuestion(new BaselineDomain($"host{_entryCount - 1}.example.com"), BaselineRecordType.A));
    }

    [Benchmark(Baseline = true)]
    public Task ResolveBaseline()
    {
        return _baselineMasterFile.Resolve(_baselineRequest);
    }

    [Benchmark]
    public Task Resolve()
    {
        return _masterFile.Resolve(_request);
    }
}
