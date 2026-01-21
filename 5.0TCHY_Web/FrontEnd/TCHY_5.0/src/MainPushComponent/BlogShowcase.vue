<template>
  <section class="blog-section">
    
    <div class="section-header">
      <div class="header-left">
        <h2 class="title">NEURAL_ARCHIVE</h2>
        <span class="subtitle">深度文章与技术文档</span>
      </div>
      <div class="header-right">
        <button class="view-all-btn">VIEW_ALL_LOGS <span class="arrow">>></span></button>
      </div>
    </div>

    <div class="blog-grid">
      
      <div 
        v-for="(post, index) in blogPosts" 
        :key="index"
        class="blog-card"
        :class="{ 'featured': index === 0 }" 
      >
        <div class="corner-decor tl"></div>
        <div class="corner-decor tr"></div>
        <div class="corner-decor bl"></div>
        <div class="corner-decor br"></div>

        <div class="card-visual">
          <img :src="post.image" loading="lazy" />
          <div class="overlay">
            <span class="read-icon">[ READ ]</span>
          </div>
          <div class="category-tag">{{ post.category }}</div>
        </div>

        <div class="card-content">
          <div class="meta-row">
            <span class="date">{{ post.date }}</span>
            <span class="id-code">LOG_0{{ index + 1 }}</span>
          </div>
          
          <h3 class="post-title">{{ post.title }}</h3>
          <p class="post-excerpt">{{ post.excerpt }}</p>
          
          <div class="card-footer">
            <div class="author">
              <div class="avatar"></div>
              <span>{{ post.author }}</span>
            </div>
            <div class="stats">
              <span><i class="icon">👁</i> {{ post.views }}</span>
              <span><i class="icon">♥</i> {{ post.likes }}</span>
            </div>
          </div>
        </div>
      </div>

    </div>
  </section>
</template>

<script setup>
import { ref } from 'vue';

// --- Mock Data ---
const blogPosts = ref([
  {
    title: "元宇宙架构：从 WebGL 到 WebGPU 的演进之路",
    excerpt: "深入探讨下一代图形渲染技术如何重塑浏览器端的 3D 体验，以及这对前端开发者意味着什么。",
    category: "TECH_DEEP_DIVE",
    author: "Code_Master",
    date: "2026.05.12",
    views: "12k",
    likes: "890",
    image: "https://images.unsplash.com/photo-1550751827-4bd374c3f58b?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80"
  },
  {
    title: "赛博朋克视觉指南：色彩与构图分析",
    excerpt: "为什么霓虹粉和青色是绝配？拆解经典赛博朋克电影的视觉语言。",
    category: "DESIGN_ART",
    author: "Neon_Artist",
    date: "2026.05.10",
    views: "8.5k",
    likes: "1.2k",
    image: "https://images.unsplash.com/photo-1515630278258-407f66498911?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80"
  },
  {
    title: "独立游戏开发日志 #42：粒子系统的优化",
    excerpt: "如何在保持 60FPS 的前提下渲染百万级粒子？我的 ECS 架构实践心得。",
    category: "GAME_DEV",
    author: "Indie_Dev",
    date: "2026.05.08",
    views: "5k",
    likes: "450",
    image: "https://images.unsplash.com/photo-1552820728-8b83bb6b773f?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80"
  },
  {
    title: "未来接口：脑机接口 UI 设计规范猜想",
    excerpt: "当屏幕消失，我们将如何交互？关于 Neuralink 时代的用户体验设计思考。",
    category: "FUTURISM",
    author: "UX_2077",
    date: "2026.05.05",
    views: "15k",
    likes: "2k",
    image: "https://images.unsplash.com/photo-1515524738708-327f6b0037a7?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80"
  }
]);
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&family=Noto+Sans+SC:wght@400;700&display=swap');

.blog-section {
  width: 100%;
  max-width: 1400px;
  margin: 80px auto; /* 上下留白 */
  padding: 0 20px;
  color: #fff;
  font-family: 'Noto Sans SC', sans-serif;
}

/* --- 顶部标题栏 --- */
.section-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  margin-bottom: 40px;
  border-bottom: 1px solid rgba(255,255,255,0.1);
  padding-bottom: 15px;
}

