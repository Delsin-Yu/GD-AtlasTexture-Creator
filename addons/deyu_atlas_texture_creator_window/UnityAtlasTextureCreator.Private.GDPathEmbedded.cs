#if TOOLS
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;

public partial class UnityAtlasTextureCreator
{
    private static class GDPath
    {
        private static readonly Regex PrefixNameRegex = new(@"(?<captured>.+):[\/\\]*.*", RegexOptions.Compiled);
        private static readonly Regex SuffixNameRegex = new(@".+:[\/\\]*(?<captured>.*)", RegexOptions.Compiled);
        
        public static string GetDirectoryName(string path)
        {
            var directoryNameRaw = Path.GetDirectoryName(path);
            return ToGDPath(directoryNameRaw);
        }
    
        public static string GetFileNameWithoutExtension(string path) => 
            Path.GetFileNameWithoutExtension(path);

        
        public static string ToGDPath(string path)
        {
            var unixStyledPath = path.Replace("\\", "/");
            var prefixMatch = PrefixNameRegex.Match(unixStyledPath);
            if (!prefixMatch.Success)
            {
                throw new FormatException(path);
            }

            var suffixMatch = SuffixNameRegex.Match(unixStyledPath);
            if (suffixMatch.Success)
            {
                return $"{prefixMatch.Groups["captured"]}://{suffixMatch.Groups["captured"]}";
            }
            
            return $"{prefixMatch.Groups["captured"]}://";
        }
    }
}
#endif