using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using THCY_BE.DataBase;
using THCY_BE.Models.UserDate;

namespace THCY_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DefaultController : ControllerBase
    {
        private readonly BasicInfoDbContext _context;

        public DefaultController(BasicInfoDbContext context)
        {
            _context = context;
        }

        // GET: api/default
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "太初寰宇API服务运行正常",
                timestamp = DateTime.Now,
                version = "1.0.0"
            });
        }

        // GET: api/default/health
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                // 测试数据库连接
                var canConnect = await _context.Database.CanConnectAsync();
                var userCount = await _context.UserAccounts.CountAsync();

                return Ok(new
                {
                    status = "healthy",
                    database = canConnect ? "connected" : "disconnected",
                    totalUsers = userCount,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "unhealthy",
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }


        // GET: api/default/users/count
        [HttpGet("users/count")]
        public async Task<ActionResult<object>> GetUserCount()
        {
            try
            {
                // 获取userdata表中的用户数量
                var userCount = await _context.UserDatas.CountAsync();

                return Ok(new
                {
                    count = userCount,
                    table = "userdata",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

    }
}