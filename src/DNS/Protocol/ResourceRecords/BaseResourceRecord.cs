using System.Text.Json.Serialization;

namespace DNS.Protocol.ResourceRecords;

public abstract class BaseResourceRecord(IResourceRecord record) : IResourceRecord
{
    private readonly IResourceRecord _record = record;

    public Domain Name
    {
        get { return _record.Name; }
    }

    public RecordType Type
    {
        get { return _record.Type; }
    }

    public RecordClass Class
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

    [JsonIgnore]
    public int Size
    {
        get { return _record.Size; }
    }

    public byte[] ToArray()
    {
        return _record.ToArray();
    }
}
