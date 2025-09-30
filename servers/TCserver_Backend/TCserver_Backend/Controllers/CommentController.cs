using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TCserver_Backend.Data;
using TCserver_Backend.Dtos;
using TCserver_Backend.Models;

namespace TCserver_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly RegisterContext _context;
        private readonly FunctionDbContext _functionDbContext;

        public CommentController(RegisterContext context, FunctionDbContext functionDbContext)
        {
            _context = context;
            _functionDbContext = functionDbContext;
        }

        // GET: api/Comment?postId=123
        // GET: api/Comment?postId=123
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ViewCommentDto>>> GetComments([FromQuery] int postId)
        {
            // 1. 获取所有评论
            var allComments = await _context.Comments
                .Where(c => c.post_id == postId && c.status == 0)
                .OrderBy(c => c.createTime)
                .ToListAsync();

            var userIds = allComments.Select(c => c.user_id).Distinct().ToList();

            // 2. 联查 useraccount 和 userdata
            var userInfos = await (from ua in _context.UserAccounts
                                   join ud in _context.UserDatas on ua.Id equals ud.id
                                   where userIds.Contains(ua.Id)
                                   select new
                                   {
                                       ua.Id,
                                       ua.username,
                                       ud.logo
                                   }).ToListAsync();

            // 3. 字典，方便查找
            var userDict = userInfos.ToDictionary(u => u.Id, u => u);

            // 4. 转换为DTO
            var commentDtos = allComments.Select(c => new ViewCommentDto
            {
                Id = c.id,
                Content = c.content,
                CreateTime = c.createTime,
                ParentCommentId = c.ParentCommentId,
                UserId = c.user_id,
                UserName = userDict.ContainsKey(c.user_id) ? userDict[c.user_id].username : "未知用户",
                UserAvatar = userDict.ContainsKey(c.user_id) ? userDict[c.user_id].logo : null,
                Replies = new List<ViewCommentDto>()
            }).ToList();

            // 5. 构建树结构
            var commentDict = commentDtos.ToDictionary(c => c.Id);
            List<ViewCommentDto> roots = new();
            foreach (var dto in commentDtos)
            {
                if (dto.ParentCommentId.HasValue && commentDict.ContainsKey(dto.ParentCommentId.Value))
                {
                    commentDict[dto.ParentCommentId.Value].Replies.Add(dto);
                }
                else
                {
                    roots.Add(dto);
                }
            }
            return Ok(roots);
        }

        // POST: api/Comment
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> AddComment([FromBody] CreateCommentDto dto)
        {
            try
            {
                // 1. 获取当前用户ID
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

                // 2. （可选）风控等级检查
                var user = await _context.UserDatas.FindAsync(currentUserId);
                if (user == null)
                {
                    return NotFound("用户不存在");
                }
                int requiredLevel = 1;
                if (user.level < requiredLevel)
                {
                    return BadRequest($"您的等级不足 {requiredLevel} 级，无法发表评论");
                }

                // 3. 添加评论
                var comment = new Comments
                {
                    post_id = dto.PostId,
                    content = dto.Content,
                    ParentCommentId = dto.ParentCommentId,
                    user_id = currentUserId,
                    createTime = DateTime.UtcNow
                };
                _context.Comments.Add(comment);

                // 4. （可选）给帖子+1评论数
                var post = await _context.Posts.FindAsync(dto.PostId);
                if (post != null)
                {
                    post.comment_count = post.comment_count + 1;
                }

                await _context.SaveChangesAsync();

                // 评论加活跃度
                var activity = new UserActivity
                {
                    UserId = currentUserId,
                    ActivityType = (int)ActivityType.Comment, // 建议有Comment类型枚举
                    Score = 3, // 评论加3分
                    CreateTime = DateTime.UtcNow
                };
                _functionDbContext.UserActivities.Add(activity);
                await _functionDbContext.SaveChangesAsync();







                return Ok(new { commentId = comment.id });
            }
            catch (Exception ex)
            {
                // 可用日志记录
                return StatusCode(500, "服务器内部错误：" + ex.Message);
            }
        }


        // Controller (在 CommentController 里)
        [HttpPost("report")]
        [Authorize]
        public async Task<IActionResult> ReportComment([FromBody] ReportCommentRequest req)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // 防止重复举报
            bool already = await _context.CommentReports.AnyAsync(x => x.CommentId == req.CommentId && x.UserId == userId);
            if (already) return BadRequest("您已举报过该评论");

            _context.CommentReports.Add(new CommentReport
            {
                CommentId = req.CommentId,
                UserId = userId,
                Reason = req.Reason,
                CreateTime = DateTime.UtcNow
            });

            int count = await _context.CommentReports.CountAsync(x => x.CommentId == req.CommentId);
            int threshold = 5;
            var comment = await _context.Comments.FindAsync(req.CommentId);
            if (comment != null && comment.status == 0 && count >= threshold)
                comment.status = 1; //审核

            await _context.SaveChangesAsync();
            return Ok(new { reportCount = count, status = comment?.status ?? 0 });
        }


        public class ReportCommentRequest
        {
            public int CommentId { get; set; }
            public string Reason { get; set; }
        }

    }
}