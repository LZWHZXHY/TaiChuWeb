// Controllers/Chai/OCBattleFieldController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using THCY_BE.DataBase;
using THCY_BE.Dto.Chai;
using THCY_BE.Models.Chai;
using THCY_BE.Services;
using System.IO;

namespace THCY_BE.Controller.Chai
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OCBattleFieldController : ControllerBase
    {
        private readonly ChaiDbContext _db;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<OCBattleFieldController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        // 本地存储路径配置
        private const string BASE_PHYSICAL_PATH = "/www/wwwroot/bianyuzhou.com/uploads";
        private const string BASE_WEB_PATH = "/uploads";

        public OCBattleFieldController(
            ChaiDbContext db,
            IFileUploadService fileUploadService,
            ILogger<OCBattleFieldController> logger,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _db = db;
            _fileUploadService = fileUploadService;
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
        }

        /// <summary>
        /// 创建OC角色并上传立绘图片 - 使用本地存储
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult> UploadContent([FromForm] OCBattleDto dto)
        {
            try
            {
                _logger.LogInformation("=== 开始创建OC角色 ===");
                _logger.LogInformation("角色名: {OCName}, 作者: {AuthorName}", dto.OCName, dto.authorName);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("数据验证失败: {Errors}", string.Join(", ", errors));
                    return BadRequest(new
                    {
                        success = false,
                        message = "数据验证失败",
                        errors = errors
                    });
                }

                if (dto.CharacterImage == null || dto.CharacterImage.Length == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "请上传角色立绘图片"
                    });
                }

                if (string.IsNullOrWhiteSpace(dto.POO))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "POO是必须的"
                    });
                }

                var existingOC = await _db.OC_Infos
                    .Where(o => o.name == dto.OCName.Trim())
                    .FirstOrDefaultAsync();

                if (existingOC != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"角色名称 '{dto.OCName}' 已存在，请使用其他名称"
                    });
                }

                var userId = GetCurrentUserId();
                var newOC_info = new OC_Info
                {
                    name = dto.OCName.Trim(),
                    gender = dto.gender,
                    age = dto.age,
                    species = dto.species.Trim(),
                    ability = dto.ability.Trim(),
                    authorName = dto.authorName.Trim(),
                    background = dto.Background?.Trim(),
                    POO = dto.POO.Trim(),
                    OC_Current_Time = dto.currentTime,
                    experience = "[]",
                    winCount = 0,
                    loseCount = 0,
                    status = 1,
                    dueling = 0,
                    version = 1,
                    authorID = userId,
                    OC_image_url = null,
                    createTime = DateTime.UtcNow,
                    updateTime = DateTime.UtcNow
                };

                _db.OC_Infos.Add(newOC_info);
                await _db.SaveChangesAsync();

                _logger.LogInformation("OC记录创建成功，ID: {OCId}", newOC_info.id);

                // 使用本地存储上传图片
                var uploadResult = await UploadImageToLocalStorageAsync(
                    dto.CharacterImage,
                    userId,
                    newOC_info.id
                );

                if (!uploadResult.Success)
                {
                    _db.OC_Infos.Remove(newOC_info);
                    await _db.SaveChangesAsync();

                    _logger.LogError("立绘图片上传失败: {ErrorMessage}", uploadResult.ErrorMessage);
                    return BadRequest(new
                    {
                        success = false,
                        message = $"立绘图片上传失败: {uploadResult.ErrorMessage}"
                    });
                }

                newOC_info.OC_image_url = uploadResult.FilePath;
                newOC_info.updateTime = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation("✅ OC角色创建成功，ID: {Id}", newOC_info.id);

                return Ok(new
                {
                    success = true,
                    message = "OC角色创建成功",
                    data = new
                    {
                        ocId = newOC_info.id,
                        ocName = newOC_info.name,
                        authorName = newOC_info.authorName,
                        age = newOC_info.age,
                        species = newOC_info.species,
                        gender = newOC_info.gender,
                        poo = newOC_info.POO,
                        currentTime = newOC_info.OC_Current_Time,
                        imageInfo = new
                        {
                            relativePath = uploadResult.FilePath,
                            fileName = uploadResult.FileName,
                            fullUrl = BuildLocalImageUrl(uploadResult.FilePath),
                            fileSize = uploadResult.FileSize
                        },
                        timestamps = new
                        {
                            createTime = newOC_info.createTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            updateTime = newOC_info.updateTime.ToString("yyyy-MM-dd HH:mm:ss")
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ OC角色创建失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "服务器内部错误: " + ex.Message
                });
            }
        }

        [HttpPost("{ocId}/update-age")]
        public async Task<ActionResult> UpdateOCAge(int ocId, [FromBody] UpdateOCAgeDto dto)
        {
            try
            {
                _logger.LogInformation("开始更新OC角色年龄，OC ID: {OCId}, 新年龄: {NewAge}", ocId, dto.NewAge);

                var ocInfo = await _db.OC_Infos.FindAsync(ocId);
                if (ocInfo == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"未找到ID为 {ocId} 的OC角色"
                    });
                }

                if (ocInfo.age == dto.NewAge)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "新年龄与当前年龄相同，无需更新"
                    });
                }

                if (dto.NewAge < 0 || dto.NewAge > 1000)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "年龄必须在0-1000之间"
                    });
                }

                var oldAge = ocInfo.age;
                ocInfo.age = dto.NewAge;
                ocInfo.updateTime = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                _logger.LogInformation("✅ OC角色年龄更新成功: ID={OCId}, 旧年龄={OldAge}, 新年龄={NewAge}", ocId, oldAge, dto.NewAge);

                return Ok(new
                {
                    success = true,
                    message = "年龄更新成功",
                    data = new
                    {
                        ocId = ocId,
                        ocName = ocInfo.name,
                        oldAge = oldAge,
                        newAge = dto.NewAge,
                        updateTime = ocInfo.updateTime.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 更新OC年龄失败，OC ID: {OCId}", ocId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "更新失败: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 更新OC角色立绘
        /// </summary>
        [HttpPost("{ocId}/update-image")]
        public async Task<ActionResult> UpdateOCImage(int ocId, IFormFile characterImage)
        {
            try
            {
                _logger.LogInformation("开始更新OC角色立绘，OC ID: {OCId}", ocId);

                var ocInfo = await _db.OC_Infos.FindAsync(ocId);
                if (ocInfo == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"未找到ID为 {ocId} 的OC角色"
                    });
                }

                if (characterImage == null || characterImage.Length == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "请上传图片文件"
                    });
                }

                // 删除旧图片（如果存在）
                if (!string.IsNullOrEmpty(ocInfo.OC_image_url))
                {
                    try
                    {
                        await DeleteLocalImageAsync(ocInfo.OC_image_url);
                        _logger.LogInformation("旧图片删除成功: {FilePath}", ocInfo.OC_image_url);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "删除旧图片失败，继续上传新图片: {FilePath}", ocInfo.OC_image_url);
                    }
                }

                // 使用本地存储上传新图片
                var uploadResult = await UploadImageToLocalStorageAsync(
                    characterImage,
                    ocInfo.authorID,
                    ocId
                );

                if (!uploadResult.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"图片上传失败: {uploadResult.ErrorMessage}"
                    });
                }

                var oldImagePath = ocInfo.OC_image_url;
                ocInfo.OC_image_url = uploadResult.FilePath;
                ocInfo.updateTime = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation("✅ OC角色立绘更新成功: ID={OCId}", ocId);

                return Ok(new
                {
                    success = true,
                    message = "角色立绘更新成功",
                    data = new
                    {
                        ocId = ocInfo.id,
                        ocName = ocInfo.name,
                        age = ocInfo.age,
                        imageInfo = new
                        {
                            oldFilePath = oldImagePath,
                            newFilePath = uploadResult.FilePath,
                            fileName = uploadResult.FileName,
                            fullUrl = BuildLocalImageUrl(uploadResult.FilePath),
                            fileSize = uploadResult.FileSize
                        },
                        updateTime = ocInfo.updateTime.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 更新OC立绘失败，OC ID: {OCId}", ocId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "更新失败: " + ex.Message
                });
            }
        }

        [HttpGet("list")]
        [AllowAnonymous]
        public async Task<ActionResult> GetOCList()
        {
            try
            {
                _logger.LogInformation("开始获取OC角色列表");

                var rawList = await _db.OC_Infos
                    .Where(o => o.status == 1)
                    .OrderByDescending(o => o.updateTime)
                    .Select(o => new
                    {
                        o.id,
                        o.name,
                        o.authorName,
                        o.species,
                        o.gender,
                        o.age,
                        o.winCount,
                        o.loseCount,
                        o.OC_Current_Time,
                        o.updateTime,
                        o.OC_image_url,
                        o.authorID
                    })
                    .ToListAsync();

                var ocList = rawList.Select(o => new
                {
                    o.id,
                    o.name,
                    o.authorName,
                    o.species,
                    o.gender,
                    o.age,
                    o.winCount,
                    o.loseCount,
                    o.OC_Current_Time,
                    o.updateTime,
                    o.authorID,
                    imageUrl = !string.IsNullOrEmpty(o.OC_image_url) ? BuildLocalImageUrl(o.OC_image_url) : null
                }).ToList();

                _logger.LogInformation("获取到 {Count} 个OC角色", ocList.Count);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        total = ocList.Count,
                        items = ocList
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 获取OC列表失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "获取OC列表失败: " + ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetOCDetail(int id)
        {
            try
            {
                _logger.LogInformation("开始获取OC角色详情，ID: {Id}", id);

                var raw = await _db.OC_Infos
                    .Where(o => o.id == id && o.status == 1)
                    .Select(o => new
                    {
                        o.id,
                        o.name,
                        o.gender,
                        o.age,
                        o.species,
                        o.ability,
                        o.authorName,
                        o.background,
                        o.POO,
                        o.OC_Current_Time,
                        o.winCount,
                        o.loseCount,
                        o.experience,
                        o.version,
                        o.createTime,
                        o.updateTime,
                        o.OC_image_url,
                        o.authorID
                    })
                    .FirstOrDefaultAsync();

                if (raw == null)
                {
                    _logger.LogWarning("未找到OC角色，ID: {Id}", id);
                    return NotFound(new
                    {
                        success = false,
                        message = "未找到指定的OC角色"
                    });
                }

                var imageUrl = !string.IsNullOrEmpty(raw.OC_image_url) ? BuildLocalImageUrl(raw.OC_image_url) : null;

                _logger.LogInformation("成功获取OC角色详情: {OCName}", raw.name);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        raw.id,
                        raw.name,
                        raw.gender,
                        raw.age,
                        raw.species,
                        raw.ability,
                        raw.authorName,
                        raw.background,
                        raw.POO,
                        raw.OC_Current_Time,
                        raw.winCount,
                        raw.loseCount,
                        raw.experience,
                        raw.version,
                        raw.createTime,
                        raw.updateTime,
                        raw.authorID,
                        imageUrl
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 获取OC详情失败，ID: {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "获取OC详情失败: " + ex.Message
                });
            }
        }

        [HttpGet("{ocId}/images-by-age")]
        [AllowAnonymous]
        public async Task<ActionResult> GetOCImagesByAge(int ocId, [FromQuery] int? age = null)
        {
            try
            {
                _logger.LogInformation("开始按年龄检索OC图片，OC ID: {OCId}, 年龄: {Age}", ocId, age);

                var ocInfo = await _db.OC_Infos
                    .Where(o => o.id == ocId && o.status == 1)
                    .Select(o => new { o.name })
                    .FirstOrDefaultAsync();

                if (ocInfo == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"未找到ID为 {ocId} 的OC角色"
                    });
                }

                var rawImages = await _db.OC_Infos
                    .Where(o => o.id == ocId && o.status == 1 && (!age.HasValue || o.age == age.Value))
                    .Select(o => new
                    {
                        o.id,
                        o.name,
                        o.age,
                        o.OC_image_url,
                        o.updateTime
                    })
                    .ToListAsync();

                var result = rawImages.Select(i => new
                {
                    age = i.age,
                    imageUrl = !string.IsNullOrEmpty(i.OC_image_url) ? BuildLocalImageUrl(i.OC_image_url) : null,
                    period = $"{i.age}岁时期",
                    updateTime = i.updateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    fileName = !string.IsNullOrEmpty(i.OC_image_url) ? Path.GetFileName(i.OC_image_url) : "暂无图片"
                }).ToList();

                _logger.LogInformation("按年龄检索完成: OC ID={OCId}, 找到 {Count} 个记录", ocId, result.Count);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        ocId,
                        ocName = ocInfo.name,
                        filterAge = age,
                        totalImages = result.Count,
                        images = result
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 按年龄检索OC图片失败，OC ID: {OCId}", ocId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "检索失败: " + ex.Message
                });
            }
        }

        #region 本地存储核心方法

        /// <summary>
        /// 上传图片到服务器本地存储 - 包含年龄信息格式（推荐版本）
        /// 格式：oc_{ocId}_{年龄}yo_{时间戳}_{随机数}
        /// </summary>
        private async Task<LocalUploadResult> UploadImageToLocalStorageAsync(IFormFile file, int userId, int ocId)
        {
            try
            {
                if (!ValidateImageFile(file))
                {
                    return new LocalUploadResult { Success = false, ErrorMessage = "文件格式不支持或文件过大" };
                }

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                // 获取OC角色的年龄和名称信息
                var ocInfo = await _db.OC_Infos
                    .Where(o => o.id == ocId)
                    .Select(o => new { o.age, o.name })
                    .FirstOrDefaultAsync();

                if (ocInfo == null)
                {
                    return new LocalUploadResult { Success = false, ErrorMessage = "未找到指定的OC角色" };
                }

                // 生成文件名：oc_{ocId}_{年龄}yo_{时间戳}_{随机数}
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var random = new Random().Next(1000, 9999); // 避免时间戳重复
                var fileName = $"oc_{ocId}_{ocInfo.age}yo_{timestamp}_{random}{fileExtension}";

                var userFolder = userId.ToString();
                var ocFolder = $"oc_{ocId}";

                // 构建路径
                var physicalPath = Path.Combine(BASE_PHYSICAL_PATH, "柴圈板块", "太初约战场", "人设图", userFolder, ocFolder, fileName);
                var relativePath = Path.Combine("柴圈板块", "太初约战场", "人设图", userFolder, ocFolder, fileName)
                    .Replace("\\", "/");

                // 确保目录存在
                var directory = Path.GetDirectoryName(physicalPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation("创建目录: {Directory}", directory);
                }

                // 保存文件
                using var stream = new FileStream(physicalPath, FileMode.Create);
                await file.CopyToAsync(stream);

                _logger.LogInformation("✅ 图片保存成功: {FileName}", fileName);
                _logger.LogInformation("📝 OC角色信息: {OCName} (ID: {OCId}, 年龄: {Age}岁)",
                    ocInfo.name, ocId, ocInfo.age);

                return new LocalUploadResult
                {
                    Success = true,
                    FileName = fileName,
                    FilePath = relativePath,
                    FileSize = file.Length,
                    PhysicalPath = physicalPath
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 本地图片上传失败");
                return new LocalUploadResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// 删除本地图片
        /// </summary>
        private async Task<bool> DeleteLocalImageAsync(string relativePath)
        {
            try
            {
                if (string.IsNullOrEmpty(relativePath))
                    return false;

                // 相对路径转换为物理路径
                var physicalPath = Path.Combine(BASE_PHYSICAL_PATH, relativePath);

                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                    _logger.LogInformation("删除本地图片: {PhysicalPath}", physicalPath);
                    return true;
                }

                _logger.LogWarning("图片文件不存在: {PhysicalPath}", physicalPath);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除本地图片失败: {RelativePath}", relativePath);
                return false;
            }
        }

        /// <summary>
        /// 验证图片文件
        /// </summary>
        private bool ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // 检查文件大小（5MB）
            if (file.Length > 5 * 1024 * 1024)
                return false;

            // 检查文件类型
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return false;

            return true;
        }

        /// <summary>
        /// 构建本地图片访问URL
        /// </summary>
        /// <summary>
        /// 构建本地图片访问URL - 修正协议格式
        /// </summary>
        private string? BuildLocalImageUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            // 开发环境使用本地地址，生产环境使用域名
            var baseUrl = _environment.IsDevelopment()
                ? "https://localhost:44359"
                : _configuration["AppSettings:ProductionUrl"] ?? "https://bianyuzhou.com";

            // 确保baseUrl格式正确
            baseUrl = baseUrl.TrimEnd('/');

            // 构建完整的URL
            var fullUrl = $"{baseUrl}/{BASE_WEB_PATH.TrimStart('/')}/{relativePath.TrimStart('/')}";

            // 修正协议格式（确保是 https:// 而不是 https:/）
            fullUrl = fullUrl.Replace("https:/", "https://")
                            .Replace("http:/", "http://")
                            .Replace("\\", "/")
                            .Replace("//", "/");

            _logger.LogInformation("构建图片URL: {FullUrl}", fullUrl);
            return fullUrl;
        }

        /// <summary>
        /// 本地上传结果类
        /// </summary>
        private class LocalUploadResult
        {
            public bool Success { get; set; }
            public string? ErrorMessage { get; set; }
            public string? FileName { get; set; }
            public string? FilePath { get; set; }
            public long FileSize { get; set; }
            public string? PhysicalPath { get; set; }
        }

        #endregion

        #region 辅助方法

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId) && userId > 0)
            {
                return userId;
            }

            _logger.LogWarning("无法从token获取用户ID，声明: {UserIdClaim}", userIdClaim);
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "未知用户";
            _logger.LogWarning("当前用户: {UserName}", userName);
            return 1;
        }

        #endregion
    }

    public class UpdateOCAgeDto
    {
        public int NewAge { get; set; }
    }
}