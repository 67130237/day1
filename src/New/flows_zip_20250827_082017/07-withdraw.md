# 07 – Withdraw

```mermaid
flowchart LR
  subgraph APP[Mobile App Screens]
    W1["Withdraw: Input"]
    W2["Withdraw: Confirm"]
    W3["Withdraw: Result"]
  end

  GW["API Gateway"]
  PAPI["withdraw-process-api<br/>POST /v1/process/withdraw/prepare<br/>POST /v1/process/withdraw/confirm"]

  TX["transaction-service<br/>POST /xapi/v1/withdrawals"]
  NOTI["notification-service<br/>POST /papi/v1/notifications"]

  W1 -->|① prepare| GW --> PAPI
  PAPI -->|② validate (local rules)| PAPI
  W2 -->|③ confirm| GW --> PAPI
  PAPI -->|④ create withdrawal| TX
  PAPI -->|⑤ push 'WITHDRAW'| NOTI
  PAPI -->|⑥ return result| GW --> W3


  %% ===== Styles =====
  classDef screen fill:#eef9ff,stroke:#0284c7,color:#083344;
  classDef gw fill:#fff7ed,stroke:#ea580c,color:#7c2d12;
  classDef svc fill:#f0fdf4,stroke:#16a34a,color:#052e16;

  class W1,W2,W3 screen
  class GW gw
  class PAPI,TX,NOTI svc
```
