# 02 – Login (PIN)

```mermaid
flowchart LR
  subgraph APP[Mobile App Screens]
    L1["Login: PIN"]
    L2["Login: OTP (if required)"]
    L3["Login: Success"]
  end

  GW["API Gateway<br/>AuthN/Z · RateLimit · xapp-trace-id"]
  PAPI["login-process-api<br/>POST /v1/process/login/pin/init<br/>POST /v1/process/login/pin/request-otp<br/>POST /v1/process/login/pin/confirm"]

  AUTH["auth-service<br/>POST /xapi/v1/auth/login/pin"]
  OTP["otp-service<br/>POST /sapi/v1/otp/send<br/>POST /sapi/v1/otp/verify"]
  NOTI["notification-service<br/>POST /papi/v1/notifications"]

  L1 -->|① submit PIN| GW -->|①' route| PAPI
  PAPI -->|② auth login/pin| AUTH
  AUTH -->|mfaRequired?| PAPI
  PAPI -->|③ request OTP (if needed)| OTP
  L2 -->|④ submit OTP| GW -->|④' route| PAPI
  PAPI -->|⑤ verify OTP| OTP
  PAPI -->|⑥ push 'SIGNIN' (optional)| NOTI
  PAPI -->|⑦ return success tokens| GW --> L3


%% ===== Styles =====
classDef screen fill:#eef9ff,stroke:#0284c7,color:#083344;
classDef gw fill:#fff7ed,stroke:#ea580c,color:#7c2d12;
classDef svc fill:#f0fdf4,stroke:#16a34a,color:#052e16;

  class L1,L2,L3 screen
  class GW gw
  class PAPI,AUTH,OTP,NOTI svc
```
