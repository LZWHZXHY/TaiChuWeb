using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using THCY_BE.DataBase;
using THCY_BE.Models.Chai;

namespace THCY_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 需携带 Bearer Token 才能访问
    public class ChaiSheTuanController : ControllerBase
    {
        private readonly ChaiDbContext _db;

        public ChaiSheTuanController(ChaiDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// 获取柴社团列表（基础信息）。
        /// 可选查询参数：
        /// - verify: 审核状态（0待审/1通过/2拒绝）
        /// - q: 关键字，匹配 name/leader
        /// - type: 类型过滤
        /// - sort: 排序字段（id_desc|name_asc|size_desc|verify_asc），默认 id_desc
        /// </summary>
        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<ChaiSheTuanBasicDto>>> List(
            [FromQuery] int? verify,
            [FromQuery] string? q,
            [FromQuery] int? type,
            [FromQuery] string? sort = "id_desc")
        {
            var queryable = _db.ChaiSheTuans.AsNoTracking().AsQueryable();

            if (verify is not null)
                queryable = queryable.Where(x => x.verify == verify);

            if (type is not null)
                queryable = queryable.Where(x => x.type == type);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var kw = $"%{q.Trim()}%";
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.name, kw) ||
                    EF.Functions.Like(x.leader, kw));
            }

            queryable = sort switch
            {
                "name_asc" => queryable.OrderBy(x => x.name).ThenByDescending(x => x.id),
                "size_desc" => queryable.OrderByDescending(x => x.size).ThenByDescending(x => x.id),
                "verify_asc" => queryable.OrderBy(x => x.verify).ThenByDescending(x => x.id),
                _ => queryable.OrderByDescending(x => x.id) // 默认：最新在前
            };

            var data = await queryable
                .Select(x => new ChaiSheTuanBasicDto
                {
                    id = x.id,
                    name = x.name,
                    leader = x.leader,
                    type = x.type,
                    test = x.test,
                    testlevel = x.testlevel,
                    url = x.url,
                    size = x.size,
                    desc = x.desc,
                    verify = x.verify
                })
                .ToListAsync();

            return Ok(data);
        }
    }

    // 基础信息 DTO（仅暴露前端显示所需字段）
    public class ChaiSheTuanBasicDto
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string leader { get; set; } = string.Empty;
        public int type { get; set; }
        public int test { get; set; }
        public int testlevel { get; set; }
        public string url { get; set; } = string.Empty;
        public int size { get; set; }
        public string desc { get; set; } = string.Empty;
        public int verify { get; set; }
    }
}