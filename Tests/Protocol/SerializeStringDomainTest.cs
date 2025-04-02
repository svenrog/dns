using DNS.Protocol;
using Xunit;

namespace DNS.Tests.Protocol;

public class SerializeStringDomainTest
{
    [Fact]
    public void EmptyStringDomain()
    {
        Domain StringDomain = new([]);
        byte[] content = Helper.ReadFixture("Domain", "empty-label");

        Assert.Equal(content, StringDomain.ToArray());
        Assert.Equal("", StringDomain.ToString());
    }

    [Fact]
    public void StringDomainWithSingleLabel()
    {
        Domain StringDomain = new("www");
        byte[] content = Helper.ReadFixture("Domain", "www-label");

        Assert.Equal(content, StringDomain.ToArray());
        Assert.Equal("www", StringDomain.ToString());
    }

    [Fact]
    public void StringDomainWithMultipleLabels()
    {
        Domain StringDomain = new(["www", "google", "com"]);
        byte[] content = Helper.ReadFixture("Domain", "www.google.com-label");

        Assert.Equal(content, StringDomain.ToArray());
        Assert.Equal("www.google.com", StringDomain.ToString());
    }
}
