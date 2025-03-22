using DNS.Protocol.Marshalling;
using DNS.Protocol.Serialization;
using System;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace DNS.Protocol.ResourceRecords
{
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

            Struct.GetBytes(head).CopyTo(data, 0);
            trg.CopyTo(data, Head.SIZE);

            return new ResourceRecord(domain, data, RecordType.SRV, RecordClass.IN, ttl);
        }

        public ServiceResourceRecord(IResourceRecord record, byte[] message, int dataOffset) : base(record)
        {
            Head head = Struct.GetStruct<Head>(message, dataOffset, Head.SIZE);

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

        [Marshalling.Endian(Marshalling.Endianness.Big)]
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct Head
        {
            public const int SIZE = 6;

            private ushort _priority;
            private ushort _weight;
            private ushort _port;

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
}
