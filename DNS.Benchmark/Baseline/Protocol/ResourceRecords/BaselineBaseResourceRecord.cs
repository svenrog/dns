using DNS.Protocol.ResourceRecords;

namespace DNS.Benchmark.Baseline.Protocol.ResourceRecords;

public abstract class BaselineBaseResourceRecord(IBaselineResourceRecord record) : IBaselineResourceRecord
{
    private readonly IBaselineResourceRecord _record = record;

    public BaselineDomain Name
    {
        get { return _record.Name; }
    }

    public BaselineRecordType Type
    {
        get { return _record.Type; }
    }

    public BaselineRecordClass Class
    {
        get { return _record.Class; }
    }

    public TimeSpan TimeToLive
    {
        get { return _record.TimeToLive; }
    }

    public int DataLength
    {
        get { return _record.DataLength; }
    }

    public byte[] Data
    {
        get { return _record.Data; }
    }

    public int Size
    {
        get { return _record.Size; }
    }

    public byte[] ToArray()
    {
        return _record.ToArray();
    }
}
