using System.Globalization;

namespace System
{
    public static class StringExtensions
    {
        public static string ToPascalCase(this string value)
        {
            return ToPascalCase(value, '_');
        }

        public static string ToPascalCase(this string value, char separator)
        {
            value = value.Replace(separator, ' ');
            var textInfo = CultureInfo.InvariantCulture.TextInfo;
            return textInfo.ToTitleCase(value).Replace(" ", string.Empty);
        }

        public static string ToSnakeCase(this string value)
        {
            return ToSnakeCase(value, '_');
        }

        public static string ToSnakeCase(this string value, char separator)
        {
            var sep = separator.ToString();
            for (int i = value.Length - 1; i > 0; i--)
            {
                if (i > 0 && char.IsUpper(value[i]))
                {
                    value = value.Insert(i, sep);
                }
            }
            return value.ToLower();
        }
    }
}
