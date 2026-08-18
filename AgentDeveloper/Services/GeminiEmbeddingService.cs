using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AgentDeveloper.Configuration;
using Microsoft.Extensions.Options;

namespace AgentDeveloper.Services;

public class GeminiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _options;

    public GeminiEmbeddingService(HttpClient http, IOptions<GeminiOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken ct = default)
    {
        var request = new EmbedContentRequest
        {
            Model = $"models/{_options.Model}",
            Content = new Content { Parts = [new Part { Text = text }] },
            OutputDimensionality = _options.Dimensions
        };

        var url = $"{_options.Endpoint}/v1beta/models/{_options.Model}:embedContent?key={_options.ApiKey}";

        var response = await _http.PostAsJsonAsync(url, request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EmbedContentResponse>(ct);
        var values = result?.Embedding?.Values ?? throw new InvalidOperationException("Resposta do Gemini sem embedding.");

        return values.Select(v => (float)v).ToArray();
    }

    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken ct = default)
    {
        var items = texts.ToList();
        var request = new BatchEmbedContentRequest
        {
            Requests = items.Select(t => new EmbedContentRequest
            {
                Model = $"models/{_options.Model}",
                Content = new Content { Parts = [new Part { Text = t }] },
                OutputDimensionality = _options.Dimensions
            }).ToList()
        };

        var url = $"{_options.Endpoint}/v1beta/models/{_options.Model}:batchEmbedContents?key={_options.ApiKey}";

        var response = await _http.PostAsJsonAsync(url, request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<BatchEmbedContentResponse>(ct);
        var embeddings = result?.Embeddings ?? throw new InvalidOperationException("Resposta do Gemini sem embeddings.");

        return embeddings.Select(e => (ReadOnlyMemory<float>)e.Values.Select(v => (float)v).ToArray()).ToList();
    }

    private sealed class EmbedContentRequest
    {
        [JsonPropertyName("model")] public string Model { get; set; } = "";
        [JsonPropertyName("content")] public Content Content { get; set; } = new();
        [JsonPropertyName("outputDimensionality")] public int? OutputDimensionality { get; set; }
    }

    private sealed class BatchEmbedContentRequest
    {
        [JsonPropertyName("requests")] public List<EmbedContentRequest> Requests { get; set; } = [];
    }

    private sealed class Content
    {
        [JsonPropertyName("parts")] public List<Part> Parts { get; set; } = [];
    }

    private sealed class Part
    {
        [JsonPropertyName("text")] public string Text { get; set; } = "";
    }

    private sealed class EmbedContentResponse
    {
        [JsonPropertyName("embedding")] public Embedding? Embedding { get; set; }
    }

    private sealed class BatchEmbedContentResponse
    {
        [JsonPropertyName("embeddings")] public List<Embedding> Embeddings { get; set; } = [];
    }

    private sealed class Embedding
    {
        [JsonPropertyName("values")] public List<double> Values { get; set; } = [];
    }
}
