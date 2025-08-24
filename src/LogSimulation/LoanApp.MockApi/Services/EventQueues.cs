using System.Collections.Concurrent;

namespace LoanApp.MockApi.Services;

public record PaymentIntentEvent(string PaymentId, DateTime FireAt);

public class EventQueues
{
    public ConcurrentQueue<PaymentIntentEvent> PaymentIntents { get; } = new();
    public ConcurrentDictionary<string, string> Payments { get; } = new(); // paymentId -> status
}