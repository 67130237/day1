
using System.Text.RegularExpressions;

namespace CreditAI.Modules.Guardrails;

public static class PiiMasking
{
    private static readonly Regex ThaiId = new Regex(@"\b\d{1}-\d{4}-\d{5}-\d{2}-\d{1}\b|\b\d{13}\b", RegexOptions.Compiled);
    private static readonly Regex Phone = new Regex(@"\b(0\d{8,9})\b", RegexOptions.Compiled);
    private static readonly Regex Email = new Regex(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}", RegexOptions.Compiled);
    private static readonly Regex Plate = new Regex(@"\b[\p{L}]{1,2}-?\d{3,4}\b|\b\d[\p{L}]{1,2}\d{3,4}\b", RegexOptions.Compiled);

    public static string Mask(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        string s = ThaiId.Replace(input, m => new string('•', m.Value.Length));
        s = Phone.Replace(s, m => new string('•', m.Value.Length));
        s = Email.Replace(s, m => MaskEmail(m.Value));
        s = Plate.Replace(s, m => new string('•', m.Value.Length));
        return s;
    }

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1) return "•••" + email;
        var name = email[..at];
        var domain = email[(at+1)..];
        var maskedName = name[0] + new string('•', Math.Max(0, name.Length-1));
        return maskedName + "@" + domain;
    }
}
