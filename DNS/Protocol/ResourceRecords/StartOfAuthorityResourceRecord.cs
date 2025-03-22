using DNS.Protocol.Marshalling;
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
                .Append(tail.ToArray());

            return new ResourceRecord(domain, data.ToArray(), RecordType.SOA, RecordClass.IN, ttl);
        }

        public StartOfAuthorityResourceRecord(IResourceRecord record, byte[] message, int dataOffset)
            : base(record)
        {
            MasterDomainName = Domain.FromArray(message, dataOffset, out dataOffset);
            ResponsibleDomainName = Domain.FromArray(message, dataOffset, out dataOffset);

            var data = new byte[Options.SIZE];
            Array.Copy(message, dataOffset, data, 0, data.Length);

            Options tail = Options.FromArray(data);

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

        // Endianness.Big
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Options
        {
            public const int SIZE = 20;

            private uint _serialNumber;
            private uint _refreshInterval;
            private uint _retryInterval;
            private uint _expireInterval;
            private uint _ttl;

            public static Options FromArray(byte[] options)
            {
                if (options.Length < SIZE)
                {
                    throw new ArgumentException("Options length too small");
                }

                ConvertEndianness(options);

                return Struct.PinStruct<Options>(options);
            }

            public readonly byte[] ToArray()
            {
                var stream = new ByteStream(SIZE);

                stream.Write(BitConverter.GetBytes(_serialNumber));
                stream.Write(BitConverter.GetBytes(_refreshInterval));
                stream.Write(BitConverter.GetBytes(_retryInterval));
                stream.Write(BitConverter.GetBytes(_expireInterval));
                stream.Write(BitConverter.GetBytes(_ttl));

                var buffer = stream.ToArray();

                ConvertEndianness(buffer);

                return buffer;
            }

            private static void ConvertEndianness(byte[] bytes)
            {
                if (!BitConverter.IsLittleEndian) return;

                // Manual endian conversion
                Array.Reverse(bytes, 0, sizeof(uint));
                Array.Reverse(bytes, 4, sizeof(uint));
                Array.Reverse(bytes, 8, sizeof(uint));
                Array.Reverse(bytes, 12, sizeof(uint));
                Array.Reverse(bytes, 16, sizeof(uint));
            }

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
