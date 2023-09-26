#if TOOLS
using System.IO;

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;

public partial class UnityAtlasTextureCreator
{
    private static class GDPath
    {
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
            return unixStyledPath.Insert(unixStyledPath.IndexOf('/'), "/");
        }
    }
}
#endif