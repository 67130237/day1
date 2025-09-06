// RagOptions.cs
namespace Mcp.Rag.Core;
public sealed class RagOptions
{
    public QdrantOptions Qdrant { get; set; } = new();
    public sealed class QdrantOptions
    {
        public string Endpoint { get; set; } = "http://localhost:6333";
        public string Collection { get; set; } = "creditai-docs";
        public string? ApiKey { get; set; }
    }
}
