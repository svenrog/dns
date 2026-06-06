using System.Text;

namespace DNS.Benchmark.Baseline.Protocol;


/// <summary>
/// Implementation of the "character-string" non-terminal as defined in
/// RFC1035 (chapter 3.3):
///   "character-string" is a single length octet followed by that number of
///    characters. "character-string" is treated as binary information, and
///    can be up to 256 characters in length (including the length octet).
/// </summary>
public class BaselineCharacterString
{
    private const int MAX_SIZE = byte.MaxValue;

    private readonly byte[] data;

    public static IList<BaselineCharacterString> GetAllFromArray(byte[] message, int offset)
    {
        return GetAllFromArray(message, offset, out offset);
    }

    public static IList<BaselineCharacterString> GetAllFromArray(byte[] message, int offset, out int endOffset)
    {
        IList<BaselineCharacterString> characterStrings = [];

        while (offset < message.Length) characterStrings.Add(FromArray(message, offset, out offset));

        endOffset = offset;
        return characterStrings;
    }

    public static BaselineCharacterString FromArray(byte[] message, int offset)
    {
        return FromArray(message, offset, out offset);
    }

    public static BaselineCharacterString FromArray(byte[] message, int offset, out int endOffset)
    {
        if (message.Length < 1) throw new ArgumentException("Empty message");

        byte len = message[offset++];
        byte[] data = new byte[len];
        Buffer.BlockCopy(message, offset, data, 0, len);
        endOffset = offset + len;
        return new BaselineCharacterString(data);
    }

    public static IList<BaselineCharacterString> FromString(string message)
    {
        return FromString(message, Encoding.ASCII);
    }

    public static IList<BaselineCharacterString> FromString(string message, Encoding encoding)
    {
        byte[] bytes = encoding.GetBytes(message);
        int size = (int)Math.Ceiling((double)bytes.Length / MAX_SIZE);
        IList<BaselineCharacterString> characterStrings = new List<BaselineCharacterString>(size);

        for (int i = 0; i < bytes.Length; i += MAX_SIZE)
        {
            int len = Math.Min(bytes.Length - i, MAX_SIZE);
            byte[] chunk = new byte[len];
            Buffer.BlockCopy(bytes, i, chunk, 0, len);
            characterStrings.Add(new BaselineCharacterString(chunk));
        }

        return characterStrings;
    }

    public BaselineCharacterString(byte[] data)
    {
        if (data.Length > MAX_SIZE) Array.Resize(ref data, MAX_SIZE);
        this.data = data;
    }

    public BaselineCharacterString(string data, Encoding encoding) : this(encoding.GetBytes(data)) { }

    public BaselineCharacterString(string data) : this(data, Encoding.ASCII) { }

    public int Size
    {
        get { return data.Length + 1; }
    }

    public byte[] ToArray()
    {
        byte[] result = new byte[Size];
        result[0] = (byte)data.Length;
        data.CopyTo(result, 1);
        return result;
    }

    public string ToString(Encoding encoding)
    {
        return encoding.GetString(data);
    }

    public override string ToString()
    {
        return ToString(Encoding.ASCII);
    }
}
