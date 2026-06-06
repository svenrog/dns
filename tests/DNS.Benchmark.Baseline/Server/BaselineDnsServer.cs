using DNS.Benchmark.Baseline.Client.RequestResolver;
using DNS.Benchmark.Baseline.Protocol;
using DNS.Client;
using DNS.Protocol.Utils;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace DNS.Benchmark.Baseline.Server;

public class BaselineDnsServer : IDisposable
{
    private const int _sioUdpConnReset = unchecked((int)0x9800000C);

    public event EventHandler<RequestedEventArgs>? Requested;
    public event EventHandler<RespondedEventArgs>? Responded;
    public event EventHandler<EventArgs>? Listening;
    public event EventHandler<ErroredEventArgs>? Errored;

    private bool _run = true;
    private bool _disposed = false;

    private UdpClient? _udp;
    private readonly IBaselineRequestResolver _resolver;

    public BaselineDnsServer(IBaselineRequestResolver resolver, IPEndPoint endServer) :
        this(new FallbackRequestResolver(resolver, new BaselineUdpRequestResolver(endServer)))
    { }

    public BaselineDnsServer(IBaselineRequestResolver resolver, IPAddress endServer, int port = 53) :
        this(resolver, new IPEndPoint(endServer, port))
    { }

    public BaselineDnsServer(IBaselineRequestResolver resolver, string endServer, int port = 53) :
        this(resolver, IPAddress.Parse(endServer), port)
    { }

    public BaselineDnsServer(IPEndPoint endServer) :
        this(new BaselineUdpRequestResolver(endServer))
    { }

    public BaselineDnsServer(IPAddress endServer, int port = 53) :
        this(new IPEndPoint(endServer, port))
    { }

    public BaselineDnsServer(string endServer, int port = 53) :
        this(IPAddress.Parse(endServer), port)
    { }

    public BaselineDnsServer(IBaselineRequestResolver resolver)
    {
        _resolver = resolver;
    }

    public Task Listen(int port = 53, IPAddress? ip = null)
    {
        return Listen(new IPEndPoint(ip ?? IPAddress.Any, port));
    }

    public async Task Listen(IPEndPoint endpoint)
    {
        await Task.Yield();

        TaskCompletionSource<object?> tcs = new();

        if (_run)
            try
            {
                _udp = new UdpClient(endpoint);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    _udp.Client.IOControl(_sioUdpConnReset, new byte[4], new byte[4]);
            }
            catch (SocketException e)
            {
                OnError(e);
                return;
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
        BaselineRequest? request = null;

        try
        {
            request = BaselineRequest.FromArray(data);
            OnEvent(Requested, new RequestedEventArgs(request, data, remote));

            IBaselineResponse? response = await _resolver.Resolve(request).ConfigureAwait(false);

            OnEvent(Responded, new RespondedEventArgs(request, response, data, remote));
            await _udp!
                .SendAsync(response?.ToArray() ?? [], response?.Size ?? 0, remote)
                .WithCancellationTimeout(TimeSpan.FromMilliseconds(2000)).ConfigureAwait(false);
        }
        catch (SocketException e) { OnError(e); }
        catch (ArgumentException e) { OnError(e); }
        catch (IndexOutOfRangeException e) { OnError(e); }
        catch (OperationCanceledException e) { OnError(e); }
        catch (IOException e) { OnError(e); }
        catch (ObjectDisposedException e) { OnError(e); }
        catch (ResponseException e)
        {
            try
            {
                await _udp!
                    .SendAsync([], 0, remote)
                    .WithCancellationTimeout(TimeSpan.FromMilliseconds(2000)).ConfigureAwait(false);
            }
            catch (SocketException) { /* Don't act */ }
            catch (OperationCanceledException) { /* Don't act */ }
            finally { OnError(e); }
        }
    }

    public class RequestedEventArgs : EventArgs
    {
        public RequestedEventArgs(IBaselineRequest request, byte[] data, IPEndPoint? remote)
        {
            Request = request;
            Data = data;
            Remote = remote;
        }

        public IBaselineRequest Request { get; }
        public byte[] Data { get; }
        public IPEndPoint? Remote { get; }
    }

    public class RespondedEventArgs : EventArgs
    {
        public RespondedEventArgs(IBaselineRequest request, IBaselineResponse? response, byte[] data, IPEndPoint? remote)
        {
            Request = request;
            Response = response;
            Data = data;
            Remote = remote;
        }

        public IBaselineRequest Request { get; }
        public IBaselineResponse? Response { get; }
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

    private sealed class FallbackRequestResolver : IBaselineRequestResolver
    {
        private readonly IBaselineRequestResolver[] _resolvers;

        public FallbackRequestResolver(params IBaselineRequestResolver[] resolvers)
        {
            _resolvers = resolvers;
        }

        public async Task<IBaselineResponse?> Resolve(IBaselineRequest request, CancellationToken cancellationToken = default)
        {
            IBaselineResponse? response = null;

            foreach (IBaselineRequestResolver resolver in _resolvers)
            {
                response = await resolver.Resolve(request, cancellationToken).ConfigureAwait(false);
                if (response?.AnswerRecords.Count > 0) break;
            }

            return response;
        }
    }
}
