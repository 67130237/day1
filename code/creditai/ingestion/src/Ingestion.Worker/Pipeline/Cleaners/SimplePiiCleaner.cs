using System.Text.RegularExpressions;

namespace Ingestion.Worker.Pipeline.Cleaners;

public interface IPiiCleaner
{
    string Scrub(string text);
}

public sealed class SimplePiiCleaner : IPiiCleaner
{
    private static readonly Regex ThaiId = new(@"\b\d{13}\b", RegexOptions.Compiled);
    private static readonly Regex Phone = new(@"\b(0\d{8,9})\b", RegexOptions.Compiled);
    private static readonly Regex Email = new(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}", RegexOptions.Compiled);

    public string Scrub(string text)
    {
        var t = ThaiId.Replace(text, "***THAI_ID***");
        t = Phone.Replace(t, "***PHONE***");
        t = Email.Replace(t, "***EMAIL***");
        return t;
    }
}
