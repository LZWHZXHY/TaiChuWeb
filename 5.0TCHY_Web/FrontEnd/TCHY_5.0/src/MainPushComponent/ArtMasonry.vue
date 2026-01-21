<template>
  <section class="art-section">
    <div class="section-header">
      <h2>GALLERY_ARCHIVE // 创作收录</h2>
    </div>

    <div class="masonry-container">
      <div v-for="(art, index) in artworks" :key="index" class="art-item">
        <div class="img-wrapper">
          <img :src="art.url" loading="lazy" />
          <div class="hover-info">
            <span class="art-title">{{ art.title }}</span>
            <span class="art-author">@{{ art.author }}</span>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
// 注意：找一些长宽比不一样的图
const artworks = [
  { title: "赛博少女", author: "Aya", url: "https://images.unsplash.com/photo-1634152962476-4b8a00e1915c?w=600&q=80" },
  { title: "机械心脏", author: "Mech", url: "https://images.unsplash.com/photo-1535378437327-1e54142ed98e?w=600&q=80" },
  { title: "霓虹雨巷", author: "City", url: "https://images.unsplash.com/photo-1555680202-c86f0e12f086?w=600&q=80" },
  { title: "故障艺术", author: "Glitch", url: "https://images.unsplash.com/photo-1550745165-9bc0b252726f?w=600&q=80" },
  { title: "未来接口", author: "UI_Dev", url: "https://images.unsplash.com/photo-1515524738708-327f6b0037a7?w=600&q=80" },
  { title: "深空探测", author: "Space", url: "https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=600&q=80" },
];
</script>

<style scoped>
.art-section {
  width: 100%; max-width: 1400px; margin: 80px auto; padding: 0 20px;
}
.section-header { margin-bottom: 30px; border-bottom: 1px solid #333; padding-bottom: 10px; }
.section-header h2 { font-family: 'Anton'; color: #fff; font-size: 2rem; letter-spacing: 1px; }

/* 瀑布流核心 CSS */
.masonry-container {
  column-count: 4; /* 默认4列 */
  column-gap: 20px;
}

.art-item {
  break-inside: avoid; /* 防止卡片断裂 */
  margin-bottom: 20px;
  position: relative;
}

.img-wrapper {
  position: relative;
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid rgba(255,255,255,0.1);
  background: #111;
  transition: 0.3s;
}
.img-wrapper:hover { transform: scale(1.02); border-color: #7000FF; box-shadow: 0 10px 30px rgba(112, 0, 255, 0.2); z-index: 2; }

.img-wrapper img { width: 100%; height: auto; display: block; }

.hover-info {
  position: absolute; inset: 0;
  background: linear-gradient(to top, rgba(0,0,0,0.9), transparent);
  display: flex; flex-direction: column; justify-content: flex-end; padding: 15px;
  opacity: 0; transition: 0.3s;
}
.img-wrapper:hover .hover-info { opacity: 1; }

.art-title { color: #fff; font-weight: bold; font-size: 14px; }
.art-author { color: #7000FF; font-family: 'JetBrains Mono'; font-size: 12px; margin-top: 5px; }

/* 响应式列数 */
@media (max-width: 1024px) { .masonry-container { column-count: 3; } }
@media (max-width: 768px) { .masonry-container { column-count: 2; } }
</style>