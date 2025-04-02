using DNS.Protocol;
using DNS.Protocol.Utils;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DNS.Client.RequestResolver;

public class UdpRequestResolver : IRequestResolver
{
    private readonly int _timeout;
    private readonly IRequestResolver _fallback;
    private readonly IPEndPoint _dns;

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
        using UdpClient udp = new(_dns.AddressFamily);

        await udp
            .SendAsync(request.ToArray(), request.Size, _dns)
            .WithCancellationTimeout(TimeSpan.FromMilliseconds(_timeout), cancellationToken).ConfigureAwait(false);

        UdpReceiveResult result = await udp
            .ReceiveAsync()
            .WithCancellationTimeout(TimeSpan.FromMilliseconds(_timeout), cancellationToken).ConfigureAwait(false);

        if (!result.RemoteEndPoint.Equals(_dns)) throw new IOException("Remote endpoint mismatch");
        byte[] buffer = result.Buffer;
        Response response = Response.FromArray(buffer);

        if (response.Truncated)
        {
            return await _fallback.Resolve(request, cancellationToken).ConfigureAwait(false);
        }

        return new ClientResponse(request, response, buffer);
    }
}
