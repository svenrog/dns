using BenchmarkDotNet.Attributes;
using DNS.Benchmark.Baseline.Client.RequestResolver;
using DNS.Benchmark.Baseline.Protocol;
using DNS.Benchmark.Baseline.Protocol.ResourceRecords;
using DNS.Benchmark.Baseline.Server;
using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Server;
using System.Net;
using System.Net.Sockets;

namespace DNS.Benchmark;

[MemoryDiagnoser]

public class ServerBenchmarks
{
    private const int _port = 64646;
    private const int _baselinePort = 64647;

    private const string _domain = "google.com";
    private static readonly IPAddress _localIp = IPAddress.Parse("192.168.0.1");
    private static readonly IPAddress _internalIp = IPAddress.Parse("127.0.0.1");

    private DnsServer? _server;
    private BaselineDnsServer? _baselineServer;
    private UdpClient? _udpClient;
    private IPEndPoint? _endpoint;
    private IPEndPoint? _baselineEndpoint;
    private ReadOnlyMemory<byte> _request = new([]);

    [GlobalSetup]
    public void Setup()
    {
        _server = new DnsServer(new IPAddressRequestResolver());
        _server.Listen(_port);

        _baselineServer = new BaselineDnsServer(new BaselineIPAddressRequestResolver());
        _baselineServer.Listen(_baselinePort);

        _udpClient = new UdpClient();
        _endpoint = new IPEndPoint(_internalIp, _port);
        _baselineEndpoint = new IPEndPoint(_internalIp, _baselinePort);

        Request request = new()
        {
            Id = 1,
            OperationCode = OperationCode.Query
        };

        request.Questions.Add(new(new Domain(_domain), RecordType.A));
        request.Questions.Add(new(new Domain(_domain), RecordType.MX));
        request.Questions.Add(new(new Domain(_domain), RecordType.PTR));
        request.Questions.Add(new(new Domain(_domain), RecordType.TXT));

        _request = request.ToArray();
    }

    [Benchmark(Baseline = true)]
    public async Task BaselineServerReceive()
    {
        await _udpClient!.SendAsync(_request, _baselineEndpoint);
        await _udpClient.ReceiveAsync();
    }

    [Benchmark]
    public async Task ServerReceive()
    {
        await _udpClient!.SendAsync(_request, _endpoint);
        await _udpClient.ReceiveAsync();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _server!.Dispose();
        _baselineServer!.Dispose();
        _udpClient!.Dispose();
    }

    private class IPAddressRequestResolver : IRequestResolver
    {
        public Task<IResponse?> Resolve(IRequest request, CancellationToken cancellationToken = default)
        {
            var response = Response.FromRequest(request);
            var record = new IPAddressResourceRecord(new Domain(_domain), _localIp);

            response.AnswerRecords.Add(record);

            return Task.FromResult<IResponse?>(response);
        }
    }

    private class BaselineIPAddressRequestResolver : IBaselineRequestResolver
    {
        public Task<IBaselineResponse?> Resolve(IBaselineRequest request, CancellationToken cancellationToken = default)
        {
            var response = BaselineResponse.FromRequest(request);
            var record = new BaselineIPAddressResourceRecord(new BaselineDomain(_domain), _localIp);

            response.AnswerRecords.Add(record);

            return Task.FromResult<IBaselineResponse?>(response);
        }
    }
}
