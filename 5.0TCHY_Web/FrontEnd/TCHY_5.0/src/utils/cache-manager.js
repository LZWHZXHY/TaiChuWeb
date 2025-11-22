// utils/cache-manager.js
export async function clearImageCache() {
  const cache = await caches.open('oc-images-v4');
  const requests = await cache.keys();
  await Promise.all(requests.map(req => cache.delete(req)));
}

export async function getCacheStats() {
  const cache = await caches.open('oc-images-v4');
  const requests = await cache.keys();
  
  let totalSize = 0;
  const stats = await Promise.all(
    requests.map(async req => {
      const res = await cache.match(req);
      const blob = await res.blob();
      totalSize += blob.size;
      return {
        url: req.url,
        size: blob.size,
        type: blob.type
      };
    })
  );
  
  return {
    count: requests.length,
    totalSize: formatBytes(totalSize),
    images: stats
  };
}

function formatBytes(bytes) {
  const units = ['B', 'KB', 'MB', 'GB'];
  let size = bytes;
  let unit = 0;
  
  while (size >= 1024 && unit < units.length - 1) {
    size /= 1024;
    unit++;
  }
  
  return `${size.toFixed(2)} ${units[unit]}`;
}