﻿using BenchmarkDotNet.Attributes;
using DNS.Benchmark.Baseline.Protocol;
using DNS.Protocol;
using DNS.Tests;

namespace DNS.Benchmark;

[MemoryDiagnoser]
[ShortRunJob]
public class DomainBenchmarks
{
    private byte[] _pointer = [];
    private byte[] _empty = [];

    private Domain _a;
    private BaselineDomain _b;

    [GlobalSetup]
    public void Setup()
    {
        _pointer = Helper.ReadFixture("Domain", "www.google.com-pointer");
        _empty = Helper.ReadFixture("Domain", "empty-pointer");

        _a = Domain.FromArray(_pointer, 16, out int _);
        _b = BaselineDomain.FromArray(_pointer, 16, out int _);
    }

    [Benchmark]
    public void ParseHeaderBaseline()
    {
        BaselineDomain.FromArray(_pointer, 16, out int _);
        BaselineDomain.FromArray(_empty, 1, out int _);
    }

    [Benchmark]
    public void ParseHeader()
    {
        Domain.FromArray(_pointer, 16, out int _);
        Domain.FromArray(_empty, 1, out int _);
    }

    [Benchmark]
    public void HeaderToArrayBaseline()
    {
        _b.ToArray();
    }

    [Benchmark]
    public void HeaderToArray()
    {
        _a.ToArray();
    }
}