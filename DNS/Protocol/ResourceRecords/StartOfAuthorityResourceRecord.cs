using DNS.Protocol.Serialization;
using DNS.Protocol.Utils;
using System;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace DNS.Protocol.ResourceRecords
{
    public class StartOfAuthorityResourceRecord : BaseResourceRecord
    {
        private static ResourceRecord Create(Domain domain, Domain master, Domain responsible, long serial,
                TimeSpan refresh, TimeSpan retry, TimeSpan expire, TimeSpan minTtl, TimeSpan ttl)
        {
            ByteStream data = new(Options.SIZE + master.Size + responsible.Size);
            Options tail = new()
            {
                SerialNumber = serial,
                RefreshInterval = refresh,
                RetryInterval = retry,
                ExpireInterval = expire,
                MinimumTimeToLive = minTtl
            };

            data
                .Append(master.ToArray())
                .Append(responsible.ToArray())
                .Append(Marshalling.Struct.GetBytes(tail));

            return new ResourceRecord(domain, data.ToArray(), RecordType.SOA, RecordClass.IN, ttl);
        }

        public StartOfAuthorityResourceRecord(IResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            MasterDomainName = Domain.FromArray(message, dataOffset, out dataOffset);
            ResponsibleDomainName = Domain.FromArray(message, dataOffset, out dataOffset);

            Options tail = Marshalling.Struct.GetStruct<Options>(message, dataOffset, Options.SIZE);

            SerialNumber = tail.SerialNumber;
            RefreshInterval = tail.RefreshInterval;
            RetryInterval = tail.RetryInterval;
            ExpireInterval = tail.ExpireInterval;
            MinimumTimeToLive = tail.MinimumTimeToLive;
        }

        public StartOfAuthorityResourceRecord(Domain domain, Domain master, Domain responsible, long serial,
                TimeSpan refresh, TimeSpan retry, TimeSpan expire, TimeSpan minTtl, TimeSpan ttl = default) :
            base(Create(domain, master, responsible, serial, refresh, retry, expire, minTtl, ttl))
        {
            MasterDomainName = master;
            ResponsibleDomainName = responsible;

            SerialNumber = serial;
            RefreshInterval = refresh;
            RetryInterval = retry;
            ExpireInterval = expire;
            MinimumTimeToLive = minTtl;
        }

        public StartOfAuthorityResourceRecord(Domain domain, Domain master, Domain responsible,
                Options options = default, TimeSpan ttl = default) :
            this(domain, master, responsible, options.SerialNumber, options.RefreshInterval, options.RetryInterval,
                    options.ExpireInterval, options.MinimumTimeToLive, ttl)
        { }

        public Domain MasterDomainName { get; }
        public Domain ResponsibleDomainName { get; }
        public long SerialNumber { get; }
        public TimeSpan RefreshInterval { get; }
        public TimeSpan RetryInterval { get; }
        public TimeSpan ExpireInterval { get; }
        public TimeSpan MinimumTimeToLive { get; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, StringifierContext.Default.StartOfAuthorityResourceRecord);
        }

        [Marshalling.Endian(Marshalling.Endianness.Big)]
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Options
        {
            public const int SIZE = 20;

            private uint _serialNumber;
            private uint _refreshInterval;
            private uint _retryInterval;
            private uint _expireInterval;
            private uint _ttl;

            public long SerialNumber
            {
                readonly get => _serialNumber; set => _serialNumber = (uint)value;
            }

            public TimeSpan RefreshInterval
            {
                readonly get => TimeSpan.FromSeconds(_refreshInterval); set => _refreshInterval = (uint)value.TotalSeconds;
            }

            public TimeSpan RetryInterval
            {
                readonly get => TimeSpan.FromSeconds(_retryInterval); set => _retryInterval = (uint)value.TotalSeconds;
            }

            public TimeSpan ExpireInterval
            {
                readonly get => TimeSpan.FromSeconds(_expireInterval); set => _expireInterval = (uint)value.TotalSeconds;
            }

            public TimeSpan MinimumTimeToLive
            {
                readonly get => TimeSpan.FromSeconds(_ttl); set => _ttl = (uint)value.TotalSeconds;
            }
        }
    }
}
