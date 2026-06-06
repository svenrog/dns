using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DNS.Tests.Client;

public class TcpRequestResolverTest
{
    [Fact]
    public async Task ReusesPooledConnection()
    {
        using TestTcpServer server = new(keepAlive: true);
        using TcpRequestResolver resolver = new(server.EndPoint);

        IResponse first = await resolver.Resolve(Query());
        IResponse second = await resolver.Resolve(Query());

        Assert.Equal("192.168.0.1", ((IPAddressResourceRecord)first.AnswerRecords[0]).IPAddress.ToString());
        Assert.Equal("192.168.0.1", ((IPAddressResourceRecord)second.AnswerRecords[0]).IPAddress.ToString());

        // The second resolve must reuse the pooled connection, not open a new one.
        Assert.Equal(1, server.AcceptedConnections);
    }

    [Fact]
    public async Task ReconnectsWhenServerClosesConnection()
    {
        using TestTcpServer server = new(keepAlive: false);
        using TcpRequestResolver resolver = new(server.EndPoint);

        IResponse first = await resolver.Resolve(Query());
        IResponse second = await resolver.Resolve(Query());

        Assert.Equal("192.168.0.1", ((IPAddressResourceRecord)first.AnswerRecords[0]).IPAddress.ToString());
        Assert.Equal("192.168.0.1", ((IPAddressResourceRecord)second.AnswerRecords[0]).IPAddress.ToString());

        // Connection was closed after the first response, so a new one is opened.
        Assert.True(server.AcceptedConnections >= 2);
    }

    private static Request Query()
    {
        Request request = new();
        request.Questions.Add(new Question(new Domain("google.com"), RecordType.A));

        return request;
    }

    private sealed class TestTcpServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly bool _keepAlive;
        private int _accepted;

        public TestTcpServer(bool keepAlive)
        {
            _keepAlive = keepAlive;
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            EndPoint = (IPEndPoint)_listener.LocalEndpoint;
            _ = AcceptLoop();
        }

        public IPEndPoint EndPoint { get; }
        public int AcceptedConnections => Volatile.Read(ref _accepted);

        private async Task AcceptLoop()
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync(_cts.Token).ConfigureAwait(false);
                    Interlocked.Increment(ref _accepted);
                    _ = Handle(client);
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (SocketException) { }
        }

        private async Task Handle(TcpClient client)
        {
            using (client)
            {
                NetworkStream stream = client.GetStream();
                byte[] lengthBuffer = new byte[2];

                try
                {
                    do
                    {
                        if (!await ReadExact(stream, lengthBuffer).ConfigureAwait(false)) return;

                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(lengthBuffer);
                        }

                        byte[] requestBuffer = new byte[BitConverter.ToUInt16(lengthBuffer, 0)];
                        if (!await ReadExact(stream, requestBuffer).ConfigureAwait(false)) return;

                        Request request = Request.FromArray(requestBuffer);
                        Response response = Response.FromRequest(request);
                        response.AnswerRecords.Add(new IPAddressResourceRecord(new Domain("google.com"), IPAddress.Parse("192.168.0.1")));

                        byte[] responseBuffer = response.ToArray();
                        byte[] responseLength = BitConverter.GetBytes((ushort)responseBuffer.Length);

                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(responseLength);
                        }

                        await stream.WriteAsync(responseLength).ConfigureAwait(false);
                        await stream.WriteAsync(responseBuffer).ConfigureAwait(false);
                    }
                    while (_keepAlive && !_cts.IsCancellationRequested);
                }
                catch (IOException) { }
                catch (SocketException) { }
                catch (OperationCanceledException) { }
            }
        }

        private async Task<bool> ReadExact(Stream stream, byte[] buffer)
        {
            int length = buffer.Length;
            int offset = 0;

            while (length > 0)
            {
                int size = await stream.ReadAsync(buffer.AsMemory(offset, length), _cts.Token).ConfigureAwait(false);
                if (size == 0) return false;

                offset += size;
                length -= size;
            }

            return true;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _listener.Stop();
            _cts.Dispose();
        }
    }
}
