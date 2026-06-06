using DNS.Benchmark.Baseline.Protocol;
using DNS.Protocol.Utils;
using System.Net;
using System.Net.Sockets;

namespace DNS.Benchmark.Baseline.Client.RequestResolver;

public class BaselineUdpRequestResolver : IBaselineRequestResolver
{
    private readonly int _timeout;
    private readonly IBaselineRequestResolver _fallback;
    private readonly IPEndPoint _dns;

    public BaselineUdpRequestResolver(IPEndPoint dns, IBaselineRequestResolver fallback, int timeout = 5000)
    {
        _dns = dns;
        _fallback = fallback;
        _timeout = timeout;
    }

    public BaselineUdpRequestResolver(IPEndPoint dns, int timeout = 5000)
    {
        _dns = dns;
        _fallback = new BaselineNullRequestResolver();
        _timeout = timeout;
    }

    public async Task<IBaselineResponse?> Resolve(IBaselineRequest request, CancellationToken cancellationToken = default)
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
        BaselineResponse response = BaselineResponse.FromArray(buffer);

        if (response.Truncated)
            return await _fallback.Resolve(request, cancellationToken).ConfigureAwait(false);

        return new BaselineClientResponse(request, response, buffer);
    }
}
