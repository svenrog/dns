using DNS.Benchmark.Baseline.Protocol.Marshalling;
using DNS.Benchmark.Baseline.Protocol.Utils;
using DNS.Protocol.ResourceRecords;
using System.Runtime.InteropServices;

namespace DNS.Benchmark.Baseline.Protocol.ResourceRecords
{
    public class BaselineResourceRecord(
        BaselineDomain domain,
        byte[] data,
        BaselineRecordType type,
        BaselineRecordClass @class = BaselineRecordClass.IN,
        TimeSpan ttl = default)
        : IBaselineResourceRecord
    {
        private readonly BaselineDomain _domain = domain;
        private readonly BaselineRecordType _type = type;
        private readonly BaselineRecordClass _class = @class;
        private readonly TimeSpan _ttl = ttl;
        private readonly byte[] _data = data;

        public static IList<BaselineResourceRecord> GetAllFromArray(byte[] message, int offset, int count)
        {
            return GetAllFromArray(message, offset, count, out _);
        }

        public static IList<BaselineResourceRecord> GetAllFromArray(byte[] message, int offset, int count, out int endOffset)
        {
            List<BaselineResourceRecord> records = new(count);

            for (int i = 0; i < count; i++)
                records.Add(FromArray(message, offset, out offset));

            endOffset = offset;
            return records;
        }

        public static BaselineResourceRecord FromArray(byte[] message, int offset)
        {
            return FromArray(message, offset, out _);
        }

        public static BaselineResourceRecord FromArray(byte[] message, int offset, out int endOffset)
        {
            BaselineDomain domain = BaselineDomain.FromArray(message, offset, out offset);

            byte[] data = new byte[Tail.SIZE];
            Array.Copy(message, offset, data, 0, data.Length);

            Tail tail = Tail.CreateFromArray(data);

            data = new byte[tail.DataLength];
            offset += Tail.SIZE;
            Array.Copy(message, offset, data, 0, data.Length);

            endOffset = offset + data.Length;

            return new BaselineResourceRecord(domain, data, tail.Type, tail.Class, tail.TimeToLive);
        }

        public static BaselineResourceRecord FromQuestion(BaselineQuestion question, byte[] data, TimeSpan ttl = default)
        {
            return new BaselineResourceRecord(question.Name, data, question.Type, question.Class, ttl);
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

        public TimeSpan TimeToLive
        {
            get { return _ttl; }
        }

        public int DataLength
        {
            get { return _data.Length; }
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public int Size
        {
            get { return _domain.Size + Tail.SIZE + _data.Length; }
        }

        public byte[] ToArray()
        {
            BaselineByteStream result = new(Size);

            Tail tail = new()
            {
                Type = Type,
                Class = Class,
                TimeToLive = _ttl,
                DataLength = _data.Length
            };

            result
                .Append(_domain.ToArray())
                .Append(tail.ToArray())
                .Append(_data);

            return result.ToArray();
        }

        // Endianness.Big
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        internal struct Tail
        {
            public const int SIZE = 10;

            private ushort _type;
            private ushort _class;
            private uint _ttl;
            private ushort _dataLength;

            private static void ConvertEndianness(byte[] bytes)
            {
                if (!BitConverter.IsLittleEndian) return;

                // Manual endian conversion
                Array.Reverse(bytes, 0, sizeof(ushort));
                Array.Reverse(bytes, 2, sizeof(ushort));
                Array.Reverse(bytes, 4, sizeof(uint));
                Array.Reverse(bytes, 8, sizeof(ushort));
            }

            public static Tail CreateFromArray(byte[] tail)
            {
                if (tail.Length < SIZE)
                    throw new ArgumentException("Tail length too small");

                ConvertEndianness(tail);

                return BaselineStruct.PinStruct<Tail>(tail);
            }

            public readonly byte[] ToArray()
            {
                var stream = new BaselineByteStream(SIZE);

                stream.Write(BitConverter.GetBytes(_type));
                stream.Write(BitConverter.GetBytes(_class));
                stream.Write(BitConverter.GetBytes(_ttl));
                stream.Write(BitConverter.GetBytes(_dataLength));

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

            public TimeSpan TimeToLive
            {
                readonly get => TimeSpan.FromSeconds(_ttl); set => _ttl = (uint)value.TotalSeconds;
            }

            public int DataLength
            {
                readonly get => _dataLength; set => _dataLength = (ushort)value;
            }
        }
    }
}
