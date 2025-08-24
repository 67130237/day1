namespace LoanApp.MockApi.Dtos;

// Auth
public record RegisterRequest(string Email, string Phone, string Password);
public record LoginRequest(string Username, string Password);
public record TokenResponse(string AccessToken, string RefreshToken, string TokenType = "Bearer");
public record OtpSendRequest(string Channel, string Destination);
public record OtpVerifyRequest(string Code);
public record DeviceBindingRequest(string DeviceId);

// KYC
public record KycProfileResponse(string Status);
public record KycSubmitRequest(string FirstName, string LastName, string NationalId, string Address);
public record CreditConsentRequest(bool Accepted);

// Loan Products
public record LoanProduct(string Id, string Name, string Type, decimal MinAmount, decimal MaxAmount, double Rate);
public record LoanCalculatorRequest(decimal Amount, int TenorMonths, string RateType);
public record LoanCalculatorResponse(decimal MonthlyPayment, decimal TotalInterest);

// Applications
public record LoanApplicationCreateRequest(string ProductId, decimal Amount, int TenorMonths, string Purpose);
public record LoanApplicationResponse(string ApplicationId, string Status);
public record UpdateApplicationRequest(string? Purpose, decimal? Amount);

// Contracts
public record ContractResponse(string ContractId, string Status, string PdfUrl);

// Loans
public record LoanResponse(string LoanId, decimal Outstanding, decimal AccruedInterest);
public record ScheduleItem(int InstallmentNo, DateOnly DueDate, decimal Principal, decimal Interest, decimal Balance);
public record QuoteRequest(decimal? Amount);
public record QuoteResponse(decimal AmountDue, DateTime ExpiresAt);

// Billing/Payments
public record InvoiceResponse(string InvoiceId, string Status, DateOnly DueDate, decimal Amount);
public record PaymentIntentRequest(string LoanId, string Method);
public record PaymentIntentResponse(string PaymentId, string Status, string? RedirectUrl, DateTime ExpiresAt);
public record PaymentChargeRequest(string LoanId, string? InvoiceId, string Method, decimal Amount);
public record PaymentStatusResponse(string PaymentId, string Status);
public record ReceiptResponse(string PaymentId, string Url);

// Notifications
public record NotificationItem(string Id, string Type, string Title, string Body, bool Read);

// Support
public record TicketCreateRequest(string Subject, string Body);
public record TicketItem(string Id, string Subject, string Status);

// Profile
public record ProfileResponse(string UserId, string Email, string Phone);
public record UpdateProfileRequest(string? Email, string? Phone);
public record BankAccountRequest(string BankName, string AccountNo);
public record DocumentItem(string Id, string Type, string Url);

// CMS
public record BannerItem(string Id, string Title, string ImageUrl, string Link);
public record ArticleItem(string Slug, string Title, string Content);

// Analytics
public record RepaymentAnalytics(decimal TotalPaid, decimal TotalOutstanding);
public record CreditHealth(int Score, string Grade);

// Admin
public record DecisionRequest(string Decision, decimal? Amount, double? Rate);