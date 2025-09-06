namespace Ingestion.Worker.Pipeline.Chunkers;

public interface IChunker
{
    List<string> Chunk(string text, int size = 800, int overlap = 200);
}

public sealed class SimpleChunker : IChunker
{
    public List<string> Chunk(string text, int size = 800, int overlap = 200)
    {
        var chunks = new List<string>();
        var i = 0;
        while (i < text.Length)
        {
            var end = Math.Min(text.Length, i + size);
            chunks.Add(text[i..end]);
            if (end == text.Length) break;
            i = Math.Max(end - overlap, i + 1);
        }
        return chunks;
    }
}
