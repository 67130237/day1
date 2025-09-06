# CreditAI Monorepo – Summary

```plaintext
creditai/
├─ README.md
├─ .editorconfig
├─ .gitignore
├─ Directory.Build.props
├─ CreditAI.sln
├─ global.json
│
├─ apis-orchestrator/        # (1) APIs + Orchestrator + Agents + Guardrails & Ops
│  ├─ src/
│  │  ├─ ChatApi/             # Minimal API (entrypoint)
│  │  ├─ Orchestrator.Core/   # OrchestratorService, Router, Composer
│  │  ├─ Agents/              # SQL Analyst, RAG Reader, Financial Calculator
│  │  ├─ Integrations/        # SemanticKernel, OpenRouter, MCP Clients
│  │  ├─ Guardrails/          # RBAC + PII Masking
│  │  └─ Shared/              # Abstractions, Infrastructure, Prompts
│
├─ mcp-sql/                  # (2) MCP Server + MSSQL tooling (RO)
│  ├─ src/
│  │  ├─ Mcp.Sql.Api/         # Minimal API endpoints
│  │  └─ Mcp.Sql.Core/        # SqlRunner, SchemaProvider
│  ├─ openapi/mcp-sql.openapi.yaml
│  └─ Dockerfile
│
├─ mcp-rag/                  # (3a) MCP Server + RAG Query
│  ├─ src/
│  │  ├─ Mcp.Rag.Api/         # rag.hybrid, rag.getDoc
│  │  └─ Mcp.Rag.Core/        # QdrantSearch (stub), LexicalSearch, HybridMerger
│  ├─ openapi/mcp-rag.openapi.yaml
│  └─ Dockerfile
│
├─ ingestion/                # (3b) Knowledge Ingestion Pipeline
│  ├─ src/
│  │  ├─ Ingestion.Worker/    # Worker: parse → clean → chunk → embed → upsert
│  │  └─ Ingestion.Api/       # Optional admin API (/ingest)
│  ├─ config/
│  │  ├─ qdrant.collection.json
│  │  └─ ingestion.rules.yaml
│  └─ Dockerfile
│
├─ stores/                   # (3b) Infra configs (Dev + Prod IaC)
│  ├─ qdrant/                 # docker-compose + terraform
│  ├─ blob/                   # minio (dev), s3 terraform (prod)
│  └─ metadata-db/            # postgres compose + migrations
│
├─ data-sources/             # (4) Upstream Data / Reference Docs
│  ├─ mssql/                  # init scripts, docker-compose
│  └─ docs-seed/              # sample business rules, schema, data dictionary
│
├─ llm/                      # (5) LLM Configs
│  ├─ openrouter/policy.md    # allowed models, rate limits, defaults
│  └─ model-maps.json         # agent/router/composer → model ids
│
├─ scripts/                  # Dev & CI helpers
│  ├─ dev-bootstrap.ps1
│  ├─ seed-docs.ps1
│  └─ ci-build.sh
│
└─ deploy/                   # Deployment
   ├─ compose/docker-compose.dev.yml
   ├─ compose/.env.example
   └─ k8s/                    # manifests (namespaces, services, deployments, hpa)
```

---

## เนื้อหาหลักของแต่ละส่วน

### Root
- `CreditAI.sln` รวมทุกโปรเจกต์
- `Directory.Build.props` → C# LangVersion, Nullable, analyzers
- `.editorconfig` / `.gitignore` → coding style + ignore rules
- `global.json` → pin .NET SDK

### apis-orchestrator
- **ChatApi** — Minimal API: `/chat/turn`, `/healthz`
- **Orchestrator.Core** — flow: RBAC → PII → IntentRouter → Agent → Composer → Mask
- **Agents**
  - SqlAnalystAgent — generate SQL (via LLM) → call `mcp-sql`
  - RagReaderAgent — call `mcp-rag` hybrid search → ground answers
  - FinancialCalculatorAgent — extract params, annuity calc, LLM explanation
- **Integrations**
  - SemanticKernel wrapper (SkKernelFacade)
  - OpenRouterClient (Chat/Embed)
  - MCP Clients (SqlToolClient, RagToolClient)
- **Guardrails** — `RbacGuard`, `PiiMasker` (+ JSON configs)
- **Shared**
  - Abstractions: DTOs, interfaces (IAgent, IRouter, IComposer, etc.)
  - Infrastructure: Polly HTTP resilience, Serilog logging
  - Prompts: ReAct, intent classify, SQL, RAG answer, Calc

### mcp-sql
- Minimal API: `/tools/sql.describeSchema`, `/tools/sql.runQuery`
- Core: `SqlRunner` (RO query exec), `SchemaProvider` (INFORMATION_SCHEMA)
- OpenAPI spec + Dockerfile

### mcp-rag
- Minimal API: `/tools/rag.hybrid`, `/tools/rag.getDoc`
- Core: `QdrantSearch` (stub), `LexicalSearch` (keyword overlap), `HybridMerger`
- OpenAPI spec + Dockerfile

### ingestion
- Worker pipeline: 
  - Parsers (txt/md), 
  - Cleaners (PII scrub), 
  - Chunkers (overlap), 
  - Embedders (OpenRouter embeddings), 
  - Upserters (Qdrant)
- Optional API: `/ingest` → batch upload docs
- Configs: Qdrant collection schema, ingestion rules
- Dockerfile (publish Worker + API)

### stores
- Qdrant: `docker-compose`, Terraform (EC2 example)
- Blob: MinIO compose (dev), S3 Terraform (bucket + IAM users)
- Metadata-DB: Postgres compose + SQL migrations (documents, sections, tags)

### data-sources
- MSSQL:
  - `00_create_db.sql` create DB + RO login
  - `01_sample_tables.sql` Customers, Contracts, Payments
  - `02_views_ro.sql` reporting views + masking
  - `03_security_ro.sql` grant RO user
  - `docker-compose.mssql.yml` dev DB
- docs-seed: business rules, schema, data dictionary (sample markdowns)

### llm
- `openrouter/policy.md` → allowed models (gpt-4o-mini, gpt-4o, embeddings)
- `model-maps.json` → intent/router/composer/agents/embeddings mapping

### scripts
- `dev-bootstrap.ps1` → init user-secrets, certs
- `seed-docs.ps1` → post docs to ingestion API
- `ci-build.sh` → build + test + publish orchestrator

### deploy
- **Compose (dev)**: bring up ChatApi, MCP-SQL, MCP-RAG, Ingestion, MSSQL, Qdrant, Redis, MinIO
- **K8s (prod)**: 
  - Namespaces
  - Deployments: orchestrator, mcp-sql, mcp-rag, ingestion
  - StatefulSets: redis, qdrant, minio
  - Services for each
  - HPA for orchestrator

---

✅ สรุป: ทั้งหมดนี้คือ **MVP monorepo** สำหรับระบบ *Agentic AI + MCP (SQL, RAG) + Ingestion + Guardrails + Orchestrator + LLM configs* พร้อมใช้งานทั้ง Dev (Compose) และ Prod (K8s + Terraform templates)
