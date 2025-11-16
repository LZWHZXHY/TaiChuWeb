using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using THCY_BE.DataBase;
using THCY_BE.Models.UserDate;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

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

        // GET: api/default/user/me - 获取当前登录用户信息（需要Token）
        [Authorize]
        [HttpGet("user/me")]
        public async Task<ActionResult<object>> GetCurrentUser()
        {
            try
            {
                // 从Token中获取用户ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { error = "无效的Token" });
                }

                // 联合查询UserAccount和UserData表
                var user = await (from account in _context.UserAccounts
                                  join data in _context.UserDatas on account.Id equals data.id
                                  where account.Id == userId
                                  select new
                                  {
                                      // 来自UserAccount的信息
                                      id = account.Id,
                                      name = account.username,
                                      email = account.email_address,
                                      registerDate = account.date,
                                      state = account.state,
                                      rank = account.rank,
                                      isVerified = account.is_verified,

                                      // 来自UserData的信息
                                      points = data.points,
                                      level = data.level,
                                      exp = data.exp,
                                      title = data.title,
                                      lastLogin = data.lastlogin,
                                      avatar = data.logo,
                                      background = data.background,
                                      likes = data.likes,
                                      lastActiveTime = data.last_active_time,
                                      byd = data.byd,
                                      creater = data.creater
                                  })
                                 .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { error = "用户不存在" });
                }

                return Ok(new
                {
                    success = true,
                    data = user,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "获取用户信息失败",
                    message = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

        // GET: api/default/user/{id} - 获取指定用户信息（公开信息）
        [HttpGet("user/{id}")]
        public async Task<ActionResult<object>> GetUserById(int id)
        {
            try
            {
                var user = await (from account in _context.UserAccounts
                                  join data in _context.UserDatas on account.Id equals data.id
                                  where account.Id == id && account.state != 2 // 排除被封禁的用户
                                  select new
                                  {
                                      // 公开信息
                                      id = account.Id,
                                      name = account.username,
                                      registerDate = account.date,
                                      state = account.state,
                                      rank = account.rank,
                                      isVerified = account.is_verified,

                                      // 来自UserData的公开信息
                                      level = data.level,
                                      title = data.title,
                                      avatar = data.logo,
                                      background = data.background,
                                      likes = data.likes,
                                      lastActiveTime = data.last_active_time,
                                      byd = data.byd,
                                      creater = data.creater,
                                      points = data.points,
                                      exp = data.exp,
                                      lastLogin = data.lastlogin
                                  })
                                 .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        error = "用户不存在或已被封禁",
                        timestamp = DateTime.Now
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = user,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "获取用户信息失败",
                    message = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

       
        

        // 原有的健康检查接口保持不变
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
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

        [HttpGet("users/count")]
        public async Task<ActionResult<object>> GetUserCount()
        {
            try
            {
                var userCount = await _context.UserAccounts.CountAsync();

                return Ok(new
                {
                    count = userCount,
                    table = "useraccount",
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