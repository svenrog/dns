using DNS.Protocol.Serialization;
using DNS.Protocol.Utils;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace DNS.Protocol
{
    // 12 bytes message header
    // Endianness.Big
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public const int SIZE = 12;

        private ushort _id;

        private byte _flag0;
        private byte _flag1;

        // Question count: number of questions in the Question section
        private ushort _qdCount;

        // Answer record count: number of records in the Answer section
        private ushort _anCount;

        // Authority record count: number of records in the Authority section
        private ushort _nsCount;

        // Additional record count: number of records in the Additional section
        private ushort _arCount;

        public static Header FromArray(byte[] header)
        {
            if (header.Length < SIZE)
            {
                throw new ArgumentException("Header length too small");
            }

            Span<byte> buffer = stackalloc byte[SIZE];

            header.AsSpan()
                .Slice(0, SIZE)
                .CopyTo(buffer);

            ConvertEndianness(ref buffer);

            return MemoryMarshal.Read<Header>(buffer);
        }

        public readonly byte[] ToArray()
        {
            Span<byte> buffer = stackalloc byte[SIZE];

            Unsafe.As<byte, ushort>(ref buffer[0]) = _id;
            Unsafe.As<byte, byte>(ref buffer[2]) = _flag0;
            Unsafe.As<byte, byte>(ref buffer[3]) = _flag1;
            Unsafe.As<byte, ushort>(ref buffer[4]) = _qdCount;
            Unsafe.As<byte, ushort>(ref buffer[6]) = _anCount;
            Unsafe.As<byte, ushort>(ref buffer[8]) = _nsCount;
            Unsafe.As<byte, ushort>(ref buffer[10]) = _arCount;

            ConvertEndianness(ref buffer);

            return buffer.ToArray();
        }

        private static void ConvertEndianness(ref Span<byte> bytes)
        {
            if (!BitConverter.IsLittleEndian) return;

            // Manual endian conversion
            bytes.Slice(0, sizeof(ushort)).Reverse();
            bytes.Slice(2, sizeof(byte)).Reverse();
            bytes.Slice(3, sizeof(byte)).Reverse();
            bytes.Slice(4, sizeof(ushort)).Reverse();
            bytes.Slice(6, sizeof(ushort)).Reverse();
            bytes.Slice(8, sizeof(ushort)).Reverse();
            bytes.Slice(10, sizeof(ushort)).Reverse();
        }

        public int Id
        {
            readonly get => _id; set => _id = (ushort)value;
        }

        public int QuestionCount
        {
            readonly get => _qdCount; set => _qdCount = (ushort)value;
        }

        public int AnswerRecordCount
        {
            readonly get => _anCount; set => _anCount = (ushort)value;
        }

        public int AuthorityRecordCount
        {
            readonly get => _nsCount; set => _nsCount = (ushort)value;
        }

        public int AdditionalRecordCount
        {
            readonly get => _arCount; set => _arCount = (ushort)value;
        }

        public bool Response
        {
            readonly get => Qr == 1; set => Qr = Convert.ToByte(value);
        }

        public OperationCode OperationCode
        {
            readonly get => (OperationCode)Opcode; set => Opcode = (byte)value;
        }

        public bool AuthorativeServer
        {
            readonly get => Aa == 1; set => Aa = Convert.ToByte(value);
        }

        public bool Truncated
        {
            readonly get => Tc == 1; set => Tc = Convert.ToByte(value);
        }

        public bool RecursionDesired
        {
            readonly get => Rd == 1; set => Rd = Convert.ToByte(value);
        }

        public bool RecursionAvailable
        {
            readonly get => Ra == 1; set => Ra = Convert.ToByte(value);
        }

        public bool AuthenticData
        {
            readonly get => Ad == 1; set => Ad = Convert.ToByte(value);
        }

        public bool CheckingDisabled
        {
            readonly get => Cd == 1; set => Cd = Convert.ToByte(value);
        }

        public ResponseCode ResponseCode
        {
            readonly get => (ResponseCode)RCode; set => RCode = (byte)value;
        }

        public override readonly string ToString()
        {
            return JsonSerializer.Serialize(this, StringifierContext.Default.Header);
        }

        // Query/Response Flag
        private byte Qr
        {
            readonly get => Flag0.GetBitValueAt(7, 1); set => Flag0 = Flag0.SetBitValueAt(7, 1, value);
        }

        // Operation Code
        private byte Opcode
        {
            readonly get => Flag0.GetBitValueAt(3, 4); set => Flag0 = Flag0.SetBitValueAt(3, 4, value);
        }

        // Authorative Answer Flag
        private byte Aa
        {
            readonly get => Flag0.GetBitValueAt(2, 1); set => Flag0 = Flag0.SetBitValueAt(2, 1, value);
        }

        // Truncation Flag
        private byte Tc
        {
            readonly get => Flag0.GetBitValueAt(1, 1); set => Flag0 = Flag0.SetBitValueAt(1, 1, value);
        }

        // Recursion Desired
        private byte Rd
        {
            readonly get => Flag0.GetBitValueAt(0, 1); set => Flag0 = Flag0.SetBitValueAt(0, 1, value);
        }

        // Recursion Available
        private byte Ra
        {
            readonly get => Flag1.GetBitValueAt(7, 1); set => Flag1 = Flag1.SetBitValueAt(7, 1, value);
        }

        // Zero (Reserved)
#pragma warning disable S1144 // Unused private types or members should be removed
        private readonly byte Z
#pragma warning restore S1144 // Unused private types or members should be removed
        {
            get => Flag1.GetBitValueAt(6, 1);
        }

        // Authentic Data
        private byte Ad
        {
            readonly get => Flag1.GetBitValueAt(5, 1); set => Flag1 = Flag1.SetBitValueAt(5, 1, value);
        }

        // Checking Disabled
        private byte Cd
        {
            readonly get => Flag1.GetBitValueAt(4, 1); set => Flag1 = Flag1.SetBitValueAt(4, 1, value);
        }

        // Response Code
        private byte RCode
        {
            readonly get => Flag1.GetBitValueAt(0, 4); set => Flag1 = Flag1.SetBitValueAt(0, 4, value);
        }

        private byte Flag0
        {
            readonly get => _flag0; set => _flag0 = value;
        }

        private byte Flag1
        {
            readonly get => _flag1; set => _flag1 = value;
        }
    }
}
