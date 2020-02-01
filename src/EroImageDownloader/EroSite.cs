using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace MyEroTool.EroImageDownloader
{
    public abstract class EroSite
    {
        protected struct EroArticle
        {
            public string
                Site, Id, Title;

            public Uri
                Uri;
        }

        protected abstract List<EroArticle> GetArticles();
        protected abstract List<Uri> GetImageUris(EroArticle eroArticle);
        private string _imageFolderRoot;
        private List<EroArticle> _articles;

        private List<EroArticle> Articles
        {
            get
            {
                _articles = _articles ??= GetArticles();
                return _articles;
            }
        }

        private string ImageFolderRoot
        {
            get
            {
                if (_imageFolderRoot != null) return _imageFolderRoot;
                using (var reader = new StreamReader(Path.Combine(Environment.CurrentDirectory, "config.json")))
                {
                    var jo = JObject.Parse(reader.ReadToEnd());
                    _imageFolderRoot = jo["image_folder"].ToString();
                }

                return _imageFolderRoot;
            }
        }

        #region

        private void ShowArticleInfos()
        {
            foreach (var article in Articles)
            {
                Console.WriteLine($"{Articles.IndexOf(article)}\t{article.Title}");
            }

            Console.WriteLine("请输入下载编号，或输入q退出");
        }

        private void DownloadImages(EroArticle article)
        {
            var imageUris = GetImageUris(article);
            Console.WriteLine($"开始下载{article.Uri}，共计{imageUris.Count}张图片");
            var folder = Path.Combine(ImageFolderRoot, article.Site, article.Id);
            Directory.CreateDirectory(folder);
            Task.WaitAll(
                (from uri in imageUris
                    let fileName = Path.Combine(folder, $"{imageUris.IndexOf(uri)}.jpg")
                    let client = new WebClient()
                    select client.DownloadFileTaskAsync(uri, fileName)
                ).ToArray());
            Console.WriteLine($"下载结束，{imageUris.Count}张图片保存至文件夹{folder}");
        }

        public void Run()
        {
            string cmd;
            do
            {
                ShowArticleInfos();
                cmd = Console.ReadLine();
                try
                {
                    var number = Convert.ToInt32(cmd);
                    DownloadImages(Articles[number]);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"无效的输入{cmd}");
#if DEBUG
                    Console.WriteLine(e);
#endif
                }
            } while (cmd != null && cmd != "q");
        }

        #endregion
    }
}