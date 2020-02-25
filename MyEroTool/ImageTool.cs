using System.IO;
using System;
using System.Runtime.Loader;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using MyEroTool.SDK;
using System.Text.Json;


namespace MyEroTool
{
    public class EroImageDownloader
    {
        private readonly IEroSite _eroSite;
        private readonly string _imageFolderDir;

        private List<EroPost> EroPosts { get; }

        internal EroImageDownloader(Type eroSiteType)
        {
            _eroSite = Activator.CreateInstance(eroSiteType) as IEroSite;
            EroPosts = _eroSite.GetLatestPosts();
            var configFilePath = Path.Combine(Environment.CurrentDirectory, "configs.json");
            using var reader = new StreamReader(configFilePath);
            var configs = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd());
            _imageFolderDir = configs["image_folder"];
        }

        internal void ShowPosts()
        {
            ConsoleHelper.ShowHeaders("以下是最新的内容：");
            EroPosts.ForEach(p => Console.WriteLine($"{EroPosts.IndexOf(p)}\t{p.Title}"));
        }

        private void DownloadImages(int i)
        {
            var post = EroPosts[i];
            var imageUrls = _eroSite.GetImageUrlsInPost(post);
            ConsoleHelper.ShowHeaders($"开始下载，共计{imageUrls.Count}张图片");
            var folderDir = Path.Combine(_imageFolderDir, _eroSite.GetType().Name, post.Id);
            Directory.CreateDirectory(folderDir);
            Task.WaitAll((
                from url in imageUrls
                let fileName = Path.Combine(folderDir, url.Split('/').Last())
                let client = new WebClient()
                select client.DownloadFileTaskAsync(url, fileName)
            ).ToArray());
            Console.WriteLine($"下载结束，{imageUrls.Count}张图片保存至文件夹{folderDir}");
        }

        private void Run()
        {
            while (true)
            {
                ShowPosts();
                ConsoleHelper.ShowHeaders("输入编号开始下载 退出请输入q");
                var cin = Console.ReadLine();
                if (cin == null || cin.ToString() == "q'") return;
                try
                {
                    DownloadImages(Convert.ToInt32(cin));
                }
                catch (Exception e)
                {
#if DEBUG
                    Console.WriteLine(e);
#endif
                    ConsoleHelper.ShowHeaders($"输入错误，无法识别的编号：【{cin}】");
                }

                ConsoleHelper.ShowHeaders("是否继续下载？（y/n）");
                var key = Convert.ToChar(Console.ReadLine());
                if (key == 'y') continue;
                break;
            }
        }

        public static void Start()
        {
            var folder = Path.Combine(Environment.CurrentDirectory, "ero_sites");
            var types = new List<Type>();
            foreach (var file in Directory.GetFiles(folder))
            {
                if (!file.EndsWith("dll")) continue;
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
                assembly.GetTypes().Where(t => t.IsClass && t.GetInterfaces().Contains(typeof(IEroSite)))
                    .ToList().ForEach(t => types.Add(t));
            }

            if (!types.Any())
            {
                ConsoleHelper.ShowHeaders("找不到合适的网站，请检查ero_sites文件夹");
                return;
            }

            types.ForEach(type => Console.WriteLine($"[{types.IndexOf(type)}]\t{type.Name}"));
            try
            {
                var number = Convert.ToInt32(Console.ReadLine());
                var downloader = new EroImageDownloader(types[number]);
                downloader.Run();
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                ConsoleHelper.ShowHeaders("输入错误。");
            }
        }
    }
}