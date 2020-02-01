using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace MyEroTool.EroImageDownloader
{
    public class Ertk : EroSite
    {
        private const string
            Scheme = "http",
            Host = "ertk.net",
            ArticleXpath = "//h1[@class=\"postTitle\"]//a",
            ImageXpath = "//span[@class=\"img_frame\"]//a";

        protected override List<EroArticle> GetArticles()
        {
            var nodes = new HtmlWeb().Load(new UriBuilder("http", Host).Uri)
                .DocumentNode.SelectNodes(ArticleXpath);
            return (
                from t in nodes
                let uri = new Uri(t.Attributes["href"].Value)
                select new EroArticle
                    {Site = "ertk", Title = t.InnerText, Uri = uri, Id = uri.Segments[^1].Split('.')[0]}
            ).ToList();
        }

        protected override List<Uri> GetImageUris(EroArticle eroArticle)
        {
            var nodes = new HtmlWeb().Load(eroArticle.Uri).DocumentNode.SelectNodes(ImageXpath);
            return nodes.Select(node => new UriBuilder
                {Scheme = Scheme, Host = Host, Path = node.Attributes["href"].Value}.Uri).ToList();
        }
    }
}