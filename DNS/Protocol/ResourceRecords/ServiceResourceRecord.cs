using DNS.Protocol.Serialization;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace DNS.Protocol.ResourceRecords;

public class ServiceResourceRecord : BaseResourceRecord
{
    private static ResourceRecord Create(Domain domain, ushort priority, ushort weight, ushort port, Domain target, TimeSpan ttl)
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

        return new ResourceRecord(domain, data, RecordType.SRV, RecordClass.IN, ttl);
    }

    public ServiceResourceRecord(IResourceRecord record, byte[] message, int dataOffset) : base(record)
    {
        var data = new byte[Head.SIZE];
        Array.Copy(message, dataOffset, data, 0, data.Length);
        Head head = Head.FromArray(data);

        Priority = head.Priority;
        Weight = head.Weight;
        Port = head.Port;
        Target = Domain.FromArray(message, dataOffset + Head.SIZE);
    }

    public ServiceResourceRecord(Domain domain, ushort priority, ushort weight, ushort port, Domain target, TimeSpan ttl = default) :
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
    public Domain Target { get; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, StringifierContext.Default.ServiceResourceRecord);
    }

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
            {
                throw new ArgumentException("Head length too small");
            }

            ConvertEndianness(head);

            return MemoryMarshal.Read<Head>(head);
        }

        public readonly byte[] ToArray()
        {
            Span<byte> span = stackalloc byte[SIZE];

            Unsafe.As<byte, ushort>(ref span[0]) = _priority;
            Unsafe.As<byte, ushort>(ref span[2]) = _weight;
            Unsafe.As<byte, ushort>(ref span[4]) = _port;

            var buffer = span.ToArray();

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
