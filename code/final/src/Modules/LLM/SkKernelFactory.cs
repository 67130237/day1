using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace CreditAI.Modules.LLM;

public static class SkKernelFactory
{
    public static void AddSkKernel(this IServiceCollection services, IConfiguration cfg)
    {
        var apiKey = cfg["openrouter:apiKey"] ?? "";
        var model = cfg["openrouter:modelId"] ?? "openai/gpt-4o-mini";
        var baseUrl = cfg["openrouter:baseUrl"] ?? "https://openrouter.ai/api/v1";

        services.AddKernel()
            .AddOpenAIChatCompletion(modelId: model, apiKey: apiKey, httpClient: new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            });

        services.AddSingleton<IChatCompletionService>(sp =>
            sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());
    }
}
