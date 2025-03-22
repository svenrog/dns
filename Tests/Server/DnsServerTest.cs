using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Server;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DNS.Tests.Server
{

    public class DnsServerTest
    {
        private const int _port = 64646;

        [Fact]
        public async Task ServerLookup()
        {
            await Create(new IPAddressRequestResolver(), async server =>
            {
                DnsServer.RequestedEventArgs requestedEvent = null;
                DnsServer.RespondedEventArgs respondedEvent = null;
                DnsServer.ErroredEventArgs erroredEvent = null;

                server.Requested += (sender, e) =>
                {
                    requestedEvent = e;
                };

                server.Responded += (sender, e) =>
                {
                    respondedEvent = e;
                };

                server.Errored += (sender, e) =>
                {
                    erroredEvent = e;
                };

                Request clientRequest = new();
                Question clientRequestQuestion = new(new Domain("google.com"), RecordType.A);

                clientRequest.Id = 1;
                clientRequest.Questions.Add(clientRequestQuestion);
                clientRequest.OperationCode = OperationCode.Query;

                IResponse clientResponse = await Resolve(clientRequest).ConfigureAwait(false);

                Assert.Equal(1, clientResponse.Id);
                Assert.Single(clientResponse.Questions);
                Assert.Single(clientResponse.AnswerRecords);
                Assert.Empty(clientResponse.AuthorityRecords);
                Assert.Empty(clientResponse.AdditionalRecords);

                Question clientResponseQuestion = clientResponse.Questions[0];

                Assert.Equal(RecordType.A, clientResponseQuestion.Type);
                Assert.Equal("google.com", clientResponseQuestion.Name.ToString());

                IResourceRecord clientResponseRecord = clientResponse.AnswerRecords[0];

                Assert.Equal("google.com", clientResponseRecord.Name.ToString());
                Assert.Equal(Helper.GetArray<byte>(192, 168, 0, 1), clientResponseRecord.Data);
                Assert.Equal(RecordType.A, clientResponseRecord.Type);

                Assert.NotNull(requestedEvent);

                Assert.NotNull(requestedEvent.Request);
                Assert.Equal(1, requestedEvent.Request.Id);
                Assert.Single(requestedEvent.Request.Questions);

                Question requestedRequestQuestion = requestedEvent.Request.Questions[0];

                Assert.Equal(RecordType.A, requestedRequestQuestion.Type);
                Assert.Equal("google.com", requestedRequestQuestion.Name.ToString());

                Assert.NotNull(respondedEvent);
                Assert.Equal(requestedEvent.Request, respondedEvent.Request);

                Assert.NotNull(respondedEvent.Response);

                Assert.Equal(1, respondedEvent.Response.Id);
                Assert.Single(respondedEvent.Response.Questions);
                Assert.Single(respondedEvent.Response.AnswerRecords);
                Assert.Empty(respondedEvent.Response.AuthorityRecords);
                Assert.Empty(respondedEvent.Response.AdditionalRecords);

                Question respondedResponseQuestion = respondedEvent.Response.Questions[0];

                Assert.Equal(RecordType.A, respondedResponseQuestion.Type);
                Assert.Equal("google.com", respondedResponseQuestion.Name.ToString());

                IResourceRecord respondedResponseRecord = respondedEvent.Response.AnswerRecords[0];

                Assert.Equal("google.com", respondedResponseRecord.Name.ToString());
                Assert.Equal(Helper.GetArray<byte>(192, 168, 0, 1), respondedResponseRecord.Data);
                Assert.Equal(RecordType.A, respondedResponseRecord.Type);

                Assert.Null(erroredEvent);
            });
        }

        private async static Task Create(IRequestResolver requestResolver, Func<DnsServer, Task> action)
        {
            TaskCompletionSource<object> tcs = new();
            DnsServer server = new(requestResolver);

            server.Listening += async (sender, _) =>
            {
                try
                {
                    await action(server).ConfigureAwait(false);
                    tcs.SetResult(null);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
                finally
                {
                    server.Dispose();
                }
            };

            await Task.WhenAll(server.Listen(_port), tcs.Task).ConfigureAwait(false);
        }

        private async static Task<IResponse> Resolve(Request request)
        {
            using UdpClient udp = new();
            IPEndPoint endPoint = new(IPAddress.Parse("127.0.0.1"), _port);

            await udp.SendAsync(request.ToArray(), request.Size, endPoint).ConfigureAwait(false);
            UdpReceiveResult result = await udp.ReceiveAsync().ConfigureAwait(false);

            return Response.FromArray(result.Buffer);
        }

        private class IPAddressRequestResolver : IRequestResolver
        {
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
            public Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default)
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
            {
                Response response = Response.FromRequest(request);
                IResourceRecord record = new IPAddressResourceRecord(
                    new Domain("google.com"),
                    IPAddress.Parse("192.168.0.1"));

                response.AnswerRecords.Add(record);

                return Task.FromResult<IResponse>(response);
            }
        }
    }
}
