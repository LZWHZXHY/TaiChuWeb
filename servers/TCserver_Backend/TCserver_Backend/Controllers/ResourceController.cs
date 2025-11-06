using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // 添加这一行
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using TCserver_Backend.Data;
using System.Security.Claims;
using System.Security.Cryptography;




namespace TCserver_Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ResourceController : ControllerBase
    {
        private readonly FunctionDbContext functionDbContext;
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;

        public ResourceController(FunctionDbContext functionDbContext, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            this.functionDbContext = functionDbContext;
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
        }


        // 获取所有资源分类
        [HttpGet("categories")]
        public async Task<ActionResult> GetResourceCategories()
        {
            try
            {
                var categories = await functionDbContext.Resource_categories
                    .Where(c => c.status == 1)
                    .OrderBy(c => c.sort_order)
                    .Select(c => new // 使用匿名对象
                    {
                        Id = c.ID,
                        Name = c.name,
                        Description = c.description,
                        SortOrder = c.sort_order,
                        CreateTime = c.create_time,
                        UpdateTime = c.update_time
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取分类失败", error = ex.Message });
            }
        }

        // 获取资源列表
        [HttpGet("")]
        public async Task<ActionResult> GetResources([FromQuery] int? categoryId = null)
        {
            try
            {
                Console.WriteLine($"开始获取资源，分类ID: {categoryId}");

                // 检查DbContext是否正常
                if (functionDbContext == null)
                {
                    Console.WriteLine("DbContext为空");
                    return StatusCode(500, new { message = "数据库上下文未初始化" });
                }

                // 检查Resources DbSet是否存在
                var resources = functionDbContext.Resources;
                if (resources == null)
                {
                    Console.WriteLine("Resources DbSet为空");
                    return StatusCode(500, new { message = "资源表不存在" });
                }

                var query = functionDbContext.Resources
                    .Where(r => r.status == 1);

                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    Console.WriteLine($"按分类筛选: {categoryId}");
                    query = query.Where(r => r.category_id == categoryId.Value);
                }

                var result = await query
                    .OrderByDescending(r => r.create_time)
                    .Select(r => new
                    {
                        Id = r.Id,
                        Title = r.title,
                        Description = r.desc,
                        CategoryId = r.category_id,
                        CategoryName = r.Resource_Categories.name,
                        DownloadUrl = r.download_url,
                        LevelRequired = r.level_required,
                        PointsRequired = r.points_required,
                        DownloadCount = r.download_count,
                        CreateTime = r.create_time,
                        UpdateTime = r.update_time
                    })
                    .ToListAsync();

                Console.WriteLine($"成功获取 {result.Count} 个资源");
                return Ok(result);
            }
            catch (Exception ex)
            {
                // 记录完整错误信息
                Console.WriteLine($"获取资源失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部异常: {ex.InnerException.Message}");
                }

                return StatusCode(500, new
                {
                    message = "获取资源失败",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }




        // ========== 签名辅助方法（新增） ==========
        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
        private static byte[] Base64UrlDecode(string input)
        {
            string padded = input.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4) { case 2: padded += "=="; break; case 3: padded += "="; break; }
            return Convert.FromBase64String(padded);
        }

        private string MakeSig(int rid, int uid, long exp)
        {
            var secret = configuration["Download:Secret"] ?? "change-me-very-secret";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var payload = $"{rid}.{uid}.{exp}";
            var mac = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Base64UrlEncode(mac);
        }

        private bool VerifySig(int rid, int uid, long exp, string sig)
        {
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp) return false;
            var expected = MakeSig(rid, uid, exp);
            try
            {
                var a = Base64UrlDecode(expected);
                var b = Base64UrlDecode(sig);
                if (a.Length != b.Length) return false;
                int diff = 0; for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
                return diff == 0;
            }
            catch { return false; }
        }

        private string BuildSignedUrl(HttpRequest req, int rid, int uid)
        {
            var minutesCfg = configuration["Download:ExpiryMinutes"];
            int minutes = 5;
            if (!string.IsNullOrEmpty(minutesCfg) && int.TryParse(minutesCfg, out var m) && m > 0) minutes = m;

            var exp = DateTimeOffset.UtcNow.AddMinutes(minutes).ToUnixTimeSeconds();
            var sig = MakeSig(rid, uid, exp);
            var baseUrl = $"{req.Scheme}://{req.Host}";
            // 为向后兼容，保留 resourceId 参数
            return $"{baseUrl}/api/Resource/download?resourceId={rid}&rid={rid}&uid={uid}&exp={exp}&sig={sig}";
        }

        // ========== 获取签名直链接口（新增） ==========
        [Authorize]
        [HttpGet("signed-url")]
        public async Task<IActionResult> GetSignedUrl([FromQuery] int resourceId)
        {
            if (resourceId <= 0) return BadRequest(new { message = "参数错误" });

            var resource = await functionDbContext.Resources
                .FirstOrDefaultAsync(r => r.Id == resourceId && r.status == 1);
            if (resource == null) return NotFound(new { message = "资源不存在" });

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            // 可选：此处可加入你“别处”的快速授权校验（已通过则发放）
            var url = BuildSignedUrl(Request, resourceId, userId);
            return Ok(new { url });
        }

        // ========== 下载接口（替换原有 Download 方法） ==========
        [AllowAnonymous] // 允许签名直下；无签名时仍按 JWT 方式鉴权
        [HttpGet("download")]
        public async Task<IActionResult> Download(
            [FromQuery] int resourceId,
            [FromQuery] int? rid,
            [FromQuery] int? uid,
            [FromQuery] long? exp,
            [FromQuery] string? sig)
        {
            // 路径A：签名直下（window.open 使用）
            int effectiveRid = resourceId;
            if (rid.HasValue && uid.HasValue && exp.HasValue && !string.IsNullOrEmpty(sig))
            {
                if (resourceId != 0 && rid.Value != resourceId)
                    return BadRequest(new { message = "资源ID不一致" });

                if (!VerifySig(rid.Value, uid.Value, exp.Value, sig))
                    return Forbid("签名无效或已过期");

                effectiveRid = rid.Value;

                // 可选：此处可记录下载日志/计数，uid.Value 即发起下载的用户ID
            }
            else
            {
                // 路径B：JWT 访问（兼容旧方式）
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr))
                    return Unauthorized(new { message = "未登录或令牌无效" });

                // 可选：兜底业务校验
            }

            // 1. 验证资源存在
            var resource = await functionDbContext.Resources
                .FirstOrDefaultAsync(r => r.Id == effectiveRid && r.status == 1);
            if (resource == null)
                return NotFound(new { message = "资源不存在" });

            // 2. 拼接 NAS URL（相对路径）
            var nasEndpoint = configuration["StorageSettings:NasEndpoint"] ?? "";
            if (string.IsNullOrWhiteSpace(nasEndpoint))
                return StatusCode(500, new { message = "文件存储配置未设置" });

            var baseEndpoint = nasEndpoint.TrimEnd('/');
            var relativePath = (resource.download_url ?? "").Replace("\\", "/").Trim('/');
            var fileUrl = $"{baseEndpoint}/{relativePath}";

            // 3. 创建 HttpClient 并准备请求（透传 Range）
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(30);

            var nasUser = configuration["NasCredentials:Username"];
            var nasPass = configuration["NasCredentials:Password"];
            if (!string.IsNullOrEmpty(nasUser))
            {
                var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{nasUser}:{nasPass}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, fileUrl);
            if (Request.Headers.ContainsKey("Range"))
                requestMessage.Headers.TryAddWithoutValidation("Range", Request.Headers["Range"].ToString());

            HttpResponseMessage nasResp;
            try
            {
                nasResp = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { message = "无法连接到文件存储", detail = ex.Message });
            }

            if (!nasResp.IsSuccessStatusCode)
            {
                var code = (int)nasResp.StatusCode;
                string detail = null;
                try { detail = await nasResp.Content.ReadAsStringAsync(); } catch { }
                return StatusCode(code, new { message = "文件服务器返回错误", detail });
            }

            // 4. 透传部分响应头并设置状态码（Range 支持）
            if (nasResp.Content.Headers.ContentType != null)
                Response.ContentType = nasResp.Content.Headers.ContentType.ToString();

            if (nasResp.Headers.AcceptRanges != null && nasResp.Headers.AcceptRanges.Any())
                Response.Headers["Accept-Ranges"] = string.Join(",", nasResp.Headers.AcceptRanges);

            if (nasResp.Content.Headers.ContentLength.HasValue)
                Response.Headers["Content-Length"] = nasResp.Content.Headers.ContentLength.Value.ToString();

            if (nasResp.Content.Headers.ContentRange != null)
            {
                Response.Headers["Content-Range"] = nasResp.Content.Headers.ContentRange.ToString();
                Response.StatusCode = (int)HttpStatusCode.PartialContent; // 206
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
            }

            // 优化大文件直传
            Response.Headers["X-Accel-Buffering"] = "no";
            Response.Headers["X-Source-Url"] = Uri.EscapeDataString(fileUrl);

            // 5. 获取流
            var stream = await nasResp.Content.ReadAsStreamAsync();

            // 6. 从 relativePath 提取文件名并确保带扩展名
            string fileName;
            try
            {
                fileName = Path.GetFileName(relativePath);
                if (string.IsNullOrWhiteSpace(fileName)) fileName = "download";
            }
            catch { fileName = "download"; }

            if (string.IsNullOrWhiteSpace(Path.GetExtension(fileName)))
            {
                var ext = Path.GetExtension(relativePath);
                if (!string.IsNullOrWhiteSpace(ext)) fileName = fileName + ext;
            }

            // 7. 设置 Content-Disposition 与 X-Filename
            try
            {
                var disposition = new ContentDispositionHeaderValue("attachment");
                var safeFileName = fileName.Replace("\"", "\\\"");
                disposition.FileName = $"\"{safeFileName}\"";
                disposition.FileNameStar = fileName;
                Response.Headers["Content-Disposition"] = disposition.ToString();
                Response.Headers["X-Filename"] = Uri.EscapeDataString(fileName);
            }
            catch
            {
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                Response.Headers["X-Filename"] = Uri.EscapeDataString(fileName);
            }

            // 8. 返回流
            return new FileStreamResult(stream, Response.ContentType ?? "application/octet-stream");
        }
    }
}
