#if TOOLS


using System;
using System.IO;
using System.Text.RegularExpressions;

namespace GodotTextureSlicer;

public partial class UnityAtlasTextureCreator
{
    private static partial class GDPath
    {
        private static readonly Regex PrefixNameRegex = GetPrefixRegex();
        private static readonly Regex SuffixNameRegex = GetSuffixRegex();

        public static string GetDirectoryName(string path)
        {
            var directoryNameRaw = Path.GetDirectoryName(path)!;
            return ToGDPath(directoryNameRaw);
        }

        public static string GetFileNameWithoutExtension(string path) =>
            Path.GetFileNameWithoutExtension(path);


        private static string ToGDPath(string path)
        {
            var unixStyledPath = path.Replace("\\", "/");
            var prefixMatch = PrefixNameRegex.Match(unixStyledPath);
            if (!prefixMatch.Success) throw new FormatException(path);

            var suffixMatch = SuffixNameRegex.Match(unixStyledPath);
            if (suffixMatch.Success) return $"{prefixMatch.Groups["captured"]}://{suffixMatch.Groups["captured"]}";

            return $"{prefixMatch.Groups["captured"]}://";
        }

        [GeneratedRegex(@"(?<captured>.+):[\/\\]*.*", RegexOptions.Compiled)]
        private static partial Regex GetPrefixRegex();
        [GeneratedRegex(@".+:[\/\\]*(?<captured>.*)", RegexOptions.Compiled)]
        private static partial Regex GetSuffixRegex();
    }
}
#endif
