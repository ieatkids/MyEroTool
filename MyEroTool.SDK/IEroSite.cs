using System.Collections.Generic;

namespace MyEroTool.SDK
{
    public interface IEroSite
    {
        public List<EroPost> GetPosts();
        public void GetImageUrls(ref EroPost post);
    }
}