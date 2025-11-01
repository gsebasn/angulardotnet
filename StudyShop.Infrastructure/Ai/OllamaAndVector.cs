using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Npgsql;
using Pgvector;
using StudyShop.Application.Ai;

namespace StudyShop.Infrastructure.Ai;

public sealed class OllamaClient(HttpClient http, IOptions<AiOptions> opts) : IOllamaClient
{
    private readonly HttpClient _http = http;
    private readonly AiOptions _opts = opts.Value;

    public async Task<float[]> EmbedAsync(string model, string text, CancellationToken ct)
    {
        var request = new { model = string.IsNullOrWhiteSpace(model) ? _opts.EmbeddingModel : model, input = text };
        using var res = await _http.PostAsJsonAsync(new Uri(new Uri(_opts.BaseUrl), "/api/embeddings"), request, ct);
        res.EnsureSuccessStatusCode();
        using var doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var arr = doc.RootElement.GetProperty("embedding");
        var list = new List<float>(arr.GetArrayLength());
        foreach (var v in arr.EnumerateArray()) list.Add(v.GetSingle());
        return list.ToArray();
    }

    public async IAsyncEnumerable<string> ChatAsync(string model, string prompt, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        var request = new { model = string.IsNullOrWhiteSpace(model) ? _opts.ChatModel : model, prompt, stream = true };
        using var res = await _http.PostAsJsonAsync(new Uri(new Uri(_opts.BaseUrl), "/api/generate"), request, ct);
        res.EnsureSuccessStatusCode();
        using var stream = await res.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;
            using var json = JsonDocument.Parse(line);
            if (json.RootElement.TryGetProperty("response", out var token))
                yield return token.GetString() ?? string.Empty;
        }
    }
}

public sealed class EmbeddingService(IOllamaClient client, IOptions<AiOptions> opts) : IEmbeddingService
{
    private readonly IOllamaClient _client = client;
    private readonly AiOptions _opts = opts.Value;
    public Task<float[]> CreateEmbedding(string text, CancellationToken ct) => _client.EmbedAsync(_opts.EmbeddingModel, text, ct);
}

public sealed class LlmService(IOllamaClient client, IOptions<AiOptions> opts) : ILlmService
{
    private readonly IOllamaClient _client = client;
    private readonly AiOptions _opts = opts.Value;
    public IAsyncEnumerable<string> Generate(string prompt, CancellationToken ct) => _client.ChatAsync(_opts.ChatModel, prompt, ct);
}

public sealed class PostgresVectorStore(string connectionString) : IVectorStore
{
    private readonly string _cs = connectionString;

    public async Task EnsureSchemaAsync(CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"CREATE EXTENSION IF NOT EXISTS vector;
CREATE TABLE IF NOT EXISTS product_embeddings (
    product_id int NOT NULL,
    chunk_idx int NOT NULL,
    content text NOT NULL,
    embedding vector(1536) NOT NULL,
    updated_at timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY(product_id, chunk_idx)
);
CREATE INDEX IF NOT EXISTS idx_product_embeddings_embedding ON product_embeddings USING ivfflat (embedding vector_cosine_ops);";
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpsertProductEmbeddingAsync(int productId, int chunkIndex, string content, float[] embedding, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO product_embeddings(product_id, chunk_idx, content, embedding)
VALUES (@pid, @idx, @content, @emb)
ON CONFLICT (product_id, chunk_idx) DO UPDATE SET content = EXCLUDED.content, embedding = EXCLUDED.embedding, updated_at = now();";
        cmd.Parameters.AddWithValue("@pid", productId);
        cmd.Parameters.AddWithValue("@idx", chunkIndex);
        cmd.Parameters.AddWithValue("@content", content);
        cmd.Parameters.AddWithValue("@emb", new Vector(embedding));
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<(int productId, string content, double score)>> QueryProductsAsync(float[] embedding, int topK, CancellationToken ct)
    {
        await using var conn = new NpgsqlConnection(_cs);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT product_id, content, (embedding <=> @emb) AS score FROM product_embeddings ORDER BY embedding <=> @emb LIMIT @k;";
        cmd.Parameters.AddWithValue("@emb", new Vector(embedding));
        cmd.Parameters.AddWithValue("@k", topK);
        using var reader = await cmd.ExecuteReaderAsync(ct);
        var results = new List<(int, string, double)>();
        while (await reader.ReadAsync(ct))
        {
            results.Add((reader.GetInt32(0), reader.GetString(1), reader.GetDouble(2)));
        }
        return results;
    }
}

public sealed class NoopVectorStore : IVectorStore
{
    public Task EnsureSchemaAsync(CancellationToken ct) => Task.CompletedTask;
    public Task UpsertProductEmbeddingAsync(int productId, int chunkIndex, string content, float[] embedding, CancellationToken ct) => Task.CompletedTask;
    public Task<IReadOnlyList<(int productId, string content, double score)>> QueryProductsAsync(float[] embedding, int topK, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<(int, string, double)>>(new List<(int, string, double)>());
}


