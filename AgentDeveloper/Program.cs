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
    .AddEnvironmentVariables();

builder.Services.Configure<QdrantOptions>(builder.Configuration.GetSection(QdrantOptions.SectionName));
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));
builder.Services.AddSingleton<IVectorRepository, VectorRepository>();

using var host = builder.Build();
await RunAsync(host.Services);

static async Task RunAsync(IServiceProvider services)
{
    var repo = services.GetRequiredService<IVectorRepository>();
    var options = services.GetRequiredService<IOptions<QdrantOptions>>().Value;
    var random = new Random(42);

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
