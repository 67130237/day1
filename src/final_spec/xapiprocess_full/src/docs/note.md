business validation
AccountService
[get] /xapi/v1/accounts
{
    items = new[] {
        new { accountId = "ACC001", type = "SAVING", numberMasked = "xxx-xxx-1234", currency = "THB", status = "ACTIVE" }
    },
    nextPageToken = (string?)null
}
[get] /xapi/v1/accounts/{accountId}/balance
    string.IsNullOrWhiteSpace(accountId)? => "ACC-BAL-404", "ไม่พบบัญชี"
    { accountId, available = "1000.00", ledger = "1000.00", currency = "THB", asOf = DateTimeOffset.Now.ToString("o") }

[get] /xapi/v1/accounts/{accountId}/transactions
    string.IsNullOrWhiteSpace(accountId)? => "ACC-BAL-404", "ไม่พบบัญชี"
    {
        items = new[] { new { txnId = Guid.NewGuid().ToString(), postedAt = DateTimeOffset.Now.ToString("o"), amount = "-100.00", currency = "THB", type = "DEBIT", desc = "Transfer to ..." } },
        nextPageToken = (string?)null
    }

AppSettingsService
[get] /xapi/v1/appsettings?scope
    !string.Equals(scope, "public") => APPS-VAL, scope ไม่รองรับ
    { minVersion = "1.2.3", featureFlags = new { transfer_otp = true } }


AuthService
[post] /xapi/v1/auth/register
    RegReq(string Mobile, string? Email, string Pin, string FirstName, string LastName, string CitizenId, bool? AcceptTerms, object? Metadata); => AUTH-REG-VAL, ข้อมูลสมัครไม่ถูกต้อง
    Status201Created
    { userId = Guid.NewGuid().ToString(), registeredAt = DateTimeOffset.Now.ToString("o") }

[post] /xapi/v1/auth/login/pin
    PinLoginReq(string Mobile, string Pin, string DeviceId); => 400, AUTH-PIN-INVALID,PIN ไม่ถูกต้อง
    !(req.Pin == "000000") => 401, "AUTH-PIN-INVALID", "PIN ไม่ถูกต้อง"
    { accessToken = "ACCESS", refreshToken = "REFRESH", expiresIn = 3600, mfaRequired = false }


[post] /xapi/v1/auth/login/biometric
    BioLoginReq(string DeviceId, string Assertion, string Nonce); =>  401, "AUTH-BIO-INVALID", "Assertion ไม่ถูกต้อง"
    { accessToken = "ACCESS", refreshToken = "REFRESH", expiresIn = 3600 }

CmsService














