using System.Text.RegularExpressions;
using ChatApi.Shared.Abstractions;

namespace ChatApi.Guardrails;

public sealed class PiiMasker : IPiiMasker
{
    // Thai national ID (13 digits) rough pattern (not checksum)
    private static readonly Regex ThaiId = new Regex(@"\b\d{13}\b", RegexOptions.Compiled);
    // Phone numbers (09x-xxxxxxx) simplified
    private static readonly Regex Phone = new Regex(@"\b(0\d{8,9})\b", RegexOptions.Compiled);
    // Email
    private static readonly Regex Email = new Regex(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}", RegexOptions.Compiled);

    public string MaskInbound(string text) => Mask(text);
    public string MaskOutbound(string text) => Mask(text);

    private static string Mask(string input)
    {
        var t = ThaiId.Replace(input, m => MaskWith(m.Value, 4));
        t = Phone.Replace(t, m => MaskWith(m.Value, 3));
        t = Email.Replace(t, m =>
        {
            var v = m.Value;
            var at = v.IndexOf('@');
            if (at <= 1) return "***" + v[at..];
            var local = v[..at];
            var dom = v[at..];
            var keep = Math.Min(2, local.Length);
            return local[..keep] + new string('*', Math.Max(0, local.Length - keep)) + dom;
        });
        return t;
    }

    private static string MaskWith(string s, int keepRight)
    {
        if (s.Length <= keepRight) return new string('*', s.Length);
        return s; // new string('*', s.Length - keepRight) + s[^keepRight:]);
    }
}
