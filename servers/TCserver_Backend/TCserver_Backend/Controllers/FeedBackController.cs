using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TCserver_Backend.Data;
using TCserver_Backend.Dtos;
using TCserver_Backend.Models;
using TCserver_Backend.Services;


namespace TCserver_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedBackController: ControllerBase
    {
        private readonly FunctionDbContext _context;

        public FeedBackController(FunctionDbContext context)
        {
            _context = context;
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            var feedbacks = await _context.FeedBacks
                .OrderByDescending(f => f.createTime)
                .Take(50)
                .Select(f => new
                {
                    id = f.id,
                    type = f.type,
                    content = f.content,
                    status = f.status,
                    createTime = f.createTime.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToListAsync();

            return Ok(feedbacks);
        }

        [HttpPost]
        [Authorize] // 需要登录
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedBack feedback)
        {
            // 1. 从 Claims 读取用户id
            var userIdClaim = User.Claims.FirstOrDefault(
                c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == "userId");
            if (userIdClaim == null)
            {
                return Forbid("找不到用户身份信息，无法提交意见。");
            }
            var userId = userIdClaim.Value;

            // 2. 将 userId 转为 int
            if (!int.TryParse(userId, out int userIdInt))
            {
                return Forbid("用户ID格式不正确，无法提交意见。");
            }

            // 3. 查询数据库得到用户等级
            var user = await _context.userdata.FirstOrDefaultAsync(u => u.id == userIdInt);
            if (user == null)
            {
                return Forbid("无法找到用户信息，无法提交意见。");
            }
            if (user.level < 2)
            {
                return Forbid("您的等级未达到2级，无法提交意见。");
            }

            // 4. 校验反馈内容
            if (feedback == null || string.IsNullOrWhiteSpace(feedback.content) || feedback.type == 0)
            {
                return BadRequest("请填写完整的建议内容和类型。");
            }

            feedback.createTime = DateTime.UtcNow;
            feedback.status = 0; // 默认未处理

            _context.FeedBacks.Add(feedback);
            await _context.SaveChangesAsync();

            return Ok(new { message = "反馈提交成功" });
        }












    }
}
