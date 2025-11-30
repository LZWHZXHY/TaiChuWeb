using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IO;
using System.Text.Json;
using System.Linq;
using THCY_BE.DataBase;
using THCY_BE.Dto.Chai;
using THCY_BE.Models.Chai;
using Microsoft.AspNetCore.Http;

namespace THCY_BE.Controllers.Chai
{
    [ApiController]
    [Route("api/[controller]")]
    public class OCBattleFieldController : ControllerBase
    {
        private readonly ChaiDbContext _db;
        private readonly ILogger<OCBattleFieldController> _logger;
        private readonly IWebHostEnvironment _environment;

        // 本地存储路径配置
        private const string BASE_PHYSICAL_PATH = "/www/wwwroot/bianyuzhou.com/uploads";
        private const string BASE_WEB_PATH = "/uploads";

        public OCBattleFieldController(
            ChaiDbContext db,
            ILogger<OCBattleFieldController> logger,
            IWebHostEnvironment environment)
        {
            _db = db;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// 创建 OC（支持可选上传多张武器立绘）
        /// </summary>
        [HttpPost("upload")]
        [Authorize]
        public async Task<ActionResult> UploadContent([FromForm] CreateOCDto dto)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("=== 开始处理OC创建请求 ===");
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new { success = false, message = "数据验证失败", errors });
                }

                if (dto.CharacterImage == null || dto.CharacterImage.Length == 0)
                {
                    return BadRequest(new { success = false, message = "请上传角色立绘图片" });
                }

                if (string.IsNullOrWhiteSpace(dto.POO))
                {
                    return BadRequest(new { success = false, message = "POO是必须的" });
                }

                var userId = GetCurrentUserId();

                var normalizedName = dto.OCName?.Trim();
                if (string.IsNullOrEmpty(normalizedName))
                    return BadRequest(new { success = false, message = "角色名称不能为空" });

                var existingOC = await _db.OC_Master
                    .Where(o => o.authorID == userId && o.name.ToLower() == normalizedName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingOC != null)
                    return BadRequest(new { success = false, message = $"你已创建过名为 '{normalizedName}' 的角色，请在已创建角色中编辑" });

                var masterRecord = new OC_Master
                {
                    name = normalizedName,
                    authorName = dto.authorName?.Trim() ?? string.Empty,
                    authorID = userId,
                    createTime = DateTime.UtcNow,
                    updateTime = DateTime.UtcNow,
                    status = 1,
                    dueling = 0,
                    currentVersionId = 0
                };

                _db.OC_Master.Add(masterRecord);
                await _db.SaveChangesAsync();

