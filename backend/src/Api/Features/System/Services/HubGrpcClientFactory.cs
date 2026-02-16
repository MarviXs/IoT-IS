using Fei.Is.Api.Grpc;
using Grpc.Net.Client;

namespace Fei.Is.Api.Features.System.Services;

public class HubGrpcClientFactory
{
    public (GrpcChannel Channel, EdgeHubSync.EdgeHubSyncClient Client) Create(string hubUrl)
    {
        if (string.IsNullOrWhiteSpace(hubUrl))
        {
            throw new ArgumentException("Hub URL is not configured.", nameof(hubUrl));
        }

        var normalized = hubUrl.Trim().TrimEnd('/');
        var channel = GrpcChannel.ForAddress(normalized);
        var client = new EdgeHubSync.EdgeHubSyncClient(channel);
        return (channel, client);
    }
}
