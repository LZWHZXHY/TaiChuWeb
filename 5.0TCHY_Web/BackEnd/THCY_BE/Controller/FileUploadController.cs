// Controllers/FileUploadController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THCY_BE.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using THCY_BE.DataBase;

namespace THCY_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 必须携带 Bearer token
    public class FileUploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<FileUploadController> _logger;
        private readonly ChaiDbContext _dbContext;

        public FileUploadController(
            IFileUploadService fileUploadService,
            ILogger<FileUploadController> logger,
            ChaiDbContext dbContext)
        {
            _fileUploadService = fileUploadService;
            _logger = logger;
            _dbContext = dbContext;
        }

        /// <summary>
        /// 上传用户头像
        /// </summary>
        /// <param name="file">头像文件</param>
        /// <returns>上传结果</returns>
        [HttpPost("avatar")]
        public async Task<ActionResult> UploadAvatar(IFormFile file)
        {
            try
            {
                _logger.LogInformation("开始上传用户头像");

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "请选择要上传的文件" });
                }

                // 从token获取用户ID
                var userId = GetCurrentUserIdFromToken();

                // 使用UserAvatar命名策略，自动生成：user_123_avatar_20231201120000.jpg
                var result = await _fileUploadService.UploadFileAsync(
                    file,
                    "avatars",
                    userId,
                    NamingStrategy.UserAvatar,
                    new Dictionary<string, object> { ["UserId"] = userId } // 确保从token获取用户ID
                );

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"头像上传失败: {result.ErrorMessage}"
                    });
                }

                // 更新用户头像路径到数据库
                await UpdateUserAvatarPath(userId, result.FilePath);

                _logger.LogInformation("用户头像上传成功: 用户ID={UserId}, 文件路径={FilePath}", userId, result.FilePath);

                return Ok(new
                {
                    success = true,
                    message = "头像上传成功",
                    data = new
                    {
                        userId = userId,
                        filePath = result.FilePath,
                        fileUrl = result.FileUrl,
                        fileName = result.FileName,
                        fileSize = result.FileSize
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "头像上传失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "上传失败: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 上传OC角色立绘图片（约战场角色）
        /// </summary>
        /// <param name="file">角色立绘文件</param>
        /// <param name="ocId">OC角色ID</param>
        /// <returns>上传结果</returns>
        [HttpPost("oc-battle-image")]
        public async Task<ActionResult> UploadOCBattleImage(IFormFile file, [FromForm] int ocId)
        {
            try
            {
                _logger.LogInformation("开始上传OC角色立绘，OC ID: {OCId}", ocId);

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "请选择要上传的文件" });
                }

                // 验证OC角色是否存在
                var ocInfo = await _dbContext.OC_Infos
                    .Where(o => o.id == ocId)
                    .Select(o => new { o.name, o.age })
                    .FirstOrDefaultAsync();

                if (ocInfo == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"未找到ID为 {ocId} 的OC角色"
                    });
                }

                // 使用OCBattleCharacter命名策略，自动生成：oc_123_角色名_18yo_20231201120000.jpg
                var result = await _fileUploadService.UploadFileAsync(
                    file,
                    "OC_Battle_Picture",
                    ocId,
                    NamingStrategy.OCBattleCharacter,
                    new Dictionary<string, object>
                    {
                        ["OCId"] = ocId,
                        ["OCAge"] = ocInfo.age
                    }
                );

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"OC角色立绘上传失败: {result.ErrorMessage}"
                    });
                }

                // 更新OC角色的图片路径到数据库
                await UpdateOCImagePath(ocId, result.FilePath);

                _logger.LogInformation("OC角色立绘上传成功: OC ID={OCId}, 文件路径={FilePath}", ocId, result.FilePath);

                return Ok(new
                {
                    success = true,
                    message = "OC角色立绘上传成功",
                    data = new
                    {
                        ocId = ocId,
                        ocName = ocInfo.name,
                        age = ocInfo.age,
                        fileInfo = new
                        {
                            filePath = result.FilePath,
                            fileName = result.FileName, // 包含角色名和年龄的文件名
                            fileUrl = result.FileUrl,
                            fileSize = result.FileSize
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OC角色立绘上传失败，OC ID: {OCId}", ocId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "上传失败: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 上传OC图库图片（多图上传）
        /// </summary>
        /// <param name="files">图库文件数组</param>
        /// <param name="ocId">OC角色ID</param>
        /// <returns>上传结果</returns>
        [HttpPost("gallery")]
        public async Task<ActionResult> UploadGallery([FromForm] IFormFile[] files, [FromForm] int ocId)
        {
            try
            {
                _logger.LogInformation("开始上传OC图库图片，OC ID: {OCId}, 文件数量: {FileCount}", ocId, files?.Length ?? 0);

                if (files == null || files.Length == 0)
                {
                    return BadRequest(new { success = false, message = "请选择要上传的文件" });
                }

                // 验证OC角色是否存在
                var ocExists = await _dbContext.OC_Infos.AnyAsync(o => o.id == ocId);
                if (!ocExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"未找到ID为 {ocId} 的OC角色"
                    });
                }

                // 使用OCGallery命名策略，自动生成：oc_123_gallery_20231201120000_1234.jpg
                var results = await _fileUploadService.UploadFilesAsync(
                    files,
                    "gallery",
                    ocId,
                    NamingStrategy.OCGallery
                );

                var successUploads = results.Where(r => r.Success).ToList();
                var failedUploads = results.Where(r => !r.Success).ToList();

                // 记录成功的文件到数据库（这里可以根据需要扩展图库表）
                await RecordGalleryImages(ocId, successUploads);

                _logger.LogInformation("OC图库上传完成: 成功 {SuccessCount} 个, 失败 {FailCount} 个",
                    successUploads.Count, failedUploads.Count);

                return Ok(new
                {
                    success = true,
                    message = $"图库上传完成: 成功 {successUploads.Count} 个, 失败 {failedUploads.Count} 个",
                    data = new
                    {
                        ocId = ocId,
                        successCount = successUploads.Count,
                        failCount = failedUploads.Count,
                        successFiles = successUploads.Select(f => new {
                            fileName = f.FileName,
                            fileUrl = f.FileUrl,
                            fileSize = f.FileSize,
                            filePath = f.FilePath
                        }),
                        failedFiles = failedUploads.Select(f => new {
                            fileName = f.FileName, // 原始文件名
                            errorMessage = f.ErrorMessage
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OC图库上传失败，OC ID: {OCId}", ocId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "图库上传失败: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 上传OC主图
        /// </summary>
        /// <param name="file">OC主图文件</param>
        /// <param name="ocId">OC角色ID</param>
        /// <returns>上传结果</returns>
        [HttpPost("oc-main-image")]
        public async Task<ActionResult> UploadOCMainImage(IFormFile file, [FromForm] int ocId)
        {
            try
            {
                _logger.LogInformation("开始上传OC主图，OC ID: {OCId}", ocId);

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "请选择要上传的文件" });
                }

                // 验证OC角色是否存在
                var ocExists = await _dbContext.OC_Infos.AnyAsync(o => o.id == ocId);
                if (!ocExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"未找到ID为 {ocId} 的OC角色"
                    });
                }

                // 使用OCMainImage命名策略，自动生成：oc_123_main_20231201120000_5678.jpg
                var result = await _fileUploadService.UploadFileAsync(
                    file,
                    "oc-images",
                    ocId,
                    NamingStrategy.OCMainImage
                );

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"OC主图上传失败: {result.ErrorMessage}"
                    });
                }

                // 可以在这里更新OC主图路径到数据库
                // await UpdateOCMainImagePath(ocId, result.FilePath);

                _logger.LogInformation("OC主图上传成功: OC ID={OCId}, 文件路径={FilePath}", ocId, result.FilePath);

                return Ok(new
                {
                    success = true,
                    message = "OC主图上传成功",
                    data = new
                    {
                        ocId = ocId,
                        fileInfo = new
                        {
                            filePath = result.FilePath,
                            fileName = result.FileName,
                            fileUrl = result.FileUrl,
                            fileSize = result.FileSize
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OC主图上传失败，OC ID: {OCId}", ocId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "上传失败: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 上传对战记录文件
        /// </summary>
        /// <param name="file">对战记录文件</param>
        /// <param name="battleId">对战ID</param>
        /// <returns>上传结果</returns>
        [HttpPost("battle-record")]
        public async Task<ActionResult> UploadBattleRecord(IFormFile file, [FromForm] int battleId)
        {
            try
            {
                _logger.LogInformation("开始上传对战记录，对战ID: {BattleId}", battleId);

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "请选择要上传的文件" });
                }

                // 使用BattleRecord命名策略，自动生成：battle_456_record_20231201120000.json
                var result = await _fileUploadService.UploadFileAsync(
                    file,
                    "battle-records",
                    battleId,
                    NamingStrategy.BattleRecord
                );

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"对战记录上传失败: {result.ErrorMessage}"
                    });
                }

                _logger.LogInformation("对战记录上传成功: 对战ID={BattleId}, 文件路径={FilePath}", battleId, result.FilePath);

                return Ok(new
                {
                    success = true,
                    message = "对战记录上传成功",
                    data = new
                    {
                        battleId = battleId,
                        fileInfo = new
                        {
                            filePath = result.FilePath,
                            fileName = result.FileName,
                            fileUrl = result.FileUrl,
                            fileSize = result.FileSize
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "对战记录上传失败，对战ID: {BattleId}", battleId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "上传失败: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 通用文件上传（使用默认命名策略）
        /// </summary>
        /// <param name="file">文件</param>
        /// <param name="folder">目标文件夹</param>
        /// <param name="customId">自定义ID（可选）</param>
        /// <returns>上传结果</returns>
        [HttpPost("general")]
        public async Task<ActionResult> UploadGeneralFile(
            IFormFile file,
            [FromForm] string folder = "uploads",
            [FromForm] int? customId = null)
        {
            try
            {
                _logger.LogInformation("开始通用文件上传，文件夹: {Folder}, 自定义ID: {CustomId}", folder, customId);

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "请选择要上传的文件" });
                }

                // 使用默认命名策略，自动根据文件夹智能选择策略
                var result = await _fileUploadService.UploadFileAsync(
                    file,
                    folder,
                    customId
                // 不指定策略，让系统自动根据文件夹选择
                );

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"文件上传失败: {result.ErrorMessage}"
                    });
                }

                _logger.LogInformation("通用文件上传成功: 文件夹={Folder}, 文件路径={FilePath}", folder, result.FilePath);

                return Ok(new
                {
                    success = true,
                    message = "文件上传成功",
                    data = new
                    {
                        folder = folder,
                        fileInfo = new
                        {
                            filePath = result.FilePath,
                            fileName = result.FileName,
                            fileUrl = result.FileUrl,
                            fileSize = result.FileSize
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "通用文件上传失败，文件夹: {Folder}", folder);
                return StatusCode(500, new
                {
                    success = false,
                    message = "上传失败: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="filePath">文件相对路径</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{*filePath}")]
        public async Task<ActionResult> DeleteFile(string filePath)
        {
            try
            {
                _logger.LogInformation("开始删除文件: {FilePath}", filePath);

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return BadRequest(new { success = false, message = "文件路径不能为空" });
                }

                var result = await _fileUploadService.DeleteFileAsync(filePath);

                if (result)
                {
                    _logger.LogInformation("文件删除成功: {FilePath}", filePath);

                    // 同时更新数据库中的相关记录
                    await ClearFileReferenceInDatabase(filePath);

                    return Ok(new
                    {
                        success = true,
                        message = "文件删除成功"
                    });
                }
                else
                {
                    _logger.LogWarning("文件删除失败: {FilePath}", filePath);
                    return BadRequest(new
                    {
                        success = false,
                        message = "文件删除失败"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件删除失败: {FilePath}", filePath);
                return StatusCode(500, new
                {
                    success = false,
                    message = "删除失败: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 获取文件URL
        /// </summary>
        /// <param name="filePath">文件相对路径</param>
        /// <returns>文件URL</returns>
        [HttpGet("url")]
        
        public ActionResult GetFileUrl([FromQuery] string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return BadRequest(new { success = false, message = "文件路径不能为空" });
                }

                var fileUrl = _fileUploadService.GenerateFileUrl(filePath);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        filePath = filePath,
                        fileUrl = fileUrl
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成文件URL失败: {FilePath}", filePath);
                return StatusCode(500, new
                {
                    success = false,
                    message = "生成URL失败: " + ex.Message
                });
            }
        }

        #region 私有辅助方法

        /// <summary>
        /// 从JWT token获取当前用户ID
        /// </summary>
        private int GetCurrentUserIdFromToken()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId) && userId > 0)
            {
                return userId;
            }

            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "未知用户";
            _logger.LogWarning("无法从token获取有效的用户ID，声明: {UserIdClaim}", userIdClaim);
            throw new UnauthorizedAccessException($"无法获取有效的用户ID，请重新登录。当前用户: {userName}");
        }

        /// <summary>
        /// 更新用户头像路径到数据库
        /// </summary>
        private async Task UpdateUserAvatarPath(int userId, string avatarPath)
        {
            try
            {
                // 这里需要根据你的用户表结构来更新
                // 示例代码，需要根据实际表结构调整
                /*
                var user = await _dbContext.Users.FindAsync(userId);
                if (user != null)
                {
                    user.AvatarPath = avatarPath;
                    user.UpdateTime = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("更新用户头像路径成功: 用户ID={UserId}, 路径={AvatarPath}", userId, avatarPath);
                }
                */

                _logger.LogInformation("待实现: 更新用户头像路径，用户ID={UserId}, 路径={AvatarPath}", userId, avatarPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户头像路径失败: 用户ID={UserId}", userId);
                // 不抛出异常，因为文件上传已经成功，只是数据库更新失败
            }
        }

        /// <summary>
        /// 更新OC角色图片路径到数据库
        /// </summary>
        private async Task UpdateOCImagePath(int ocId, string imagePath)
        {
            try
            {
                var ocInfo = await _dbContext.OC_Infos.FindAsync(ocId);
                if (ocInfo != null)
                {
                    ocInfo.OC_image_url = imagePath;
                    ocInfo.updateTime = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("更新OC角色图片路径成功: OC ID={OCId}, 路径={ImagePath}", ocId, imagePath);
                }
                else
                {
                    _logger.LogWarning("未找到OC角色，无法更新图片路径: OC ID={OCId}", ocId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新OC角色图片路径失败: OC ID={OCId}", ocId);
            }
        }

        /// <summary>
        /// 记录图库图片到数据库
        /// </summary>
        private async Task RecordGalleryImages(int ocId, List<FileUploadResult> successUploads)
        {
            try
            {
                // 这里可以根据需要创建图库记录表
                // 示例代码，需要根据实际需求实现
                /*
                foreach (var file in successUploads)
                {
                    var galleryImage = new OC_Gallery
                    {
                        OCId = ocId,
                        ImagePath = file.FilePath,
                        FileName = file.FileName,
                        FileSize = file.FileSize,
                        CreateTime = DateTime.UtcNow
                    };
                    _dbContext.OC_Galleries.Add(galleryImage);
                }
                await _dbContext.SaveChangesAsync();
                */

                _logger.LogInformation("待实现: 记录图库图片，OC ID={OCId}, 图片数量={Count}", ocId, successUploads.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录图库图片失败: OC ID={OCId}", ocId);
            }
        }

        /// <summary>
        /// 清除数据库中的文件引用
        /// </summary>
        private async Task ClearFileReferenceInDatabase(string filePath)
        {
            try
            {
                // 查找并清除OC角色表中的图片引用
                var ocWithImage = await _dbContext.OC_Infos
                    .FirstOrDefaultAsync(o => o.OC_image_url == filePath);

                if (ocWithImage != null)
                {
                    ocWithImage.OC_image_url = null;
                    ocWithImage.updateTime = DateTime.UtcNow;
                    _logger.LogInformation("清除OC角色图片引用: OC ID={OCId}, 文件路径={FilePath}", ocWithImage.id, filePath);
                }

                // 可以在这里添加其他表的清理逻辑
                // 比如用户头像、图库记录等

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("数据库文件引用清理完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清除数据库文件引用失败: {FilePath}", filePath);
            }
        }

        #endregion
    }
}