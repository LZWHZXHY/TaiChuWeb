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

namespace THCY_BE.Controller.Chai
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 必须携带 Bearer token
    public class OCBattleFieldController : ControllerBase
    {
        private readonly ChaiDbContext _db;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<OCBattleFieldController> _logger;
        private readonly IConfiguration _configuration;

        public OCBattleFieldController(
            ChaiDbContext db,
            IFileUploadService fileUploadService,
            ILogger<OCBattleFieldController> logger,
            IConfiguration configuration)
        {
            _db = db;
            _fileUploadService = fileUploadService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// 创建OC角色并上传立绘图片
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
                    authorID = GetCurrentUserId(),
                    OC_image_url = null, // 数据库字段名：OC_image_url
                    createTime = DateTime.UtcNow,
                    updateTime = DateTime.UtcNow
                };

                _db.OC_Infos.Add(newOC_info);
                await _db.SaveChangesAsync();

                _logger.LogInformation("OC记录创建成功，ID: {OCId}, 年龄: {OCAge}", newOC_info.id, newOC_info.age);

                var uploadResult = await _fileUploadService.UploadFileAsync(
                    dto.CharacterImage,
                    "OC_Battle_Picture",
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

                newOC_info.OC_image_url = uploadResult.FilePath; // 存储相对路径到数据库
                newOC_info.updateTime = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation("✅ OC角色创建成功，ID: {Id}, 文件名: {FileName}", newOC_info.id, uploadResult.FileName);

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
                            relativePath = uploadResult.FilePath, // 相对路径
                            fileName = uploadResult.FileName,
                            fullUrl = BuildNasImageUrl(uploadResult.FilePath), // 直接使用NAS地址
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

                if (!string.IsNullOrEmpty(ocInfo.OC_image_url))
                {
                    try
                    {
                        await _fileUploadService.DeleteFileAsync(ocInfo.OC_image_url);
                        _logger.LogInformation("旧图片删除成功: {FilePath}", ocInfo.OC_image_url);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "删除旧图片失败，继续上传新图片: {FilePath}", ocInfo.OC_image_url);
                    }
                }

                var uploadResult = await _fileUploadService.UploadFileAsync(
                    characterImage,
                    "OC_Battle_Picture",
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
                ocInfo.OC_image_url = uploadResult.FilePath; // 更新数据库中的相对路径
                ocInfo.updateTime = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation("✅ OC角色立绘更新成功: ID={OCId}, 新文件: {FileName}", ocId, uploadResult.FileName);

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
                            fullUrl = BuildNasImageUrl(uploadResult.FilePath), // 直接使用NAS地址
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

        /// <summary>
        /// 获取OC角色列表
        /// </summary>
        [HttpGet("list")]
        [AllowAnonymous] // 允许匿名访问
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
                        o.OC_image_url // 数据库字段：OC_image_url
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
                    imageUrl = !string.IsNullOrEmpty(o.OC_image_url) ?
                        BuildNasImageUrl(o.OC_image_url) : null // 直接构建NAS地址
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
                        o.OC_image_url // 数据库字段：OC_image_url
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

                var imageUrl = !string.IsNullOrEmpty(raw.OC_image_url) ?
                    BuildNasImageUrl(raw.OC_image_url) : null; // 直接构建NAS地址

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

        /// <summary>
        /// 按年龄检索OC角色的图片历史
        /// </summary>
        [HttpGet("{ocId}/images-by-age")]
        [AllowAnonymous] // 允许匿名访问
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
                        o.OC_image_url, // 数据库字段：OC_image_url
                        o.updateTime
                    })
                    .ToListAsync();

                var result = rawImages.Select(i => new
                {
                    age = i.age,
                    imageUrl = !string.IsNullOrEmpty(i.OC_image_url) ?
                        BuildNasImageUrl(i.OC_image_url) : null, // 直接构建NAS地址
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

        #region 私有辅助方法

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

        /// <summary>
        /// 新的方法：直接构建NAS图片访问URL
        /// </summary>
        /// <summary>
        /// 新的方法：根据环境自动构建NAS图片访问URL
        /// </summary>
        private string? BuildNasImageUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            // 获取当前环境
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var isProduction = environment == "Production";

            _logger.LogInformation("当前环境: {Environment}, 是否为生产环境: {IsProduction}", environment, isProduction);

            // 根据环境选择NAS地址
            var nasBaseUrl = isProduction
                ? _configuration["StorageSettings:ProdNasEndpoint"]
                : _configuration["StorageSettings:DevNasEndpoint"];

            // 安全回退
            if (string.IsNullOrEmpty(nasBaseUrl))
            {
                nasBaseUrl = isProduction
                    ? "http://100.102.164.127:7506/官网资源地址"
                    : "http://192.168.50.225:7506/官网资源地址";

                _logger.LogWarning("NAS地址未配置，使用默认{Environment}地址: {NasUrl}",
                    isProduction ? "生产" : "开发", nasBaseUrl);
            }

            var fullUrl = $"{nasBaseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
            _logger.LogInformation("构建图片URL: 环境={Environment}, 相对路径={RelativePath}, 完整URL={FullUrl}",
                environment, relativePath, fullUrl);

            return fullUrl;
        }

        // 保留旧的BuildImageApiUrl方法（可选，用于向后兼容）
        [Obsolete("请使用BuildNasImageUrl方法，直接返回NAS地址")]
        private string? BuildImageApiUrl(string? relativePath, int? width = null, int? height = null, int? quality = null, string? format = null, bool webp = true)
        {
            // 可以调用新的方法，或者直接弃用
            return BuildNasImageUrl(relativePath);
        }

        #endregion
    }

    public class UpdateOCAgeDto
    {
        public int NewAge { get; set; }
    }
}