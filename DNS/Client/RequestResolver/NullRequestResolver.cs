using DNS.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace DNS.Client.RequestResolver
{
    public class NullRequestResolver : IRequestResolver
    {
        public Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default)
        {
            throw new ResponseException("Request failed");
        }
    }
}
