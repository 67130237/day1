namespace Ingestion.Worker.Pipeline.Parsers;

public interface IParser
{
    Task<string> ParseAsync(string path, CancellationToken ct);
}

public sealed class TextLikeParser : IParser
{
    public Task<string> ParseAsync(string path, CancellationToken ct)
    {
        // MVP: read txt/md as-is; extend for pdf/docx later
        return Task.FromResult(File.ReadAllText(path));
    }
}
