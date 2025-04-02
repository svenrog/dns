using DNS.Protocol.Utils;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;

namespace DNS.Protocol;

public class Domain : IComparable<Domain?>
{
    private const int _compressionIterationMax = 1000;

    private readonly string[] _labels;
    private readonly int _length;

    public static Domain FromString(string domain)
    {
        return new Domain(domain);
    }

    public static Domain FromArray(ReadOnlySpan<byte> message, int offset)
    {
        return FromArray(message, offset, out _);
    }

    public static Domain FromArray(ReadOnlySpan<byte> message, int offset, out int endOffset)
    {
        endOffset = 0;

        List<string> labels = new(3);

        bool endOffsetAssigned = false;
        byte lengthOrPointer;
        int iterations = 0;
        int bufferSize = 0;

        while ((lengthOrPointer = message[offset++]) > 0)
        {
            // Two highest bits are set (pointer)
            if (lengthOrPointer.GetBitValueAt(6, 2) == 3)
            {
                if (!endOffsetAssigned)
                {
                    endOffsetAssigned = true;
                    endOffset = offset + 1;
                }

                ushort pointer = lengthOrPointer.GetBitValueAt(0, 6);
                offset = (pointer << 8) | message[offset];

                if (iterations++ > _compressionIterationMax)
                    throw new ArgumentException("Compression pointer loop detected");

                continue;
            }

            if (lengthOrPointer.GetBitValueAt(6, 2) != 0)
            {
                throw new ArgumentException("Unexpected bit pattern in label length");
            }


            ReadOnlySpan<byte> slice = message.Slice(offset, lengthOrPointer);

            labels.Add(Encoding.ASCII.GetString(slice));

            offset += lengthOrPointer;
            bufferSize += lengthOrPointer;
        }

        if (!endOffsetAssigned)
        {
            endOffset = offset;
        }

        return new Domain([.. labels], bufferSize);
    }

    public static Domain PointerName(IPAddress ip)
    {
        return new Domain(FormatReverseIP(ip));
    }

    private static string FormatReverseIP(IPAddress ip)
    {
        byte[] address = ip.GetAddressBytes();

        if (address.Length == 4)
        {
            return string.Join(".", address.Reverse().Select(b => b.ToString())) + ".in-addr.arpa";
        }

        byte[] nibbles = new byte[address.Length * 2];

        for (int i = 0, j = 0; i < address.Length; i++, j = 2 * i)
        {
            byte b = address[i];

            nibbles[j] = b.GetBitValueAt(4, 4);
            nibbles[j + 1] = b.GetBitValueAt(0, 4);
        }

        return string.Join(".", nibbles.Reverse().Select(b => b.ToString("x"))) + ".ip6.arpa";
    }

    public Domain(string[] labels) : this(labels, labels.Sum(Encoding.ASCII.GetByteCount)) { }

    public Domain(string[] labels, int length)
    {
        _labels = labels;
        _length = length;
    }

    public Domain(string? domain)
    {
        if (string.IsNullOrEmpty(domain))
        {
            _labels = [];
            _length = 0;
            return;
        }

        var buffer = domain.AsSpan();
        var ranges = buffer.Split('.');
        var count = 0;

        foreach (Range _ in ranges)
        {
            count++;
        }

        _labels = new string[count];
        _length = 0;

        var i = 0;

        foreach (Range range in ranges)
        {
            var slice = buffer[range.Start.Value..range.End.Value];

            _labels[i] = new string(slice);
            _length += Encoding.ASCII.GetByteCount(slice);
            i++;
        }
    }

    [JsonIgnore]
    public int Size
    {
        get { return _length + _labels.Length + 1; }
    }

    public byte[] ToArray()
    {
        byte[] result = new byte[Size];
        int offset = 0;

        foreach (string label in _labels)
        {
            byte length = (byte)Encoding.ASCII.GetByteCount(label);

            result[offset++] = length;
            Encoding.ASCII.GetBytes(label, 0, label.Length, result, offset);

            offset += label.Length;
        }

        result[offset] = 0;
        return result;
    }

    public override string ToString()
    {
        return string.Join(".", _labels);
    }

    public int CompareTo(Domain? other)
    {
        if (other is null)
            return -1;

        int length = Math.Min(_labels.Length, other._labels.Length);

        for (int i = 0; i < length; i++)
        {
            int v = string.Compare(_labels[i], other._labels[i], true);
            if (v != 0) return v;
        }

        return _labels.Length - other._labels.Length;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }
        if (obj is not Domain)
        {
            return false;
        }

        return CompareTo(obj as Domain) == 0;
    }

    public override int GetHashCode()
    {
        int hash = 17;

        foreach (string label in _labels)
        {
            foreach (char c in label)
            {
                char n = char.IsLower(c) ? c : char.ToLower(c);
                hash = hash * 31 + n.GetHashCode();
            }
        }

        return hash;
    }
    public static bool operator ==(Domain? left, Domain? right)
    {
        if (ReferenceEquals(left, null))
            return ReferenceEquals(right, null);

        return left.Equals(right);
    }

    public static bool operator !=(Domain? left, Domain? right)
    {
        return !(left == right);
    }

    public static bool operator <(Domain? left, Domain? right)
    {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(Domain? left, Domain? right)
    {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator >(Domain? left, Domain? right)
    {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(Domain? left, Domain? right)
    {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }
}
