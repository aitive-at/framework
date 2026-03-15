using System.Net.Http.Headers;
using Aitive.Framework.Cryptography.Hashing.Algorithms;
using Aitive.Framework.Data.Transfer;
using Aitive.Framework.Functional;

namespace Aitive.Framework.Http.Transfer;

/// <summary>
/// An <see cref="IDataTransferHandler"/> that reads a single HTTP resource via range requests.
/// The resource URL is resolved from <paramref name="baseUrl"/> and <paramref name="relativePath"/>
/// at construction time.
/// </summary>
/// <remarks>
/// If the server includes a <c>Content-Digest</c> header (RFC 9530) with a <c>sha-256</c>
/// member, it is exposed as the chunk hash in <see cref="DataTransferReadResult"/>.
/// When absent, the hash is <see cref="Optional{T}.None"/> and the transfer infrastructure
/// skips per-chunk integrity verification.
/// </remarks>
public sealed class HttpDataTransferHandler : IDataTransferHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Uri _baseUrl;

    /// <param name="httpClientFactory">Factory for creating <see cref="HttpClient"/> instances.</param>
    /// <param name="baseUrl">
    /// Absolute base URL. Must end with a trailing slash when it represents a directory
    /// (e.g. <c>https://cdn.example.com/files/</c>).
    /// </param>
    public HttpDataTransferHandler(IHttpClientFactory httpClientFactory, Uri baseUrl)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(baseUrl);

        if (!baseUrl.IsAbsoluteUri)
        {
            throw new ArgumentException("Base URL must be absolute.", nameof(baseUrl));
        }

        _httpClientFactory = httpClientFactory;
        _baseUrl = baseUrl;
    }

    public async Task<Result<DataTransferReadResult, DataTransferError>> Read(
        string path,
        Memory<byte> target,
        long offset,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            var url = new Uri(_baseUrl, path);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Range = new RangeHeaderValue(offset, offset + target.Length - 1);

            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            var bytesRead = await ReadResponseBodyAsync(response, target, cancellationToken);

            if (bytesRead <= 0)
            {
                return new Result<DataTransferReadResult, DataTransferError>(
                    DataTransferError.Unknown
                );
            }

            var hash = TryParseContentDigest(response);

            return new Result<DataTransferReadResult, DataTransferError>(
                new DataTransferReadResult(bytesRead, hash)
            );
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return new Result<DataTransferReadResult, DataTransferError>(DataTransferError.Unknown);
        }
    }

    // ──────────────────────────────────────────────────────────────────
    //  Response body reading
    // ──────────────────────────────────────────────────────────────────

    private static async Task<int> ReadResponseBodyAsync(
        HttpResponseMessage response,
        Memory<byte> target,
        CancellationToken cancellationToken
    )
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var totalRead = 0;
        while (totalRead < target.Length)
        {
            var read = await stream.ReadAsync(target[totalRead..], cancellationToken);

            if (read == 0)
            {
                break;
            }

            totalRead += read;
        }

        return totalRead;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Content-Digest parsing (RFC 9530)
    // ──────────────────────────────────────────────────────────────────

    private static Optional<Sha256Value> TryParseContentDigest(HttpResponseMessage response)
    {
        if (
            !response.Headers.TryGetValues("Content-Digest", out var values)
            && !response.Content.Headers.TryGetValues("Content-Digest", out values)
        )
        {
            return Optional<Sha256Value>.None;
        }

        // Content-Digest is a Structured Fields Dictionary (RFC 8941).
        // We look for the sha-256 member: sha-256=:base64data:
        foreach (var headerValue in values)
        {
            var hash = TryExtractSha256(headerValue);
            if (hash)
            {
                return hash;
            }
        }

        return Optional<Sha256Value>.None;
    }

    /// <summary>
    /// Extracts the sha-256 byte-sequence from a Structured Fields Dictionary value.
    /// Expected format: <c>sha-256=:base64data:</c> (members separated by <c>, </c>).
    /// </summary>
    private static Optional<Sha256Value> TryExtractSha256(string headerValue)
    {
        foreach (var member in headerValue.Split(','))
        {
            var trimmed = member.Trim();

            if (!trimmed.StartsWith("sha-256=:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Strip "sha-256=:" prefix.
            var base64Part = trimmed["sha-256=:".Length..];

            // Value must be terminated by ':'.
            var closingColon = base64Part.IndexOf(':');
            if (closingColon < 0)
            {
                continue;
            }

            base64Part = base64Part[..closingColon];

            try
            {
                var bytes = Convert.FromBase64String(base64Part);
                if (bytes.Length == Sha256Value.Size)
                {
                    return Sha256Value.FromBytes(bytes);
                }
            }
            catch (FormatException)
            {
                // Malformed base64 — skip this member.
            }
        }

        return Optional<Sha256Value>.None;
    }
}
