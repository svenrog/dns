﻿using DNS.Protocol;
using System.Net;
using System.Net.Sockets;

namespace DNS.Client.RequestResolver;

public class TcpRequestResolver : IRequestResolver
{
    private readonly IPEndPoint _dns;

    public TcpRequestResolver(IPEndPoint dns)
    {
        _dns = dns;
    }

    public async Task<IResponse?> Resolve(IRequest request, CancellationToken cancellationToken = default)
    {
        using TcpClient tcp = new(_dns.AddressFamily);
        await tcp.ConnectAsync(_dns.Address, _dns.Port).ConfigureAwait(false);

        NetworkStream stream = tcp.GetStream();
        byte[] buffer = request.ToArray();
        byte[] length = BitConverter.GetBytes((ushort)buffer.Length);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(length);
        }

        await stream.WriteAsync(length, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);

        buffer = new byte[2];
        await Read(stream, buffer, cancellationToken).ConfigureAwait(false);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(buffer);
        }

        buffer = new byte[BitConverter.ToUInt16(buffer, 0)];
        await Read(stream, buffer, cancellationToken).ConfigureAwait(false);

        IResponse response = Response.FromArray(buffer);
        return new ClientResponse(request, response, buffer);
    }

    private static async Task Read(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        int length = buffer.Length;
        int offset = 0;
        int size;

        while (length > 0 && (size = await stream.ReadAsync(buffer, offset, length, cancellationToken).ConfigureAwait(false)) > 0)
        {
            offset += size;
            length -= size;
        }

        if (length > 0)
        {
            throw new IOException("Unexpected end of stream");
        }
    }
}
