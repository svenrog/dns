using DNS.Benchmark.Baseline.Client.RequestResolver;
using DNS.Benchmark.Baseline.Protocol;
using DNS.Client;
using DNS.Protocol.ResourceRecords;
using System.Collections.ObjectModel;
using System.Net;

namespace DNS.Benchmark.Baseline.Client;

public class BaselineClientRequest : IBaselineRequest
{
    private readonly IBaselineRequestResolver _resolver;
    private readonly BaselineRequest _request;

    public BaselineClientRequest(IPEndPoint dns, IBaselineRequest? request = null) :
        this(new BaselineUdpRequestResolver(dns), request)
    { }

    public BaselineClientRequest(IPAddress ip, int port = 53, IBaselineRequest? request = null) :
        this(new IPEndPoint(ip, port), request)
    { }

    public BaselineClientRequest(string ip, int port = 53, IBaselineRequest? request = null) :
        this(IPAddress.Parse(ip), port, request)
    { }

    public BaselineClientRequest(IBaselineRequestResolver resolver, IBaselineRequest? request = null)
    {
        _resolver = resolver;
        _request = request == null ? new BaselineRequest() : new BaselineRequest(request);
    }

    public int Id
    {
        get => _request.Id;
        set => _request.Id = value;
    }

    public IList<IBaselineResourceRecord> AdditionalRecords
    {
        get { return new ReadOnlyCollection<IBaselineResourceRecord>(_request.AdditionalRecords); }
    }

    public BaselineOperationCode OperationCode
    {
        get => _request.OperationCode;
        set => _request.OperationCode = value;
    }

    public bool RecursionDesired
    {
        get => _request.RecursionDesired;
        set => _request.RecursionDesired = value;
    }

    public IList<BaselineQuestion> Questions
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
    public async Task<IBaselineResponse> Resolve(CancellationToken cancellationToken = default)
    {
        try
        {
            IBaselineResponse? response = await _resolver.Resolve(this, cancellationToken).ConfigureAwait(false)
                ?? throw new ResponseException("Response is null");

            if (response.Id != Id)
                throw new Exception("Mismatching request/response IDs");

            if (response.ResponseCode != BaselineResponseCode.NoError)
                throw new Exception($"Error code {response.ResponseCode}");

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
