using DNS.Benchmark.Baseline.Protocol;
using DNS.Protocol.ResourceRecords;
using System.Collections.ObjectModel;

namespace DNS.Benchmark.Baseline.Client;

public class BaselineClientResponse : IBaselineResponse
{
    private readonly IBaselineResponse _response;
    private readonly byte[] _message;

    public static BaselineClientResponse FromArray(IBaselineRequest request, byte[] message)
    {
        BaselineResponse response = BaselineResponse.FromArray(message);
        return new BaselineClientResponse(request, response, message);
    }

    internal BaselineClientResponse(IBaselineRequest request, IBaselineResponse response, byte[] message)
    {
        Request = request;

        _message = message;
        _response = response;
    }

    internal BaselineClientResponse(IBaselineRequest request, IBaselineResponse response)
    {
        Request = request;

        _message = response.ToArray();
        _response = response;
    }

    public IBaselineRequest Request { get; }

    public int Id
    {
        get => _response.Id;
        set { throw new NotSupportedException(); }
    }

    public IList<IBaselineResourceRecord> AnswerRecords
    {
        get { return _response.AnswerRecords; }
    }

    public IList<IBaselineResourceRecord> AuthorityRecords
    {
        get { return new ReadOnlyCollection<IBaselineResourceRecord>(_response.AuthorityRecords); }
    }

    public IList<IBaselineResourceRecord> AdditionalRecords
    {
        get { return new ReadOnlyCollection<IBaselineResourceRecord>(_response.AdditionalRecords); }
    }

    public bool RecursionAvailable
    {
        get => _response.RecursionAvailable;
        set { throw new NotSupportedException(); }
    }

    public bool AuthenticData
    {
        get => _response.AuthenticData;
        set { throw new NotSupportedException(); }
    }

    public bool CheckingDisabled
    {
        get => _response.CheckingDisabled;
        set { throw new NotSupportedException(); }
    }

    public bool AuthorativeServer
    {
        get => _response.AuthorativeServer;
        set { throw new NotSupportedException(); }
    }

    public bool Truncated
    {
        get => _response.Truncated;
        set { throw new NotSupportedException(); }
    }

    public BaselineOperationCode OperationCode
    {
        get => _response.OperationCode;
        set { throw new NotSupportedException(); }
    }

    public BaselineResponseCode ResponseCode
    {
        get => _response.ResponseCode;
        set { throw new NotSupportedException(); }
    }

    public IList<BaselineQuestion> Questions
    {
        get { return new ReadOnlyCollection<BaselineQuestion>(_response.Questions); }
    }

    public int Size
    {
        get { return _message.Length; }
    }

    public byte[] ToArray()
    {
        return _message;
    }

    public override string? ToString()
    {
        return _response.ToString();
    }
}
