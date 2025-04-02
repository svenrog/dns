using DNS.Protocol;
using System.Text;
using Xunit;

namespace DNS.Tests.Protocol;


public class CompareDomainTest
{
    [Fact]
    public void SameDomainInstance()
    {
        Domain domain = new(Helper.GetArray("www"));
        Assert.Equal(0, domain.CompareTo(domain));
    }

    [Fact]
    public void SameDomainsWithSingleLabelDifferentCasing()
    {
        Domain a = new(Helper.GetArray("www"));
        Domain b = new(Helper.GetArray("WWW"));

        Assert.Equal(0, a.CompareTo(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void SameDomainsWithSingleLabelNonAlphabeticCodes()
    {
        Domain a = new(Helper.GetArray(
            Encoding.ASCII.GetString(Helper.GetArray<byte>(119, 0, 119))
        ));
        Domain b = new(Helper.GetArray(
            Encoding.ASCII.GetString(Helper.GetArray<byte>(119, 0, 119))
        ));

        Assert.Equal(0, a.CompareTo(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void SameDomainsWithMultipleLabels()
    {
        Domain a = new(Helper.GetArray(
            Encoding.ASCII.GetString(Helper.GetArray<byte>(119, 119, 119)),
            Encoding.ASCII.GetString(Helper.GetArray<byte>(103, 111, 111, 103, 108, 101)),
            Encoding.ASCII.GetString(Helper.GetArray<byte>(99, 0, 109))
        ));
        Domain b = new(Helper.GetArray(
            Encoding.ASCII.GetString(Helper.GetArray<byte>(87, 87, 87)),
            Encoding.ASCII.GetString(Helper.GetArray<byte>(103, 79, 79, 103, 108, 101)),
            Encoding.ASCII.GetString(Helper.GetArray<byte>(99, 0, 77))
        ));

        Assert.Equal(0, a.CompareTo(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void DifferentDomainsWithSingleLabelSameLength()
    {
        Domain a = new(Helper.GetArray("aww"));
        Domain b = new(Helper.GetArray("www"));

        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void DifferentDomainsWithSingleLabelDifferentLength()
    {
        Domain a = new(Helper.GetArray("ww"));
        Domain b = new(Helper.GetArray("www"));

        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void DifferentDomainsWithSingleLabelNonAlphabeticCodes()
    {
        Domain a = new(Helper.GetArray(
            Encoding.ASCII.GetString(Helper.GetArray<byte>(119, 0, 119))
        ));
        Domain b = new(Helper.GetArray(
            Encoding.ASCII.GetString(Helper.GetArray<byte>(119, 119, 119))
        ));

        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void DifferentDomainsWithMultipleLabelsDifferentAmount()
    {
        Domain a = new(Helper.GetArray("www"));
        Domain b = new(Helper.GetArray("www", "google"));

        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }
}
