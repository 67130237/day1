using ChatApi.Shared.Abstractions;

namespace ChatApi.Shared.Infrastructure;

public sealed class FilePromptRepository : IPromptRepository
{
    private readonly string _root;

    public FilePromptRepository(IConfiguration cfg, IHostEnvironment env)
    {
        // อ่านจาก config: Prompts:Root (ถ้าไม่ตั้ง จะ default ไปที่ Shared.Prompts ภายใต้ repo)
        var configured = cfg["Prompts:Root"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            _root = Path.GetFullPath(configured);
        }
        else
        {
            // default: ../Shared/Shared.Prompts (relative จาก ContentRoot)
            // โครงของเราคือ apis-orchestrator/src/Shared/Shared.Prompts
            _root = Path.GetFullPath(Path.Combine(env.ContentRootPath,
                "src", "Shared", "Shared.Prompts"));
        }
    }

    public async Task<string> GetAsync(string name, CancellationToken ct)
    {
        var path = Path.Combine(_root, name);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Prompt not found: {path}");

        using var fs = File.OpenRead(path);
        using var sr = new StreamReader(fs);
        return await sr.ReadToEndAsync();
    }
}
