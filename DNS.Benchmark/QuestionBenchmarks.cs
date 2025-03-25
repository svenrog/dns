using BenchmarkDotNet.Attributes;
using DNS.Benchmark.Baseline.Protocol;
using DNS.Protocol;
using DNS.Tests;

namespace DNS.Benchmark;

[MemoryDiagnoser]
[ShortRunJob]
public class QuestionBenchmarks
{
    private byte[] _all = [];
    private byte[] _empty = [];

    private Question _a;
    private BaselineQuestion _b;

    [GlobalSetup]
    public void Setup()
    {
        _all = Helper.ReadFixture("Question", "www.google.com_all");
        _empty = Helper.ReadFixture("Question", "empty-domain_any");

        _a = Question.FromArray(_all, 0, out int _);
        _b = BaselineQuestion.FromArray(_all, 0, out int _);
    }

    [Benchmark]
    public void QuestionParseArrayBaseline()
    {
        BaselineQuestion.FromArray(_all, 0, out int _);
        BaselineQuestion.FromArray(_empty, 0, out int _);
    }

    [Benchmark]
    public void QuestionParseArray()
    {
        Question.FromArray(_all, 0, out int _);
        Question.FromArray(_empty, 0, out int _);
    }


    [Benchmark]
    public void QuestionToArrayBaseline()
    {
        _b.ToArray();
    }

    [Benchmark]
    public void QuestionToArray()
    {
        _a.ToArray();
    }
}