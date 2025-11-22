public class ImageUrlService
{
    private readonly IConfiguration _configuration;

    public ImageUrlService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// 构建完整的图片URL（统一处理所有表的图片地址）
    /// </summary>
    /// <param name="relativePath">数据库中的相对路径</param>
    /// <returns>完整的图片URL</returns>
    public string BuildImageUrl(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return string.Empty;

        // 统一使用NAS地址
        var nasBaseUrl = _configuration["StorageSettings:NasEndpoint"];
        return $"{nasBaseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
    }

    /// <summary>
    /// 批量构建图片URL（用于列表数据）
    /// </summary>
    public List<string> BuildImageUrls(List<string> relativePaths)
    {
        return relativePaths.Select(BuildImageUrl).ToList();
    }

    /// <summary>
    /// 处理可能包含占位符或默认图片的情况
    /// </summary>
    public string BuildImageUrlWithFallback(string relativePath, string fallbackImage = "/default.jpg")
    {
        if (string.IsNullOrEmpty(relativePath) || relativePath == "default" || relativePath == "placeholder")
            return BuildImageUrl(fallbackImage);

        return BuildImageUrl(relativePath);
    }
}