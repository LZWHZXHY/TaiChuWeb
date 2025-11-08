using System.Collections.Generic;

namespace TCserver_Backend.Dtos
{
    public class PostsViewDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int PostType { get; set; }
        public List<string> Images { get; set; }
        public List<string> Videos { get; set; }
        public string Author { get; set; }
        public string CreateTime { get; set; }

        public int LikeCount { get; set; }
        public int CommentCount { get; set; } // 新增：评论数
        public int ViewCount { get; set; }    // 新增：浏览量
        public int HotScore { get; set; }     // 新增：热度分
        public bool LikedByMe { get; set; }
        public string AuthorAvatar { get; set; } // 新增字段

        public PostsViewDto()
        {
            Images = new List<string>();
            Videos = new List<string>();
        }
        // ... 其它你要展示的字段
    }
}