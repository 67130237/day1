# MCP RAG Server

RAG query MCP service providing:
- `POST /tools/rag.hybrid`
- `POST /tools/rag.getDoc`
- (optional) `POST /tools/rag.rerank`

Backed by Qdrant (vector) + in-memory lexical (BM25-lite). For MVP, document bodies can be stored in Qdrant payload directly.

## Run (dev)
```bash
dotnet run --project src/Mcp.Rag.Api/Mcp.Rag.Api.csproj
# or Docker
docker build -t mcp-rag .
docker run --rm -p 8082:8080 -e Qdrant__Endpoint=http://qdrant:6333 mcp-rag
```
