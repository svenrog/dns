using DNS.Benchmark.Baseline.Protocol;
using DNS.Benchmark.Baseline.Protocol.ResourceRecords;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace DNS.Protocol.ResourceRecords;

public partial class BaselineTextResourceRecord : BaselineBaseResourceRecord
{
    /// Regular expression that matches the attribute name/value.
    /// The first unescaped equal sign is the name/value delimiter.
    private static readonly Regex _patternTxtRecord = PatternTxtRecord();

    [GeneratedRegex(@"^([ -~]*?)(?<!`)=([ -~]*)$")]
    private static partial Regex PatternTxtRecord();

    /// Regular expression that matches unescaped leading/trailing whitespace.
    private static readonly Regex _patternTrimName = PatternTrimName();

    [GeneratedRegex(@"^\s+|((?<!`)\s)+$")]
    private static partial Regex PatternTrimName();

    /// Regular expression that matches unescaped characters.
    private static readonly Regex _patternEscape = PatternEscape();

    [GeneratedRegex(@"([`=])")]
    private static partial Regex PatternEscape();

    /// Regular expression that matches escaped characters.
    private static readonly Regex _patternUnescape = PatternUnescape();

    [GeneratedRegex(@"`([`=\s])")]
    private static partial Regex PatternUnescape();

    private static string Trim(string value) => _patternTrimName.Replace(value, string.Empty);
    private static string Escape(string value) => _patternEscape.Replace(value, "`$1");
    private static string Unescape(string value) => _patternUnescape.Replace(value, "$1");

    private static BaselineResourceRecord Create(BaselineDomain domain, IList<BaselineCharacterString> characterStrings, TimeSpan ttl)
    {
        byte[] data = new byte[characterStrings.Sum(c => c.Size)];
        int offset = 0;

        foreach (BaselineCharacterString characterString in characterStrings)
        {
            characterString.ToArray().CopyTo(data, offset);
            offset += characterString.Size;
        }

        return new BaselineResourceRecord(domain, data, BaselineRecordType.TXT, BaselineRecordClass.IN, ttl);
    }

    private static IList<BaselineCharacterString> FormatAttributeNameValue(string attributeName, string attributeValue)
    {
        return BaselineCharacterString.FromString($"{Escape(attributeName)}={attributeValue}");
    }

    public BaselineTextResourceRecord(IBaselineResourceRecord record) :
        base(record)
    {
        TextData = BaselineCharacterString.GetAllFromArray(Data, 0);
    }

    public BaselineTextResourceRecord(BaselineDomain domain, IList<BaselineCharacterString> characterStrings,
            TimeSpan ttl = default) : base(Create(domain, characterStrings, ttl))
    {
        TextData = new ReadOnlyCollection<BaselineCharacterString>(characterStrings);
    }

    public BaselineTextResourceRecord(BaselineDomain domain, string attributeName, string attributeValue,
            TimeSpan ttl = default) :
            this(domain, FormatAttributeNameValue(attributeName, attributeValue), ttl)
    { }

    public IList<BaselineCharacterString> TextData { get; }

    public KeyValuePair<string, string> Attribute
    {
        get
        {
            string text = ToStringTextData();
            Match match = _patternTxtRecord.Match(text);

            if (match.Success)
            {
                string attributeName = (match.Groups[1].Length > 0) ?
                    Unescape(Trim(match.Groups[1].ToString())) : null;
                string attributeValue = Unescape(match.Groups[2].ToString());
                return new KeyValuePair<string, string>(attributeName, attributeValue);
            }
            else
            {
                return new KeyValuePair<string, string>(null, Unescape(text));
            }
        }
    }

    public string ToStringTextData()
    {
        return ToStringTextData(Encoding.ASCII);
    }

    public string ToStringTextData(Encoding encoding)
    {
        return string.Join(string.Empty, TextData.Select(c => c.ToString(encoding)));
    }
}