.title {
  font-family: 'Anton', sans-serif;
  font-size: 3rem;
  line-height: 1;
  background: linear-gradient(90deg, #fff, #999);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  margin-bottom: 5px;
}

.subtitle {
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.9rem;
  color: #00F0FF;
  letter-spacing: 1px;
}

.view-all-btn {
  background: transparent;
  border: 1px solid rgba(255,255,255,0.2);
  color: #aaa;
  padding: 8px 20px;
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.8rem;
  cursor: pointer;
  transition: 0.3s;
}
.view-all-btn:hover {
  border-color: #00F0FF;
  color: #00F0FF;
  box-shadow: 0 0 10px rgba(0, 240, 255, 0.2);
}

/* --- 网格布局 --- */
.blog-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 30px;
}

/* --- 卡片样式 --- */
.blog-card {
  position: relative;
  background: rgba(20, 20, 20, 0.6);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.05);
  display: flex;
  flex-direction: column;
  transition: all 0.4s cubic-bezier(0.23, 1, 0.32, 1);
  cursor: pointer;
  overflow: hidden;
}

/* 第一张卡片横跨两列 (Featured) */
@media (min-width: 900px) {
  .blog-card.featured {
    grid-column: span 2;
    flex-direction: row;
  }
  .blog-card.featured .card-visual {
    width: 50%;
    height: auto;
  }
  .blog-card.featured .card-content {
    width: 50%;
  }
}

.blog-card:hover {
  transform: translateY(-5px);
  border-color: rgba(0, 240, 255, 0.5);
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.5), 0 0 20px rgba(0, 240, 255, 0.1);
}

/* 角落装饰 */
.corner-decor {
  position: absolute;
  width: 10px;
  height: 10px;
  border: 2px solid transparent;
  transition: 0.3s;
  z-index: 2;
}
.blog-card:hover .corner-decor { border-color: #00F0FF; }
.tl { top: 0; left: 0; border-top: 2px solid rgba(255,255,255,0.2); border-left: 2px solid rgba(255,255,255,0.2); }
.tr { top: 0; right: 0; border-top: 2px solid rgba(255,255,255,0.2); border-right: 2px solid rgba(255,255,255,0.2); }
.bl { bottom: 0; left: 0; border-bottom: 2px solid rgba(255,255,255,0.2); border-left: 2px solid rgba(255,255,255,0.2); }
.br { bottom: 0; right: 0; border-bottom: 2px solid rgba(255,255,255,0.2); border-right: 2px solid rgba(255,255,255,0.2); }

/* 图片部分 */
.card-visual {
  position: relative;
  height: 200px;
  overflow: hidden;
}
.card-visual img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: 0.6s;
  filter: grayscale(0.3);
}
.blog-card:hover .card-visual img {
  transform: scale(1.05);
  filter: grayscale(0);
}

.category-tag {
  position: absolute;
  top: 10px;
  left: 10px;
  background: rgba(0,0,0,0.8);
  color: #00F0FF;
  font-family: 'JetBrains Mono';
  font-size: 0.7rem;
  padding: 4px 8px;
  border: 1px solid #00F0FF;
}

.overlay {
  position: absolute;
  inset: 0;
  background: rgba(0,0,0,0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: 0.3s;
}
.blog-card:hover .overlay { opacity: 1; }
.read-icon {
  color: #fff;
  font-family: 'JetBrains Mono';
  font-weight: bold;
  letter-spacing: 2px;
  border: 1px solid #fff;
  padding: 5px 15px;
}

/* 内容部分 */
.card-content {
  padding: 20px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  flex: 1;
}

.meta-row {
  display: flex;
  justify-content: space-between;
  font-family: 'JetBrains Mono';
  font-size: 0.75rem;
  color: #666;
  margin-bottom: 10px;
}
.id-code { color: #00F0FF; }

.post-title {
  font-size: 1.2rem;
  font-weight: 700;
  margin-bottom: 10px;
  line-height: 1.4;
  color: #eee;
}
.blog-card:hover .post-title { color: #00F0FF; }

.post-excerpt {
  font-size: 0.9rem;
  color: #aaa;
  margin-bottom: 20px;
  line-height: 1.6;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.card-footer {
  margin-top: auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-top: 1px solid rgba(255,255,255,0.1);
  padding-top: 15px;
  font-size: 0.8rem;
  color: #888;
}

.author {
  display: flex;
  align-items: center;
  gap: 8px;
}
.avatar {
  width: 20px;
  height: 20px;
  background: linear-gradient(45deg, #00F0FF, #7000FF);
  border-radius: 2px;
}

.stats {
  font-family: 'JetBrains Mono';
  display: flex;
  gap: 15px;
}
.stats .icon { margin-right: 4px; }
</style>