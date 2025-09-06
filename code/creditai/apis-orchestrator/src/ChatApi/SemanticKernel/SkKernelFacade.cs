
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Text;

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
namespace ChatApi.SemanticKernel;

public sealed class SkKernelFacade
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
    private readonly IConfiguration _cfg;

    // cache prompt functions ตาม text เพื่อ reuse
    private readonly ConcurrentDictionary<string, KernelFunction> _promptCache = new();

    public SkKernelFacade(Kernel kernel, IConfiguration cfg)
    {
        _kernel = kernel;
        _cfg = cfg;
        _chat = _kernel.GetRequiredService<IChatCompletionService>();
    }

    // === 1) Prompt-as-function (non-stream) ===
    public async Task<string> RunPromptAsync(
        string promptText,
        Dictionary<string, object?>? vars = null,
        CancellationToken ct = default)
    {
        var func = _promptCache.GetOrAdd(promptText, text =>
            _kernel.CreateFunctionFromPrompt(text, new PromptExecutionSettings
            {
                // ตั้งค่าได้ตามต้องการ
                // Temperature = 0.2,
                // TopP = 0.9,
                // MaxTokens = 1200
            }));

        var args = new KernelArguments();
        if (vars is not null)
            foreach (var kv in vars) args[kv.Key] = kv.Value;

        var result = await _kernel.InvokeAsync(func, args, ct);
        return result.GetValue<string>() ?? string.Empty;
    }

    // === 2) JSON helper ===
    public async Task<T?> RunJsonAsync<T>(string promptText, object input, CancellationToken ct = default)
    {
        var vars = input.GetType()
            .GetProperties()
            .ToDictionary(p => p.Name, p => (object?)Convert.ToString(p.GetValue(input)));

        var raw = await RunPromptAsync(promptText, vars!, ct);
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(raw, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            var json = ExtractJson(raw);
            if (!string.IsNullOrWhiteSpace(json))
                return System.Text.Json.JsonSerializer.Deserialize<T>(json);
            return default;
        }
    }

    private static string? ExtractJson(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var match = Regex.Match(input, @"(\{[\s\S]*\}|\[[\s\S]*\])");
        return match.Success ? match.Value : null;
    }

    // === 3) Chat completion (non-stream) ===
    public async Task<string> ChatCompleteAsync(
        string systemPrompt,
        string userMessage,
        string? modelOverride = null,
        CancellationToken ct = default)
    {
        var history = new ChatHistory();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
            history.AddSystemMessage(systemPrompt);
        history.AddUserMessage(userMessage);

        var settings = new PromptExecutionSettings
        {
            // ปรับตามต้องการ
            // Temperature = 0.3,
            // TopP = 0.9,
            // MaxTokens = 1400
        };

        var msg = await _chat.GetChatMessageContentAsync(history, settings, _kernel, ct);
        return msg?.Content?.Trim() ?? string.Empty;
    }

    // === 4) Chat streaming (ใช้กับ SSE/SignalR) ===
    public async IAsyncEnumerable<string> ChatStreamAsync(
        string systemPrompt,
        string userMessage,
        string? modelOverride = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var history = new ChatHistory();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
            history.AddSystemMessage(systemPrompt);
        history.AddUserMessage(userMessage);

        var settings = new PromptExecutionSettings
        {
            // Temperature = 0.3,
            // TopP = 0.9,
            // MaxTokens = 1800
        };

        await foreach (var delta in _chat.GetStreamingChatMessageContentsAsync(history, settings, _kernel, ct))
        {
            if (delta is { Content: not null })
                yield return delta.Content!;
        }
    }
}
