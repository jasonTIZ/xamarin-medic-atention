using System;
using System.Globalization;
using System.Text;

namespace Medical_atention.Helpers
{
    public static class StringNormalizationHelper
    {
        public static string NormalizeForSearch(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);

            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;

                if (char.IsLetterOrDigit(c))
                    sb.Append(char.ToLowerInvariant(c));
            }

            return sb.ToString();
        }

        public static bool ContainsNormalized(string source, string query)
        {
            if (string.IsNullOrEmpty(query))
                return true;

            if (string.IsNullOrWhiteSpace(source))
                return false;

            return NormalizeForSearch(source).Contains(NormalizeForSearch(query));
        }

        public static bool ContainsNormalized(string source, string queryNormalized, bool queryAlreadyNormalized)
        {
            if (string.IsNullOrEmpty(queryNormalized))
                return true;

            if (string.IsNullOrWhiteSpace(source))
                return false;

            return NormalizeForSearch(source).Contains(queryNormalized);
        }
    }
}
