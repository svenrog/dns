using DNS.Client;
using DNS.Server;
using System;
using System.Threading.Tasks;

namespace Examples.Server
{
    static class ServerExample
    {
        public static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        public async static Task MainAsync(string[] args)
        {
            MasterFile masterFile = new();
            DnsServer server = new(masterFile, "8.8.8.8");

            foreach (string domain in args)
            {
                Console.WriteLine("Redirecting {0} to localhost", domain);
                masterFile.AddIPAddressResourceRecord(domain, "127.0.0.1");
            }

            server.Responded += (sender, e) => Console.WriteLine("{0} => {1}", e.Request, e.Response);
            server.Listening += (sender, e) => Console.WriteLine("Listening");
            server.Errored += (sender, e) =>
            {
                Console.WriteLine("Errored: {0}", e.Exception);

                if (e.Exception is ResponseException responseError)
                    Console.WriteLine(responseError.Response);
            };

            await server.Listen().ConfigureAwait(false);
        }
    }
}
