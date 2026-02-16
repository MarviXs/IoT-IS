using System.Net.Http.Headers;

namespace Fei.Is.Api.Features.System.Services;

public sealed class HubApiClientFactory(IHttpClientFactory httpClientFactory)
{
    private const string ApiSegment = "api";
    private const string EdgeTokenHeader = "x-edge-token";

    public HttpClient Create(string hubUrl, string hubToken)
    {
        if (string.IsNullOrWhiteSpace(hubUrl))
        {
            throw new ArgumentException("Hub URL is not configured.", nameof(hubUrl));
        }

        if (string.IsNullOrWhiteSpace(hubToken))
        {
            throw new ArgumentException("Hub token is not configured.", nameof(hubToken));
        }

        if (!Uri.TryCreate(hubUrl.Trim(), UriKind.Absolute, out var baseUri))
        {
            throw new ArgumentException("Hub URL is invalid.", nameof(hubUrl));
        }

        var client = httpClientFactory.CreateClient(nameof(HubApiClientFactory));
        client.BaseAddress = BuildApiBaseAddress(baseUri);
        client.DefaultRequestHeaders.Remove(EdgeTokenHeader);
        client.DefaultRequestHeaders.Add(EdgeTokenHeader, hubToken.Trim());
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return client;
    }

    private static Uri BuildApiBaseAddress(Uri hubUri)
    {
        var path = hubUri.AbsolutePath.TrimEnd('/');
        if (!path.EndsWith($"/{ApiSegment}", StringComparison.OrdinalIgnoreCase))
        {
            path = string.IsNullOrEmpty(path) ? $"/{ApiSegment}" : $"{path}/{ApiSegment}";
        }

        var builder = new UriBuilder(hubUri)
        {
            Path = $"{path}/",
            Query = string.Empty,
            Fragment = string.Empty
        };

        return builder.Uri;
    }
}
