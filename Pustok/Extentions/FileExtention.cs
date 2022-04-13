using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.Extentions
{
    public static class FileExtention
    {
        public static bool CheckContentType(this IFormFile file, string contentType)
        {
            return file.ContentType != contentType;
        }

        public static bool CheckSize(this IFormFile file, double size)
        {
            return (double)file.Length / 1024 > size;
        }

        public async static Task<string> FileCreateAsync(this IFormFile file, IWebHostEnvironment _env, params string[] folders)
        {
            string filename = Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + file.FileName;

            string path = _env.WebRootPath;
            //string path = Path.Combine(_env.WebRootPath, "image", "products", filename);

            foreach (string item in folders)
            {
                path = Path.Combine(path,item);
            }

            path = Path.Combine(path, filename);

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return filename;
        }
    }
}
