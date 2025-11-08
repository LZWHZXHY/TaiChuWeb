using Microsoft.AspNetCore.Mvc;
using TCserver_Backend.Data;
using TCserver_Backend.Models;
using Microsoft.Extensions.Logging; // 添加这个命名空间



namespace TCserver_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChailianheController : ControllerBase
    {
        private readonly ChailianheDbContext _context;
        private readonly ILogger<ChailianheController> _logger; // 添加日志记录器字段
        public ChailianheController(
            ChailianheDbContext context)
            //ILogger<ChailianheController> logger) // 注入日志记录器
        {
            _context = context;
            //_logger = logger;      // 初始化日志记录器
        }



        //操作内容
        [HttpGet]
        public IActionResult GetAll()
        {
            Console.WriteLine("联合测试");
            var data = _context.Chailianhes.ToList();
            return Ok(data);
        }

        //用户可以申请投稿新的联合
        [HttpPost("ApplyLianHe")]
        public IActionResult ApplyLianHe([FromBody] chailianhe data)
        {
            // 添加日志记录
            //_logger.LogInformation("收到申请数据: {@Data}", data);

            try
            {
                if (data == null)
                {
                    _logger.LogWarning("接收到空数据");
                    return BadRequest("请求数据为空");
                }

                // 手动验证模型
                var missingFields = new List<string>();
                if (string.IsNullOrEmpty(data.name)) missingFields.Add("name");
                if (string.IsNullOrEmpty(data.host)) missingFields.Add("host");
                if (data.type == 0) missingFields.Add("type"); // 0 是无效类型
                if (data.startdate == default) missingFields.Add("startdate");

                if (missingFields.Any())
                {
                    _logger.LogWarning("必填字段缺失: {Fields}", missingFields);
                    return BadRequest($"缺少必填字段: {string.Join(", ", missingFields)}");
                }

                // 确保所有字段有默认值
                data.verify = 0;
                data.report = 0;
                data.desc ??= string.Empty;
                data.QQgroup ??= string.Empty;
                data.require ??= string.Empty;

                // 保存到数据库
                _context.Chailianhes.Add(data);
                int changes = _context.SaveChanges();

                return changes > 0
                    ? Ok(new { success = true, message = "申请成功", id = data.id })
                    : StatusCode(500, "数据保存失败");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存申请时出错");
                return StatusCode(500, new
                {
                    error = "服务器内部错误",
                    exception = ex.Message
                });
            }
        }

    }
}
