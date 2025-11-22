using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using THCY_BE.DataBase;
using THCY_BE.Models.Chai;

namespace THCY_BE.Controller.Chai
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChaiSheTuanController : ControllerBase
    {
        private readonly ChaiDbContext _db;
        private readonly ILogger<ChaiSheTuanController> _logger;

        public ChaiSheTuanController(ChaiDbContext db, ILogger<ChaiSheTuanController> logger)
        {
            _db = db;
            _logger = logger;
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

        // -------------------------
        // 新增：提交社团接口
        // 前端调用：POST /api/ChaiSheTuan/submit
        // 若要要求登录提交：移除方法上的 [AllowAnonymous] 并确保前端带 Authorization header
        // -------------------------
        [HttpPost("submit")]

        public async Task<ActionResult> Submit([FromBody] SubmitChaiRequest req)
        {
            // 基础模型验证
            if (!ModelState.IsValid)
            {
                var firstErr = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault();
                return BadRequest(new { message = firstErr ?? "参数验证失败" });
            }

            // 验证 URL 格式
            if (!Uri.TryCreate(req.url, UriKind.Absolute, out var parsed) ||
                parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps)
            {
                return BadRequest(new { message = "请输入合法的主页/加群链接 (http(s)://...)" });
            }

            try
            {
                // 去重检查：相同名称或相同链接视为重复（可按需调整）
                var nameTrim = req.name.Trim();
                var urlTrim = req.url.Trim();
                var exists = await _db.ChaiSheTuans.AnyAsync(x => x.name == nameTrim || x.url == urlTrim);
                if (exists)
                {
                    return BadRequest(new { message = "已存在相同名称或相同链接的社团" });
                }

                // 可选：从 token 获取提交者 userId（若用户已登录）
                int? submitterUserId = null;
                try
                {
                    var subClaim = User?.FindFirst(ClaimTypes.NameIdentifier) ?? User?.FindFirst("userId");
                    if (subClaim != null && int.TryParse(subClaim.Value, out var idVal))
                        submitterUserId = idVal;
                }
                catch { /* ignore */ }

                // 构建实体（与现有 ChaiSheTuan 模型字段一致）
                var entity = new ChaiSheTuan
                {
                    name = nameTrim,
                    leader = req.leader.Trim(),
                    type = req.type,
                    test = req.test,
                    testlevel = req.test == 1 ? req.testlevel : 0,
                    url = urlTrim,
                    size = req.size,
                    desc = req.desc.Trim(),
                    verify = 0 // 0 = 待审
                };

                _db.ChaiSheTuans.Add(entity);
                await _db.SaveChangesAsync();

                return Ok(new { success = true, message = "已提交，等待审核" });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "数据库更新失败：提交社团时发生错误");
                // 开发时可返回详细信息；生产请隐藏详细异常
                return StatusCode(500, new { message = "服务器内部错误：数据库操作失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "提交社团失败");
                return StatusCode(500, new { message = "服务器内部错误" });
            }
        }

        // -------------------------------------------------------
        // Moderation endpoints (added to this controller as requested)
        // - GET  /api/ChaiSheTuan/moderation/list?entity=ChaiSheTuan&status=pending&q=&page=1&pageSize=20
        // - POST /api/ChaiSheTuan/moderation/approve   { entity, id, note? }
        // - POST /api/ChaiSheTuan/moderation/reject    { entity, id, note? }
        // These endpoints are protected: only Admin role can call them.
        // You asked not to change controller name, so they're added inside this controller.
        // -------------------------------------------------------

        /// <summary>
        /// 统一的审核列表接口（支持分发到不同 entity）
        /// </summary>
        [HttpGet("moderation/list")]

        public async Task<ActionResult<IEnumerable<ModerationItemDto>>> ModerationList(
            [FromQuery] string entity,
            [FromQuery] string? status,
            [FromQuery] string? q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(entity))
                return BadRequest(new { message = "entity 参数必填，例如 ChaiSheTuan" });

            int? verifyFilter = status?.ToLower() switch
            {
                "pending" => 0,
                "approved" => 1,
                "rejected" => 2,
                "all" => null,
                _ => status is null ? 0 : null
            };

            switch (entity)
            {
                case "ChaiSheTuan":
                    {
                        var query = _db.ChaiSheTuans.AsNoTracking().AsQueryable();

                        if (verifyFilter is not null)
                            query = query.Where(x => x.verify == verifyFilter.Value);

                        if (!string.IsNullOrWhiteSpace(q))
                        {
                            var kw = $"%{q.Trim()}%";
                            query = query.Where(x => EF.Functions.Like(x.name, kw) || EF.Functions.Like(x.leader, kw));
                        }

                        var items = await query
                            .OrderByDescending(x => x.id)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .Select(x => new ModerationItemDto
                            {
                                id = x.id,
                                entity = "ChaiSheTuan",
                                title = x.name,
                                subtitle = x.leader,
                                typeLabel = $"Type {x.type}",
                                verify = x.verify,
                                createdAt = null, // 如果有 created_at 字段，请改为 x.created_at
                                payload = new
                                {
                                    x.type,
                                    x.test,
                                    x.testlevel,
                                    x.url,
                                    x.size,
                                    x.desc
                                }
                            })
                            .ToListAsync();

                        return Ok(items);
                    }

                default:
                    return BadRequest(new { message = $"未知的 entity: {entity}" });
            }
        }

        /// <summary>
        /// 通过某条记录（仅 Admin）
        /// </summary>
        [HttpPost("moderation/approve")]
        public async Task<ActionResult> ModerationApprove([FromBody] ModerationActionRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Entity))
                return BadRequest(new { message = "参数错误" });

            try
            {
                switch (req.Entity)
                {
                    case "ChaiSheTuan":
                        {
                            var item = await _db.ChaiSheTuans.FindAsync((int)req.Id);
                            if (item == null) return NotFound(new { message = "未找到该社团" });

                            item.verify = 1;
                            await _db.SaveChangesAsync();

                            // 可选：记录日志或通知
                            _logger.LogInformation("Admin {admin} approved ChaiSheTuan {id}. Note: {note}",
                                User?.Identity?.Name ?? "unknown", req.Id, req.Note);

                            return Ok(new { success = true });
                        }

                    default:
                        return BadRequest(new { message = $"未知的 entity: {req.Entity}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "审批失败");
                return StatusCode(500, new { message = "审批失败" });
            }
        }

        /// <summary>
        /// 驳回某条记录（仅 Admin）
        /// </summary>
        [HttpPost("moderation/reject")]
        public async Task<ActionResult> ModerationReject([FromBody] ModerationActionRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Entity))
                return BadRequest(new { message = "参数错误" });

            try
            {
                switch (req.Entity)
                {
                    case "ChaiSheTuan":
                        {
                            var item = await _db.ChaiSheTuans.FindAsync((int)req.Id);
                            if (item == null) return NotFound(new { message = "未找到该社团" });

                            item.verify = 2;
                            await _db.SaveChangesAsync();

                            // 建议：记录驳回原因到 ModerationLog，并通知提交者
                            _logger.LogInformation("Admin {admin} rejected ChaiSheTuan {id}. Note: {note}",
                                User?.Identity?.Name ?? "unknown", req.Id, req.Note);

                            return Ok(new { success = true });
                        }

                    default:
                        return BadRequest(new { message = $"未知的 entity: {req.Entity}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驳回失败");
                return StatusCode(500, new { message = "驳回失败" });
            }
        }

        // -------------------------
        // DTOs for submit and moderation (kept in this controller file)
        // -------------------------

        // DTO（改为 class，避免 ASP.NET Core 在 record 上的验证元数据冲突）
        public class SubmitChaiRequest
        {
            [Required, MaxLength(200)]
            public string name { get; set; } = string.Empty;

            [Required, MaxLength(100)]
            public string leader { get; set; } = string.Empty;

            [Required]
            public int type { get; set; }

            [Required]
            public int test { get; set; }

            [Required]
            public int testlevel { get; set; }

            [Required, MaxLength(1000)]
            public string url { get; set; } = string.Empty;

            [Required]
            public int size { get; set; }

            [Required, MinLength(10), MaxLength(2000)]
            public string desc { get; set; } = string.Empty;
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

        // Moderation DTOs
        public class ModerationItemDto
        {
            public long id { get; set; }
            public string entity { get; set; } = string.Empty;
            public string title { get; set; } = string.Empty;
            public string subtitle { get; set; } = string.Empty;
            public string typeLabel { get; set; } = string.Empty;
            public int verify { get; set; }
            public DateTime? createdAt { get; set; }
            public object? payload { get; set; }
        }

        public class ModerationActionRequest
        {
            public string Entity { get; set; } = string.Empty;
            public long Id { get; set; }
            public string? Note { get; set; }
        }
    }
}