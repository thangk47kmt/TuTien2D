namespace TuTien.Domain.Services;

public static class NameValidator
{
    public static string? NormalizeOrError(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Ten khong duoc de trong.";
        var trimmed = string.Join(' ', raw.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        if (trimmed.Length is < 2 or > 18) return "Ten phai tu 2 den 18 ky tu.";
        foreach (var ch in trimmed)
            if (char.IsControl(ch)) return "Ten chua ky tu khong hop le.";
        return null;
    }
    public static string Normalize(string raw)
        => string.Join(' ', raw.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
