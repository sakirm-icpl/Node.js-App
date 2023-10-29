using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Courses.API.Helper
{
    public class File_Operation
    {
        public static async Task<string> SaveFile(IFormFile uploadedFile, string path, string extension)
        {
            string fileDir = string.Empty;
            string file = string.Empty;
            string fileName = string.Empty;
            if (!Directory.Exists(fileDir))
                Directory.CreateDirectory(fileDir);
            file = Path.Combine(fileDir, DateTime.Now.Ticks + Record.Dot + extension);
            fileName = Path.Combine(DateTime.Now.Ticks + Record.Dot + extension);
            using (var fs = new FileStream(Path.Combine(file), FileMode.Create))
                await uploadedFile.CopyToAsync(fs);
            if (String.IsNullOrEmpty(file))
                return null;
            return file.Substring(file.LastIndexOf("\\" + extension));
        }

        public static void DeleteDirectory(string path)
        {
            // Delete all files from the Directory
            foreach (string filename in Directory.GetFiles(path))
                File.Delete(filename);
            // Check all child Directories and delete files
            foreach (string subfolder in Directory.GetDirectories(path))
                DeleteDirectory(subfolder);
            Directory.Delete(path);
        }
    }
}
