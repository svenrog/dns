using BenchmarkDotNet.Attributes;
using DNS.Benchmark.Baseline.Client.RequestResolver;
using DNS.Benchmark.Baseline.Protocol;
using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Server;
using System.Net;

namespace DNS.Benchmark;

[MemoryDiagnoser]
[ShortRunJob]
public class UdpResolverBenchmarks
{
    private const int _port = 64648;
    private const string _domain = "google.com";
    private static readonly IPAddress _localIp = IPAddress.Parse("192.168.0.1");
    private static readonly IPAddress _internalIp = IPAddress.Parse("127.0.0.1");

    private DnsServer? _server;
    private UdpRequestResolver? _resolver;
    private BaselineUdpRequestResolver? _baselineResolver;
    private Request _request = new();
    private BaselineRequest _baselineRequest = new();

    [GlobalSetup]
    public void Setup()
    {
        _server = new DnsServer(new IPAddressRequestResolver());
        _server.Listen(_port);

        IPEndPoint endpoint = new(_internalIp, _port);
        _resolver = new UdpRequestResolver(endpoint);
        _baselineResolver = new BaselineUdpRequestResolver(endpoint);

        _request = new Request();
        _request.Questions.Add(new Question(new Domain(_domain), RecordType.A));

        _baselineRequest = new BaselineRequest();
        _baselineRequest.Questions.Add(new BaselineQuestion(new BaselineDomain(_domain), BaselineRecordType.A));
    }

    [Benchmark(Baseline = true)]
    public Task ResolveBaseline()
    {
        return _baselineResolver!.Resolve(_baselineRequest);
    }

    [Benchmark]
    public Task Resolve()
    {
        return _resolver!.Resolve(_request);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _resolver?.Dispose();
        _server?.Dispose();
    }

    private class IPAddressRequestResolver : IRequestResolver
    {
        public Task<IResponse?> Resolve(IRequest request, CancellationToken cancellationToken = default)
        {
            var response = Response.FromRequest(request);
            response.AnswerRecords.Add(new IPAddressResourceRecord(new Domain(_domain), _localIp));

            return Task.FromResult<IResponse?>(response);
        }
    }
}
