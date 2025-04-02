using DNS.Protocol.ResourceRecords;
using DNS.Protocol.Serialization;
using DNS.Protocol.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DNS.Protocol;

public class Response : IResponse
{
    private Header _header;
    private readonly IList<Question> _questions;
    private readonly IList<IResourceRecord> _answers;
    private readonly IList<IResourceRecord> _authority;
    private readonly IList<IResourceRecord> _additional;

    public static Response FromRequest(IRequest request)
    {
        Response response = new()
        {
            Id = request.Id
        };

        foreach (Question question in request.Questions)
        {
            response.Questions.Add(question);
        }

        return response;
    }

    public static Response FromArray(byte[] message)
    {
        Header header = Header.FromArray(message);
        int offset = Header.SIZE;

        if (!header.Response)
        {
            throw new ArgumentException("Invalid response message");
        }

        if (header.Truncated)
        {
            return new Response(header,
                Question.GetAllFromArray(message, offset, header.QuestionCount), [], [], []);
        }

        return new Response(header,
            Question.GetAllFromArray(message, offset, header.QuestionCount, out offset),
            ResourceRecordFactory.GetAllFromArray(message, offset, header.AnswerRecordCount, out offset),
            ResourceRecordFactory.GetAllFromArray(message, offset, header.AuthorityRecordCount, out offset),
            ResourceRecordFactory.GetAllFromArray(message, offset, header.AdditionalRecordCount, out _));
    }

    public Response(Header header, IList<Question> questions, IList<IResourceRecord> answers,
            IList<IResourceRecord> authority, IList<IResourceRecord> additional)
    {
        _header = header;
        _questions = questions;
        _answers = answers;
        _authority = authority;
        _additional = additional;
    }

    public Response()
    {
        _header = new Header();
        _questions = [];
        _answers = [];
        _authority = [];
        _additional = [];

        _header.Response = true;
    }

    public Response(IResponse response)
    {
        _header = new Header();
        _questions = [.. response.Questions];
        _answers = [.. response.AnswerRecords];
        _authority = [.. response.AuthorityRecords];
        _additional = [.. response.AdditionalRecords];

        _header.Response = true;

        Id = response.Id;
        RecursionAvailable = response.RecursionAvailable;
        AuthorativeServer = response.AuthorativeServer;
        OperationCode = response.OperationCode;
        ResponseCode = response.ResponseCode;
    }

    public IList<Question> Questions
    {
        get { return _questions; }
    }

    public IList<IResourceRecord> AnswerRecords
    {
        get { return _answers; }
    }

    public IList<IResourceRecord> AuthorityRecords
    {
        get { return _authority; }
    }

    public IList<IResourceRecord> AdditionalRecords
    {
        get { return _additional; }
    }

    public int Id
    {
        get => _header.Id; set => _header.Id = value;
    }

    public bool RecursionAvailable
    {
        get => _header.RecursionAvailable; set => _header.RecursionAvailable = value;
    }

    public bool AuthenticData
    {
        get => _header.AuthenticData; set => _header.AuthenticData = value;
    }

    public bool CheckingDisabled
    {
        get => _header.CheckingDisabled; set => _header.CheckingDisabled = value;
    }

    public bool AuthorativeServer
    {
        get => _header.AuthorativeServer; set => _header.AuthorativeServer = value;
    }

    public bool Truncated
    {
        get => _header.Truncated; set => _header.Truncated = value;
    }

    public OperationCode OperationCode
    {
        get => _header.OperationCode; set => _header.OperationCode = value;
    }

    public ResponseCode ResponseCode
    {
        get => _header.ResponseCode; set => _header.ResponseCode = value;
    }

    [JsonIgnore]
    public int Size
    {
        get
        {
            return Header.SIZE +
                _questions.Sum(q => q.Size) +
                _answers.Sum(a => a.Size) +
                _authority.Sum(a => a.Size) +
                _additional.Sum(a => a.Size);
        }
    }

    public byte[] ToArray()
    {
        UpdateHeader();
        ByteArrayBuilder builder = new(Size);

        builder
            .Append(_header.ToArray())
            .Append(_questions.Select(q => q.ToArray()))
            .Append(_answers.Select(a => a.ToArray()))
            .Append(_authority.Select(a => a.ToArray()))
            .Append(_additional.Select(a => a.ToArray()));

        return builder.Build();
    }

    public override string ToString()
    {
        UpdateHeader();

        return JsonSerializer.Serialize(this, StringifierContext.Default.Response);
    }

    private void UpdateHeader()
    {
        _header.QuestionCount = _questions.Count;
        _header.AnswerRecordCount = _answers.Count;
        _header.AuthorityRecordCount = _authority.Count;
        _header.AdditionalRecordCount = _additional.Count;
    }
}
