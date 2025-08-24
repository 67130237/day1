using System.Collections.Concurrent;
using LoanApp.MockApi.Dtos;

namespace LoanApp.MockApi.Services;

public class InMemoryStore
{
    public ConcurrentDictionary<string, object> Data { get; } = new();
    public ConcurrentDictionary<string, string> Idempotency { get; } = new();
}