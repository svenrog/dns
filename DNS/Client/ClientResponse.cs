using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DNS.Client;

public class ClientResponse : IResponse
{
    private readonly IResponse _response;
    private readonly byte[] _message;

    public static ClientResponse FromArray(IRequest request, byte[] message)
    {
        Response response = Response.FromArray(message);
        return new ClientResponse(request, response, message);
    }

    internal ClientResponse(IRequest request, IResponse response, byte[] message)
    {
        Request = request;

        _message = message;
        _response = response;
    }

    internal ClientResponse(IRequest request, IResponse response)
    {
        Request = request;

        _message = response.ToArray();
        _response = response;
    }

    public IRequest Request { get; }

    public int Id
    {
        get => _response.Id;
        set { throw new NotSupportedException(); }
    }

    public IList<IResourceRecord> AnswerRecords
    {
        get { return _response.AnswerRecords; }
    }

    public IList<IResourceRecord> AuthorityRecords
    {
        get { return new ReadOnlyCollection<IResourceRecord>(_response.AuthorityRecords); }
    }

    public IList<IResourceRecord> AdditionalRecords
    {
        get { return new ReadOnlyCollection<IResourceRecord>(_response.AdditionalRecords); }
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

    public OperationCode OperationCode
    {
        get => _response.OperationCode;
        set { throw new NotSupportedException(); }
    }

    public ResponseCode ResponseCode
    {
        get => _response.ResponseCode;
        set { throw new NotSupportedException(); }
    }

    public IList<Question> Questions
    {
        get { return new ReadOnlyCollection<Question>(_response.Questions); }
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
