namespace DNS.Protocol.Utils;

public sealed class ByteArrayBuilder
{
    private readonly byte[] _buffer;
    private int _offset = 0;

    public ByteArrayBuilder(int capacity)
    {
        _buffer = new byte[capacity];
    }

    public ByteArrayBuilder Append(IEnumerable<byte[]> buffers)
    {
        foreach (byte[] buffer in buffers)
        {
            Write(buffer);
        }

        return this;
    }

    public ByteArrayBuilder Append(byte[] buffer)
    {
        Write(buffer);

        return this;
    }

    public byte[] Build()
    {
        return _buffer;
    }

    public void Write(byte[] buffer)
    {
        Array.Copy(buffer, 0, _buffer, _offset, buffer.Length);
        _offset += buffer.Length;
    }
}
