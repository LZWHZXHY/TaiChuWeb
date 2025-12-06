using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using THCY_BE.Dto.Drawing;
using THCY_BE.Models.Draws;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace THCY_BE.DataBase
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DrawingController : ControllerBase
    {
        private readonly DrawingDbContext _context;
        private readonly ILogger<DrawingController> _logger;

        // 保持和你项目一致的物理路径/WEB 映射路径
        private const string BASE_PHYSICAL_PATH = "/www/wwwroot/bianyuzhou.com/uploads";
        private const string BASE_WEB_PATH = "/uploads";

        // 允许的扩展名
        private static readonly string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".heic" };

        // 最大文件 20MB（按需调整）
        private const long MAX_FILE_BYTES = 20 * 1024 * 1024;

        public DrawingController(DrawingDbContext context, ILogger<DrawingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 上传绘画作品（按你要求的路径与 DB 存储相对路径格式）
        /// 物理路径: /www/wwwroot/bianyuzhou.com/uploads/绘画展厅/{用户id}/{作品id}/{作品id}_{用户id}_{时间戳}.{ext}
        /// DB 存储: 绘画展厅/{用户id}/{作品id}/{作品id}_{用户id}_{时间戳}.{ext}
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] DrawingUploadDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "请求数据不完整或格式不正确" });

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { success = false, message = "未授权" });

            var file = dto.Image;
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "未检测到上传文件" });

            if (!ValidateImageFile(file))
                return BadRequest(new { success = false, message = "文件类型不支持或文件过大" });

            // Step 1: 创建 DB 记录（先占位以获取作品id）
            var drawing = new Drawing
            {
                UploaderId = userId.Value,
                AuthorName = string.IsNullOrWhiteSpace(dto.AuthorName) ? GetCurrentUserName() ?? "匿名" : dto.AuthorName.Trim(),
                ImageUrl = "", // 先占位，之后更新为正确相对路径（DB 非空列用空字符串占位）
                UploadAt = DateTime.UtcNow,
                status = 0,
                likes = 0,
                report = 0,
                title = string.IsNullOrWhiteSpace(dto.Title) ? "未命名" : dto.Title.Trim(),
                desc = string.IsNullOrWhiteSpace(dto.Desc) ? null : dto.Desc.Trim()
            };

            try
            {
                _context.Set<Drawing>().Add(drawing);
                await _context.SaveChangesAsync(); // 此处拿到 drawing.Id（作品id）

                // Step 2: 生成文件名与路径，按你指定的规则
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var fileName = $"{drawing.Id}_{userId}_{timestamp}{ext}";

                var folder = Path.Combine("绘画展厅", userId.ToString(), drawing.Id.ToString());
                var relativePath = Path.Combine(folder, fileName).Replace("\\", "/"); // 存入 DB 的格式
                var physicalDirectory = Path.Combine(BASE_PHYSICAL_PATH, folder);
                var physicalPath = Path.Combine(physicalDirectory, fileName);

                // 确保目录存在
                if (!Directory.Exists(physicalDirectory))
                {
                    Directory.CreateDirectory(physicalDirectory);
                    _logger.LogInformation("创建目录: {Dir}", physicalDirectory);
                }

                // Step 3: 将文件保存到磁盘
                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("用户 {User} 的作品已保存到磁盘: {Path}", userId, physicalPath);

                // Step 4: 更新 DB 的 ImageUrl 字段为相对路径
                drawing.ImageUrl = relativePath;
                _context.Set<Drawing>().Update(drawing);
                await _context.SaveChangesAsync();

                // 构建可访问完整 URL（基于当前请求 host）
                var imageUrl = BuildImageUrl(relativePath);

                return Ok(new
                {
                    success = true,
                    message = "上传成功",
                    data = new
                    {
                        id = drawing.Id,
                        title = drawing.title,
                        imageUrl,
                        relativePath // 也返回相对路径供前端或其他用途
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传失败，尝试回滚：删除文件或 DB 记录");

                // 若文件已写入，但中途出错，尝试删除文件并删除 DB 占位记录
                try
                {
                    var possibleFolder = Path.Combine(BASE_PHYSICAL_PATH, "绘画展厅", userId.ToString(), drawing.Id.ToString());
                    if (Directory.Exists(possibleFolder))
                    {
                        var files = Directory.GetFiles(possibleFolder, $"{drawing.Id}_{userId}_*");
                        foreach (var f in files)
                        {
                            try { System.IO.File.Delete(f); _logger.LogInformation("已删除回滚文件: {File}", f); } catch { }
                        }
                    }

                    var tracked = await _context.Set<Drawing>().FindAsync(drawing.Id);
                    if (tracked != null)
                    {
                        _context.Set<Drawing>().Remove(tracked);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("已删除回滚 DB 记录: {Id}", drawing.Id);
                    }
                }
                catch (Exception rollEx)
                {
                    _logger.LogWarning(rollEx, "回滚时遇到问题");
                }

                return StatusCode(500, new { success = false, message = "服务器错误，上传失败" });
            }
        }

        // 替换现有 List 方法：cursor 格式 "2025-12-02T12:34:56.789Z|123"
        [HttpGet("list")]
        public async Task<IActionResult> List(
            [FromQuery] int pageSize = 20,
            [FromQuery] string sort = "new",
            [FromQuery] string? cursor = null) // optional cursor
        {
            if (pageSize < 1) pageSize = 20;

            try
            {
                var baseQuery = _context.Set<Drawing>().AsNoTracking();

                // parse cursor (uploadAt|id)
                DateTime? cursorTime = null;
                int? cursorId = null;
                if (!string.IsNullOrEmpty(cursor))
                {
                    var parts = cursor.Split('|');
                    if (parts.Length == 2)
                    {
                        if (DateTime.TryParse(parts[0], null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var dt))
                            cursorTime = dt;
                        if (int.TryParse(parts[1], out var id)) cursorId = id;
                    }
                    else
                    {
                        return BadRequest(new { success = false, message = "cursor 格式不正确，示例：2025-12-02T12:34:56.789Z|123" });
                    }
                }

                // only support sort=new for keyset; others can remain offset-based if needed
                if (!string.Equals(sort, "new", StringComparison.OrdinalIgnoreCase))
                {
                    // fallback offset pagination for non-new sorts (simple implementation)
                    var ordered = sort switch
                    {
                        "hot" => baseQuery.OrderByDescending(x => x.likes),
                        "alpha" => baseQuery.OrderBy(x => x.title),
                        _ => baseQuery.OrderByDescending(x => x.UploadAt)
                    };

                    var itemsOffset = await ordered.Take(pageSize).ToListAsync(); // simple first page fallback
                    var resultOffset = itemsOffset.Select(x => new {
                        id = x.Id,
                        title = x.title,
                        authorName = x.AuthorName,
                        likes = x.likes ?? 0,
                        uploadAt = x.UploadAt,
                        imageUrl = x.ImageUrl,
                        imageUrlFull = BuildImageUrl(x.ImageUrl)
                    }).ToList();

                    return Ok(new { success = true, data = new { items = resultOffset, hasMore = resultOffset.Count == pageSize } });
                }

                // sort == "new": keyset pagination (UploadAt desc, Id desc)
                IQueryable<Drawing> q = baseQuery;
                if (cursorTime.HasValue && cursorId.HasValue)
                {
                    // uploadAt < cursorTime OR (uploadAt == cursorTime AND Id < cursorId)
                    q = q.Where(x => x.UploadAt < cursorTime.Value || (x.UploadAt == cursorTime.Value && x.Id < cursorId.Value));
                }

                var pageItems = await q
                    .OrderByDescending(x => x.UploadAt)
                    .ThenByDescending(x => x.Id)
                    .Take(pageSize + 1) // fetch one extra to detect hasMore
                    .ToListAsync();

                var hasMore = pageItems.Count > pageSize;
                if (hasMore) pageItems = pageItems.Take(pageSize).ToList();

                var items = pageItems.Select(x => new {
                    id = x.Id,
                    title = x.title,
                    authorName = x.AuthorName,
                    likes = x.likes ?? 0,
                    uploadAt = x.UploadAt,
                    desc = x.desc,
                    imageUrl = x.ImageUrl,
                    imageUrlFull = !string.IsNullOrEmpty(x.ImageUrl) ? BuildImageUrl(x.ImageUrl) : null
                }).ToList();

                // last cursor for next page (use last item's uploadAt|id)
                string? nextCursor = null;
                if (pageItems.Any())
                {
                    var last = pageItems.Last();
                    nextCursor = $"{last.UploadAt.ToUniversalTime():o}|{last.Id}";
                }

                return Ok(new { success = true, data = new { items, nextCursor, hasMore } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取绘画列表失败");
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                if (env.Equals("Development", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { success = false, message = "获取列表失败（开发模式）", error = ex.Message, stack = ex.ToString() });
                return StatusCode(500, new { success = false, message = "获取列表失败" });
            }
        }

        /// <summary>
        /// 验证文件扩展名和大小。生产建议再加入 MIME / magic-bytes 检查。
        /// </summary>
        private bool ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;
            if (file.Length > MAX_FILE_BYTES) return false;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext)) return false;

            return true;
        }

        /// <summary>
        /// 根据相对路径构建完整 URL（基于当前请求 host）
        /// </summary>
        private string BuildImageUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;
            var baseUrl = $"{Request.Scheme}://{Request.Host}".TrimEnd('/');
            var url = $"{baseUrl}/{BASE_WEB_PATH.TrimStart('/').TrimEnd('/')}/{relativePath.TrimStart('/')}";
            return url.Replace("\\", "/");
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : (int?)null;
        }

        private string GetCurrentUserName()
        {
            var claim = User.FindFirst(ClaimTypes.Name);
            return claim?.Value;
        }
    }
}