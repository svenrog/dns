using DNS.Benchmark.Baseline.Protocol.Marshalling;
using DNS.Benchmark.Baseline.Protocol.Utils;
using DNS.Protocol.ResourceRecords;
using System.Runtime.InteropServices;

namespace DNS.Benchmark.Baseline.Protocol.ResourceRecords;

public class BaselineServiceResourceRecord : BaselineBaseResourceRecord
{
    private static BaselineResourceRecord Create(BaselineDomain domain, ushort priority, ushort weight, ushort port, BaselineDomain target, TimeSpan ttl)
    {
        byte[] trg = target.ToArray();
        byte[] data = new byte[Head.SIZE + trg.Length];

        Head head = new()
        {
            Priority = priority,
            Weight = weight,
            Port = port
        };

        head.ToArray().CopyTo(data, 0);
        trg.CopyTo(data, Head.SIZE);

        return new BaselineResourceRecord(domain, data, BaselineRecordType.SRV, BaselineRecordClass.IN, ttl);
    }

    public BaselineServiceResourceRecord(IBaselineResourceRecord record, byte[] message, int dataOffset) : base(record)
    {
        var data = new byte[Head.SIZE];
        Array.Copy(message, dataOffset, data, 0, data.Length);
        Head head = Head.FromArray(data);

        Priority = head.Priority;
        Weight = head.Weight;
        Port = head.Port;
        Target = BaselineDomain.FromArray(message, dataOffset + Head.SIZE);
    }

    public BaselineServiceResourceRecord(BaselineDomain domain, ushort priority, ushort weight, ushort port, BaselineDomain target, TimeSpan ttl = default) :
            base(Create(domain, priority, weight, port, target, ttl))
    {
        Priority = priority;
        Weight = weight;
        Port = port;
        Target = target;
    }

    public ushort Priority { get; }
    public ushort Weight { get; }
    public ushort Port { get; }
    public BaselineDomain Target { get; }

    // Endianness.Big
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct Head
    {
        public const int SIZE = 6;

        private ushort _priority;
        private ushort _weight;
        private ushort _port;

        public static Head FromArray(byte[] head)
        {
            if (head.Length < SIZE)
                throw new ArgumentException("Head length too small");

            ConvertEndianness(head);

            return BaselineStruct.GetStruct<Head>(head, 0, SIZE);
        }

        public readonly byte[] ToArray()
        {
            var stream = new BaselineByteStream(SIZE);

            stream.Write(BitConverter.GetBytes(_priority));
            stream.Write(BitConverter.GetBytes(_weight));
            stream.Write(BitConverter.GetBytes(_port));

            var buffer = stream.ToArray();

            ConvertEndianness(buffer);

            return buffer;
        }

        private static void ConvertEndianness(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian) return;

            // Manual endian conversion
            Array.Reverse(bytes, 0, sizeof(ushort));
            Array.Reverse(bytes, 2, sizeof(ushort));
            Array.Reverse(bytes, 4, sizeof(ushort));
        }

        public ushort Priority
        {
            readonly get => _priority; set => _priority = value;
        }

        public ushort Weight
        {
            readonly get => _weight; set => _weight = value;
        }

        public ushort Port
        {
            readonly get => _port; set => _port = value;
        }
    }
}
