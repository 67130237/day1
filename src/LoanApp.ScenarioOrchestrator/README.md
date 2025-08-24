# LoanApp.ScenarioOrchestrator

Console app that simulates user journeys (time-series) against the LoanApp.MockApi.
It tags each request with rich headers (traceparent, X-Scenario-Id, X-Feature, X-Flow-Step, X-User-Id) so logs are easy to analyze.
Polly retries are enabled for transient errors to mimic realistic client behavior.

## Requirements
- .NET 8 SDK
- A running API at `http://localhost:5080/api/v1` (LoanApp.MockApi.Upgraded)

## Run
```bash
dotnet restore
dotnet run -- --baseUrl http://localhost:5080/api/v1 --scenario ./scenarios/sample-scenario.yaml
```

### Arguments
- `--baseUrl`   : Base API URL (default: http://localhost:5080/api/v1)
- `--scenario`  : Path to YAML scenario file (creates a default one if missing)
- `--timeout`   : HttpClient timeout seconds (default: 30)

## Scenario YAML
```yaml
scenarioId: payday-peak-01
users: 50
rps: 10
durationSeconds: 0   # 0 = wait for all users to finish
thinkTimeSeconds: { min: 1, max: 3 }
features:
  - name: loan-apply
    weight: 0.3
  - name: payment-due
    weight: 0.5
  - name: browse-products
    weight: 0.2
```

## What it does
- **loan-apply**: GET /loan-products -> POST /loan-calculator -> POST /loan-applications (Idempotency-Key) -> POST /loan-applications/{id}/submit -> poll status -> accept offer
- **payment-due**: GET next-due -> POST payments/intent -> poll payments/{id} -> GET receipt
- **browse-products**: GET loan-products -> POST loan-calculator

Each request includes:
- `traceparent`: generated per flow
- `X-Scenario-Id`, `X-User-Id`, `X-Session-Id`, `X-Device-Id`
- `X-Feature`, `X-Flow-Step`
- Optional: `Idempotency-Key` for critical POSTs

## Notes
- Combine with Mock API's fault controller to inject latency/errors by route or time window.
- Adjust scenario weights for different traffic mixes.