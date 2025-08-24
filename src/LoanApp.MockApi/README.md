
# LoanApp.MockApi (.NET 8/9 Mock Controllers)

Mock Web API for Loan App features, grouped by domain controllers. Each endpoint returns example JSON and uses in-memory state.

## Requirements
- .NET SDK 8 or 9 (`dotnet --version`)
- Run:  
```bash
dotnet restore
dotnet run --project LoanApp.MockApi.csproj
```
- Base URL: `http://localhost:5080/api/v1` (see `launchSettings.json`)

## Domains
- Auth, KYC, LoanProducts, LoanApplications, Contracts, Loans, Billing, Payments, Notifications, Support, Profile, Cms, Analytics, Admin, Webhooks

## Notes
- Idempotency: send header `Idempotency-Key` for POST endpoints (mock-validated)
- All responses are mocked; adjust DTOs in `Dtos/` and controllers in `Controllers/`
