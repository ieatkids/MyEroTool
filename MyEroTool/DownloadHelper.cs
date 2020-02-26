using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MyEroTool.SDK
{
    public static class DownloadHelper
    {
        public static void DownloadImages(string folderDir, IEnumerable<string> imageUrls)
        {
            var urls = imageUrls as string[] ?? imageUrls.ToArray();
            if (urls.Length == 0) return;
            Console.WriteLine($"开始下载，共计{urls.Count()}张图片");
            Directory.CreateDirectory(folderDir);
            Task.WaitAll((
                from url in urls
                let fileName = Path.Combine(folderDir, url.Split('/').Last())
                let client = new WebClient()
                select client.DownloadFileTaskAsync(url, fileName)
            ).ToArray());
            Console.WriteLine($"下载结束，{urls.Count()}张图片保存至文件夹{folderDir}");
        }
    }
}