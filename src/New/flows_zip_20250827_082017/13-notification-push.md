# 13 – Notification (Push only)

```mermaid
flowchart LR
  subgraph APP[Mobile App Screens]
    N0["Any Feature Result"]
  end

  GW["API Gateway"]
  PAPI["*-process-api (caller)<br/>POST /v1/process/*/..."]
  NOTI["notification-service<br/>POST /papi/v1/notifications"]

  N0 -->|"feature success/fail triggers" | PAPI
  PAPI -->|① enqueue push (topic: e.g., transfer.success)| NOTI
  NOTI -->|② accepted (202)| PAPI


  %% ===== Styles =====
  classDef screen fill:#eef9ff,stroke:#0284c7,color:#083344;
  classDef gw fill:#fff7ed,stroke:#ea580c,color:#7c2d12;
  classDef svc fill:#f0fdf4,stroke:#16a34a,color:#052e16;

  class N0 screen
  class GW gw
  class PAPI,NOTI svc
```
