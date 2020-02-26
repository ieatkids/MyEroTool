using System.Collections.Generic;

namespace MyEroTool.SDK
{
    public interface IEroSite
    {
        public List<EroPost> GetLatestPosts();
        public List<string> GetImageUrlsInPost(EroPost post);
    }
}