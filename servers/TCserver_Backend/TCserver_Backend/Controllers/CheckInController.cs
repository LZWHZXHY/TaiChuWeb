using Microsoft.AspNetCore.Mvc;
using TCserver_Backend.Data;
using TCserver_Backend.Models;

namespace TCserver_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckInController : ControllerBase
    {
        private readonly CheckInDateDbContext _context;

        public CheckInController(CheckInDateDbContext context)
        {
            _context = context;
        }


        //操作内容

        [HttpGet]
        public IActionResult GetCheckIns([FromQuery] int userId)
        {
            // 检查 userId 是否存在
            var userExists = _context.UserAccounts.Any(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound(new { message = "用户不存在" });
            }

            // 查询打卡记录
            var checkins = _context.UserCheckinRecords
                .Where(c => c.UserId == userId)
                .ToList();

            return Ok(checkins);
        }



        [HttpPost]
        public IActionResult CheckIn([FromBody] CheckInRequest req)
        {
            var userId = req.UserId;

            var user = _context.UserDatas.FirstOrDefault(u => u.id == userId);
            if (user == null)
            {
                return NotFound(new { message = "用户不存在" });
            }

            var today = DateTime.Now.Date;
            var alreadyCheckedIn = _context.UserCheckinRecords
                .Any(c => c.UserId == userId && c.CheckinDate.Date == today);

            if (alreadyCheckedIn)
            {
                return BadRequest(new { message = "今天已经打卡" });
            }

            var record = new UserCheckinRecord
            {
                UserId = userId,
                CheckinDate = today,
                Type = 1
            };
            _context.UserCheckinRecords.Add(record);

            // 加经验
            user.exp += 10;

            // 升级逻辑
            while (user.exp >= ExpRequiredForLevel(user.level))
            {
                user.exp -= ExpRequiredForLevel(user.level);
                user.level += 1;
            }

            _context.SaveChanges();

            return Ok(new
            {
                message = "打卡成功，经验+10",
                newExp = user.exp,
                newLevel = user.level
            });
        }

        private int ExpRequiredForLevel(int level)
        {
            return level * 10 + 50;
        }


    }
    public class CheckInRequest
    {
        public int UserId { get; set; }
    }

}
