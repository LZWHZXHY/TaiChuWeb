using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using TCserver_Backend.Data;
using TCserver_Backend.Dtos;

namespace TCserver_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly FunctionDbContext _context;
        public InventoryController(FunctionDbContext context)
        {
            _context = context;
        }

        [HttpPost("use-rename-card")]
        [Authorize]
        public async Task<IActionResult> UseRenameCard([FromBody] RenameRequest req)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized("用户未登录");

            // 1. 校验：1-25个任意字符
            if (string.IsNullOrWhiteSpace(req.NewUsername) || req.NewUsername.Length < 1 || req.NewUsername.Length > 25)
                return BadRequest("用户名长度需为1-25个字符");

            // 2. 检查用户名是否已存在
            if (await _context.useraccount.AnyAsync(u => u.username == req.NewUsername))
                return BadRequest("用户名已被占用");

            // 3. 检查是否有对应编号的改名卡
            var renameCard = await _context.UserInventories
                .FirstOrDefaultAsync(x => x.userId == userId && x.itemId == req.ItemId && x.count > 0);

            if (renameCard == null)
                return BadRequest("你没有该编号的改名卡");

            // 4. 扣除改名卡
            renameCard.count -= 1;

            // 新增：如果用完了就删除
            if (renameCard.count == 0)
            {
                _context.UserInventories.Remove(renameCard);
            }

            // 5. 修改用户名
            var user = await _context.useraccount.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return NotFound("用户不存在");

            user.username = req.NewUsername;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "改名成功" });
        }


    }
}