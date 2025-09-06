<#
seed-docs.ps1
Push sample docs into ingestion API for dev
#>

param(
  [string]$ApiUrl = "http://localhost:8080/ingest",
  [string]$DocsPath = "../data-sources/docs-seed"
)

Write-Host "Seeding docs from $DocsPath to $ApiUrl"

if (!(Test-Path $DocsPath)) {
  Write-Error "Docs path not found: $DocsPath"
  exit 1
}

$body = @{ paths = @($DocsPath) } | ConvertTo-Json -Depth 5
Invoke-RestMethod -Uri $ApiUrl -Method POST -ContentType "application/json" -Body $body

Write-Host "Done."
