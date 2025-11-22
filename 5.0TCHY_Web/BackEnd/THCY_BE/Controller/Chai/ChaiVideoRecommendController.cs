using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using THCY_BE.DataBase;
using THCY_BE.Models.Chai;

namespace THCY_BE.Controller.Chai
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 必须携带 Bearer token
    public class ChaiVideoRecommendController : ControllerBase
    {
        private readonly ChaiDbContext _db;

        public ChaiVideoRecommendController(ChaiDbContext chaiDb)
        {
            _db = chaiDb;
        }

        // ==================== DTOs (数据传输对象) ====================
        public class VideoRecommendDto
        {
            public int ID { get; set; }
            public string title { get; set; } = string.Empty;
            public string description { get; set; } = string.Empty;
            public string video_url { get; set; } = string.Empty;
            public int category_id { get; set; }
            public string author { get; set; } = string.Empty;
            public int? Bestyear { get; set; }
            public int? monthRecommend { get; set; }

            // 计算属性
            public string category_name => GetCategoryName(category_id);
            public bool is_daily_recommend => IsDailyRecommend();
            public bool is_monthly_recommend => monthRecommend.HasValue;
            public bool is_yearly_recommend => Bestyear.HasValue;

            private string GetCategoryName(int categoryId)
            {
                return categoryId switch
                {
                    1 => "接力类",
                    2 => "联合类",
                    3 => "世界观企划类",
                    _ => "未知分类"
                };
            }

            private bool IsDailyRecommend()
            {
                // 今日推荐逻辑：如果月份推荐是当前月份，则视为今日推荐
                var currentMonth = DateTime.Now.Year * 100 + DateTime.Now.Month;
                return monthRecommend == currentMonth;
            }
        }

        public class CreateVideoRecommendRequest
        {
            [Required(ErrorMessage = "标题不能为空")]
            [MaxLength(200, ErrorMessage = "标题长度不能超过200个字符")]
            public string title { get; set; } = string.Empty;

            [Required(ErrorMessage = "描述不能为空")]
            public string description { get; set; } = string.Empty;

            [Required(ErrorMessage = "视频链接不能为空")]
            [Url(ErrorMessage = "请输入有效的视频链接")]
            public string video_url { get; set; } = string.Empty;

            [Required(ErrorMessage = "分类ID不能为空")]
            [Range(1, 3, ErrorMessage = "分类ID必须是1-3之间的数字")]
            public int category_id { get; set; }

            [Required(ErrorMessage = "作者不能为空")]
            [MaxLength(100, ErrorMessage = "作者名称长度不能超过100个字符")]
            public string author { get; set; } = string.Empty;

            [Range(2020, 2030, ErrorMessage = "年度最佳年份必须在2020-2030之间")]
            public int? Bestyear { get; set; }

            public int? monthRecommend { get; set; }
        }

        public class UpdateVideoRecommendRequest
        {
            [MaxLength(200, ErrorMessage = "标题长度不能超过200个字符")]
            public string? title { get; set; }

            public string? description { get; set; }

            [Url(ErrorMessage = "请输入有效的视频链接")]
            public string? video_url { get; set; }

            [Range(1, 3, ErrorMessage = "分类ID必须是1-3之间的数字")]
            public int? category_id { get; set; }

            [MaxLength(100, ErrorMessage = "作者名称长度不能超过100个字符")]
            public string? author { get; set; }

            [Range(2020, 2030, ErrorMessage = "年度最佳年份必须在2020-2030之间")]
            public int? Bestyear { get; set; }

            public int? monthRecommend { get; set; }
        }

        public class UpdateRecommendationRequest
        {
            [Range(2020, 2030, ErrorMessage = "年度最佳年份必须在2020-2030之间")]
            public int? Bestyear { get; set; }

            public int? monthRecommend { get; set; }
        }

        // ==================== API 接口 ====================

        // GET: api/ChaiVideoRecommend
        [HttpGet]
        public async Task<ActionResult> GetAllRecommendations()
        {
            try
            {
                var videos = await _db.videoRecommends
                    .Select(v => new VideoRecommendDto
                    {
                        ID = v.ID,
                        title = v.title,
                        description = v.description,
                        video_url = v.video_url,
                        category_id = v.category_id,
                        author = v.author,
                        Bestyear = v.Bestyear,
                        monthRecommend = v.monthRecommend
                    })
                    .OrderByDescending(v => v.Bestyear.HasValue)
                    .ThenByDescending(v => v.monthRecommend.HasValue)
                    .ThenByDescending(v => v.ID)
                    .ToListAsync();

                return Ok(new { success = true, data = videos, count = videos.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取推荐列表失败", error = ex.Message });
            }
        }

        // GET: api/ChaiVideoRecommend/type/daily
        [HttpGet("type/{type}")]
        public async Task<ActionResult> GetRecommendationsByType(string type)
        {
            try
            {
                IQueryable<VideoRecommend> query = _db.videoRecommends;
                var currentMonth = DateTime.Now.Year * 100 + DateTime.Now.Month;

                switch (type.ToLower())
                {
                    case "daily":
                        // 今日推荐：当前月份的推荐
                        query = query.Where(v => v.monthRecommend == currentMonth);
                        break;
                    case "monthly":
                        // 月度推荐：所有有月份标记的
                        query = query.Where(v => v.monthRecommend.HasValue);
                        break;
                    case "yearly":
                        // 年度最佳：所有有年度标记的
                        query = query.Where(v => v.Bestyear.HasValue);
                        break;
                    case "featured":
                        // 特别推荐：年度或月度推荐
                        query = query.Where(v => v.Bestyear.HasValue || v.monthRecommend.HasValue);
                        break;
                    case "all":
                        // 返回所有
                        break;
                    default:
                        return BadRequest(new { message = "无效的推荐类型。支持：daily, monthly, yearly, featured, all" });
                }

                var videos = await query
                    .Select(v => new VideoRecommendDto
                    {
                        ID = v.ID,
                        title = v.title,
                        description = v.description,
                        video_url = v.video_url,
                        category_id = v.category_id,
                        author = v.author,
                        Bestyear = v.Bestyear,
                        monthRecommend = v.monthRecommend
                    })
                    .OrderByDescending(v => v.Bestyear.HasValue)
                    .ThenByDescending(v => v.monthRecommend.HasValue)
                    .ThenByDescending(v => v.ID)
                    .ToListAsync();

                return Ok(new { success = true, data = videos, count = videos.Count, type = type });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取{type}推荐失败", error = ex.Message });
            }
        }

        // GET: api/ChaiVideoRecommend/bestyear/2023
        [HttpGet("bestyear/{year}")]
        public async Task<ActionResult> GetBestOfYear(int year)
        {
            try
            {
                var videos = await _db.videoRecommends
                    .Where(v => v.Bestyear == year)
                    .Select(v => new VideoRecommendDto
                    {
                        ID = v.ID,
                        title = v.title,
                        description = v.description,
                        video_url = v.video_url,
                        category_id = v.category_id,
                        author = v.author,
                        Bestyear = v.Bestyear,
                        monthRecommend = v.monthRecommend
                    })
                    .OrderByDescending(v => v.ID)
                    .ToListAsync();

                return Ok(new { success = true, data = videos, count = videos.Count, year = year });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取{year}年度最佳失败", error = ex.Message });
            }
        }

        // GET: api/ChaiVideoRecommend/month/202312
        [HttpGet("month/{yearMonth}")]
        public async Task<ActionResult> GetMonthRecommendations(int yearMonth)
        {
            try
            {
                var videos = await _db.videoRecommends
                    .Where(v => v.monthRecommend == yearMonth)
                    .Select(v => new VideoRecommendDto
                    {
                        ID = v.ID,
                        title = v.title,
                        description = v.description,
                        video_url = v.video_url,
                        category_id = v.category_id,
                        author = v.author,
                        Bestyear = v.Bestyear,
                        monthRecommend = v.monthRecommend
                    })
                    .OrderByDescending(v => v.ID)
                    .ToListAsync();

                return Ok(new { success = true, data = videos, count = videos.Count, month = yearMonth });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取{yearMonth}月度推荐失败", error = ex.Message });
            }
        }

        // GET: api/ChaiVideoRecommend/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetRecommendationById(int id)
        {
            try
            {
                var video = await _db.videoRecommends
                    .Where(v => v.ID == id)
                    .Select(v => new VideoRecommendDto
                    {
                        ID = v.ID,
                        title = v.title,
                        description = v.description,
                        video_url = v.video_url,
                        category_id = v.category_id,
                        author = v.author,
                        Bestyear = v.Bestyear,
                        monthRecommend = v.monthRecommend
                    })
                    .FirstOrDefaultAsync();

                if (video == null)
                    return NotFound(new { message = "推荐视频不存在" });

                return Ok(new { success = true, data = video });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取推荐视频失败", error = ex.Message });
            }
        }

        // POST: api/ChaiVideoRecommend
        [HttpPost]
        public async Task<ActionResult> CreateRecommendation([FromBody] CreateVideoRecommendRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { message = "参数验证失败", errors = errors });
                }

                var video = new VideoRecommend
                {
                    
                    title = request.title.Trim(),
                    description = request.description.Trim(),
                    video_url = request.video_url.Trim(),
                    category_id = request.category_id,
                    author = request.author.Trim(),
                    Bestyear = request.Bestyear,
                    monthRecommend = request.monthRecommend
                };

                _db.videoRecommends.Add(video);
                await _db.SaveChangesAsync();

                var videoDto = new VideoRecommendDto
                {
                    ID = video.ID,
                    title = video.title,
                    description = video.description,
                    video_url = video.video_url,
                    category_id = video.category_id,
                    author = video.author,
                    Bestyear = video.Bestyear,
                    monthRecommend = video.monthRecommend
                };

                return CreatedAtAction(nameof(GetRecommendationById), new { id = video.ID }, new { success = true, message = "创建成功", data = videoDto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "创建推荐视频失败", error = ex.Message });
            }
        }

    }
}