using System.Text.RegularExpressions;

namespace AutogenRundown.Utils;

public class Filesystem
{
    public static string Filename(string name) => Regex.Replace(
        name,
        @"<color(\s*=\s*[^>]+)?>|</color>|<s>|</s>|/|\?|!",
        string.Empty,
        RegexOptions.IgnoreCase);
}
