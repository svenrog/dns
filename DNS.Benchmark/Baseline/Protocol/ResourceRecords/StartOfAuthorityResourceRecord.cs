using DNS.Benchmark.Baseline.Protocol.Marshalling;
using DNS.Benchmark.Baseline.Protocol.Utils;
using DNS.Protocol.ResourceRecords;
using System.Runtime.InteropServices;

namespace DNS.Benchmark.Baseline.Protocol.ResourceRecords;

public class StartOfAuthorityResourceRecord : BaselineBaseResourceRecord
{
    private static BaselineResourceRecord Create(BaselineDomain domain, BaselineDomain master, BaselineDomain responsible, long serial,
            TimeSpan refresh, TimeSpan retry, TimeSpan expire, TimeSpan minTtl, TimeSpan ttl)
    {
        BaselineByteStream data = new(Options.SIZE + master.Size + responsible.Size);

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

        return new BaselineResourceRecord(domain, data.ToArray(), BaselineRecordType.SOA, BaselineRecordClass.IN, ttl);
    }

    public StartOfAuthorityResourceRecord(IBaselineResourceRecord record, byte[] message, int dataOffset)
        : base(record)
    {
        MasterDomainName = BaselineDomain.FromArray(message, dataOffset, out dataOffset);
        ResponsibleDomainName = BaselineDomain.FromArray(message, dataOffset, out dataOffset);

        var data = new byte[Options.SIZE];
        Array.Copy(message, dataOffset, data, 0, data.Length);

        Options tail = Options.FromArray(data);

        SerialNumber = tail.SerialNumber;
        RefreshInterval = tail.RefreshInterval;
        RetryInterval = tail.RetryInterval;
        ExpireInterval = tail.ExpireInterval;
        MinimumTimeToLive = tail.MinimumTimeToLive;
    }

    public StartOfAuthorityResourceRecord(BaselineDomain domain, BaselineDomain master, BaselineDomain responsible, long serial,
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

    public StartOfAuthorityResourceRecord(BaselineDomain domain, BaselineDomain master, BaselineDomain responsible,
            Options options = default, TimeSpan ttl = default) :
        this(domain, master, responsible, options.SerialNumber, options.RefreshInterval, options.RetryInterval,
                options.ExpireInterval, options.MinimumTimeToLive, ttl)
    { }

    public BaselineDomain MasterDomainName { get; }
    public BaselineDomain ResponsibleDomainName { get; }
    public long SerialNumber { get; }
    public TimeSpan RefreshInterval { get; }
    public TimeSpan RetryInterval { get; }
    public TimeSpan ExpireInterval { get; }
    public TimeSpan MinimumTimeToLive { get; }

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
                throw new ArgumentException("Options length too small");

            ConvertEndianness(options);

            return BaselineStruct.GetStruct<Options>(options, 0, SIZE);
        }

        public readonly byte[] ToArray()
        {
            var stream = new BaselineByteStream(SIZE);

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
