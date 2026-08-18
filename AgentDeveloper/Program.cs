using AgentDeveloper.Configuration;
using AgentDeveloper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Qdrant.Client.Grpc;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Services.Configure<QdrantOptions>(builder.Configuration.GetSection(QdrantOptions.SectionName));
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection(GeminiOptions.SectionName));
builder.Services.AddSingleton<IVectorRepository, VectorRepository>();
builder.Services.AddHttpClient<IEmbeddingService, GeminiEmbeddingService>();

using var host = builder.Build();
await RunAsync(host.Services);

static async Task RunAsync(IServiceProvider services)
{
    var repo = services.GetRequiredService<IVectorRepository>();
    var options = services.GetRequiredService<IOptions<QdrantOptions>>().Value;
    var embeddingService = services.GetRequiredService<IEmbeddingService>();
    var geminiOptions = services.GetRequiredService<IOptions<GeminiOptions>>().Value;
    var random = new Random(42);

    Console.WriteLine("== Fase 2: embeddings Gemini ==");
    Console.WriteLine($"Modelo: {geminiOptions.Model} | Dimensão alvo: {geminiOptions.Dimensions}");

    var text = "O Qdrant é um banco de dados vetorial de alto desempenho.";
    var vector = await embeddingService.EmbedAsync(text);
    Console.WriteLine($"Embedding de \"{text}\":");
    Console.WriteLine($"  Dimensões: {vector.Length}");
    Console.WriteLine($"  Primeiros valores: {string.Join(", ", vector.Span[..Math.Min(5, vector.Length)].ToArray().Select(v => v.ToString("0.000")))}");

    var similar = await embeddingService.EmbedAsync("O Qdrant é um banco vetorial rápido para busca semântica.");
    var different = await embeddingService.EmbedAsync("Receita de bolo de chocolate.");

    float Cosine(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        float dot = 0, na = 0, nb = 0;
        for (int i = 0; i < a.Length; i++) { dot += a[i] * b[i]; na += a[i] * a[i]; nb += b[i] * b[i]; }
        return dot / (MathF.Sqrt(na) * MathF.Sqrt(nb));
    }

    Console.WriteLine($"  Similaridade coseno (textos parecidos): {Cosine(vector.Span, similar.Span):0.000}");
    Console.WriteLine($"  Similaridade coseno (texto diferente):   {Cosine(vector.Span, different.Span):0.000}");

    Console.WriteLine("== Fase 1: conexão com Qdrant ==");
    Console.WriteLine($"Coleção: {options.Collection} | Dimensão: {options.Dimensions}");

    await repo.EnsureCollectionAsync();
    Console.WriteLine("Coleção garantida (criada se não existia).");

    var count = await repo.CountAsync();
    Console.WriteLine($"Pontos antes: {count}");

    var points = Enumerable.Range(1, 5).Select(i =>
    {
        var v = Enumerable.Range(0, options.Dimensions)
            .Select(_ => (float)(random.NextDouble() * 2 - 1))
            .ToArray();
        return new PointStruct
        {
            Id = (ulong)i,
            Vectors = v,
            Payload = { ["text"] = $"Documento exemplo {i}" }
        };
    }).ToList();

    await repo.UpsertAsync(points);
    Console.WriteLine($"Inseridos {points.Count} pontos de exemplo.");

    count = await repo.CountAsync();
    Console.WriteLine($"Pontos depois: {count}");

    var all = await repo.ListAsync();
    Console.WriteLine($"Listados {all.Count} pontos:");
    foreach (var p in all)
    {
        Console.WriteLine($"  - Id {p.Id}: {p.Payload["text"].StringValue}");
    }

    Console.WriteLine("== Sucesso: Qdrant conectado e operacional ==");
}
