using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Corrector.Core;

public static class OutputChecks
{
    public static string Normalize(string text)
    {
        var result = new StringBuilder();
        foreach (char c in text.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;
            result.Append(char.IsLetterOrDigit(c) || c is '+' or '-' ? char.ToLowerInvariant(c) : ' ');
        }
        return Regex.Replace(result.ToString(), @"\s+", " ").Trim();
    }

    public static bool ContainsAny(string output, params string[] alternatives)
    {
        string normalized = Normalize(output);
        return alternatives.Any(value => normalized.Contains(Normalize(value), StringComparison.Ordinal));
    }

    public static IReadOnlyList<double> Numbers(string output)
    {
        var numbers = new List<double>();
        foreach (Match match in Regex.Matches(output,
                     @"(?<![\p{L}\d])[-+]?\d+(?:[.,]\d+)*(?![\p{L}\d])"))
        {
            string value = match.Value;
            int lastDot = value.LastIndexOf('.');
            int lastComma = value.LastIndexOf(',');
            if (lastDot >= 0 && lastComma >= 0)
            {
                char decimalSeparator = lastDot > lastComma ? '.' : ',';
                char groupingSeparator = decimalSeparator == '.' ? ',' : '.';
                value = value.Replace(groupingSeparator.ToString(), "").Replace(decimalSeparator, '.');
            }
            else
                value = value.Replace(',', '.');
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double number)
                && double.IsFinite(number))
                numbers.Add(number);
        }
        return numbers;
    }

    public static bool ContainsNumber(string output, double expected, double tolerance = 0.01) =>
        Numbers(output).Any(value => Math.Abs(value - expected) <= tolerance + 1e-9);

    public static bool ContainsNumericSequence(string output, params double[] expected)
    {
        int index = 0;
        foreach (double number in Numbers(output))
        {
            if (index < expected.Length && Math.Abs(number - expected[index]) <= 0.011)
                index++;
        }
        return index == expected.Length;
    }
}
