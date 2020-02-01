using MyEroTool.SDK;
using System.IO;
using System;
using System.Runtime.Loader;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


namespace MyEroTool
{
    public class ImageTool
    {
        private string _imageFolder;
        private List<IEroSite> _eroSites;

        private List<IEroSite> EroSites
        {
            get
            {
                if (_eroSites != null) return _eroSites;
                _eroSites = new List<IEroSite>();
                var folder = Path.Combine(Environment.CurrentDirectory, "ero_site");
                foreach (var file in Directory.GetFiles(folder))
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
                    foreach (var type in assembly.GetTypes())
                    {
                        if ((type.IsClass) && (type.GetInterfaces().Contains(typeof(IEroSite))))
                        {
                            var o = Activator.CreateInstance(type) as IEroSite;
                            _eroSites.Add(o);
                        }
                    }
                }

                return _eroSites;
            }
        }

        private string ImageFolder
        {
            get
            {
                if (_imageFolder != null) return _imageFolder;
                using (var reader = new StreamReader(Path.Combine(Environment.CurrentDirectory, "config.json")))
                {
                    var jo = JObject.Parse(reader.ReadToEnd());
                    _imageFolder = jo["image_folder"].ToString();
                }

                return _imageFolder;
            }
        }

        private void DownloadImages(IEroSite site, EroPost post)
        {
            if (post.ImageUrls == null)
            {
                site.GetImageUrls(ref post);
            }

            if (post.ImageUrls.Count <= 0)
            {
                return;
            }

            Console.WriteLine($"开始下载，共计{post.ImageUrls.Count}张图片");
            var folder = Path.Combine(ImageFolder, site.GetType().Name, post.Id);
            Directory.CreateDirectory(folder);
            Task.WaitAll((
                from url in post.ImageUrls
                let fileName = Path.Combine(folder, $"{post.ImageUrls.IndexOf(url)}.jpg")
                let client = new WebClient()
                select client.DownloadFileTaskAsync(url, fileName)
            ).ToArray());
            Console.WriteLine($"下载结束，{post.ImageUrls.Count}张图片保存至文件夹{folder}");
        }

        private IEroSite SelectEroSite()
        {
            while (true)
            {
                foreach (var site in EroSites)
                {
                    Console.WriteLine($"[{EroSites.IndexOf(site)}]\t{site.GetType().Name}");
                }

                Console.WriteLine($"请选择要下载的网站，输入编号并回车确认");
                try
                {
                    var index = Convert.ToInt32(Console.ReadLine());
                    return EroSites[index];
                }
                catch (Exception e)
                {
                    Console.WriteLine("输入错误，请重新输入");
#if DEBUG
                    Console.WriteLine(e);
#endif
                    throw;
                }
            }
        }

        private void SelectEroPost(IEroSite eroSite)
        {
            var eroPosts = eroSite.GetPosts();
            while (true)
            {
                foreach (var post in eroPosts)
                {
                    Console.WriteLine($"[{eroPosts.IndexOf(post)}]\t{post.Title}");
                }

                Console.WriteLine($"请选择要下载的内容，输入编号并回车确认。退出请按q。");
                var cmd = Console.ReadLine();
                if ("q" == cmd)
                {
                    return;
                }
                else
                {
                    try
                    {
                        var index = Convert.ToInt32(cmd);
                        DownloadImages(eroSite, eroPosts[index]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("输入错误，请重新输入");
#if DEBUG
                        Console.WriteLine(e);
#endif
                        throw;
                    }
                }
            }
        }

        public void Start()
        {
            var eroSite = SelectEroSite();
            SelectEroPost(eroSite);
        }
    }
}