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
            int size = Header.SIZE;

            for (int i = 0; i < _questions.Count; i++) size += _questions[i].Size;
            for (int i = 0; i < _additional.Count; i++) size += _additional[i].Size;

            return size;
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

        byte[] result = new byte[Size];
        Span<byte> span = result;
        int offset = 0;

        _header.WriteTo(span);
        offset += Header.SIZE;

        for (int i = 0; i < _questions.Count; i++)
        {
            Question q = _questions[i];
            q.WriteTo(span[offset..]);
            offset += q.Size;
        }

        for (int i = 0; i < _additional.Count; i++)
        {
            byte[] bytes = _additional[i].ToArray();
            bytes.CopyTo(span[offset..]);
            offset += bytes.Length;
        }

        return result;
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
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        _random.GetBytes(buffer);

        return BitConverter.ToUInt16(buffer);
    }
}
