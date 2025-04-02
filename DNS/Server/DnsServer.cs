using DNS.Client;
using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.Utils;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DNS.Server;

public class DnsServer : IDisposable
{
    private const int _sioUdpConnReset = unchecked((int)0x9800000C);
    private const int _defaultPort = 53;
    private const int _udpTimeout = 2000;

    public event EventHandler<RequestedEventArgs>? Requested;
    public event EventHandler<RespondedEventArgs>? Responded;
    public event EventHandler<EventArgs>? Listening;
    public event EventHandler<ErroredEventArgs>? Errored;

    private bool _run = true;
    private bool _disposed = false;

    private UdpClient? _udp;
    private readonly IRequestResolver _resolver;

    public DnsServer(IRequestResolver resolver, IPEndPoint endServer) :
        this(new FallbackRequestResolver(resolver, new UdpRequestResolver(endServer)))
    { }

    public DnsServer(IRequestResolver resolver, IPAddress endServer, int port = _defaultPort) :
        this(resolver, new IPEndPoint(endServer, port))
    { }

    public DnsServer(IRequestResolver resolver, string endServer, int port = _defaultPort) :
        this(resolver, IPAddress.Parse(endServer), port)
    { }

    public DnsServer(IPEndPoint endServer) :
        this(new UdpRequestResolver(endServer))
    { }

    public DnsServer(IPAddress endServer, int port = _defaultPort) :
        this(new IPEndPoint(endServer, port))
    { }

    public DnsServer(string endServer, int port = _defaultPort) :
        this(IPAddress.Parse(endServer), port)
    { }

    public DnsServer(IRequestResolver resolver)
    {
        _resolver = resolver;
    }

    public Task Listen(int port = _defaultPort, IPAddress? ip = null)
    {
        return Listen(new IPEndPoint(ip ?? IPAddress.Any, port));
    }

    public async Task Listen(IPEndPoint endpoint)
    {
        await Task.Yield();

        TaskCompletionSource<object?> tcs = new();

        if (_run)
        {
            try
            {
                _udp = new UdpClient(endpoint);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _udp.Client.IOControl(_sioUdpConnReset, new byte[4], new byte[4]);
                }
            }
            catch (SocketException e)
            {
                OnError(e);
                return;
            }
        }

        void ReceiveCallback(IAsyncResult result)
        {
            byte[] data;

            try
            {
                IPEndPoint? remote = new(0, 0);
                data = _udp.EndReceive(result, ref remote);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                HandleRequest(data, remote);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            catch (ObjectDisposedException)
            {
                // run should already be false
                _run = false;
            }
            catch (SocketException e)
            {
                OnError(e);
            }

            if (_run) _udp.BeginReceive(ReceiveCallback, null);
            else tcs.SetResult(null);
        }

        _udp!.BeginReceive(ReceiveCallback, null);
        OnEvent(Listening, EventArgs.Empty);
        await tcs.Task.ConfigureAwait(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;

            if (disposing)
            {
                _run = false;
                _udp?.Dispose();
            }
        }
    }

    protected virtual void OnEvent<T>(EventHandler<T>? handler, T args)
    {
        handler?.Invoke(this, args);
    }

    private void OnError(Exception e)
    {
        OnEvent(Errored, new ErroredEventArgs(e));
    }

    private async Task HandleRequest(byte[] data, IPEndPoint? remote)
    {
        Request? request = null;

        try
        {
            request = Request.FromArray(data);
            OnEvent(Requested, new RequestedEventArgs(request, data, remote));

            IResponse? response = await _resolver.Resolve(request).ConfigureAwait(false);

            OnEvent(Responded, new RespondedEventArgs(request, response, data, remote));
            await _udp!
                .SendAsync(response?.ToArray() ?? [], response?.Size ?? 0, remote)
                .WithCancellationTimeout(TimeSpan.FromMilliseconds(_udpTimeout)).ConfigureAwait(false);
        }
        catch (SocketException e) { OnError(e); }
        catch (ArgumentException e) { OnError(e); }
        catch (IndexOutOfRangeException e) { OnError(e); }
        catch (OperationCanceledException e) { OnError(e); }
        catch (IOException e) { OnError(e); }
        catch (ObjectDisposedException e) { OnError(e); }
        catch (ResponseException e)
        {
            IResponse? response = e.Response;

            if (request != null && response == null)
            {
                response = Response.FromRequest(request);
            }

            try
            {
                await _udp!
                    .SendAsync(response?.ToArray() ?? [], response?.Size ?? 0, remote)
                    .WithCancellationTimeout(TimeSpan.FromMilliseconds(_udpTimeout)).ConfigureAwait(false);
            }
            catch (SocketException) { /* Don't act */ }
            catch (OperationCanceledException) { /* Don't act */ }
            finally { OnError(e); }
        }
    }

    public class RequestedEventArgs : EventArgs
    {
        public RequestedEventArgs(IRequest request, byte[] data, IPEndPoint? remote)
        {
            Request = request;
            Data = data;
            Remote = remote;
        }

        public IRequest Request { get; }
        public byte[] Data { get; }
        public IPEndPoint? Remote { get; }
    }

    public class RespondedEventArgs : EventArgs
    {
        public RespondedEventArgs(IRequest request, IResponse? response, byte[] data, IPEndPoint? remote)
        {
            Request = request;
            Response = response;
            Data = data;
            Remote = remote;
        }

        public IRequest Request { get; }
        public IResponse? Response { get; }
        public byte[] Data { get; }
        public IPEndPoint? Remote { get; }
    }

    public class ErroredEventArgs : EventArgs
    {
        public ErroredEventArgs(Exception e)
        {
            Exception = e;
        }

        public Exception Exception { get; }
    }

    private sealed class FallbackRequestResolver : IRequestResolver
    {
        private readonly IRequestResolver[] _resolvers;

        public FallbackRequestResolver(params IRequestResolver[] resolvers)
        {
            _resolvers = resolvers;
        }

        public async Task<IResponse?> Resolve(IRequest request, CancellationToken cancellationToken = default)
        {
            IResponse? response = null;

            foreach (IRequestResolver resolver in _resolvers)
            {
                response = await resolver.Resolve(request, cancellationToken).ConfigureAwait(false);
                if (response?.AnswerRecords.Count > 0) break;
            }

            return response;
        }
    }
}
