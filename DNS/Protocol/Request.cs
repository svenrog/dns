using DNS.Protocol.ResourceRecords;
using DNS.Protocol.Serialization;
using DNS.Protocol.Utils;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DNS.Protocol;

public class Request : IRequest
{
    private static readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();

    private Header _header;
    private readonly IList<Question> _questions;

    private readonly IList<IResourceRecord> _additional;

    public static Request FromArray(byte[] message)
    {
        Header header = Header.FromArray(message);
        int offset = Header.SIZE;

        if (header.Response || header.QuestionCount == 0 ||
                header.AnswerRecordCount + header.AuthorityRecordCount > 0 ||
                header.ResponseCode != ResponseCode.NoError)
        {

            throw new ArgumentException("Invalid request message");
        }

        return new Request(header,
            Question.GetAllFromArray(message, offset, header.QuestionCount, out offset),
            ResourceRecordFactory.GetAllFromArray(message, offset, header.AdditionalRecordCount, out _));
    }

    public Request(Header header, IList<Question> questions, IList<IResourceRecord> additional)
    {
        _header = header;
        _questions = questions;
        _additional = additional;
    }

    public Request()
    {
        _questions = [];
        _header = new Header();
        _additional = [];

        _header.OperationCode = OperationCode.Query;
        _header.Response = false;
        _header.Id = NextRandomId();
    }

    public Request(IRequest request)
    {
        _header = new Header();
        _questions = [.. request.Questions];
        _additional = [.. request.AdditionalRecords];

        _header.Response = false;

        Id = request.Id;
        OperationCode = request.OperationCode;
        RecursionDesired = request.RecursionDesired;
    }

    public IList<Question> Questions
    {
        get { return _questions; }
    }

    public IList<IResourceRecord> AdditionalRecords
    {
        get { return _additional; }
    }

    [JsonIgnore]
    public int Size
    {
        get
        {
            return Header.SIZE +
                _questions.Sum(q => q.Size) +
                _additional.Sum(a => a.Size);
        }
    }

    public int Id
    {
        get => _header.Id; set => _header.Id = value;
    }

    public OperationCode OperationCode
    {
        get => _header.OperationCode; set => _header.OperationCode = value;
    }

    public bool RecursionDesired
    {
        get => _header.RecursionDesired; set => _header.RecursionDesired = value;
    }

    public byte[] ToArray()
    {
        UpdateHeader();
        ByteArrayBuilder builder = new(Size);

        builder
            .Append(_header.ToArray())
            .Append(_questions.Select(q => q.ToArray()))
            .Append(_additional.Select(a => a.ToArray()));

        return builder.Build();
    }

    public override string ToString()
    {
        UpdateHeader();

        return JsonSerializer.Serialize(this, StringifierContext.Default.Request);
    }

    private void UpdateHeader()
    {
        _header.QuestionCount = _questions.Count;
        _header.AdditionalRecordCount = _additional.Count;
    }

    private static ushort NextRandomId()
    {
        byte[] buffer = new byte[sizeof(ushort)];
        _random.GetBytes(buffer);

        return BitConverter.ToUInt16(buffer, 0);
    }
}
