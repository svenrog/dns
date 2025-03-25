using DNS.Protocol.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;

namespace DNS.Protocol
{
    public class Domain : IComparable<Domain>
    {
        private const int _compressionIterationMax = 1000;
        private const byte _asciiUppercaseFirst = 65;
        private const byte _asciiUppercaseLast = 90;
        private const byte _asciiLowercaseFirst = 97;
        private const byte _asciiLowercaseLast = 122;
        private const byte _asciiUppercaseMask = 223;

        private readonly byte[][] _labels;

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

            List<byte[]> labels = [];

            bool endOffsetAssigned = false;
            byte lengthOrPointer;
            int iterations = 0;

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

                byte length = lengthOrPointer;
                byte[] label = message.Slice(offset, length).ToArray();

                labels.Add(label);

                offset += length;
            }

            if (!endOffsetAssigned)
            {
                endOffset = offset;
            }

            return new Domain(labels.ToArray());
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

        private static bool IsASCIIAlphabet(byte b)
        {
            return (_asciiUppercaseFirst <= b && b <= _asciiUppercaseLast) ||
                (_asciiLowercaseFirst <= b && b <= _asciiLowercaseLast);
        }

        private static int CompareTo(byte a, byte b)
        {
            if (IsASCIIAlphabet(a) && IsASCIIAlphabet(b))
            {
                a &= _asciiUppercaseMask;
                b &= _asciiUppercaseMask;
            }

            return a - b;
        }

        private static int CompareTo(byte[] a, byte[] b)
        {
            int length = Math.Min(a.Length, b.Length);

            for (int i = 0; i < length; i++)
            {
                int v = CompareTo(a[i], b[i]);
                if (v != 0) return v;
            }

            return a.Length - b.Length;
        }

#pragma warning disable S2368 // Public methods should not have multidimensional array parameters
        public Domain(byte[][] labels)
#pragma warning restore S2368 // Public methods should not have multidimensional array parameters
        {
            _labels = labels;
        }

        public Domain(string[] labels, Encoding encoding)
        {
            _labels = [.. labels.Select(encoding.GetBytes)];
        }

        public Domain(string domain)
        {
            var buffer = domain.AsSpan();
            var ranges = buffer.Split('.');
            var count = 0;

            foreach (Range _ in ranges)
            {
                count++;
            }

            _labels = new byte[count][];
            var i = 0;

            foreach (var range in ranges)
            {
                var slice = buffer[range.Start.Value..range.End.Value];
                var target = new byte[Encoding.ASCII.GetByteCount(slice)];

                Encoding.ASCII.GetBytes(slice, target);

                _labels[i] = target;
                i++;
            }
        }

        public Domain(string[] labels) : this(labels, Encoding.ASCII) { }

        [JsonIgnore]
        public int Size
        {
            get { return _labels.Sum(l => l.Length) + _labels.Length + 1; }
        }

        public byte[] ToArray()
        {
            byte[] result = new byte[Size];
            int offset = 0;

            foreach (byte[] label in _labels)
            {
                result[offset++] = (byte)label.Length;
                label.CopyTo(result, offset);
                offset += label.Length;
            }

            result[offset] = 0;
            return result;
        }

        public string ToString(Encoding encoding)
        {
            return string.Join(".", _labels.Select(encoding.GetString));
        }

        public override string ToString()
        {
            return ToString(Encoding.ASCII);
        }

        public int CompareTo(Domain other)
        {
            int length = Math.Min(_labels.Length, other._labels.Length);

            for (int i = 0; i < length; i++)
            {
                int v = CompareTo(_labels[i], other._labels[i]);
                if (v != 0) return v;
            }

            return _labels.Length - other._labels.Length;
        }

        public override bool Equals(object obj)
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
            unchecked
            {
                int hash = 17;

                foreach (byte[] label in _labels)
                {
                    foreach (byte b in label)
                    {
                        hash = hash * 31 + (IsASCIIAlphabet(b) ? b & _asciiUppercaseMask : b);
                    }
                }

                return hash;
            }
        }
        public static bool operator ==(Domain left, Domain right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            return left.Equals(right);
        }

        public static bool operator !=(Domain left, Domain right)
        {
            return !(left == right);
        }

        public static bool operator <(Domain left, Domain right)
        {
            return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(Domain left, Domain right)
        {
            return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
        }

        public static bool operator >(Domain left, Domain right)
        {
            return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
        }

        public static bool operator >=(Domain left, Domain right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
        }
    }
}