                // 上传人物立绘
                var charUpload = await UploadSingleFileToLocalAsync(dto.CharacterImage, userId, masterRecord.id, "char");
                if (!charUpload.Success)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { success = false, message = $"人物图片上传失败: {charUpload.ErrorMessage}" });
                }

                // 上传武器图片（可选）
                List<string> weaponPaths = new();
                if (dto.WeaponImages != null && dto.WeaponImages.Length > 0)
                {
                    var multiRes = await UploadMultipleFilesToLocalAsync(dto.WeaponImages, userId, masterRecord.id, "weapon");
                    if (!multiRes.Success)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { success = false, message = $"武器图片上传失败: {multiRes.ErrorMessage}" });
                    }
                    weaponPaths = multiRes.FilePaths;
                }

                // worldTime in OC_Versions might be int, ensure assign via ?? 0
                var worldTimeValue = dto.currentTime ?? 0;

                var versionRecord = new OC_Versions
                {
                    ocMasterId = masterRecord.id,
                    versionNumber = 1,
                    versionDescription = dto.VersionDescription ?? "初始版本",
                    isCurrent = true,
                    name = normalizedName,
                    // fix: use null-coalescing to provide int when model expects int
                    gender = dto.gender ?? 0,
                    age = dto.age ?? 0,
                    species = dto.species?.Trim(),
                    ability = dto.ability?.Trim(),
                    character = dto.character?.Trim(),
                    background = dto.background?.Trim(),
                    colors = dto.colors?.Trim(),
                    OC_WeapenImgUrl = weaponPaths.Count > 0 ? JsonSerializer.Serialize(weaponPaths) : null,
                    OC_WeapenDesc = dto.weaponDesc?.Trim(),
                    ExtraDesc = dto.extraDesc?.Trim(),
                    OC_status = dto.ocStatus ?? 0,
                    worldTime = worldTimeValue,
                    experience = "[]",
                    winCount = 0,
                    loseCount = 0,
                    POO = dto.POO?.Trim(),
                    createTime = DateTime.UtcNow,
                    OC_image_url = charUpload.FilePath
                };

                _db.OC_Versions.Add(versionRecord);
                await _db.SaveChangesAsync();

                masterRecord.currentVersionId = versionRecord.id;
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "OC角色创建成功",
                    data = new
                    {
                        ocId = masterRecord.id,
                        versionId = versionRecord.id,
                        versionNumber = versionRecord.versionNumber,
                        imageInfo = new
                        {
                            character = charUpload.FilePath,
                            weapons = weaponPaths
                        }
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "数据库保存失败");
                return StatusCode(500, new { success = false, message = "数据库保存失败", detail = dbEx.InnerException?.Message ?? dbEx.Message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "服务器错误");
                return StatusCode(500, new { success = false, message = "服务器错误", detail = ex.Message });
            }
        }

        /// <summary>
        /// 更新 OC（新增版本），支持追加武器图
        /// </summary>
        [HttpPost("{ocId}/update")]
        [Authorize]
        public async Task<ActionResult> UpdateOC(int ocId, [FromForm] UpdateOCDto dto)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("开始更新OC角色，OC ID: {OCId}", ocId);
                var userId = GetCurrentUserId();

                var masterRecord = await _db.OC_Master.FirstOrDefaultAsync(m => m.id == ocId && m.authorID == userId && m.status == 1);
                if (masterRecord == null) return NotFound(new { success = false, message = "未找到OC或无权限" });

                var currentVersion = await _db.OC_Versions.FirstOrDefaultAsync(v => v.ocMasterId == ocId && v.isCurrent);
                if (currentVersion == null) return NotFound(new { success = false, message = "未找到当前版本" });

                _logger.LogInformation("Request.HasFormContentType: {HasForm}", Request.HasFormContentType);

                var maxVersion = await _db.OC_Versions.Where(v => v.ocMasterId == ocId).MaxAsync(v => (int?)v.versionNumber) ?? 0;
                var nextVersionNumber = maxVersion + 1;

                // 上传新增武器图片（如果提供）
                List<string> uploadedNewWeapons = new();
                if (dto.WeaponImages != null && dto.WeaponImages.Length > 0)
                {
                    var res = await UploadMultipleFilesToLocalAsync(dto.WeaponImages, userId, ocId, "weapon");
                    if (!res.Success)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { success = false, message = $"武器图片上传失败: {res.ErrorMessage}" });
                    }
                    uploadedNewWeapons = res.FilePaths;
                }

                // 将现有 weapon JSON 解析并合并
                var mergedWeapons = new List<string>();
                if (!string.IsNullOrEmpty(currentVersion.OC_WeapenImgUrl))
                {
                    try
                    {
                        var existing = JsonSerializer.Deserialize<List<string>>(currentVersion.OC_WeapenImgUrl);
                        if (existing != null) mergedWeapons.AddRange(existing);
                    }
                    catch { /* ignore */ }
                }
                if (uploadedNewWeapons.Any()) mergedWeapons.AddRange(uploadedNewWeapons);

                // form -> dto -> current fallback helper
                IFormCollection form = Request.HasFormContentType ? Request.Form : null!;
                string? GetForm(string key) => form != null && form.ContainsKey(key) ? form[key].FirstOrDefault() : null;

                string resolvedName = GetForm("name") ?? dto?.name ?? currentVersion.name;
                string resolvedAbility = GetForm("ability") ?? GetForm("Ability") ?? dto?.ability ?? currentVersion.ability;
                string resolvedSpecies = GetForm("species") ?? dto?.species ?? currentVersion.species;
                int resolvedGender = int.TryParse(GetForm("gender") ?? dto?.gender?.ToString(), out var gval) ? gval : (currentVersion.gender);
                int resolvedAge = int.TryParse(GetForm("age") ?? dto?.age?.ToString(), out var aval) ? aval : (currentVersion.age);
                int resolvedWorldTime = dto?.currentTime ?? currentVersion.worldTime;

                string resolvedDesc = GetForm("updateDescription") ?? GetForm("editDescription") ?? dto?.updateDescription ?? "内容更新";

                var newVersion = new OC_Versions
                {
                    ocMasterId = ocId,
                    versionNumber = nextVersionNumber,
                    versionDescription = resolvedDesc,
                    isCurrent = true,
                    createTime = DateTime.UtcNow,
                    name = resolvedName,
                    gender = resolvedGender,
                    age = resolvedAge,
                    species = resolvedSpecies,
                    ability = resolvedAbility,
                    character = GetForm("character") ?? dto?.character ?? currentVersion.character,
                    background = GetForm("background") ?? dto?.background ?? currentVersion.background,
                    colors = GetForm("colors") ?? dto?.colors ?? currentVersion.colors,
                    OC_WeapenImgUrl = mergedWeapons.Count > 0 ? JsonSerializer.Serialize(mergedWeapons) : currentVersion.OC_WeapenImgUrl,
                    OC_WeapenDesc = GetForm("weaponDesc") ?? dto?.weaponDesc ?? currentVersion.OC_WeapenDesc,
                    ExtraDesc = GetForm("extraDesc") ?? dto?.extraDesc ?? currentVersion.ExtraDesc,
                    OC_status = dto?.ocStatus ?? currentVersion.OC_status,
                    worldTime = resolvedWorldTime,
                    experience = currentVersion.experience,
                    winCount = currentVersion.winCount,
                    loseCount = currentVersion.loseCount,
                    POO = GetForm("POO") ?? dto?.POO ?? currentVersion.POO,
                    OC_image_url = currentVersion.OC_image_url
                };

                // 如果有人上传了新的 CharacterImage，则替换
                if (dto?.CharacterImage != null && dto.CharacterImage.Length > 0)
                {
                    var charRes = await UploadSingleFileToLocalAsync(dto.CharacterImage, userId, ocId, "char");
                    if (charRes.Success) newVersion.OC_image_url = charRes.FilePath;
                }

                // 标记旧版为非当前
                currentVersion.isCurrent = false;

                _db.OC_Versions.Add(newVersion);
                await _db.SaveChangesAsync();

                masterRecord.currentVersionId = newVersion.id;
                masterRecord.updateTime = DateTime.UtcNow;
                masterRecord.name = newVersion.name;

                newVersion.isCurrent = true;
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "OC角色更新成功",
                    data = new { ocId = ocId, versionId = newVersion.id, versionNumber = newVersion.versionNumber }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "更新失败");
                return StatusCode(500, new { success = false, message = "更新失败", detail = ex.Message });
            }
        }

        /// <summary>
        /// 获取列表（容错）并返回 weaponImages 完整 URL 列表（若存在）
        /// </summary>
        [HttpGet("list")]
        [AllowAnonymous]
        public async Task<ActionResult> GetOCList()
        {
            try
            {
                var latestPerMaster = _db.OC_Versions
                    .GroupBy(v => v.ocMasterId)
                    .Select(g => new { ocMasterId = g.Key, latestVersionId = g.OrderByDescending(v => v.versionNumber).FirstOrDefault().id });

                var query = from master in _db.OC_Master
                            join lv in latestPerMaster on master.id equals lv.ocMasterId into lvj
                            from lv in lvj.DefaultIfEmpty()
                            join versionOnCurrent in _db.OC_Versions on master.currentVersionId equals versionOnCurrent.id into vcurj
                            from versionOnCurrent in vcurj.DefaultIfEmpty()
                            join versionLatest in _db.OC_Versions on lv.latestVersionId equals versionLatest.id into vlastj
                            from versionLatest in vlastj.DefaultIfEmpty()
                            where master.status == 1
                            orderby master.updateTime descending
                            select new
                            {
                                master.id,
                                version = (versionOnCurrent != null && versionOnCurrent.isCurrent) ? versionOnCurrent : versionLatest,
                                master.authorName,
                                master.authorID
                            };

                var list = await query.ToListAsync();

                var result = list.Where(x => x.version != null)
                    .Select(x =>
                    {
                        List<string>? weaponImgs = null;
                        try
                        {
                            if (!string.IsNullOrEmpty(x.version.OC_WeapenImgUrl))
                                weaponImgs = JsonSerializer.Deserialize<List<string>>(x.version.OC_WeapenImgUrl);
                        }
                        catch { weaponImgs = null; }

                        return new
                        {
                            id = x.id,
                            name = x.version.name,
                            authorName = x.authorName,
                            species = x.version.species,
                            gender = x.version.gender,
                            age = x.version.age,
                            worldTime = x.version.worldTime,
                            createTime = x.version.createTime,
                            OC_image_url = x.version.OC_image_url,
                            weaponImages = weaponImgs,
                            versionNumber = x.version.versionNumber,
                            authorID = x.authorID
                        };
                    }).ToList();

                var final = result.Select(o => new
                {
                    o.id,
                    o.name,
                    o.authorName,
                    o.species,
                    o.gender,
                    o.age,
                    o.worldTime,
                    o.createTime,
                    o.versionNumber,
                    o.authorID,
                    imageUrl = !string.IsNullOrEmpty(o.OC_image_url) ? BuildImageUrl(o.OC_image_url) : null,
                    weaponImages = o.weaponImages?.Select(p => BuildImageUrl(p)).ToList()
                }).ToList();

                return Ok(new { success = true, data = new { total = final.Count, items = final } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取列表失败");
                return StatusCode(500, new { success = false, message = "获取列表失败" });
            }
        }

        /// <summary>
        /// 获取单个 OC 详情（包含 weaponImages 数组）
        /// </summary>
        [HttpGet("{ocId}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetOCDetail(int ocId)
        {
            try
            {
                var detail = await (from master in _db.OC_Master
                                    join version in _db.OC_Versions on master.currentVersionId equals version.id
                                    where master.id == ocId && master.status == 1 && version.isCurrent
                                    select new { master, version }).FirstOrDefaultAsync();

                OC_Master masterRec;
                OC_Versions curr;

                if (detail == null)
                {
                    curr = await _db.OC_Versions.Where(v => v.ocMasterId == ocId).OrderByDescending(v => v.versionNumber).FirstOrDefaultAsync();
                    if (curr == null) return NotFound(new { success = false, message = "未找到指定的OC角色" });
                    masterRec = await _db.OC_Master.FirstOrDefaultAsync(m => m.id == ocId && m.status == 1);
                    if (masterRec == null) return NotFound(new { success = false, message = "未找到指定的OC角色" });
                }
                else
                {
                    masterRec = detail.master;
                    curr = detail.version;
                }

                List<string>? weaponImgs = null;
                try
                {
                    if (!string.IsNullOrEmpty(curr.OC_WeapenImgUrl))
                        weaponImgs = JsonSerializer.Deserialize<List<string>>(curr.OC_WeapenImgUrl);
                }
                catch { weaponImgs = null; }

                var versionHistory = await _db.OC_Versions.Where(v => v.ocMasterId == ocId)
                    .OrderByDescending(v => v.versionNumber)
                    .Select(v => new
                    {
                        v.versionNumber,
                        v.versionDescription,
                        v.createTime,
                        v.age,
                        v.worldTime,
                        v.OC_image_url,
                        isCurrent = v.isCurrent
                    }).ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        ocId = masterRec.id,
                        name = curr.name,
                        authorName = masterRec.authorName,
                        authorId = masterRec.authorID,
                        createTime = masterRec.createTime,
                        updateTime = masterRec.updateTime,
                        gender = curr.gender,
                        age = curr.age,
                        species = curr.species,
                        ability = curr.ability,
                        character = curr.character,
                        background = curr.background,
                        colors = curr.colors,
                        worldTime = curr.worldTime,
                        POO = curr.POO,
                        winCount = curr.winCount,
                        loseCount = curr.loseCount,
                        experience = curr.experience,
                        weaponImageUrl = curr.OC_WeapenImgUrl,
                        weaponImages = weaponImgs?.Select(p => BuildImageUrl(p)).ToList(),
                        imageUrl = BuildImageUrl(curr.OC_image_url),
                        currentVersion = new { curr.versionNumber, curr.versionDescription },
                        versionHistory = versionHistory.Select(v => new
                        {
                            v.versionNumber,
                            v.versionDescription,
                            v.createTime,
                            v.age,
                            v.worldTime,
                            imageUrl = BuildImageUrl(v.OC_image_url),
                            v.isCurrent
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取详情失败");
                return StatusCode(500, new { success = false, message = "获取详情失败" });
            }
        }

        #region 辅助方法

        private async Task<UploadResult> UploadSingleFileToLocalAsync(IFormFile file, int userId, int ocId, string rolePrefix)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return new UploadResult { Success = false, ErrorMessage = "文件为空" };

                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext)) return new UploadResult { Success = false, ErrorMessage = "不支持的文件格式" };

                if (file.Length > 5 * 1024 * 1024) return new UploadResult { Success = false, ErrorMessage = "文件大小不能超过5MB" };

                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var random = new Random().Next(1000, 9999);
                var safePrefix = string.IsNullOrWhiteSpace(rolePrefix) ? "file" : rolePrefix;
                var fileName = $"oc_{ocId}_{safePrefix}_{timestamp}_{random}{ext}";

                var userFolder = userId.ToString();
                var ocFolder = $"oc_{ocId}";
                var relativePath = Path.Combine("柴圈板块", "太初约战场", "人设图", userFolder, ocFolder, fileName).Replace("\\", "/");
                var physicalPath = Path.Combine(BASE_PHYSICAL_PATH, relativePath);

                var directory = Path.GetDirectoryName(physicalPath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                using var stream = new FileStream(physicalPath, FileMode.Create);
                await file.CopyToAsync(stream);

                return new UploadResult { Success = true, FileName = fileName, FilePath = relativePath, FileSize = file.Length, PhysicalPath = physicalPath };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传文件失败");
                return new UploadResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        private async Task<(bool Success, List<string> FilePaths, string ErrorMessage)> UploadMultipleFilesToLocalAsync(IFormFile[] files, int userId, int ocId, string rolePrefix)
        {
            var paths = new List<string>();
            try
            {
                var seq = 1;
                foreach (var file in files)
                {
                    if (file == null || file.Length == 0) continue;
                    var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowed.Contains(ext)) continue;
                    if (file.Length > 5 * 1024 * 1024) continue;

                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    var fileName = $"oc_{ocId}_{rolePrefix}_{timestamp}_{seq}{ext}";

                    var userFolder = userId.ToString();
                    var ocFolder = $"oc_{ocId}";
                    var relativePath = Path.Combine("柴圈板块", "太初约战场", "人设图", userFolder, ocFolder, fileName).Replace("\\", "/");
                    var physicalPath = Path.Combine(BASE_PHYSICAL_PATH, relativePath);

                    var directory = Path.GetDirectoryName(physicalPath);
                    if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                    using var stream = new FileStream(physicalPath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    paths.Add(relativePath);
                    seq++;
                }

                return (true, paths, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量上传失败");
                return (false, paths, ex.Message);
            }
        }

        private string? BuildImageUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;
            var baseUrl = _environment.IsDevelopment() ? "https://localhost:44359" : "https://bianyuzhou.com";
            var fullUrl = $"{baseUrl}/{BASE_WEB_PATH.TrimStart('/')}/{relativePath.TrimStart('/')}";
            fullUrl = fullUrl.Replace("https:/", "https://").Replace("http:/", "http://").Replace("\\", "/").Replace("//", "/");
            return fullUrl;
        }

        private int GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out int userId) && userId > 0) return userId;
                return 1;
            }
            catch
            {
                return 1;
            }
        }

        private class UploadResult
        {
            public bool Success { get; set; }
            public string? ErrorMessage { get; set; }
            public string? FileName { get; set; }
            public string? FilePath { get; set; }
            public long FileSize { get; set; }
            public string? PhysicalPath { get; set; }
        }

        #endregion
    }
}