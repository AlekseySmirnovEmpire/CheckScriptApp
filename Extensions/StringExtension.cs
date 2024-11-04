using System.Text.RegularExpressions;

namespace CheckScriptApp.Extensions;

public static class StringExtension
{
    public static Uri? CheckIfCorrectHostStringAndReturnUri(this string url)
    {
        if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
        {
            return null;
        }

        return new Uri(url);
    }
}