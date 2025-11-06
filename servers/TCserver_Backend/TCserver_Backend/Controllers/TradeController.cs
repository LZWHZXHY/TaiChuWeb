using Microsoft.AspNetCore.Mvc;
using TCserver_Backend.Data;
using TCserver_Backend.Models.Trade;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TCserver_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TradeController : ControllerBase
    {
        private readonly FunctionDbContext _db;

        public TradeController(FunctionDbContext db)
        {
            _db = db;
        }

        // 获取商品列表
        [HttpGet("items")]
        public async Task<IActionResult> GetShopItems()
        {
            var items = await _db.ShopItems
                .Where(i => i.enable == 1)
                .OrderBy(i => i.category)
                .Select(i => new {
                    i.id,
                    i.name,
                    i.desc,
                    i.icon,
                    i.cost,
                    i.category,
                    i.stock,
                    level = i.level,             // 新增
                    requireLevel = i.requireLevel// 新增
                })
                .ToListAsync();
            return Ok(items);
        }

        // 查询用户已拥有的物品
        [Authorize]
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            // 获取当前登录用户ID，假设用 int.Parse，实际视你的认证方式，也可以直接 int 类型
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized("未登录或用户ID错误");

            var inventory = await _db.UserInventories
                .Where(inv => inv.userId == userId && inv.status == 0)
                .Include(inv => inv.shopItem) // 加载商品详情
                .Select(inv => new {
                    inv.id,
                    inv.itemId,
                    inv.count,
                    inv.acquireTime,
                    inv.status,
                    name = inv.shopItem.name,
                    desc = inv.shopItem.desc,
                    icon = inv.shopItem.icon,
                    category = inv.shopItem.category,
                    level = inv.shopItem.level,
                    requireLevel = inv.shopItem.requireLevel
                })
                .ToListAsync();

            return Ok(inventory);
        }

        [Authorize]
        [HttpPost("buy")]
        public async Task<IActionResult> Buy([FromBody] BuyRequest req)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized("未登录");

            if (req.Count <= 0) req.Count = 1;

            // 查找商品
            var item = await _db.ShopItems.FirstOrDefaultAsync(i => i.id == req.ItemId && i.enable == 1);
            if (item == null) return NotFound("商品不存在或已下架");

            // 查找用户
            var user = await _db.userdata.FirstOrDefaultAsync(u => u.id == userId);
            if (user == null) return Unauthorized("用户不存在");

            // 新增：校验用户等级
            if (item.requireLevel.HasValue && user.level < item.requireLevel.Value)
                return BadRequest($"您的账户等级不足，兑换该道具需等级{item.requireLevel.Value}，当前等级{user.level}");

            // 校验积分
            var totalCost = item.cost * req.Count;
            if (user.points < totalCost)
                return BadRequest("间隙粒子不足");

            // 校验库存
            if (item.stock != -1 && item.stock < req.Count)
                return BadRequest("库存不足");

            // 扣除积分
            user.points -= totalCost;

            // 扣除库存（无限则不变）
            if (item.stock != -1)
                item.stock -= req.Count;

            // 增加/累加用户库存
            var inventory = await _db.UserInventories
                .Where(inv => inv.userId == userId && inv.itemId == req.ItemId && inv.status == 0)
                .FirstOrDefaultAsync();

            if (inventory == null)
            {
                _db.UserInventories.Add(new UserInventory
                {
                    userId = userId,
                    itemId = req.ItemId,
                    count = req.Count,
                    acquireTime = DateTime.Now,
                    status = 0
                });
            }
            else
            {
                inventory.count += req.Count;
            }

            // 写入订单表
            _db.ShopOrders.Add(new ShopOrder
            {
                userId = userId,
                itemId = req.ItemId,
                count = req.Count,
                cost = totalCost,
                createTime = DateTime.Now,
                status = 1
            });

            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "兑换成功" });
        }

        public class BuyRequest
        {
            public int ItemId { get; set; }
            public int Count { get; set; } = 1;
        }


        [Authorize]
        [HttpGet("points")]
        public async Task<IActionResult> GetMyPoints()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized("无效用户");

            var user = await _db.userdata.FirstOrDefaultAsync(u => u.id == userId);
            if (user == null)
                return NotFound("用户不存在");

            return Ok(new { points = user.points });
        }

        [Authorize]
        [HttpGet("userinfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized("无效用户");

            var user = await _db.userdata.FirstOrDefaultAsync(u => u.id == userId);
            if (user == null)
                return NotFound("用户不存在");

            return Ok(new { level = user.level, id = user.id });
        }

    }
}