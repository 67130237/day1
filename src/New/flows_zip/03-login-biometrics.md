# 03 – Login (Biometric)

```mermaid
flowchart LR
  subgraph APP[Mobile App Screens]
    B1["Login: Biometric"]
    B2["Login: Success"]
  end

  GW["API Gateway<br/>AuthN/Z · RateLimit · xapp-trace-id"]
  PAPI["login-process-api<br/>POST /v1/process/login/biometric/verify"]

  AUTH["auth-service<br/>POST /xapi/v1/auth/login/biometric"]
  NOTI["notification-service<br/>POST /papi/v1/notifications"]

  B1 -->|① submit assertion| GW -->|①' route| PAPI
  PAPI -->|② auth login/biometric| AUTH
  PAPI -->|③ push 'SIGNIN' (optional)| NOTI
  PAPI -->|④ return success tokens| GW --> B2


%% ===== Styles =====
classDef screen fill:#eef9ff,stroke:#0284c7,color:#083344;
classDef gw fill:#fff7ed,stroke:#ea580c,color:#7c2d12;
classDef svc fill:#f0fdf4,stroke:#16a34a,color:#052e16;

  class B1,B2 screen
  class GW gw
  class PAPI,AUTH,NOTI svc
```
