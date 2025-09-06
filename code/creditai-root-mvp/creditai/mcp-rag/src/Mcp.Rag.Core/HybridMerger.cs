namespace Mcp.Rag.Core;

public sealed class HybridMerger
{
    public List<Passage> Merge(List<Passage> dense, List<Passage> lexical, int topK)
    {
        var dict = new Dictionary<string, Passage>();
        foreach (var p in dense) dict[p.id] = p;
        foreach (var p in lexical)
        {
            if (dict.TryGetValue(p.id, out var ex))
                dict[p.id] = ex with { score = Math.Max(ex.score, p.score) + 0.1 }; // boost for hybrid hit
            else
                dict[p.id] = p;
        }
        return dict.Values.OrderByDescending(p => p.score).Take(topK).ToList();
    }
}
