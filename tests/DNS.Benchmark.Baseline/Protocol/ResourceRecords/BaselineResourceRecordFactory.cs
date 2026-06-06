using DNS.Protocol.ResourceRecords;

namespace DNS.Benchmark.Baseline.Protocol.ResourceRecords;

public static class BaselineResourceRecordFactory
{
    public static IList<IBaselineResourceRecord> GetAllFromArray(byte[] message, int offset, int count)
    {
        return GetAllFromArray(message, offset, count, out _);
    }

    public static IList<IBaselineResourceRecord> GetAllFromArray(byte[] message, int offset, int count, out int endOffset)
    {
        List<IBaselineResourceRecord> result = new(count);

        for (int i = 0; i < count; i++)
            result.Add(FromArray(message, offset, out offset));

        endOffset = offset;
        return result;
    }

    public static IBaselineResourceRecord FromArray(byte[] message, int offset)
    {
        return FromArray(message, offset, out _);
    }

    public static IBaselineResourceRecord FromArray(byte[] message, int offset, out int endOffset)
    {
        BaselineResourceRecord record = BaselineResourceRecord.FromArray(message, offset, out endOffset);
        int dataOffset = endOffset - record.DataLength;

        return record.Type switch
        {
            BaselineRecordType.A or BaselineRecordType.AAAA => new BaselineIPAddressResourceRecord(record),
            BaselineRecordType.NS => new BaselineNameServerResourceRecord(record, message, dataOffset),
            BaselineRecordType.CNAME => new BaselineCanonicalNameResourceRecord(record, message, dataOffset),
            BaselineRecordType.SOA => new BaselineStartOfAuthorityResourceRecord(record, message, dataOffset),
            BaselineRecordType.PTR => new BaselinePointerResourceRecord(record, message, dataOffset),
            BaselineRecordType.MX => new BaselineMailExchangeResourceRecord(record, message, dataOffset),
            BaselineRecordType.TXT => new BaselineTextResourceRecord(record),
            BaselineRecordType.SRV => new BaselineServiceResourceRecord(record, message, dataOffset),
            _ => record,
        };
    }
}
