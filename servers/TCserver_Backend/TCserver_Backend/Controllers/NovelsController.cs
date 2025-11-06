using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TCserver_Backend.Data;
using TCserver_Backend.Models.Novels;

namespace TCserver_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NovelsController : ControllerBase
    {
        private readonly FunctionDbContext _db;
        public NovelsController(FunctionDbContext db)
        {
            _db = db;
        }

        // 工具方法：安全获取 userId
        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub")
                ?? User.FindFirst("user_id")
                ?? User.FindFirst("id")
                ?? User.FindFirst("Id");
            if (claim == null)
                throw new UnauthorizedAccessException("Token中无用户ID");
            return int.Parse(claim.Value);
        }

        // 获取小说列表
        [HttpGet]
        public async Task<IActionResult> GetNovels()
        {
            var novels = await _db.Novels
                .OrderByDescending(n => n.create_time)
                .ToListAsync();
            return Ok(novels);
        }

        // 获取小说详情
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNovel(int id)
        {
            var novel = await _db.Novels.FindAsync(id);
            if (novel == null) return NotFound(new { message = "小说未找到" });
            return Ok(novel);
        }

        // 获取小说章节目录
        [HttpGet("{id}/chapters")]
        public async Task<IActionResult> GetChapters(int id)
        {
            var chapters = await _db.NovelChapters
                .Where(c => c.novel_id == id)
                .OrderBy(c => c.order_num)
                .Select(c => new {
                    c.id,
                    c.title,
                    c.order_num,
                    c.author,
                    c.createTime,
                    c.updateTime,
                    c.likes,
                    c.reports,
                    c.views,
                    c.status
                })
                .ToListAsync();
            return Ok(chapters);
        }

        // 获取单个章节内容（防刷view）
        [HttpGet("chapter/{chapterId}")]
        public async Task<IActionResult> GetChapterContent(int chapterId)
        {
            var chapter = await _db.NovelChapters.FindAsync(chapterId);
            if (chapter == null) return NotFound(new { message = "章节未找到" });

            int userId;
            try { userId = GetUserId(); }
            catch { return Unauthorized(new { message = "请重新登录" }); }

            var oneHourAgo = DateTime.UtcNow.AddHours(-1);

            bool hasViewed = await _db.NovelChaptersActions.AnyAsync(a =>
                a.chapterId == chapterId &&
                a.userId == userId &&
                a.actionType == 7 &&
                a.actionTime > oneHourAgo
            );

            if (!hasViewed)
            {
                chapter.views = (chapter.views ?? 0) + 1;
                _db.NovelChaptersActions.Add(new NovelChaptersAction
                {
                    chapterId = chapter.id,
                    userId = userId,
                    actionType = 7,
                    actionTime = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }

            return Ok(chapter);
        }

        // 点赞章节（每人只能点赞一次）
        [HttpPost("chapter/{chapterId}/like")]
        public async Task<IActionResult> LikeChapter(int chapterId)
        {
            var chapter = await _db.NovelChapters.FindAsync(chapterId);
            if (chapter == null) return NotFound(new { message = "章节未找到" });

            int userId;
            try { userId = GetUserId(); }
            catch { return Unauthorized(new { message = "请重新登录" }); }

            // 已点赞则不能再点赞
            bool hasLiked = await _db.NovelChaptersActions.AnyAsync(a =>
                a.chapterId == chapterId &&
                a.userId == userId &&
                a.actionType == 1
            );
            if (hasLiked)
                return BadRequest(new { message = "每人只能点赞一次" });

            chapter.likes = (chapter.likes ?? 0) + 1;
            _db.NovelChaptersActions.Add(new NovelChaptersAction
            {
                chapterId = chapter.id,
                userId = userId,
                actionType = 1,
                actionTime = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return Ok(new { likes = chapter.likes });
        }

        // 举报章节（每人只能举报一次）
        [HttpPost("chapter/{chapterId}/report")]
        public async Task<IActionResult> ReportChapter(int chapterId)
        {
            var chapter = await _db.NovelChapters.FindAsync(chapterId);
            if (chapter == null) return NotFound(new { message = "章节未找到" });

            int userId;
            try { userId = GetUserId(); }
            catch { return Unauthorized(new { message = "请重新登录" }); }

            // 已举报则不能再举报
            bool hasReported = await _db.NovelChaptersActions.AnyAsync(a =>
                a.chapterId == chapterId &&
                a.userId == userId &&
                a.actionType == 2
            );
            if (hasReported)
                return BadRequest(new { message = "每人只能举报一次" });

            chapter.reports = (chapter.reports ?? 0) + 1;
            _db.NovelChaptersActions.Add(new NovelChaptersAction
            {
                chapterId = chapter.id,
                userId = userId,
                actionType = 2,
                actionTime = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return Ok(new { reports = chapter.reports });
        }

        // 获取当前用户对章节的行为状态（如已点赞/已举报/已浏览）
        [HttpGet("chapter/{chapterId}/actionstatus")]
        public async Task<IActionResult> GetChapterActionStatus(int chapterId)
        {
            int userId;
            try { userId = GetUserId(); }
            catch
            {
                // 返回标准 JSON 和提示，不要返回 Unauthorized！
                return Ok(new
                {
                    hasViewed = false,
                    hasLiked = false,
                    hasReported = false,
                    message = "请重新登录"
                });
            }

            var oneHourAgo = DateTime.UtcNow.AddHours(-1);

            bool hasViewed = await _db.NovelChaptersActions.AnyAsync(a =>
                a.chapterId == chapterId &&
                a.userId == userId &&
                a.actionType == 7 &&
                a.actionTime > oneHourAgo
            );
            bool hasLiked = await _db.NovelChaptersActions.AnyAsync(a =>
                a.chapterId == chapterId &&
                a.userId == userId &&
                a.actionType == 1
            );
            bool hasReported = await _db.NovelChaptersActions.AnyAsync(a =>
                a.chapterId == chapterId &&
                a.userId == userId &&
                a.actionType == 2
            );

            return Ok(new
            {
                hasViewed,
                hasLiked,
                hasReported
            });
        }
    }
}