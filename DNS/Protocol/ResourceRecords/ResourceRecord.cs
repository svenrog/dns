using DNS.Protocol.Marshalling;
using DNS.Protocol.Serialization;
using DNS.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DNS.Protocol.ResourceRecords
{
    public class ResourceRecord(
        Domain domain,
        byte[] data,
        RecordType type,
        RecordClass @class = RecordClass.IN,
        TimeSpan ttl = default)
        : IResourceRecord
    {
        private readonly Domain _domain = domain;
        private readonly RecordType _type = type;
        private readonly RecordClass _class = @class;
        private readonly TimeSpan _ttl = ttl;
        private readonly byte[] _data = data;

        public static IList<ResourceRecord> GetAllFromArray(byte[] message, int offset, int count)
        {
            return GetAllFromArray(message, offset, count, out _);
        }

        public static IList<ResourceRecord> GetAllFromArray(byte[] message, int offset, int count, out int endOffset)
        {
            List<ResourceRecord> records = new(count);

            for (int i = 0; i < count; i++)
            {
                records.Add(FromArray(message, offset, out offset));
            }

            endOffset = offset;
            return records;
        }

        public static ResourceRecord FromArray(byte[] message, int offset)
        {
            return FromArray(message, offset, out _);
        }

        public static ResourceRecord FromArray(byte[] message, int offset, out int endOffset)
        {
            Domain domain = Domain.FromArray(message, offset, out offset);
            Tail tail = Struct.GetStruct<Tail>(message, offset, Tail.SIZE);

            byte[] data = new byte[tail.DataLength];

            offset += Tail.SIZE;
            Array.Copy(message, offset, data, 0, data.Length);

            endOffset = offset + data.Length;

            return new ResourceRecord(domain, data, tail.Type, tail.Class, tail.TimeToLive);
        }

        public static ResourceRecord FromQuestion(Question question, byte[] data, TimeSpan ttl = default)
        {
            return new ResourceRecord(question.Name, data, question.Type, question.Class, ttl);
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

        [JsonIgnore]
        public int Size
        {
            get { return _domain.Size + Tail.SIZE + _data.Length; }
        }

        public byte[] ToArray()
        {
            ByteStream result = new(Size);

            result
                .Append(_domain.ToArray())
                .Append(Struct.GetBytes(new Tail()
                {
                    Type = Type,
                    Class = Class,
                    TimeToLive = _ttl,
                    DataLength = _data.Length
                }))
                .Append(_data);

            return result.ToArray();
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, StringifierContext.Default.ResourceRecord);
        }

        [Endian(Endianness.Big)]
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct Tail
        {
            public const int SIZE = 10;

            private ushort _type;
            private ushort _class;
            private uint _ttl;
            private ushort _dataLength;

            public RecordType Type
            {
                readonly get => (RecordType)_type; set => _type = (ushort)value;
            }

            public RecordClass Class
            {
                readonly get => (RecordClass)_class; set => _class = (ushort)value;
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
