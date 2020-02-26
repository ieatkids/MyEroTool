using System.IO;
using System;
using System.Runtime.Loader;
using System.Collections.Generic;
using System.Linq;
using MyEroTool;
using System.Text.Json;


namespace ConsoleApp
{
    public class App
    {
        private IEroSite EroSite { get; }
        private string ImageFolderDir { get; }
        private List<EroPost> EroPosts { get; }

        private App(Type eroSiteType)
        {
            EroSite = Activator.CreateInstance(eroSiteType) as IEroSite;
            if (EroSite != null) EroPosts = EroSite.GetLatestPosts().ToList();
            var configFilePath = Path.Combine(Environment.CurrentDirectory, "configs.json");
            using var reader = new StreamReader(configFilePath);
            var configs = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd());
            ImageFolderDir = configs["image_folder"];
        }

        private void OnShowPosts()
        {
            ConsoleHelper.ShowHeaders("以下是最新的内容：");
            EroPosts.ForEach(p => Console.WriteLine($"{EroPosts.IndexOf(p)}\t{p.Title}"));
        }

        private bool OnDownloadImages(string cin)
        {
            try
            {
                var post = EroPosts[Convert.ToInt32(cin)];
                var folderDir = Path.Combine(ImageFolderDir, EroSite.GetType().Name, post.Id);
                DownloadHelper.DownloadImages(folderDir, EroSite.GetImageUrlsInPost(post));
                return true;
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                ConsoleHelper.ShowHeaders($"输入错误，无法识别的编号：【{cin}】");
                return false;
            }
        }

        private void Run()
        {
            while (true)
            {
                OnShowPosts();
                ConsoleHelper.ShowHeaders("输入编号开始下载 退出请输入q");
                var cin = Console.ReadLine();
                if (cin == "q") return;
                if (OnDownloadImages(cin))
                {
                    ConsoleHelper.ShowHeaders("下载成功。是否继续下载？（y/n）");
                    var key = Convert.ToChar(Console.ReadLine());
                    if (key == 'y') continue;
                    break;
                }

                ConsoleHelper.ShowHeaders("下载失败。请重新输入");
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
                var downloader = new App(types[number]);
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