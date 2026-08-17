using Qdrant.Client.Grpc;

namespace AgentDeveloper.Services;

public interface IVectorRepository
{
    Task EnsureCollectionAsync(CancellationToken ct = default);
    Task UpsertAsync(IReadOnlyList<PointStruct> points, CancellationToken ct = default);
    Task DeleteAsync(IReadOnlyList<ulong> ids, CancellationToken ct = default);
    Task<ulong> CountAsync(CancellationToken ct = default);
    Task<IReadOnlyList<RetrievedPoint>> ListAsync(int limit = 100, CancellationToken ct = default);
    Task<IReadOnlyList<ScoredPoint>> SearchAsync(ReadOnlyMemory<float> vector, int limit = 10, CancellationToken ct = default);
}
