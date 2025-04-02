using DNS.Benchmark.Baseline.Protocol;

namespace DNS.Benchmark.Baseline.Client.RequestResolver;

public interface IBaselineRequestResolver
{
    Task<IBaselineResponse?> Resolve(IBaselineRequest request, CancellationToken cancellationToken = default);
}
