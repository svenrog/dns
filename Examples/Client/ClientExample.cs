using DNS.Client;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Examples.Client;

static class ClientExample
{
    public static void Main(string[] args)
    {
        MainAsync(args).Wait();
    }

    public async static Task MainAsync(string[] args)
    {
        DnsClient client = new("8.8.8.8");

        foreach (string domain in args)
        {
            IList<IPAddress> ips = await client.Lookup(domain).ConfigureAwait(false);
            Console.WriteLine("{0} => {1}", domain, string.Join(", ", ips));
        }
    }
}
