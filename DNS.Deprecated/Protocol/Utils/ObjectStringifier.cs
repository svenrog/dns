using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace DNS.Protocol.Utils
{
    [Obsolete("This requires dynamic code and should be replaced.", error: true)]
    public class ObjectStringifier
    {
        public static ObjectStringifier New(object obj)
        {
            return new ObjectStringifier(obj);
        }

        public static string Stringify(object? obj)
        {
            return StringifyObject(obj);
        }

        private static string StringifyObject(object? obj)
        {
            if (obj is string stringValue)
            {
                return stringValue;
            }
            else if (obj is IDictionary dictionaryValue)
            {
                return StringifyDictionary(dictionaryValue);
            }
            else if (obj is IEnumerable enumerableValue)
            {
                return StringifyList(enumerableValue);
            }
            else
            {
                return obj?.ToString() ?? "null";
            }
        }

        private static string StringifyList(IEnumerable enumerable)
        {
            return "[" + string.Join(", ", enumerable.Cast<object>().Select(o => StringifyObject(o)).ToArray()) + "]";
        }

        private static string StringifyDictionary(IDictionary dict)
        {
            StringBuilder result = new();

            result.Append('{');

            foreach (DictionaryEntry pair in dict)
            {
                result
                    .Append(pair.Key)
                    .Append('=')
                    .Append(StringifyObject(pair.Value))
                    .Append(", ");
            }

            if (result.Length > 1)
            {
                result.Remove(result.Length - 2, 2);
            }

            return result.Append('}').ToString();
        }

        private readonly object _obj;
        private readonly Dictionary<string, string> _pairs;

        public ObjectStringifier(object obj)
        {
            _obj = obj;
            _pairs = [];
        }

        public ObjectStringifier Remove(params string[] names)
        {
            foreach (string name in names)
            {
                _pairs.Remove(name);
            }

            return this;
        }

        public ObjectStringifier Add(params string[] names)
        {
            Type type = _obj.GetType();

            foreach (string name in names)
            {
                PropertyInfo? property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                object? value = property?.GetValue(_obj, []);

                _pairs.Add(name, StringifyObject(value));
            }

            return this;
        }

        public ObjectStringifier Add(string name, object value)
        {
            _pairs.Add(name, StringifyObject(value));
            return this;
        }

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
        public ObjectStringifier AddAll()
        {
            PropertyInfo[] properties = _obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                object? value = property.GetValue(_obj, []);
                _pairs.Add(property.Name, StringifyObject(value));
            }

            return this;
        }

        public override string ToString()
        {
            return StringifyDictionary(_pairs);
        }
    }
}
