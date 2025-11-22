// utils/image-utils.js
export function optimizeImageUrl(url, options = {}) {
  if (!url) return '/placeholder-avatar.jpg';
  
  // 已经是完整URL
  if (url.startsWith('http')) {
    return addOptimizationParams(url, options);
  }
  
  // 处理相对路径
  const baseUrl = options.baseUrl || 'https://your-cdn.com';
  const fullUrl = `${baseUrl}/${url.replace(/^\//, '')}`;
  
  return addOptimizationParams(fullUrl, options);
}

function addOptimizationParams(url, { width, height, quality, format }) {
  const parsed = new URL(url);
  const params = parsed.searchParams;
  
  if (width) params.set('w', width);
  if (height) params.set('h', height);
  if (quality) params.set('q', quality);
  if (format) params.set('fm', format);
  
  return parsed.toString();
}