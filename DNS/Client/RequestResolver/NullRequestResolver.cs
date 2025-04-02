using DNS.Protocol;

namespace DNS.Client.RequestResolver;

public class NullRequestResolver : IRequestResolver
{
    public Task<IResponse?> Resolve(IRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IResponse?>(null);
    }
}
