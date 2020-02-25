using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using MyEroTool.SDK;

namespace MyEroTool.Lib
{
    public class Ertk : IEroSite
    {
        private const string Url = @"http://ertk.net";
        private const string PostXpath = @"//h1[@class=""postTitle""]//a";
        private const string ImageXpath = @"//span[@class=""img_frame""]//a";

        public List<EroPost> GetLatestPosts()
        {
            var nodes = new HtmlWeb().Load(Url).DocumentNode.SelectNodes(PostXpath);
            return nodes.Select(n => new EroPost
                {
                    Title = n.InnerText,
                    Url = n.Attributes["href"].Value,
                    Id = n.Attributes["href"].Value.Split('/')[^1].Split('.')[0]
                })
                .ToList();
        }

        public List<string> GetImageUrlsInPost(EroPost post)
        {
            var nodes = new HtmlWeb().Load(post.Url).DocumentNode.SelectNodes(ImageXpath);
            return nodes.Select(n => $"{Url}{n.Attributes["href"].Value}").ToList();
        }
    }
}