using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System.Net;
using System.Text.RegularExpressions;

namespace DNS.Server;

public class MasterFile : IRequestResolver
{
    protected static readonly TimeSpan DEFAULT_TTL = new(0);

    protected static bool Matches(Domain domain, Domain entry)
    {
        return BuildMatcher(entry).IsMatch(domain.ToString());
    }

    private static Regex BuildMatcher(Domain entry)
    {
        string[] labels = entry.ToString().Split('.');
        string[] patterns = new string[labels.Length];

        for (int i = 0; i < labels.Length; i++)
        {
            string label = labels[i];
            patterns[i] = label == "*" ? "(\\w+)" : Regex.Escape(label);
        }

        return new Regex("^" + string.Join("\\.", patterns) + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    private static bool IsWildcard(Domain entry)
    {
        foreach (string label in entry.ToString().Split('.'))
        {
            if (label == "*") return true;
        }

        return false;
    }

    protected static void Merge<T>(IList<T> l1, IList<T> l2)
    {
        foreach (T obj in l2)
        {
            l1.Add(obj);
        }
    }

    protected IList<IResourceRecord> entries = [];
    protected TimeSpan ttl = DEFAULT_TTL;

    // Lookup index, rebuilt lazily whenever the entry count changes.
    private readonly Dictionary<Domain, List<IResourceRecord>> _exact = [];
    private readonly List<(Regex Pattern, IResourceRecord Record)> _wildcard = [];
    private int _indexedCount = -1;

    public MasterFile(TimeSpan ttl)
    {
        this.ttl = ttl;
    }

    public MasterFile() { }

    public void Add(IResourceRecord entry)
    {
        entries.Add(entry);
    }

    public void AddIPAddressResourceRecord(string domain, string ip)
    {
        AddIPAddressResourceRecord(new Domain(domain), IPAddress.Parse(ip));
    }

    public void AddIPAddressResourceRecord(Domain domain, IPAddress ip)
    {
        Add(new IPAddressResourceRecord(domain, ip, ttl));
    }

    public void AddNameServerResourceRecord(string domain, string nsDomain)
    {
        AddNameServerResourceRecord(new Domain(domain), new Domain(nsDomain));
    }

    public void AddNameServerResourceRecord(Domain domain, Domain nsDomain)
    {
        Add(new NameServerResourceRecord(domain, nsDomain, ttl));
    }

    public void AddCanonicalNameResourceRecord(string domain, string cname)
    {
        AddCanonicalNameResourceRecord(new Domain(domain), new Domain(cname));
    }

    public void AddCanonicalNameResourceRecord(Domain domain, Domain cname)
    {
        Add(new CanonicalNameResourceRecord(domain, cname, ttl));
    }

    public void AddPointerResourceRecord(string ip, string pointer)
    {
        AddPointerResourceRecord(IPAddress.Parse(ip), new Domain(pointer));
    }

    public void AddPointerResourceRecord(IPAddress ip, Domain pointer)
    {
        Add(new PointerResourceRecord(ip, pointer, ttl));
    }

    public void AddMailExchangeResourceRecord(string domain, int preference, string exchange)
    {
        AddMailExchangeResourceRecord(new Domain(domain), preference, new Domain(exchange));
    }

    public void AddMailExchangeResourceRecord(Domain domain, int preference, Domain exchange)
    {
        Add(new MailExchangeResourceRecord(domain, preference, exchange));
    }

    public void AddTextResourceRecord(string domain, string attributeName, string attributeValue)
    {
        Add(new TextResourceRecord(new Domain(domain), attributeName, attributeValue, ttl));
    }

    public void AddServiceResourceRecord(Domain domain, ushort priority, ushort weight, ushort port, Domain target)
    {
        Add(new ServiceResourceRecord(domain, priority, weight, port, target, ttl));
    }

    public void AddServiceResourceRecord(string domain, ushort priority, ushort weight, ushort port, string target)
    {
        AddServiceResourceRecord(new Domain(domain), priority, weight, port, new Domain(target));
    }

    public Task<IResponse?> Resolve(IRequest request, CancellationToken cancellationToken = default)
    {
        Response response = Response.FromRequest(request);

        foreach (Question question in request.Questions)
        {
            IList<IResourceRecord> answers = Get(question);

            if (answers.Count > 0)
            {
                Merge(response.AnswerRecords, answers);
            }
            else
            {
                response.ResponseCode = ResponseCode.NameError;
            }
        }

        return Task.FromResult<IResponse?>(response);
    }

    protected IList<IResourceRecord> Get(Domain domain, RecordType type)
    {
        EnsureIndex();

        List<IResourceRecord> results = [];

        if (_exact.TryGetValue(domain, out List<IResourceRecord>? exact))
        {
            foreach (IResourceRecord entry in exact)
            {
                if (entry.Type == type || type == RecordType.ANY) results.Add(entry);
            }
        }

        if (_wildcard.Count > 0)
        {
            string domainText = domain.ToString();

            foreach ((Regex pattern, IResourceRecord entry) in _wildcard)
            {
                if ((entry.Type == type || type == RecordType.ANY) && pattern.IsMatch(domainText)) results.Add(entry);
            }
        }

        return results;
    }

    protected IList<IResourceRecord> Get(Question question)
    {
        return Get(question.Name, question.Type);
    }

    private void EnsureIndex()
    {
        // entries is protected, so rebuild whenever its size no longer matches the index.
        if (_indexedCount == entries.Count) return;

        _exact.Clear();
        _wildcard.Clear();

        foreach (IResourceRecord entry in entries)
        {
            if (IsWildcard(entry.Name))
            {
                _wildcard.Add((BuildMatcher(entry.Name), entry));
            }
            else
            {
                if (!_exact.TryGetValue(entry.Name, out List<IResourceRecord>? list))
                {
                    list = [];
                    _exact[entry.Name] = list;
                }

                list.Add(entry);
            }
        }

        _indexedCount = entries.Count;
    }
}
