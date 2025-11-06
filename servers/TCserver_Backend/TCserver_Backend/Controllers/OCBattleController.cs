using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using TCserver_Backend.Data;
using TCserver_Backend.Dtos;
using TCserver_Backend.Models.OCBattle;

namespace TCserver_Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OCBattleController : ControllerBase
    {
        private readonly FunctionDbContext functionDbContext;
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<OCBattleController> logger;

        public OCBattleController(FunctionDbContext functionDbContext, IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<OCBattleController> logger)
        {
            this.functionDbContext = functionDbContext;
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        // POST: api/OCBattle/create
        // Replace only the CreateOC method in your OCBattleController with the following implementation.

        [HttpPost("create")]
        public async Task<IActionResult> CreateOC([FromBody] CreateOCRequest req)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Keys
                    .SelectMany(k => ModelState[k]!.Errors.Select(e => new { field = k, error = e.ErrorMessage }))
                    .ToList();
                return BadRequest(new { message = "Validation failed", errors });
            }

            // 从 token/claims 获取认证用户 id 与用户名（用于回退/记录）
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var tokenUserName = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name);

            // 解析 authorID（保持 server 控制）
            int parsedAuthorId = 0;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var aid))
            {
                parsedAuthorId = aid;
            }

            // 使用前端提交的 AuthorName（显示名称）优先；如为空则使用 token 名称作为回退
            string finalAuthorName = (req.AuthorName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(finalAuthorName))
            {
                finalAuthorName = tokenUserName ?? string.Empty;
            }
            // 安全：限制长度，防止非常长的字符串导致问题（数据库列长度、UI）
            const int MAX_AUTHOR_NAME_LENGTH = 100;
            if (finalAuthorName.Length > MAX_AUTHOR_NAME_LENGTH)
            {
                finalAuthorName = finalAuthorName.Substring(0, MAX_AUTHOR_NAME_LENGTH);
            }

            // 辅助：安全地裁剪并限制长度
            static string SafeTrim(string? s, int maxLen = 4000)
            {
                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                var t = s.Trim();
                return t.Length <= maxLen ? t : t.Substring(0, maxLen);
            }

            // 构造实体，注意字段名和序列化
            var oc = new OC_Info
            {
                name = SafeTrim(req.Name, 200),
                gender = req.Gender,
                age = req.Age,
                species = SafeTrim(req.Species, 200),
                POO = SafeTrim(req.POO, 200),
                OC_Current_Time = req.OC_Current_Time,
                // 把前端传送的 Character 写入实体
                character = string.IsNullOrWhiteSpace(req.Character) ? null : SafeTrim(req.Character, 2000),
                ability = SafeTrim(req.ability, 4000),
                colors = SafeTrim(req.Colors, 1000),
                background = string.IsNullOrWhiteSpace(req.Background) ? null : SafeTrim(req.Background, 4000),
                OC_WeapenDesc = string.IsNullOrWhiteSpace(req.OC_WeapenDesc) ? null : SafeTrim(req.OC_WeapenDesc, 1000),
                // 将数字数组序列化为合法 JSON 文本写入 experience 列
                experience = JsonSerializer.Serialize(req.Experience ?? Array.Empty<int>()),

                // server-controlled fields:
                authorID = parsedAuthorId,
                authorName = finalAuthorName,
                status = 1,
                OC_status = 1,
                dueling = 0,
                version = 1,
                winCount = 0,
                loseCount = 0,
                createTime = DateTime.UtcNow,
                updateTime = DateTime.UtcNow
            };

            // 临时占位：避免因数据库列为 NOT NULL 导致插入失败（建议长期方案用 migration 将列改为 nullable）
            oc.OC_image_url = oc.OC_image_url ?? "";

            functionDbContext.Add(oc);
            await functionDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOCById), new { id = oc.id }, new { id = oc.id, name = oc.name, authorName = oc.authorName });
        }

        // GET: api/OCBattle/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOCById(int id)
        {
            var oc = await functionDbContext.Set<OC_Info>().AsNoTracking().FirstOrDefaultAsync(o => o.id == id);
            if (oc == null) return NotFound(new { message = "OC not found" });

            return Ok(oc);
        }

        // POST: api/OCBattle/{id}/upload-files
        [HttpPost("{id:int}/upload-files")]
        [RequestSizeLimit(200_000_000)]
        public async Task<IActionResult> UploadFiles(int id, IFormFile? avatar, IFormFile? weapon, [FromForm] IFormFile[]? gallery)
        {
            var oc = await functionDbContext.Set<OC_Info>().FirstOrDefaultAsync(o => o.id == id);
            if (oc == null) return NotFound(new { message = "OC not found" });

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "未登录或用户标识无效" });
            }
            var isOwner = oc.authorID == currentUserId;
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Administrator");
            if (!isOwner && !isAdmin)
            {
                return Forbid();
            }

            var nasEndpoint = GetNasEndpoint();
            var username = configuration["NasCredentials:Username"];
            var password = configuration["NasCredentials:Password"];
            if (string.IsNullOrEmpty(nasEndpoint) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                logger.LogError("NAS 配置缺失");
                return StatusCode(500, new { message = "服务器存储配置缺失" });
            }

            var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var folder = "OC_Battle_Picture";
            var maxBytes = configuration.GetValue<long?>("StorageSettings:MaxUploadBytes") ?? (10 * 1024 * 1024);

            var uploaded = new List<string>();
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            async Task<(bool success, string relativePath)> uploadOne(IFormFile file, int seq)
            {
                if (file == null || file.Length == 0) return (false, string.Empty);
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExt.Contains(ext)) throw new InvalidOperationException($"不支持的文件类型: {ext}");
                if (file.Length > maxBytes) throw new InvalidOperationException($"文件大小超过限制: {maxBytes} bytes");

                var fileName = $"{id}_{timestamp}_{seq}{ext}";
                var relativePath = $"{folder}/{fileName}";
                if (!ValidateRelativePath(relativePath)) throw new InvalidOperationException("生成的路径包含不安全或非法字符");

                var uploadUrl = $"{nasEndpoint}/{relativePath}";
                var success = await UploadStreamToNasWithRetry(uploadUrl, file.OpenReadStream(), ext, username, password, true, file.Length);
                return (success, relativePath);
            }

            try
            {
                int seqBase = 0;

                if (avatar != null)
                {
                    seqBase++;
                    var (ok, rel) = await uploadOne(avatar, seqBase);
                    if (ok)
                    {
                        uploaded.Add(rel);
                        oc.OC_image_url = rel;
                    }
                    else
                    {
                        logger.LogError("avatar 上传失败 ocId={id}", id);
                        return StatusCode(500, new { message = "avatar 上传失败" });
                    }
                }

                if (gallery != null && gallery.Length > 0)
                {
                    for (int i = 0; i < gallery.Length; i++)
                    {
                        seqBase++;
                        var file = gallery[i];
                        try
                        {
                            var (ok, rel) = await uploadOne(file, seqBase);
                            if (ok)
                            {
                                uploaded.Add(rel);
                            }
                            else
                            {
                                logger.LogError("gallery 文件上传失败 ocId={id} idx={i}", id, i);
                                return StatusCode(500, new { message = "gallery 某文件上传失败", index = i });
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            return BadRequest(new { message = ex.Message });
                        }
                    }

                    if (string.IsNullOrEmpty(oc.OC_image_url) && uploaded.Count > 0)
                    {
                        oc.OC_image_url = uploaded.First();
                    }
                }

                if (weapon != null)
                {
                    seqBase++;
                    var (ok, rel) = await uploadOne(weapon, seqBase);
                    if (ok)
                    {
                        uploaded.Add(rel);
                        oc.OC_WeapenImgUrl = rel;
                    }
                    else
                    {
                        logger.LogError("weapon 上传失败 ocId={id}", id);
                        return StatusCode(500, new { message = "weapon 上传失败" });
                    }
                }

                oc.updateTime = DateTime.UtcNow;

                functionDbContext.Update(oc);
                await functionDbContext.SaveChangesAsync();

                return Ok(new { uploaded = uploaded, primary = oc.OC_image_url, weapon = oc.OC_WeapenImgUrl });
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "上传参数校验失败");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "上传过程中发生错误，已部分上传的文件不会自动回滚");
                return StatusCode(500, new { message = "上传失败", error = ex.Message, uploaded });
            }
        }

        // -------------------------
        // Helper: upload stream to nas with retry using HttpClient PUT
        // -------------------------
        private async Task<bool> UploadStreamToNasWithRetry(string uploadUrl, Stream inputStream, string ext, string username, string password, bool overwrite, long contentLength)
        {
            const int maxAttempts = 3;
            const int delayMs = 1000;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var client = httpClientFactory.CreateClient();
                    client.Timeout = TimeSpan.FromSeconds(60);
                    var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                    if (inputStream.CanSeek) inputStream.Position = 0;

                    using var buffered = new BufferedStream(inputStream, 8192);
                    using var content = new StreamContent(buffered);
                    content.Headers.ContentType = new MediaTypeHeaderValue(GetContentTypeByExtension(ext));
                    content.Headers.ContentLength = contentLength;

                    var response = await client.PutAsync(uploadUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        logger.LogInformation("文件上传成功: {url}", uploadUrl);
                        return true;
                    }
                    else
                    {
                        var resp = await response.Content.ReadAsStringAsync();
                        logger.LogWarning("上传尝试 #{attempt} 失败: {status} - {resp}", attempt, response.StatusCode, resp);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "上传尝试异常");
                    if (attempt == maxAttempts)
                    {
                        logger.LogError(ex, "所有上传尝试失败");
                        return false;
                    }
                }

                await Task.Delay(delayMs);
                if (!inputStream.CanSeek)
                {
                    logger.LogWarning("输入流不可回退，无法重试多次上传");
                    break;
                }
            }

            return false;
        }

        // Validate that path is a safe relative path (no scheme, no host, no .. segments)
        private bool ValidateRelativePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            if (path.Contains("..")) return false;
            if (path.StartsWith("/") || path.StartsWith("\\")) return false;
            if (path.Contains("://")) return false;
            if (path.Length > 1 && path[1] == ':') return false;
            var invalidChars = Path.GetInvalidPathChars();
            if (path.IndexOfAny(invalidChars) >= 0) return false;
            return true;
        }

        // sanitize folder name: remove dangerous characters and trim slashes
        private string? SanitizeFolder(string? folder)
        {
            if (string.IsNullOrWhiteSpace(folder)) return null;
            var f = folder.Trim().Replace('\\', '/').Trim('/');
            if (f.Split('/').Any(p => p == "..")) return null;
            f = string.Join("/", f.Split('/').Select(p => Path.GetFileName(p)));
            return f;
        }

        private string GetNasEndpoint()
        {
            return configuration["StorageSettings:NasEndpoint"]?.TrimEnd('/') ?? throw new InvalidOperationException("NasEndpoint 未配置");
        }

        private string GetContentTypeByExtension(string ext)
        {
            if (string.IsNullOrEmpty(ext)) return "application/octet-stream";
            switch (ext.ToLower())
            {
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".webp": return "image/webp";
                default: return "application/octet-stream";
            }
        }

        // Image proxy endpoint so frontend can fetch images via backend (keeps NAS credentials private)
        // GET: /api/OCBattle/imageproxy?path={relativePath}
        [AllowAnonymous]
        [HttpGet("imageproxy")]
        public async Task<IActionResult> ImageProxy([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest(new { message = "path 参数不能为空" });
            }

            try
            {
                // 防止目录穿越或非法路径
                if (path.Contains("..") || path.StartsWith("/") || path.StartsWith("\\") || path.Contains("://"))
                {
                    return BadRequest(new { message = "不安全的路径" });
                }

                var nasEndpoint = configuration["StorageSettings:NasEndpoint"]?.TrimEnd('/');
                var username = configuration["NasCredentials:Username"];
                var password = configuration["NasCredentials:Password"];
                if (string.IsNullOrEmpty(nasEndpoint) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    logger.LogError("NAS 配置缺失，无法代理图片");
                    return StatusCode(500, new { message = "服务器未配置 NAS 存储" });
                }

                // 构建完整 URL
                var fullUrl = $"{nasEndpoint}/{path.TrimStart('/')}";

                using var client = httpClientFactory.CreateClient();
                // Basic auth
                var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
                client.Timeout = TimeSpan.FromSeconds(30);

                var resp = await client.GetAsync(fullUrl, HttpCompletionOption.ResponseHeadersRead);
                if (!resp.IsSuccessStatusCode)
                {
                    logger.LogWarning("从 NAS 获取图片失败: {url} => {status}", fullUrl, resp.StatusCode);
                    return StatusCode((int)resp.StatusCode, new { message = "无法获取图片", status = resp.StatusCode.ToString() });
                }

                var contentType = resp.Content.Headers.ContentType?.MediaType ?? GetContentTypeByExtension(Path.GetExtension(path));
                var stream = await resp.Content.ReadAsStreamAsync();

                // Optionally set cache headers here (Cache-Control) if desired:
                // Response.Headers["Cache-Control"] = "public, max-age=600";

                return File(stream, contentType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ImageProxy 异常：path={path}", path);
                return StatusCode(500, new { message = "图片代理失败", error = ex.Message });
            }
        }

        // GET: api/OCBattle/list
        [AllowAnonymous]
        [HttpGet("list")]
        public async Task<IActionResult> ListPersonas(
            [FromQuery] string? search,
            [FromQuery] string? tags,         // comma separated tags
            [FromQuery] int? ownerId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            // 只取 status == 1 的公开人设（如需改用 OC_status，请替换）
            var q = functionDbContext.Set<OC_Info>().AsNoTracking()
                .Where(o => o.status == 1);

            if (ownerId.HasValue)
                q = q.Where(o => o.authorID == ownerId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                q = q.Where(o =>
                    ((o.name ?? "").ToLower().Contains(s)) ||
                    ((o.authorName ?? "").ToLower().Contains(s)) ||
                    ((o.background ?? "").ToLower().Contains(s)) ||
                    ((o.POO ?? "").ToLower().Contains(s))
                );
            }

            List<string> tagList = new();
            if (!string.IsNullOrWhiteSpace(tags))
            {
                tagList = tags.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(t => t.Trim())
                              .Where(t => !string.IsNullOrEmpty(t)).ToList();
            }

            // 投影：返回用户上传的真实字段，便于前端直接显示
            var baseQuery = q.Select(o => new
            {
                o.id,
                o.authorID,
                authorName = o.authorName ?? string.Empty,
                name = o.name ?? string.Empty,
                age = o.age,
                gender = o.gender,
                species = o.species ?? string.Empty,
                alias = o.POO ?? string.Empty,
                avatar = o.OC_image_url,                // 可能为 null
                weaponImg = o.OC_WeapenImgUrl,          // 可能为 null
                background = o.background,
                character = o.character,
                colors = o.colors,
                OC_WeapenDesc = o.OC_WeapenDesc,
                OC_Current_Time = o.OC_Current_Time,
                POO = o.POO,
                ExtraDesc = o.ExtraDesc,
                abilityRaw = o.ability ?? string.Empty,
                experienceRaw = o.experience ?? "[]",
                createTime = o.createTime,
                updateTime = o.updateTime
            });

            var total = await baseQuery.CountAsync();
            var items = await baseQuery
                .OrderByDescending(x => x.updateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // helper to convert relative path to proxy url
            string ToProxyPath(string? rel)
            {
                if (string.IsNullOrWhiteSpace(rel)) return null;
                if (Uri.IsWellFormedUriString(rel, UriKind.Absolute)) return rel;
                return $"/api/OCBattle/imageproxy?path={Uri.EscapeDataString(rel)}";
            }

            var personas = items.Select(x =>
            {
                // parse tags/colors into array
                List<string> parsedTags = new();
                var rawTags = (x.colors ?? "").Trim();
                if (!string.IsNullOrEmpty(rawTags))
                {
                    if (rawTags.StartsWith("[") || rawTags.StartsWith("{"))
                    {
                        try { parsedTags = JsonSerializer.Deserialize<List<string>>(rawTags) ?? new List<string>(); }
                        catch { parsedTags = rawTags.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).Where(t => t != "").ToList(); }
                    }
                    else parsedTags = rawTags.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).Where(t => t != "").ToList();
                }

                // parse ability (JSON with freeText or plain text)
                string abilityText = null;
                try
                {
                    var a = (x.abilityRaw ?? "").Trim();
                    if (!string.IsNullOrEmpty(a) && (a.StartsWith("{") || a.StartsWith("[")))
                    {
                        using var doc = JsonDocument.Parse(a);
                        if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("freeText", out var ft))
                        {
                            abilityText = ft.GetString();
                        }
                        else
                        {
                            abilityText = doc.RootElement.ToString();
                        }
                    }
                    else abilityText = string.IsNullOrWhiteSpace(a) ? null : a;
                }
                catch
                {
                    abilityText = string.IsNullOrWhiteSpace(x.abilityRaw) ? null : x.abilityRaw;
                }

                // parse experience into string[] fallback
                string[] experienceArr = Array.Empty<string>();
                try
                {
                    if (!string.IsNullOrWhiteSpace(x.experienceRaw))
                    {
                        // try deserialize to string[] or int[] then stringify
                        try
                        {
                            var sa = JsonSerializer.Deserialize<string[]>(x.experienceRaw);
                            if (sa != null) experienceArr = sa;
                            else
                            {
                                var ia = JsonSerializer.Deserialize<int[]>(x.experienceRaw);
                                if (ia != null) experienceArr = ia.Select(i => i.ToString()).ToArray();
                            }
                        }
                        catch
                        {
                            experienceArr = (x.experienceRaw ?? "").Split(new[] { ',', ';', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
                        }
                    }
                }
                catch { experienceArr = Array.Empty<string>(); }

                return new
                {
                    id = x.id.ToString(),
                    ownerId = x.authorID.ToString(),
                    authorName = x.authorName,
                    name = x.name,
                    age = x.age,
                    gender = x.gender,
                    species = string.IsNullOrWhiteSpace(x.species) ? null : x.species,
                    alias = string.IsNullOrWhiteSpace(x.alias) ? null : x.alias,
                    // return proxy URLs so frontend can directly use them in <img src="...">
                    avatarUrl = string.IsNullOrWhiteSpace(x.avatar) ? null : ToProxyPath(x.avatar),
                    weaponImg = string.IsNullOrWhiteSpace(x.weaponImg) ? null : ToProxyPath(x.weaponImg),
                    background = string.IsNullOrWhiteSpace(x.background) ? null : x.background,
                    character = string.IsNullOrWhiteSpace(x.character) ? null : x.character,
                    tags = parsedTags,
                    OC_WeapenDesc = string.IsNullOrWhiteSpace(x.OC_WeapenDesc) ? null : x.OC_WeapenDesc,
                    OC_Current_Time = x.OC_Current_Time,
                    POO = string.IsNullOrWhiteSpace(x.POO) ? null : x.POO,
                    ExtraDesc = string.IsNullOrWhiteSpace(x.ExtraDesc) ? null : x.ExtraDesc,
                    ability = abilityText,
                    experience = experienceArr,
                    createdAt = x.createTime.ToString("o"),
                    updatedAt = x.updateTime.ToString("o")
                };
            }).ToList();

            if (tagList.Any())
            {
                personas = personas.Where(p => tagList.All(t => ((List<string>)p.tags).Contains(t))).ToList();
            }

            return Ok(new { total, page, pageSize, items = personas });
        }

    }
}