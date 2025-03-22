using DNS.Protocol.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DNS.Protocol.ResourceRecords
{
    public partial class TextResourceRecord : BaseResourceRecord
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

        private static ResourceRecord Create(Domain domain, IList<CharacterString> characterStrings, TimeSpan ttl)
        {
            byte[] data = new byte[characterStrings.Sum(c => c.Size)];
            int offset = 0;

            foreach (CharacterString characterString in characterStrings)
            {
                characterString.ToArray().CopyTo(data, offset);
                offset += characterString.Size;
            }

            return new ResourceRecord(domain, data, RecordType.TXT, RecordClass.IN, ttl);
        }

        private static IList<CharacterString> FormatAttributeNameValue(string attributeName, string attributeValue)
        {
            return CharacterString.FromString($"{Escape(attributeName)}={attributeValue}");
        }

        public TextResourceRecord(IResourceRecord record) :
            base(record)
        {
            TextData = CharacterString.GetAllFromArray(Data, 0);
        }

        public TextResourceRecord(Domain domain, IList<CharacterString> characterStrings,
                TimeSpan ttl = default) : base(Create(domain, characterStrings, ttl))
        {
            TextData = new ReadOnlyCollection<CharacterString>(characterStrings);
        }

        public TextResourceRecord(Domain domain, string attributeName, string attributeValue,
                TimeSpan ttl = default) :
                this(domain, FormatAttributeNameValue(attributeName, attributeValue), ttl)
        { }

        public IList<CharacterString> TextData { get; }

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

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, StringifierContext.Default.TextResourceRecord);
        }


    }
}
