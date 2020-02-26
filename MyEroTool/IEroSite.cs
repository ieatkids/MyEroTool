using System.Collections.Generic;

namespace MyEroTool
{
    public interface IEroSite
    {
        public IEnumerable<EroPost> GetLatestPosts();
        public IEnumerable<string> GetImageUrlsInPost(EroPost post);
    }
}