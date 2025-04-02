using DNS.Benchmark.Baseline.Protocol;

namespace DNS.Benchmark.Baseline.Client.RequestResolver;

public class BaselineNullRequestResolver : IBaselineRequestResolver
{
    public Task<IBaselineResponse?> Resolve(IBaselineRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IBaselineResponse?>(null);
    }
}
