# MCP SQL Server (Read-Only)

Minimal Compute-Protocol (MCP) server for SQL Server tooling (read-only).  
Endpoints:
- `POST /tools/sql.describeSchema`
- `POST /tools/sql.runQuery`
- `GET  /healthz`

## Run (dev)
```bash
dotnet run --project src/Mcp.Sql.Api/Mcp.Sql.Api.csproj
# or Docker
docker build -t mcp-sql .
docker run --rm -p 8081:8080 -e ConnectionStrings__SqlRo="..." mcp-sql
```
