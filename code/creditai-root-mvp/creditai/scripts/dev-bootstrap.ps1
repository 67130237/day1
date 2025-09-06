<#
dev-bootstrap.ps1
Bootstrap dev environment: user-secrets, seed data, dev certs
#>

Write-Host "=== CreditAI Dev Bootstrap ==="

# 1. Ensure dotnet user-secrets
dotnet user-secrets init

# 2. Set placeholder secrets (adjust as needed)
dotnet user-secrets set "OpenRouter:ApiKey" "sk-dev-yourkey"
dotnet user-secrets set "ConnectionStrings:SqlRo" "Server=localhost,1433;Database=CreditAI;User Id=creditai_ro_login;Password=StrongPassword1!;TrustServerCertificate=true"

# 3. Generate dev cert (ASP.NET)
dotnet dev-certs https --trust

Write-Host "Bootstrap complete."
