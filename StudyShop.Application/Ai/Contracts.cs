namespace StudyShop.Application.Ai;

public sealed class AiOptions
{
    public string BaseUrl { get; set; } = "http://ollama:11434";
    public string ChatModel { get; set; } = "llama3.2:3b";
    public string EmbeddingModel { get; set; } = "bge-m3";
}

public sealed class VectorStoreOptions
{
    public string ConnectionString { get; set; } = string.Empty;
}

public interface IOllamaClient
{
    Task<float[]> EmbedAsync(string model, string text, CancellationToken ct);
    IAsyncEnumerable<string> ChatAsync(string model, string prompt, CancellationToken ct);
}

public interface IEmbeddingService
{
    Task<float[]> CreateEmbedding(string text, CancellationToken ct);
}

public interface ILlmService
{
    IAsyncEnumerable<string> Generate(string prompt, CancellationToken ct);
}

public interface IVectorStore
{
    Task EnsureSchemaAsync(CancellationToken ct);
    Task UpsertProductEmbeddingAsync(int productId, int chunkIndex, string content, float[] embedding, CancellationToken ct);
    Task<IReadOnlyList<(int productId, string content, double score)>> QueryProductsAsync(float[] embedding, int topK, CancellationToken ct);
}


