using DNS.Protocol;
using DNS.Protocol.Utils;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace DNS.Client.RequestResolver;

public class UdpRequestResolver : IRequestResolver, IDisposable
{
    private const int _maxPoolSize = 64;

    private readonly int _timeout;
    private readonly IRequestResolver _fallback;
    private readonly IPEndPoint _dns;
    private readonly ConcurrentQueue<UdpClient> _pool = new();
    private int _poolCount;
    private bool _disposed;

    public UdpRequestResolver(IPEndPoint dns, IRequestResolver fallback, int timeout = 5000)
    {
        _dns = dns;
        _fallback = fallback;
        _timeout = timeout;
    }

    public UdpRequestResolver(IPEndPoint dns, int timeout = 5000)
    {
        _dns = dns;
        _fallback = new NullRequestResolver();
        _timeout = timeout;
    }

    public async Task<IResponse?> Resolve(IRequest request, CancellationToken cancellationToken = default)
    {
        UdpClient udp = Rent();
        bool reusable = false;

        try
        {
            await udp
                .SendAsync(request.ToArray(), request.Size, _dns)
                .WithCancellationTimeout(TimeSpan.FromMilliseconds(_timeout), cancellationToken).ConfigureAwait(false);

            UdpReceiveResult result = await udp
                .ReceiveAsync()
                .WithCancellationTimeout(TimeSpan.FromMilliseconds(_timeout), cancellationToken).ConfigureAwait(false);

            if (!result.RemoteEndPoint.Equals(_dns)) throw new IOException("Remote endpoint mismatch");

            // Only recycle a socket that is clean — nothing buffered behind the
            // response (e.g. a duplicate or late datagram) that could leak into
            // the next request that reuses it.
            reusable = udp.Available == 0;

            byte[] buffer = result.Buffer;
            Response response = Response.FromArray(buffer);

            if (response.Truncated)
            {
                return await _fallback.Resolve(request, cancellationToken).ConfigureAwait(false);
            }

            return new ClientResponse(request, response, buffer);
        }
        finally
        {
            if (reusable) Return(udp);
            else udp.Dispose();
        }
    }

    private UdpClient Rent()
    {
        if (_pool.TryDequeue(out UdpClient? udp))
        {
            Interlocked.Decrement(ref _poolCount);
            return udp;
        }

        return new UdpClient(_dns.AddressFamily);
    }

    private void Return(UdpClient udp)
    {
        if (_disposed || Interlocked.Increment(ref _poolCount) > _maxPoolSize)
        {
            Interlocked.Decrement(ref _poolCount);
            udp.Dispose();
            return;
        }

        _pool.Enqueue(udp);
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
            while (_pool.TryDequeue(out UdpClient? udp))
            {
                udp.Dispose();
            }
        }
    }
}
