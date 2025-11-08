using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BilibiliProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public BilibiliProxyController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("fans")]
        public async Task<IActionResult> GetFans()
        {
            var apiUrl = "https://api.bilibili.com/x/relation/stat?vmid=3546736970696727&web_location=333.1387";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "B站接口请求失败");
            }

            var json = await response.Content.ReadAsStringAsync();
            // 设置允许任何来源跨域（开发环境可用，生产建议指定域名）
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return Content(json, "application/json");
        }
    }
}