using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using THCY_BE.DataBase;
using THCY_BE.Models.Chai;

namespace THCY_BE.Controller.Chai
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 必须携带 Bearer token
    public class ChaiLianHeController : ControllerBase
    {
        private readonly ChaiDbContext _db;
        private readonly ILogger<ChaiLianHeController> _logger;

        public ChaiLianHeController(ChaiDbContext db, ILogger<ChaiLianHeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET api/ChaiLianHe/list
        // 支持 verify/type/q/page/pageSize
        [HttpGet("list")]
        public async Task<ActionResult> List(
            [FromQuery] int? verify,
            [FromQuery] string? q,
            [FromQuery] int? type,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 500) pageSize = 20;

            var dbSet = _db.Set<ChaiLianHe>().AsNoTracking();

            var query = dbSet.AsQueryable();

            if (verify is not null)
                query = query.Where(x => x.verify == verify.Value);

            if (type is not null)
                query = query.Where(x => x.type == type.Value);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var kw = $"%{q.Trim()}%";
                query = query.Where(x =>
                    EF.Functions.Like(x.name, kw) ||
                    EF.Functions.Like(x.host, kw) ||
                    EF.Functions.Like(x.desc, kw) ||
                    EF.Functions.Like(x.require, kw));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ChaiLianHeDto
                {
                    id = x.id,
                    name = x.name,
                    host = x.host,
                    type = x.type,
                    startdate = x.startdate,
                    enddate = x.enddate,
                    desc = x.desc,
                    QQgroup = x.QQgroup,
                    rules = x.require,
                    verify = x.verify,
                    report = x.report
                })
                .ToListAsync();

            return Ok(new { data = items, total });
        }

        // GET api/ChaiLianHe/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult> Get(int id)
        {
            var dbSet = _db.Set<ChaiLianHe>().AsNoTracking();

            var item = await dbSet
                .Where(x => x.id == id)
                .Select(x => new ChaiLianHeDto
                {
                    id = x.id,
                    name = x.name,
                    host = x.host,
                    type = x.type,
                    startdate = x.startdate,
                    enddate = x.enddate,
                    desc = x.desc,
                    QQgroup = x.QQgroup,
                    rules = x.require,
                    verify = x.verify,
                    report = x.report
                })
                .FirstOrDefaultAsync();

            if (item == null) return NotFound(new { message = "未找到该联合活动" });

            return Ok(item);
        }

        // POST api/ChaiLianHe/create
        [HttpPost("create")]
        public async Task<ActionResult> Create([FromBody] CreateChaiLianHeRequest req)
        {
            if (!ModelState.IsValid)
            {
                var firstErr = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault();
                return BadRequest(new { message = firstErr ?? "参数错误" });
            }

            try
            {
                var entity = new ChaiLianHe
                {
                    name = req.name.Trim(),
                    host = req.host.Trim(),
                    type = req.type,
                    startdate = req.startdate,
                    enddate = req.enddate,
                    desc = req.desc?.Trim() ?? string.Empty,
                    QQgroup = req.QQgroup?.Trim() ?? string.Empty,
                    require = req.rules?.Trim() ?? string.Empty,
                    verify = 0, // 默认待审
                    report = 0
                };

                _db.Add(entity);
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(Get), new { entity.id }, new { success = true, entity.id });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "创建 ChaiLianHe 失败");
                return StatusCode(500, new { message = "服务器内部错误：无法创建记录" });
            }
        }

        // PUT api/ChaiLianHe/{id}
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateChaiLianHeRequest req)
        {
            if (!ModelState.IsValid)
            {
                var firstErr = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault();
                return BadRequest(new { message = firstErr ?? "参数错误" });
            }

            var entity = await _db.Set<ChaiLianHe>().FindAsync(id);
            if (entity == null) return NotFound(new { message = "未找到该联合活动" });

            // 更新允许的字段（若传 null 则保持原值）
            if (!string.IsNullOrWhiteSpace(req.name)) entity.name = req.name.Trim();
            if (!string.IsNullOrWhiteSpace(req.host)) entity.host = req.host.Trim();
            if (req.type.HasValue) entity.type = req.type.Value;
            if (req.startdate.HasValue) entity.startdate = req.startdate.Value;
            if (req.enddate.HasValue) entity.enddate = req.enddate;
            if (req.desc != null) entity.desc = req.desc;
            if (req.QQgroup != null) entity.QQgroup = req.QQgroup;
            if (req.rules != null) entity.require = req.rules;

            try
            {
                await _db.SaveChangesAsync();
                return Ok(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "更新 ChaiLianHe 失败 id={Id}", id);
                return StatusCode(500, new { message = "服务器内部错误：无法保存更改" });
            }
        }

        // DELETE api/ChaiLianHe/{id}
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var entity = await _db.Set<ChaiLianHe>().FindAsync(id);
            if (entity == null) return NotFound(new { message = "未找到该联合活动" });

            _db.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Moderation endpoints - Admin only
        [HttpPost("moderation/approve")]
       
        public async Task<ActionResult> Approve([FromBody] ModerationActionRequest req)
        {
            if (req == null || req.Id <= 0) return BadRequest(new { message = "参数错误" });

            var item = await _db.Set<ChaiLianHe>().FindAsync(req.Id);
            if (item == null) return NotFound(new { message = "未找到该联合活动" });

            item.verify = 1;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Admin {admin} approved ChaiLianHe {id}", User?.Identity?.Name ?? "unknown", req.Id);
            return Ok(new { success = true });
        }

        [HttpPost("moderation/reject")]
        
        public async Task<ActionResult> Reject([FromBody] ModerationActionRequest req)
        {
            if (req == null || req.Id <= 0) return BadRequest(new { message = "参数错误" });

            var item = await _db.Set<ChaiLianHe>().FindAsync(req.Id);
            if (item == null) return NotFound(new { message = "未找到该联合活动" });

            item.verify = 2;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Admin {admin} rejected ChaiLianHe {id}. Note: {note}", User?.Identity?.Name ?? "unknown", req.Id, req.Note);
            return Ok(new { success = true });
        }

        // DTOs / Request models
        public class ChaiLianHeDto
        {
            public int id { get; set; }
            public string name { get; set; } = string.Empty;
            public string host { get; set; } = string.Empty;
            public int type { get; set; }
            public DateTime startdate { get; set; }
            public DateTime? enddate { get; set; }
            public string desc { get; set; } = string.Empty;
            public string QQgroup { get; set; } = string.Empty;
            public string rules { get; set; } = string.Empty;
            public int verify { get; set; }
            public int report { get; set; }
        }

        public class CreateChaiLianHeRequest
        {
            [Required, MaxLength(300)]
            public string name { get; set; } = string.Empty;

            [Required, MaxLength(200)]
            public string host { get; set; } = string.Empty;

            [Required]
            public int type { get; set; }

            [Required]
            public DateTime startdate { get; set; }

            public DateTime? enddate { get; set; }

            [MaxLength(2000)]
            public string? desc { get; set; }

            [MaxLength(128)]
            public string? QQgroup { get; set; }

            [MaxLength(2000)]
            public string? rules { get; set; }
        }

        public class UpdateChaiLianHeRequest
        {
            [MaxLength(300)]
            public string? name { get; set; }

            [MaxLength(200)]
            public string? host { get; set; }

            public int? type { get; set; }

            public DateTime? startdate { get; set; }

            public DateTime? enddate { get; set; }

            [MaxLength(2000)]
            public string? desc { get; set; }

            [MaxLength(128)]
            public string? QQgroup { get; set; }

            [MaxLength(2000)]
            public string? rules { get; set; }
        }

        public class ModerationActionRequest
        {
            public int Id { get; set; }
            public string? Note { get; set; }
        }
    }
}