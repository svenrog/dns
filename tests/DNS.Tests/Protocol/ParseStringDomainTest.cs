using DNS.Protocol;
using System;
using Xunit;

namespace DNS.Tests.Protocol;


public class ParseStringDomainTest
{
    [Fact]
    public void EmptyStringDomain()
    {
        byte[] content = Helper.ReadFixture("Domain", "empty-label");
        Domain StringDomain = Domain.FromArray(content, 0, out int endOffset);

        Assert.Equal("", StringDomain.ToString());
        Assert.Equal(1, StringDomain.Size);
        Assert.Equal(1, endOffset);
    }

    [Fact]
    public void StringDomainWithSingleLabel()
    {
        byte[] content = Helper.ReadFixture("Domain", "www-label");
        Domain StringDomain = Domain.FromArray(content, 0, out int endOffset);

        Assert.Equal("www", StringDomain.ToString());
        Assert.Equal(5, StringDomain.Size);
        Assert.Equal(5, endOffset);
    }

    [Fact]
    public void StringDomainWithMultipleLabels()
    {
        byte[] content = Helper.ReadFixture("Domain", "www.google.com-label");
        Domain StringDomain = Domain.FromArray(content, 0, out int endOffset);

        Assert.Equal("www.google.com", StringDomain.ToString());
        Assert.Equal(16, StringDomain.Size);
        Assert.Equal(16, endOffset);
    }

    [Fact]
    public void StringDomainWithMultipleLabelsPreceededByHeader()
    {
        byte[] content = Helper.ReadFixture("Domain", "empty-header_www.google.com-label");
        Domain StringDomain = Domain.FromArray(content, 12, out int endOffset);

        Assert.Equal("www.google.com", StringDomain.ToString());
        Assert.Equal(16, StringDomain.Size);
        Assert.Equal(28, endOffset);
    }

    [Fact]
    public void EmptyPointerStringDomain()
    {
        byte[] content = Helper.ReadFixture("Domain", "empty-pointer");
        Domain StringDomain = Domain.FromArray(content, 1, out int endOffset);

        Assert.Equal("", StringDomain.ToString());
        Assert.Equal(1, StringDomain.Size);
        Assert.Equal(3, endOffset);
    }

    [Fact]
    public void PointerStringDomainWithSingleLabel()
    {
        byte[] content = Helper.ReadFixture("Domain", "www-pointer");
        Domain StringDomain = Domain.FromArray(content, 5, out int endOffset);

        Assert.Equal("www", StringDomain.ToString());
        Assert.Equal(5, StringDomain.Size);
        Assert.Equal(7, endOffset);
    }

    [Fact]
    public void PointerStringDomainWithMultipleLabels()
    {
        byte[] content = Helper.ReadFixture("Domain", "www.google.com-pointer");
        Domain StringDomain = Domain.FromArray(content, 16, out int endOffset);

        Assert.Equal("www.google.com", StringDomain.ToString());
        Assert.Equal(16, StringDomain.Size);
        Assert.Equal(18, endOffset);
    }

    [Fact]
    public void PointerStringDomainWithMultipleLabelsPreceededByHeader()
    {
        byte[] content = Helper.ReadFixture("Domain", "empty-header_www.google.com-pointer");
        Domain StringDomain = Domain.FromArray(content, 28, out int endOffset);

        Assert.Equal("www.google.com", StringDomain.ToString());
        Assert.Equal(16, StringDomain.Size);
        Assert.Equal(30, endOffset);
    }

    [Fact]
    public void PointerStringDomainLoopDetection()
    {
        int endOffset = 0;
        byte[] content = Helper.ReadFixture("Domain", "pointer-loop");
        Assert.Throws<ArgumentException>(() => Domain.FromArray(content, 16, out endOffset));
    }
}
