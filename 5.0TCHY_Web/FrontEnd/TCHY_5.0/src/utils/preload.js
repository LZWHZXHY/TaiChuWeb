// utils/preload.js
export function preloadImages(imageUrls) {
  imageUrls.forEach(url => {
    const link = document.createElement('link');
    link.rel = 'preload';
    link.as = 'image';
    link.href = url;
    document.head.appendChild(link);
  });
}

// 在路由导航前预加载
router.beforeEach((to, from, next) => {
  if (to.meta.preloadImages) {
    preloadImages(to.meta.preloadImages);
  }
  next();
});