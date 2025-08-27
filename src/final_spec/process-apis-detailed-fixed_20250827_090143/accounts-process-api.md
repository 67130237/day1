# accounts-process-api @ 1.0.0

## Overview
List accounts and get account detail (balance + transactions).

## Global Headers (per GlobalContract)
- `xapp-trace-id`: **string(UUID)** — required
- `Authorization`: **Bearer &lt;JWT&gt;** — required unless explicitly public
- `Accept-Language`: **th-TH | en-US** — optional

**Response Header:** `xapp-source: accounts-process-api@1.0.0`  
**Error Envelope (all endpoints):**
```json
{
  "code": "STRING",
  "message": "STRING",
  "traceId": "UUID",
  "details": {}
}
```

---
## List Accounts
**GET /v1/process/accounts/list**  
Return list of user's accounts.



### Response Fields
| Field | Type | Description |
|---|---|---|
| items | array<object> | Accounts |
| nextPageToken | string|null | Pagination token |

#### Response Example
```json
{ "items":[{"accountId":"acc_01H","type":"SAVING","numberMasked":"xxx-xxx-1234","currency":"THB","status":"ACTIVE"}], "nextPageToken": null }
```



---
## Account Detail
**GET /v1/process/accounts/detail**  
Return balance and transactions for a given account.

### Request Fields
| Field | Type | Required | Description |
|---|---|---|---|
| accountId | string | yes | Query param |
| from | string(ISO8601) | no | Transactions from |
| to | string(ISO8601) | no | Transactions to |
| page | number | no | Page number |
| pageSize | number | no | Items per page |


### Response Fields
| Field | Type | Description |
|---|---|---|
| balance | object | Balance information |
| transactions | object | Paged transactions |
| traceid | string | Trace id |

#### Response Example
```json
{ "balance":{"accountId":"acc_01H","available":"1000.00","ledger":"1000.00","currency":"THB","asOf":"ISO8601"}, "transactions":{"items":[{"txnId":"...","postedAt":"ISO8601","amount":"-50.00","currency":"THB","type":"DEBIT","desc":"Transfer"}], "nextPageToken": null}, "traceid":"1f3c..." }
```


### Errors
| Code | HTTP | Message |
|---|---|---|
PACC-DB-001 | 404 | Account not found
PACC-SYS-001 | 500 | Internal error
