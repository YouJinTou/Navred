using Navred.Core.Tools;
using System.IO;
using System.Linq;

namespace Navred.Core.Extensions
{
    public static class IOExtensions
    {
        public static string GetFirstFilePathMatch(this string fileName)
        {
            var cwd = Directory.GetCurrentDirectory();
            var path = GetFilePathRecursive(cwd, fileName);

            Validator.ThrowIfNullOrWhiteSpace(path, $"File path not found: {fileName}");

            return path;
        }

        private static string GetFilePathRecursive(string dir, string fileName)
        {
            var target = Path.Combine(dir, fileName);
            var file = Directory.GetFiles(dir).FirstOrDefault(f => f.Equals(target));

            if (!string.IsNullOrWhiteSpace(file))
            {
                return file;
            }

            foreach (var directory in Directory.GetDirectories(dir))
            {
                var result = GetFilePathRecursive(directory, fileName);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    return result;
                }
            }

            return null;
        }
    }
}
