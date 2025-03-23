using DNS.Benchmark.Baseline.Protocol.Marshalling;
using DNS.Benchmark.Baseline.Protocol.Utils;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace DNS.Benchmark.Baseline.Protocol;

public class BaselineQuestion : IBaselineMessageEntry
{
    public static IList<BaselineQuestion> GetAllFromArray(byte[] message, int offset, int questionCount)
    {
        return GetAllFromArray(message, offset, questionCount, out _);
    }

    public static IList<BaselineQuestion> GetAllFromArray(byte[] message, int offset, int questionCount, out int endOffset)
    {
        List<BaselineQuestion> questions = new(questionCount);

        for (int i = 0; i < questionCount; i++)
            questions.Add(FromArray(message, offset, out offset));

        endOffset = offset;
        return questions;
    }

    public static BaselineQuestion FromArray(byte[] message, int offset)
    {
        return FromArray(message, offset, out _);
    }

    public static BaselineQuestion FromArray(byte[] message, int offset, out int endOffset)
    {
        BaselineDomain domain = BaselineDomain.FromArray(message, offset, out offset);

        byte[] data = new byte[Tail.SIZE];
        Array.Copy(message, offset, data, 0, Tail.SIZE);

        Tail tail = Tail.CreateFromArray(data);

        endOffset = offset + Tail.SIZE;

        return new BaselineQuestion(domain, tail.Type, tail.Class);
    }

    private readonly BaselineDomain _domain;
    private readonly BaselineRecordType _type;
    private readonly BaselineRecordClass _class;

    public BaselineQuestion(BaselineDomain domain, BaselineRecordType type = BaselineRecordType.A, BaselineRecordClass @class = BaselineRecordClass.IN)
    {
        _domain = domain;
        _type = type;
        _class = @class;
    }

    public BaselineDomain Name
    {
        get { return _domain; }
    }

    public BaselineRecordType Type
    {
        get { return _type; }
    }

    public BaselineRecordClass Class
    {
        get { return _class; }
    }

    [JsonIgnore]
    public int Size
    {
        get { return _domain.Size + Tail.SIZE; }
    }


    public byte[] ToArray()
    {
        BaselineByteStream result = new(Size);

        Tail tail = new()
        {
            Type = Type,
            Class = Class
        };

        result
            .Append(_domain.ToArray())
            .Append(tail.ToArray());

        return result.ToArray();
    }

    // Endianness.Big
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct Tail
    {
        public const int SIZE = 4;

        private ushort _type;
        private ushort _class;

        private static void ConvertEndianness(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian) return;

            // Manual endian conversion
            Array.Reverse(bytes, 0, sizeof(ushort));
            Array.Reverse(bytes, 2, sizeof(ushort));
        }

        public static Tail CreateFromArray(byte[] tail)
        {
            if (tail.Length < SIZE)
                throw new ArgumentException("Header length too small");

            ConvertEndianness(tail);

            return BaselineStruct.PinStruct<Tail>(tail);
        }

        public readonly byte[] ToArray()
        {
            var stream = new BaselineByteStream(SIZE);

            stream.Write(BitConverter.GetBytes(_type));
            stream.Write(BitConverter.GetBytes(_class));

            var buffer = stream.ToArray();

            ConvertEndianness(buffer);

            return buffer;
        }

        public BaselineRecordType Type
        {
            readonly get => (BaselineRecordType)_type; set => _type = (ushort)value;
        }

        public BaselineRecordClass Class
        {
            readonly get => (BaselineRecordClass)_class; set => _class = (ushort)value;
        }
    }
}
