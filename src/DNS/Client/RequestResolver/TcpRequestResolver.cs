using DNS.Protocol;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace DNS.Client.RequestResolver;

public class TcpRequestResolver : IRequestResolver, IDisposable
{
    private const int _maxPoolSize = 64;

    private readonly IPEndPoint _dns;
    private readonly ConcurrentQueue<TcpClient> _pool = new();
    private int _poolCount;
    private bool _disposed;

    public TcpRequestResolver(IPEndPoint dns)
    {
        _dns = dns;
    }

    public async Task<IResponse?> Resolve(IRequest request, CancellationToken cancellationToken = default)
    {
        byte[] requestBytes = request.ToArray();

        // Prefer a pooled connection; if the exchange fails because it went
        // stale (server closed it since it was returned), retry once on a fresh
        // connection. DNS queries are idempotent, so re-sending is safe.
        if (TryRent(out TcpClient? pooled))
        {
            try
            {
                return await ExchangeAndRecycle(pooled!, request, requestBytes, cancellationToken).ConfigureAwait(false);
            }
            catch (IOException) { pooled!.Dispose(); }
            catch (SocketException) { pooled!.Dispose(); }
        }

        TcpClient fresh = new(_dns.AddressFamily);

        try
        {
            await fresh.ConnectAsync(_dns.Address, _dns.Port, cancellationToken).ConfigureAwait(false);
            return await ExchangeAndRecycle(fresh, request, requestBytes, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            fresh.Dispose();
            throw;
        }
    }

    private async Task<IResponse?> ExchangeAndRecycle(TcpClient tcp, IRequest request, byte[] requestBytes, CancellationToken cancellationToken)
    {
        IResponse response = await Exchange(tcp, request, requestBytes, cancellationToken).ConfigureAwait(false);
        Return(tcp);
        return response;
    }

    private static async Task<IResponse> Exchange(TcpClient tcp, IRequest request, byte[] requestBytes, CancellationToken cancellationToken)
    {
        NetworkStream stream = tcp.GetStream();

        byte[] length = BitConverter.GetBytes((ushort)requestBytes.Length);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(length);
        }

        await stream.WriteAsync(length, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(requestBytes, cancellationToken).ConfigureAwait(false);

        byte[] header = new byte[2];
        await Read(stream, header, cancellationToken).ConfigureAwait(false);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(header);
        }

        byte[] buffer = new byte[BitConverter.ToUInt16(header, 0)];
        await Read(stream, buffer, cancellationToken).ConfigureAwait(false);

        IResponse response = Response.FromArray(buffer);
        return new ClientResponse(request, response, buffer);
    }

    private static async Task Read(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        int length = buffer.Length;
        int offset = 0;
        int size;

        while (length > 0 && (size = await stream.ReadAsync(buffer.AsMemory(offset, length), cancellationToken).ConfigureAwait(false)) > 0)
        {
            offset += size;
            length -= size;
        }

        if (length > 0)
        {
            throw new IOException("Unexpected end of stream");
        }
    }

    private bool TryRent(out TcpClient? tcp)
    {
        while (_pool.TryDequeue(out TcpClient? candidate))
        {
            Interlocked.Decrement(ref _poolCount);

            if (!IsStale(candidate))
            {
                tcp = candidate;
                return true;
            }

            candidate.Dispose();
        }

        tcp = null;
        return false;
    }

    private void Return(TcpClient tcp)
    {
        if (_disposed || IsStale(tcp))
        {
            tcp.Dispose();
            return;
        }

        if (Interlocked.Increment(ref _poolCount) > _maxPoolSize)
        {
            Interlocked.Decrement(ref _poolCount);
            tcp.Dispose();
            return;
        }

        _pool.Enqueue(tcp);
    }

    // A connection the peer has closed shows up as readable with no data to read.
    private static bool IsStale(TcpClient tcp)
    {
        try
        {
            Socket socket = tcp.Client;
            return !tcp.Connected || (socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0);
        }
        catch (SocketException)
        {
            return true;
        }
        catch (ObjectDisposedException)
        {
            return true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;

        if (disposing)
        {
            while (_pool.TryDequeue(out TcpClient? tcp))
            {
                tcp.Dispose();
            }
        }
    }
}
