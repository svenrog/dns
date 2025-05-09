using DNS.Protocol;

namespace DNS.Client.RequestResolver;

public interface IRequestResolver
{
    Task<IResponse?> Resolve(IRequest request, CancellationToken cancellationToken = default);
}
