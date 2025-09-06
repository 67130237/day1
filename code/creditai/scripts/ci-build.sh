#!/usr/bin/env bash
set -euo pipefail

echo "=== CI Build (CreditAI) ==="

# Restore & build solution
dotnet restore CreditAI.sln
dotnet build CreditAI.sln -c Release --no-restore

# Run tests (if any)
if ls **/*Tests.csproj 1> /dev/null 2>&1; then
  dotnet test CreditAI.sln -c Release --no-build --logger "trx;LogFileName=test_results.trx"
else
  echo "No test projects found."
fi

# Example: publish orchestrator
dotnet publish apis-orchestrator/src/ChatApi/ChatApi.csproj -c Release -o out/chatapi

echo "CI build completed."
