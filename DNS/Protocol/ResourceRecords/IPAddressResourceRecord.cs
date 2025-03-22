using DNS.Protocol.Serialization;
using System;
using System.Net;
using System.Text.Json;

namespace DNS.Protocol.ResourceRecords
{
    public class IPAddressResourceRecord : BaseResourceRecord
    {
        private static ResourceRecord Create(Domain domain, IPAddress ip, TimeSpan ttl)
        {
            byte[] data = ip.GetAddressBytes();
            RecordType type = data.Length == 4 ? RecordType.A : RecordType.AAAA;

            return new ResourceRecord(domain, data, type, RecordClass.IN, ttl);
        }

        public IPAddressResourceRecord(IResourceRecord record) : base(record)
        {
            IPAddress = new IPAddress(Data);
        }

        public IPAddressResourceRecord(Domain domain, IPAddress ip, TimeSpan ttl = default) :
            base(Create(domain, ip, ttl))
        {
            IPAddress = ip;
        }

        public IPAddress IPAddress { get; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, StringifierContext.Default.IPAddressResourceRecord);
        }
    }
}
