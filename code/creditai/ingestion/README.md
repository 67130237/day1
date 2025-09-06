# Ingestion Service (Worker + Optional Admin API)

Pipeline: **parse → clean → chunk → embed → upsert** to Qdrant (+ optional blob).

## Quick start (dev)
```bash
# Worker
dotnet run --project src/Ingestion.Worker/Ingestion.Worker.csproj

# API (optional)
dotnet run --project src/Ingestion.Api/Ingestion.Api.csproj
# POST /ingest  body: { "paths": ["./data/docs"] }
```
