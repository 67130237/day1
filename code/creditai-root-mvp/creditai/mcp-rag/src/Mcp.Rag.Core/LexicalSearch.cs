namespace Mcp.Rag.Core;

public sealed class LexicalSearch
{
    // Very small in-memory corpus for MVP (replace with BM25 over an index or MCP to a text DB)
    private static readonly string[] Seed = new[]
    {
        "กฎเกณฑ์การปล่อยสินเชื่อต้องตรวจสอบรายได้และภาระหนี้รวมไม่เกินเกณฑ์ที่กำหนด",
        "ผู้กู้ต้องมีอายุ 20 ปีขึ้นไป และมีรายได้ประจำตามเงื่อนไขผลิตภัณฑ์",
        "Data dictionary อธิบายความหมายของฟิลด์ เช่น CustomerId, ContractNo, OutstandingBalance",
        "การปรับโครงสร้างหนี้ต้องได้รับอนุมัติจากคณะกรรมการความเสี่ยงตามนโยบาย"
    };

    public Task<List<Passage>> SearchAsync(string query, int topK, CancellationToken ct)
    {
        // naive scoring: count overlap words
        var q = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var scored = Seed.Select((t, i) => new Passage(i.ToString(), t, Score(q, t)))
                         .OrderByDescending(p => p.score)
                         .Take(topK)
                         .ToList();
        return Task.FromResult(scored);
    }

    private static double Score(string[] q, string text)
    {
        var s = 0;
        foreach (var w in q)
            if (text.Contains(w, StringComparison.OrdinalIgnoreCase)) s++;
        return s;
    }
}
