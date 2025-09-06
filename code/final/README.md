# CreditAI Orchestrator (Single .csproj)

- .NET 8 + Microsoft.SemanticKernel
- Agentic Orchestrator with SQL Analyst (MSSQL MCP) and RAG Reader (RAG MCP: search)
- Guardrails: API Key middleware + PII masking (Thai ID, phone, email, license plate)
- Endpoints: `/chat`, `/agents/sql:run`, `/rag/search`, `/rag/ingest`, `/tools`, `/health`

## Run
```bash
dotnet run --project src/CreditAI.Orchestrator.csproj
```

Headers:
```
X-API-Key: dev-key-123
Content-Type: application/json
```

## Config
Edit `src/appsettings.json` for OpenRouter, MCP transports (stdio/http), and RBAC keys.

## Notes
- `StdioMcpClient` is a placeholder with TODO for full MCP stdio handshake and tool calls.
- `HttpMcpClient` assumes a simple `/tools` and `/call` shape; adjust to your actual server.
- RAG MCP only supports `rag.search(query,k)` per requirements; `/rag/ingest` is stubbed for now.
