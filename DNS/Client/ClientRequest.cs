using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System.Collections.ObjectModel;
using System.Net;

namespace DNS.Client;

public class ClientRequest : IRequest
{
    private const int _defaultPort = 53;

    private readonly IRequestResolver _resolver;
    private readonly Request _request;

    public ClientRequest(IPEndPoint dns, IRequest? request = null) :
        this(new UdpRequestResolver(dns), request)
    { }

    public ClientRequest(IPAddress ip, int port = _defaultPort, IRequest? request = null) :
        this(new IPEndPoint(ip, port), request)
    { }

    public ClientRequest(string ip, int port = _defaultPort, IRequest? request = null) :
        this(IPAddress.Parse(ip), port, request)
    { }

    public ClientRequest(IRequestResolver resolver, IRequest? request = null)
    {
        _resolver = resolver;
        _request = request == null ? new Request() : new Request(request);
    }

    public int Id
    {
        get => _request.Id;
        set => _request.Id = value;
    }

    public IList<IResourceRecord> AdditionalRecords
    {
        get { return new ReadOnlyCollection<IResourceRecord>(_request.AdditionalRecords); }
    }

    public OperationCode OperationCode
    {
        get => _request.OperationCode;
        set => _request.OperationCode = value;
    }

    public bool RecursionDesired
    {
        get => _request.RecursionDesired;
        set => _request.RecursionDesired = value;
    }

    public IList<Question> Questions
    {
        get { return _request.Questions; }
    }

    public int Size
    {
        get { return _request.Size; }
    }

    public byte[] ToArray()
    {
        return _request.ToArray();
    }

    public override string? ToString()
    {
        return _request.ToString();
    }

    /// <summary>
    /// Resolves this request into a response using the provided DNS information. The given
    /// request strategy is used to retrieve the response.
    /// </summary>
    /// <exception cref="ResponseException">Throw if a malformed response is received from the server</exception>
    /// <exception cref="IOException">Thrown if a IO error occurs</exception>
    /// <exception cref="SocketException">Thrown if the reading or writing to the socket fails</exception>
    /// <exception cref="OperationCanceledException">Thrown if reading or writing to the socket timeouts</exception>
    /// <returns>The response received from server</returns>
    public async Task<IResponse> Resolve(CancellationToken cancellationToken = default)
    {
        try
        {
            IResponse? response = await _resolver.Resolve(this, cancellationToken).ConfigureAwait(false)
                ?? throw new ResponseException("Response is null");

            if (response.Id != Id)
            {
                throw new ResponseException(response, "Mismatching request/response IDs");
            }

            if (response.ResponseCode != ResponseCode.NoError)
            {
                throw new ResponseException(response);
            }

            return response;
        }
        catch (ArgumentException e)
        {
            throw new ResponseException("Invalid response", e);
        }
        catch (IndexOutOfRangeException e)
        {
            throw new ResponseException("Invalid response", e);
        }
    }
}
