using AgentDeveloper.Configuration;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace AgentDeveloper.Services;

public class VectorRepository : IVectorRepository
{
    private readonly QdrantClient _client;
    private readonly QdrantOptions _options;

    public VectorRepository(IOptions<QdrantOptions> options)
    {
        _options = options.Value;
        _client = new QdrantClient(_options.Host, _options.Port, https: _options.UseHttps, apiKey: _options.ApiKey);
    }

    public async Task EnsureCollectionAsync(CancellationToken ct = default)
    {
        var exists = await _client.CollectionExistsAsync(_options.Collection, ct);
        if (exists)
        {
            return;
        }

        await _client.CreateCollectionAsync(
            _options.Collection,
            new VectorParams
            {
                Size = (ulong)_options.Dimensions,
                Distance = Distance.Cosine
            },
            cancellationToken: ct);
    }

    public Task UpsertAsync(IReadOnlyList<PointStruct> points, CancellationToken ct = default)
        => _client.UpsertAsync(_options.Collection, points.ToList(), cancellationToken: ct);

    public Task DeleteAsync(IReadOnlyList<ulong> ids, CancellationToken ct = default)
        => _client.DeleteAsync(_options.Collection, ids.ToList(), cancellationToken: ct);

    public Task<ulong> CountAsync(CancellationToken ct = default)
        => _client.CountAsync(_options.Collection, cancellationToken: ct);

    public async Task<IReadOnlyList<RetrievedPoint>> ListAsync(int limit = 100, CancellationToken ct = default)
    {
        var response = await _client.ScrollAsync(_options.Collection, limit: (uint)limit, cancellationToken: ct);
        return response.Result;
    }

    public Task<IReadOnlyList<ScoredPoint>> SearchAsync(ReadOnlyMemory<float> vector, int limit = 10, CancellationToken ct = default)
        => _client.QueryAsync(_options.Collection, query: vector.ToArray(), limit: (ulong)limit, cancellationToken: ct);
}
