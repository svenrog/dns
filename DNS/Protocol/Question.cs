using DNS.Protocol.Marshalling;
using DNS.Protocol.Serialization;
using DNS.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DNS.Protocol
{
    public class Question : IMessageEntry
    {
        public static IList<Question> GetAllFromArray(byte[] message, int offset, int questionCount)
        {
            return GetAllFromArray(message, offset, questionCount, out _);
        }

        public static IList<Question> GetAllFromArray(byte[] message, int offset, int questionCount, out int endOffset)
        {
            List<Question> questions = new(questionCount);

            for (int i = 0; i < questionCount; i++)
            {
                questions.Add(FromArray(message, offset, out offset));
            }

            endOffset = offset;
            return questions;
        }

        public static Question FromArray(byte[] message, int offset)
        {
            return FromArray(message, offset, out _);
        }

        public static Question FromArray(byte[] message, int offset, out int endOffset)
        {
            Domain domain = Domain.FromArray(message, offset, out offset);

            byte[] data = new byte[Tail.SIZE];
            Array.Copy(message, offset, data, 0, Tail.SIZE);

            Tail tail = Tail.CreateFromArray(data);

            endOffset = offset + Tail.SIZE;

            return new Question(domain, tail.Type, tail.Class);
        }

        private readonly Domain _domain;
        private readonly RecordType _type;
        private readonly RecordClass _class;

        public Question(Domain domain, RecordType type = RecordType.A, RecordClass @class = RecordClass.IN)
        {
            _domain = domain;
            _type = type;
            _class = @class;
        }

        public Domain Name
        {
            get { return _domain; }
        }

        public RecordType Type
        {
            get { return _type; }
        }

        public RecordClass Class
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
            ByteStream result = new(Size);

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

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, StringifierContext.Default.Question);
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
                {
                    throw new ArgumentException("Header length too small");
                }

                ConvertEndianness(tail);

                return Struct.PinStruct<Tail>(tail);
            }

            public readonly byte[] ToArray()
            {
                var stream = new ByteStream(SIZE);

                stream.Write(BitConverter.GetBytes(_type));
                stream.Write(BitConverter.GetBytes(_class));

                var buffer = stream.ToArray();

                ConvertEndianness(buffer);

                return buffer;
            }

            public RecordType Type
            {
                readonly get => (RecordType)_type; set => _type = (ushort)value;
            }

            public RecordClass Class
            {
                readonly get => (RecordClass)_class; set => _class = (ushort)value;
            }
        }
    }
}
