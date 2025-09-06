# Stores (IaC / Config)

Infra and dev tooling for data stores used by CreditAI:

- **Qdrant** (Vector DB)
- **Blob** (MinIO for dev, S3 for prod)
- **Metadata DB** (Postgres; optional)

> Dev uses Docker Compose. Prod uses Terraform stubs you can adapt to your cloud.

## Dev Quickstart

### Qdrant
```bash
cd stores/qdrant/docker
docker compose -f docker-compose.qdrant.yml up -d
# UI: http://localhost:6333/dashboard (if enabled in your image)
```

### MinIO
```bash
cd stores/blob/minio
docker compose -f docker-compose.minio.yml up -d
# Console: http://localhost:9001  (user=minio, pass=minio123)
```

### Postgres
```bash
cd stores/metadata-db
docker compose -f docker-compose.postgres.yml up -d
# Apply migrations in ./migrations/*.sql to the 'creditai_meta' database
```

## Prod (Terraform)
- `stores/qdrant/terraform/` – example: provision a VM + security group and run Qdrant via Docker.
- `stores/blob/s3/` – example: S3 bucket + IAM policy/user for write-only ingestion and read for RAG.

> These are **templates**; review security, networking, cost and compliance before applying.
