using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TCserver_Backend.Data;
using TCserver_Backend.Models;



namespace TCserver_Backend.Controllers
{
    [ApiController]
    [Authorize] // 要求认证（除非某些 Action 标记 [AllowAnonymous]）
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly FunctionDbContext functionDB;

        public UserController(FunctionDbContext functionDB)
        {
            this.functionDB = functionDB;
        }


        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserData()
        {
            try
            {
                var claim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out int currentUserId))
                {
                    return Unauthorized(new { message = "未登录或用户标识无效" });
                }

                var userData = await functionDB.userdata
                    .Where(u => u.id == currentUserId)
                    .Select(u => new
                    {
                        id = u.id,
                        points = u.points,
                        level = u.level,
                        exp = u.exp,
                        title = u.title,
                        lastlogin = u.lastlogin,
                        logo = u.logo,
                        background = u.background,
                        likes = u.likes,
                        last_active_time = u.last_active_time,
                        byd = u.byd,
                        creater = u.creater,
                        username = u.useraccount.username,
                        email = u.useraccount.email_address
                    })
                    .FirstOrDefaultAsync();

                if (userData == null)
                    return NotFound(new { message = "用户数据不存在" });

                return Ok(userData);
            }
            catch (Exception ex)
            {
                // 记录日志，返回通用错误
                Console.WriteLine($"GetCurrentUserData error: {ex}");
                return StatusCode(500, new { message = "获取用户数据失败" });
            }
        }

        [HttpPost("deduct")]
        public async Task<IActionResult> Deduct([FromQuery] int amount)
        {
            if (amount <= 0) return BadRequest(new { message = "扣减金额必须大于 0" });

            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int userId))
            {
                return Unauthorized(new { message = "未登录或用户标识无效" });
            }

            // 只查询 points 字段（轻量检查）
            var pointsEntry = await functionDB.userdata
                .Where(u => u.id == userId)
                .Select(u => new { u.points })
                .FirstOrDefaultAsync();

            if (pointsEntry == null)
            {
                return NotFound(new { message = "用户不存在" });
            }

            // 兼容 nullable int? 与 int
            int currentPoints = pointsEntry.points;

            if (currentPoints < amount)
            {
                return BadRequest(new { message = "积分不足，无法扣除" });
            }

            // 计算新的余额
            int newPoints = currentPoints - amount;

            // 在事务中重新加载实体并更新（EF 跟踪后 SaveChanges 会持久化）
            using var tx = await functionDB.Database.BeginTransactionAsync();
            try
            {
                var user = await functionDB.userdata.FirstOrDefaultAsync(u => u.id == userId);
                if (user == null)
                {
                    await tx.RollbackAsync();
                    return NotFound(new { message = "用户不存在" });
                }

                // 将新的值写回实体
                user.points = newPoints; // 如果 model 是 int? 也可以赋 int，新值会转换

                // EF Core 会跟踪这个实体，SaveChanges 会把修改写入数据库
                await functionDB.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(new { message = "扣减成功", deducted = amount, remaining = user.points });
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();
                return StatusCode(409, new { message = "并发冲突，请重试" });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                Console.WriteLine($"Deduct error: {ex}");
                return StatusCode(500, new { message = "扣减失败" });
            }
        }

    }
}
