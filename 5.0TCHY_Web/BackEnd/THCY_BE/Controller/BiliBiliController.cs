using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace THCY_BE.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class BiliBiliController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BiliBiliController> _logger;

        private const int CacheTtlSeconds = 60 * 60; // 1 hour
        private const int MaxImageSize = 512 * 1024; // 512KB

        public BiliBiliController(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<BiliBiliController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? bvid, [FromQuery] string? aid, [FromQuery] string? url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bvid) && string.IsNullOrWhiteSpace(aid) && string.IsNullOrWhiteSpace(url))
                {
                    return BadRequest(new { success = false, message = "需要提供 bvid 或 aid 或 url 参数" });
                }

                if (string.IsNullOrWhiteSpace(bvid) && string.IsNullOrWhiteSpace(aid) && !string.IsNullOrWhiteSpace(url))
                {
                    bvid = ExtractBvidFromUrl(url);
                    aid = ExtractAidFromUrl(url);
                }

                if (string.IsNullOrWhiteSpace(bvid) && string.IsNullOrWhiteSpace(aid))
                {
                    return BadRequest(new { success = false, message = "无法从提供的 url 中解析出 bvid 或 aid" });
                }

                var cacheKey = bvid != null ? $"bili:info:bvid:{bvid}" : $"bili:info:aid:{aid}";
                if (_cache.TryGetValue<string>(cacheKey, out var cachedPic))
                {
                    return Ok(new { success = true, pic = cachedPic, source = "cache" });
                }

                var apiUrl = bvid != null
                    ? $"https://api.bilibili.com/x/web-interface/view?bvid={Uri.EscapeDataString(bvid)}"
                    : $"https://api.bilibili.com/x/web-interface/view?aid={Uri.EscapeDataString(aid!)}";

                var client = _httpClientFactory.CreateClient("bili-client");
                client.Timeout = TimeSpan.FromSeconds(8);

                using var resp = await client.GetAsync(apiUrl);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Bilibili API 返回非 2xx: {Status}", resp.StatusCode);
                    return StatusCode(502, new { success = false, message = "B站返回错误" });
                }

                using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                if (doc.RootElement.TryGetProperty("data", out var dataEl)
                    && dataEl.TryGetProperty("pic", out var picEl)
                    && picEl.ValueKind == JsonValueKind.String)
                {
                    var pic = picEl.GetString()!;
                    _cache.Set(cacheKey, pic, TimeSpan.FromSeconds(CacheTtlSeconds));
                    return Ok(new { success = true, pic, source = "bili" });
                }

                return NotFound(new { success = false, message = "未找到封面" });
            }
            catch (TaskCanceledException)
            {
                return StatusCode(504, new { success = false, message = "请求 B 站超时" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 B 站封面失败");
                return StatusCode(500, new { success = false, message = "获取封面失败" });
            }
        }

        [AllowAnonymous]
        [HttpGet("cover")]
        public async Task<IActionResult> Cover([FromQuery] string? url, [FromQuery] int? w = 400, [FromQuery] int? h = 250, [FromQuery] int? q = 80)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest(new { success = false, message = "需要提供 url 参数" });

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return BadRequest(new { success = false, message = "无效的 url" });

            var host = uri.Host.ToLowerInvariant();

            // 白名单检查
            var allowedHosts = new[] { "hdslb.com", "bilivideo.com","localhost","127.0.0.1" };
            var hostAllowed = false;
            foreach (var allowedHost in allowedHosts)
            {
                if (host.EndsWith(allowedHost))
                {
                    hostAllowed = true;
                    break;
                }
            }

            if (!hostAllowed)
            {
                _logger.LogWarning("拒绝代理非白名单主机图片请求: {Url}", url);
                return BadRequest(new { success = false, message = "不允许的图片主机" });
            }

            // 参数验证和优化
            var width = Math.Min(Math.Max(w.Value, 50), 800);
            var height = Math.Min(Math.Max(h.Value, 50), 600);
            var quality = Math.Min(Math.Max(q.Value, 30), 95);

            var cacheKey = $"bili:image:{width}x{height}q{quality}:{url}";

            if (_cache.TryGetValue(cacheKey, out CachedImage cached))
            {
                return File(cached.Data, cached.ContentType);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("bili-client");
                var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Referrer = new Uri("https://www.bilibili.com/");

                using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
                if (!resp.IsSuccessStatusCode)
                {
                    return StatusCode((int)resp.StatusCode);
                }

                var contentType = resp.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

                // 检查响应大小
                var contentLength = resp.Content.Headers.ContentLength;
                if (contentLength > MaxImageSize)
                {
                    _logger.LogWarning("图片过大: {Size} bytes", contentLength);
                }

                await using var sourceStream = await resp.Content.ReadAsStreamAsync();
                await using var tempStream = new MemoryStream();
                await sourceStream.CopyToAsync(tempStream);

                var imageData = tempStream.ToArray();

                // 简单缓存，不进行图片处理以降低服务器负载
                var cachedImage = new CachedImage { Data = imageData, ContentType = contentType };
                _cache.Set(cacheKey, cachedImage, TimeSpan.FromSeconds(CacheTtlSeconds));

                return File(imageData, contentType);
            }
            catch (TaskCanceledException)
            {
                return StatusCode(504, new { success = false, message = "请求超时" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "代理图片失败");
                return StatusCode(500, new { success = false, message = "代理失败" });
            }
        }

        private string? ExtractBvidFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            var bvMatch = Regex.Match(url, @"(BV[0-9A-Za-z]{2,})", RegexOptions.IgnoreCase);
            return bvMatch.Success ? bvMatch.Groups[1].Value : null;
        }

        private string? ExtractAidFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            var aidMatch = Regex.Match(url, @"(?:aid=)(\d+)", RegexOptions.IgnoreCase);
            if (aidMatch.Success) return aidMatch.Groups[1].Value;
            var avMatch = Regex.Match(url, @"(?:av)(\d+)", RegexOptions.IgnoreCase);
            return avMatch.Success ? avMatch.Groups[1].Value : null;
        }

        private record CachedImage
        {
            public byte[] Data { get; init; } = Array.Empty<byte>();
            public string ContentType { get; init; } = "image/jpeg";
        }
    }
}