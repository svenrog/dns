using BenchmarkDotNet.Attributes;
using DNS.Benchmark.Baseline.Client.RequestResolver;
using DNS.Benchmark.Baseline.Protocol;
using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System.Net;
using System.Net.Sockets;

namespace DNS.Benchmark;

[MemoryDiagnoser]
[ShortRunJob]
public class TcpResolverBenchmarks
{
    private const int _port = 64649;
    private const string _domain = "google.com";
    private static readonly IPAddress _localIp = IPAddress.Parse("192.168.0.1");
    private static readonly IPAddress _internalIp = IPAddress.Parse("127.0.0.1");

    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private TcpRequestResolver? _resolver;
    private BaselineTcpRequestResolver? _baselineResolver;
    private Request _request = new();
    private BaselineRequest _baselineRequest = new();

    [GlobalSetup]
    public void Setup()
    {
        _cts = new CancellationTokenSource();
        _listener = new TcpListener(_internalIp, _port);
        _listener.Start();
        _ = AcceptLoop(_listener, _cts.Token);

        IPEndPoint endpoint = new(_internalIp, _port);
        _resolver = new TcpRequestResolver(endpoint);
        _baselineResolver = new BaselineTcpRequestResolver(endpoint);

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
        _cts?.Cancel();
        _listener?.Stop();
    }

    // A persistent TCP DNS responder: each connection serves length-prefixed
    // requests in a loop, so the pooling resolver can reuse the connection.
    private static async Task AcceptLoop(TcpListener listener, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(token).ConfigureAwait(false);
                _ = HandleClient(client, token);
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
        catch (SocketException) { }
    }

    private static async Task HandleClient(TcpClient client, CancellationToken token)
    {
        using (client)
        {
            NetworkStream stream = client.GetStream();
            byte[] lengthBuffer = new byte[2];

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (!await ReadExact(stream, lengthBuffer, token).ConfigureAwait(false)) return;

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(lengthBuffer);
                    }

                    byte[] requestBuffer = new byte[BitConverter.ToUInt16(lengthBuffer, 0)];
                    if (!await ReadExact(stream, requestBuffer, token).ConfigureAwait(false)) return;

                    Request request = Request.FromArray(requestBuffer);
                    Response response = Response.FromRequest(request);
                    response.AnswerRecords.Add(new IPAddressResourceRecord(new Domain(_domain), _localIp));

                    byte[] responseBuffer = response.ToArray();
                    byte[] responseLength = BitConverter.GetBytes((ushort)responseBuffer.Length);

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(responseLength);
                    }

                    await stream.WriteAsync(responseLength, token).ConfigureAwait(false);
                    await stream.WriteAsync(responseBuffer, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { }
            catch (IOException) { }
            catch (SocketException) { }
        }
    }

    private static async Task<bool> ReadExact(Stream stream, byte[] buffer, CancellationToken token)
    {
        int length = buffer.Length;
        int offset = 0;

        while (length > 0)
        {
            int size = await stream.ReadAsync(buffer.AsMemory(offset, length), token).ConfigureAwait(false);
            if (size == 0) return false;

            offset += size;
            length -= size;
        }

        return true;
    }
}
