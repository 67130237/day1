# CreditAI Monorepo (MVP Root)

This repository is the **root** for the CreditAI project — an agentic AI system with:
- **APIs + Orchestrator + Agents + Guardrails** (aspnetcore)
- **MCP Servers** (SQL, RAG)
- **Ingestion** pipeline and **Stores** (Qdrant/Blob)
- **Deploy** assets (Docker/K8s/Compose)

> This commit contains only **root-level scaffolding**. You can open the solution and add or load projects as we build each module (e.g., `mcp-sql`, `mcp-rag`, `apis-orchestrator`).

## Quick Start

1. **Open solution**
   - Visual Studio / Rider / VS Code: open `CreditAI.sln`
   - .NET SDK: 2025-09-05 — pinned via `global.json` (LTS)

2. **Add modules (later, step-by-step)**
   - `apis-orchestrator/` (ChatApi, Orchestrator.Core, Agents, Guardrails)
   - `mcp-sql/` (Tools: `sql.describeSchema`, `sql.runQuery`)
   - `mcp-rag/` (Tools: `rag.hybrid`, `rag.getDoc`)
   - `ingestion/` (Worker & optional API)
   - `deploy/` (compose/k8s)

3. **Conventions**
   - **C#**: .NET 8 LTS, C# 12 (see `Directory.Build.props`)
   - **Analyzer level**: `latest-recommended`
   - **Nullable**: enabled
   - **ImplicitUsings**: enabled

4. **Branch/CI suggestions**
   - `main`: protected, release tags
   - `dev`: default development branch
   - CI: build solution, run analyzers, create artifacts

## Repo Layout (target)
```
creditai/
  apis-orchestrator/  # Web API + Orchestrator + Agents
  mcp-sql/            # MCP Server (SQL tooling, RO)
  mcp-rag/            # MCP Server (RAG Query)
  ingestion/          # Ingestion worker + (optional) API
  stores/             # Qdrant/Blob configs
  deploy/             # compose & k8s
```
> We'll add these folders as we implement each module.

## License
Internal; choose a license later.
