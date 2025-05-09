﻿using System;
using Xunit;
using DNS.Protocol;

namespace DNS.Tests.Protocol; 

public class ParseDomainTest {
    [Fact]
    public void EmptyDomain() {
        byte[] content = Helper.ReadFixture("Domain", "empty-label");
        Domain domain = Domain.FromArray(content, 0, out int endOffset);

        Assert.Equal("", domain.ToString());
        Assert.Equal(1, domain.Size);
        Assert.Equal(1, endOffset);
    }

    [Fact]
    public void DomainWithSingleLabel() {
        byte[] content = Helper.ReadFixture("Domain", "www-label");
        Domain domain = Domain.FromArray(content, 0, out int endOffset);

        Assert.Equal("www", domain.ToString());
        Assert.Equal(5, domain.Size);
        Assert.Equal(5, endOffset);
    }

    [Fact]
    public void DomainWithMultipleLabels() {
        byte[] content = Helper.ReadFixture("Domain", "www.google.com-label");
        Domain domain = Domain.FromArray(content, 0, out int endOffset);

        Assert.Equal("www.google.com", domain.ToString());
        Assert.Equal(16, domain.Size);
        Assert.Equal(16, endOffset);
    }

    [Fact]
    public void DomainWithMultipleLabelsPreceededByHeader() {
        byte[] content = Helper.ReadFixture("Domain", "empty-header_www.google.com-label");
        Domain domain = Domain.FromArray(content, 12, out int endOffset);

        Assert.Equal("www.google.com", domain.ToString());
        Assert.Equal(16, domain.Size);
        Assert.Equal(28, endOffset);
    }

    [Fact]
    public void EmptyPointerDomain() {
        byte[] content = Helper.ReadFixture("Domain", "empty-pointer");
        Domain domain = Domain.FromArray(content, 1, out int endOffset);

        Assert.Equal("", domain.ToString());
        Assert.Equal(1, domain.Size);
        Assert.Equal(3, endOffset);
    }

    [Fact]
    public void PointerDomainWithSingleLabel() {
        byte[] content = Helper.ReadFixture("Domain", "www-pointer");
        Domain domain = Domain.FromArray(content, 5, out int endOffset);

        Assert.Equal("www", domain.ToString());
        Assert.Equal(5, domain.Size);
        Assert.Equal(7, endOffset);
    }

    [Fact]
    public void PointerDomainWithMultipleLabels() {
        byte[] content = Helper.ReadFixture("Domain", "www.google.com-pointer");
        Domain domain = Domain.FromArray(content, 16, out int endOffset);

        Assert.Equal("www.google.com", domain.ToString());
        Assert.Equal(16, domain.Size);
        Assert.Equal(18, endOffset);
    }

    [Fact]
    public void PointerDomainWithMultipleLabelsPreceededByHeader() {
        byte[] content = Helper.ReadFixture("Domain", "empty-header_www.google.com-pointer");
        Domain domain = Domain.FromArray(content, 28, out int endOffset);

        Assert.Equal("www.google.com", domain.ToString());
        Assert.Equal(16, domain.Size);
        Assert.Equal(30, endOffset);
    }

    [Fact]
    public void PointerDomainLoopDetection() {
        int endOffset = 0;
        byte[] content = Helper.ReadFixture("Domain", "pointer-loop");
        Assert.Throws<ArgumentException>(() => Domain.FromArray(content, 16, out endOffset));
    }
}
