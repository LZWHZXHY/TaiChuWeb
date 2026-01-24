<template>
  <section class="art-section">
    <div class="section-header">
      <h2>GALLERY_ARCHIVE // 创作收录</h2>
      
      <div class="header-actions">
        <button 
          class="cyber-btn refresh-btn" 
          @click="fetchRandomArtworks" 
          :disabled="loading"
        >
          <span class="btn-icon" :class="{ spinning: loading }">↻</span>
          <span class="btn-text">RANDOMIZE</span>
        </button>

        <button class="cyber-btn view-all-btn" @click="goToWorkCenter">
          <span class="btn-text">VIEW ALL</span>
          <span class="btn-icon">→</span>
        </button>
      </div>
    </div>

    <div v-if="loading && artworks.length === 0" class="loading-state">
      LOADING ARCHIVES...
    </div>

    <div v-else class="masonry-container" :class="{ 'opacity-50': loading }">
      <div v-for="(art, index) in artworks" :key="art.id || index" class="art-item">
        <div class="img-wrapper">
          <img :src="art.url" loading="lazy" :alt="art.title" @error="handleImgError" />
          <div class="hover-info">
            <span class="art-title">{{ art.title || '无题' }}</span>
            <span class="art-author">@{{ art.authorName || '佚名' }}</span>
          </div>
        </div>
      </div>
    </div>
    
    <div v-if="!loading && artworks.length === 0" class="empty-state">
      暂无收录作品
    </div>
  </section>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router'; // 1. 引入路由
import apiClient from '@/utils/api'; 

const router = useRouter(); // 2. 初始化路由
const artworks = ref([]);
const loading = ref(false);

// 跳转到画廊
const goToWorkCenter = () => {
  router.push('/WorkCenter');
};

// 获取随机作品
const fetchRandomArtworks = async () => {
  // 如果已经在加载中，防止重复点击
  if (loading.value) return;
  
  loading.value = true;
  try {
    // 3. 核心修改：加入 _t 时间戳参数，强制浏览器不读取缓存，实现真实随机刷新
    const res = await apiClient.get('/Drawing/random', {
      params: {
        count: 10,
        _t: new Date().getTime() 
      }
    });
    
    if (res.data.success) {
      artworks.value = res.data.data;
    }
  } catch (error) {
    console.error('获取作品墙失败:', error);
  } finally {
    loading.value = false;
  }
};

const handleImgError = (e) => {
  // 这里建议换成你项目里真实的占位图路径
  e.target.src = './public/土豆.jpg'; 
};

onMounted(() => {
  fetchRandomArtworks();
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.art-section {
  width: 100%; max-width: 1400px; margin: 80px auto; padding: 0 20px;
}

/* --- 头部布局修改 --- */
.section-header { 
  margin-bottom: 30px; 
  border-bottom: 1px solid #333; 
  padding-bottom: 15px; 
  display: flex; /* 开启 Flex 布局 */
  justify-content: space-between; /* 左右分布 */
  align-items: center; /* 垂直居中 */
  flex-wrap: wrap; /* 小屏幕允许换行 */
  gap: 20px;
}

.section-header h2 { 
  font-family: 'Anton'; 
  color: #fff; 
  font-size: 2rem; 
  letter-spacing: 1px; 
  margin: 0;
}

/* --- 按钮样式 --- */
.header-actions {
  display: flex;
  gap: 15px;
}

.cyber-btn {
  background: transparent;
  border: 1px solid #444;
  color: #888;
  font-family: 'JetBrains Mono', monospace;
  font-size: 12px;
  padding: 8px 16px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.3s ease;
  outline: none;
}

.cyber-btn:hover {
  border-color: #fff;
  color: #fff;
  background: rgba(255,255,255,0.05);
}

.cyber-btn:active {
  transform: translateY(2px);
}

/* 随机按钮特有样式 */
.refresh-btn:hover {
  border-color: #00F0FF;
  color: #00F0FF;
  box-shadow: 0 0 10px rgba(0, 240, 255, 0.2);
}

/* 查看全部按钮特有样式 */
.view-all-btn:hover {
  border-color: #7000FF;
  color: #7000FF;
  box-shadow: 0 0 10px rgba(112, 0, 255, 0.2);
}

/* 图标旋转动画 */
.spinning {
  animation: spin 1s linear infinite;
  display: inline-block;
}

@keyframes spin { 100% { transform: rotate(360deg); } }

/* --- 瀑布流内容 --- */
.masonry-container {
  column-count: 4; 
  column-gap: 20px;
  transition: opacity 0.3s;
}

/* 刷新时降低透明度，提供视觉反馈 */
.opacity-50 {
  opacity: 0.5;
  pointer-events: none;
}

.art-item {
  break-inside: avoid; 
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

.loading-state, .empty-state {
  text-align: center;
  color: #666;
  padding: 40px;
  font-family: 'JetBrains Mono';
}

@media (max-width: 1024px) { .masonry-container { column-count: 3; } }
@media (max-width: 768px) { 
  .masonry-container { column-count: 2; } 
  .section-header h2 { font-size: 1.5rem; }
  .cyber-btn { padding: 6px 12px; font-size: 10px; }
}
</style>