using System.Collections.Generic;
using System.Linq;
using MyEroTool.SDK;
using HtmlAgilityPack;

namespace EroSite.Lib
{
    public class Ertk : IEroSite
    {
        private const string
            Url = "http://ertk.net",
            PostXpath = "//h1[@class=\"postTitle\"]//a",
            ImageXpath = "//span[@class=\"img_frame\"]//a";

        public List<EroPost> GetPosts()
        {
            var nodes = new HtmlWeb().Load(Url)
                .DocumentNode.SelectNodes(PostXpath);
            return nodes.Select(n => new EroPost
            {
                Title = n.InnerText,
                Url = n.Attributes["href"].Value,
                Id = n.Attributes["href"].Value.Split('/')[^1].Split('.')[0]
            }).ToList();
        }

        public void GetImageUrls(ref EroPost post)
        {
            var nodes = new HtmlWeb().Load(post.Url).DocumentNode.SelectNodes(ImageXpath);
            post.ImageUrls = nodes.Select(n => $"{Url}{n.Attributes["href"].Value}").ToList();
        }
    }
}