using DNS.Benchmark.Baseline.Client.RequestResolver;
using DNS.Benchmark.Baseline.Protocol;
using DNS.Benchmark.Baseline.Protocol.ResourceRecords;
using DNS.Protocol.ResourceRecords;
using System.Net;
using System.Text.RegularExpressions;

namespace DNS.Benchmark.Baseline.Server;

public class BaselineMasterFile : IBaselineRequestResolver
{
    protected static readonly TimeSpan DEFAULT_TTL = new(0);

    protected static bool Matches(BaselineDomain domain, BaselineDomain entry)
    {
        string[] labels = entry.ToString().Split('.');
        string[] patterns = new string[labels.Length];

        for (int i = 0; i < labels.Length; i++)
        {
            string label = labels[i];
            patterns[i] = label == "*" ? "(\\w+)" : Regex.Escape(label);
        }

        Regex re = new("^" + string.Join("\\.", patterns) + "$", RegexOptions.IgnoreCase);
        return re.IsMatch(domain.ToString());
    }

    protected static void Merge<T>(IList<T> l1, IList<T> l2)
    {
        foreach (T obj in l2)
            l1.Add(obj);
    }

    protected IList<IBaselineResourceRecord> entries = [];
    protected TimeSpan ttl = DEFAULT_TTL;

    public BaselineMasterFile(TimeSpan ttl)
    {
        this.ttl = ttl;
    }

    public BaselineMasterFile() { }

    public void Add(IBaselineResourceRecord entry)
    {
        entries.Add(entry);
    }

    public void AddIPAddressResourceRecord(string domain, string ip)
    {
        AddIPAddressResourceRecord(new BaselineDomain(domain), IPAddress.Parse(ip));
    }

    public void AddIPAddressResourceRecord(BaselineDomain domain, IPAddress ip)
    {
        Add(new BaselineIPAddressResourceRecord(domain, ip, ttl));
    }

    public void AddNameServerResourceRecord(string domain, string nsDomain)
    {
        AddNameServerResourceRecord(new BaselineDomain(domain), new BaselineDomain(nsDomain));
    }

    public void AddNameServerResourceRecord(BaselineDomain domain, BaselineDomain nsDomain)
    {
        Add(new BaselineNameServerResourceRecord(domain, nsDomain, ttl));
    }

    public void AddCanonicalNameResourceRecord(string domain, string cname)
    {
        AddCanonicalNameResourceRecord(new BaselineDomain(domain), new BaselineDomain(cname));
    }

    public void AddCanonicalNameResourceRecord(BaselineDomain domain, BaselineDomain cname)
    {
        Add(new BaselineCanonicalNameResourceRecord(domain, cname, ttl));
    }

    public void AddPointerResourceRecord(string ip, string pointer)
    {
        AddPointerResourceRecord(IPAddress.Parse(ip), new BaselineDomain(pointer));
    }

    public void AddPointerResourceRecord(IPAddress ip, BaselineDomain pointer)
    {
        Add(new BaselinePointerResourceRecord(ip, pointer, ttl));
    }

    public void AddMailExchangeResourceRecord(string domain, int preference, string exchange)
    {
        AddMailExchangeResourceRecord(new BaselineDomain(domain), preference, new BaselineDomain(exchange));
    }

    public void AddMailExchangeResourceRecord(BaselineDomain domain, int preference, BaselineDomain exchange)
    {
        Add(new BaselineMailExchangeResourceRecord(domain, preference, exchange));
    }

    public void AddTextResourceRecord(string domain, string attributeName, string attributeValue)
    {
        Add(new BaselineTextResourceRecord(new BaselineDomain(domain), attributeName, attributeValue, ttl));
    }

    public void AddServiceResourceRecord(BaselineDomain domain, ushort priority, ushort weight, ushort port, BaselineDomain target)
    {
        Add(new BaselineServiceResourceRecord(domain, priority, weight, port, target, ttl));
    }

    public void AddServiceResourceRecord(string domain, ushort priority, ushort weight, ushort port, string target)
    {
        AddServiceResourceRecord(new BaselineDomain(domain), priority, weight, port, new BaselineDomain(target));
    }

    public Task<IBaselineResponse?> Resolve(IBaselineRequest request, CancellationToken cancellationToken = default)
    {
        BaselineResponse response = BaselineResponse.FromRequest(request);

        foreach (BaselineQuestion question in request.Questions)
        {
            IList<IBaselineResourceRecord> answers = Get(question);

            if (answers.Count > 0)
                Merge(response.AnswerRecords, answers);
            else
                response.ResponseCode = BaselineResponseCode.NameError;
        }

        return Task.FromResult<IBaselineResponse?>(response);
    }

    protected IList<IBaselineResourceRecord> Get(BaselineDomain domain, BaselineRecordType type)
    {
        return [.. entries.Where(e => Matches(domain, e.Name) && (e.Type == type || type == BaselineRecordType.ANY))];
    }

    protected IList<IBaselineResourceRecord> Get(BaselineQuestion question)
    {
        return Get(question.Name, question.Type);
    }
}
