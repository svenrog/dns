using DNS.Client;
using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DNS.Tests.Client;


public class DnsClientTest
{
    [Fact]
    public async Task ClientLookup()
    {
        DnsClient client = new(new IPAddressRequestResolver());
        IList<IPAddress> ips = await client.Lookup("google.com");

        Assert.Single(ips);
        Assert.Equal("192.168.0.1", ips[0].ToString());
    }

    [Fact]
    public async Task ClientReverse()
    {
        DnsClient client = new(new PointerRequestResolver());
        string domain = await client.Reverse("192.168.0.1");

        Assert.Equal("google.com", domain);
    }

    [Fact]
    public async Task ClientNameError()
    {
        DnsClient client = new(new NameErrorRequestResolver());

        await Assert.ThrowsAsync<ResponseException>(() =>
        {
            return client.Lookup("google.com");
        });
    }

    private class IPAddressRequestResolver : IRequestResolver
    {
        public Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default)
        {
            Response response = Response.FromRequest(request);
            IResourceRecord record = new IPAddressResourceRecord(
                new Domain("google.com"),
                IPAddress.Parse("192.168.0.1"));

            response.AnswerRecords.Add(record);
            return Task.FromResult<IResponse>(response);
        }
    }

    private class PointerRequestResolver : IRequestResolver
    {
        public Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default)
        {
            Response response = Response.FromRequest(request);
            IResourceRecord record = new PointerResourceRecord(
                IPAddress.Parse("192.168.0.1"),
                new Domain("google.com"));

            response.AnswerRecords.Add(record);
            return Task.FromResult<IResponse>(response);
        }
    }

    private class NameErrorRequestResolver : IRequestResolver
    {
        public Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default)
        {
            Response response = Response.FromRequest(request);
            response.ResponseCode = ResponseCode.NameError;
            return Task.FromResult<IResponse>(response);
        }
    }
}
