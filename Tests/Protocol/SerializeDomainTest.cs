﻿using DNS.Protocol;
using System.Text;
using Xunit;

namespace DNS.Tests.Protocol;


public class SerializeDomainTest
{
    [Fact]
    public void EmptyDomain()
    {
        Domain domain = new(Helper.GetArray<string>());
        byte[] content = Helper.ReadFixture("Domain", "empty-label");

        Assert.Equal(content, domain.ToArray());
        Assert.Equal("", domain.ToString());
    }

    [Fact]
    public void DomainWithSingleLabel()
    {
        Domain domain = new(Helper.GetArray("www"));
        byte[] content = Helper.ReadFixture("Domain", "www-label");

        Assert.Equal(content, domain.ToArray());
        Assert.Equal("www", domain.ToString());
    }

    [Fact]
    public void DomainWithMultipleLabels()
    {
        Domain domain = new(Helper.GetArray("www", "google", "com"));
        byte[] content = Helper.ReadFixture("Domain", "www.google.com-label");

        Assert.Equal(content, domain.ToArray());
        Assert.Equal("www.google.com", domain.ToString());
    }

    [Fact]
    public void DomainWithSingleBinaryLabel()
    {
        Domain domain = new("www");
        byte[] content = Helper.ReadFixture("Domain", "www-label");

        Assert.Equal(content, domain.ToArray());
        Assert.Equal("www", domain.ToString());
    }

    [Fact]
    public void DomainWithMultipleBinaryLabels()
    {
        Domain domain = new(Helper.GetArray(
            Encoding.ASCII.GetString(Helper.GetArray<byte>(119, 119, 119)),
            Encoding.ASCII.GetString(Helper.GetArray<byte>(103, 111, 111, 103, 108, 101)),
            Encoding.ASCII.GetString(Helper.GetArray<byte>(99, 111, 109))
        ));
        byte[] content = Helper.ReadFixture("Domain", "www.google.com-label");

        Assert.Equal(content, domain.ToArray());
        Assert.Equal("www.google.com", domain.ToString());
    }
}
