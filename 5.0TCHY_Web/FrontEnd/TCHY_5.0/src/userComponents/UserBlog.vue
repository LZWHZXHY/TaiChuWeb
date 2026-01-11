<template>
  <div class="page-wrapper">
    <div class="fixed-bg"></div>

    <div class="hero-banner">
      <div class="banner-overlay"></div>
      <div class="banner-content">
        <h2 class="slogan">"在代码与魔法的间隙中，寻找世界的真理。"</h2>
      </div>
    </div>

    <div class="main-container">
      <a-row :gutter="32">
        <a-col :xs="24" :lg="17">
          
          <div class="profile-card glass-effect">
            <div class="profile-layout">
              <div class="avatar-wrapper">
                <a-avatar :size="100" :src="blogger.avatar" class="user-avatar" />
                <div class="online-status" v-if="isOwner"></div>
              </div>
              <div class="profile-info">
                <div class="name-row">
                  <h1 class="username">{{ blogger.name }}</h1>
                  <a-tag color="cyan" class="role-tag">LV.99 架构师</a-tag>
                </div>
                <p class="bio">{{ blogger.bio }}</p>
                
                <div class="stats-row">
                  <div class="stat-item">
                    <span class="num">{{ blogger.articleCount }}</span>
                    <span class="label">文章</span>
                  </div>
                  <div class="stat-item">
                    <span class="num">{{ blogger.totalViews }}</span>
                    <span class="label">热度</span>
                  </div>
                  <div class="stat-item">
                    <span class="num">{{ blogger.totalLikes }}</span>
                    <span class="label">认可</span>
                  </div>
                </div>
              </div>

              <div class="profile-actions">
                <template v-if="isOwner">
                  <a-button type="primary" shape="round" size="large" class="action-btn gradient-btn" @click="router.push('/dashboard/editor')">
                    <EditOutlined /> 撰写笔记
                  </a-button>
                  <a-button shape="circle" size="large" class="icon-btn" @click="router.push('/dashboard/posts')">
                    <SettingOutlined />
                  </a-button>
                </template>
                <template v-else>
                  <a-button type="primary" shape="round" size="large" class="action-btn gradient-btn">
                    <PlusOutlined /> 关注
                  </a-button>
                </template>
              </div>
            </div>
          </div>

          <div class="article-section">
            <a-tabs v-model:activeKey="activeTab" class="custom-tabs">
              <a-tab-pane key="latest" tab="最新收录" />
              <a-tab-pane key="hot" tab="殿堂精选" />
            </a-tabs>

            <transition-group name="list" tag="div">
              <div v-for="item in articleList" :key="item.id" class="article-card glass-effect" @click="router.push(`/post/${item.id}`)">
                <div class="card-cover" v-if="item.coverImage" :style="{ backgroundImage: `url(${item.coverImage})` }"></div>
                
                <div class="card-content">
                  <div class="card-meta-top">
                    <span class="time">{{ item.publishTime }}</span>
                    <span class="category-dot"></span>
                    <span class="category-text">UE5 开发</span>
                  </div>
                  <h3 class="card-title">{{ item.title }}</h3>
                  <p class="card-summary">{{ item.summary }}</p>
                  
                  <div class="card-footer">
                    <div class="tags">
                      <span v-for="tag in item.tags" :key="tag" class="mini-tag"># {{ tag }}</span>
                    </div>
                    <div class="interactions">
                      <span><EyeOutlined /> {{ item.viewCount }}</span>
                      <span><LikeOutlined /> {{ item.likeCount }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </transition-group>
          </div>
        </a-col>

        <a-col :xs="0" :lg="7">
          <div class="sticky-sidebar">
            <div class="sidebar-widget glass-effect search-widget">
              <a-input-search placeholder="检索数据库..." size="large" :bordered="false" class="custom-search" />
            </div>

            <div class="sidebar-widget glass-effect">
              <h4 class="widget-title">知识图谱</h4>
              <div class="category-list">
                <div v-for="cat in categories" :key="cat.name" class="cat-item">
                  <span class="cat-name">{{ cat.name }}</span>
                  <span class="cat-count">{{ cat.count }}</span>
                </div>
              </div>
            </div>

            <div class="sidebar-widget glass-effect">
              <h4 class="widget-title">灵感碎片</h4>
              <div class="tag-cloud">
                <span v-for="tag in hotTags" :key="tag" class="cloud-tag">{{ tag }}</span>
              </div>
            </div>
          </div>
        </a-col>
      </a-row>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { 
  EditOutlined, SettingOutlined, PlusOutlined, EyeOutlined, LikeOutlined 
} from '@ant-design/icons-vue';

const router = useRouter();
const loading = ref(false);
const activeTab = ref('latest');
const currentUserId = ref(1001); 

// --- Data (保持原有逻辑，数据略) ---
// 为了节省篇幅，这里复用之前的数据结构，
// 但博主名字和简介我改成更有“感觉”的文案。
const blogger = reactive({
  id: 1001, 
  name: 'SkyChen',
  avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=Felix', 
  bio: 'Lv.99 独立游戏开发者 | 正在构建代号"Aether"的异世界',
  articleCount: 142,
  totalViews: '23.5k',
  totalLikes: 4890
});

const isOwner = computed(() => blogger.id === currentUserId.value);

const articleList = ref([]);
const categories = ref([]);
const hotTags = ref([]);

const fetchBlogData = async () => {
  // ... 复用之前的 fetchBlogData 逻辑，生成数据 ...
  // 这里为了演示，手动填几个数据
  articleList.value = [
    {
      id: 1,
      title: '虚幻5 Lumen 光照系统的性能黑魔法',
      summary: '在构建地下城场景时，我发现 Lumen 的开销主要来自于 Mesh Distance Fields。通过调整体素密度，我成功将帧率从 45 提升到了 80...',
      coverImage: 'https://picsum.photos/id/122/600/300', 
      publishTime: '2025-12-16',
      tags: ['UE5', 'Rendering'],
      viewCount: 1204, likeCount: 88
    },
    {
      id: 2,
      title: '关于“间隙粒子”的物理规则推演',
      summary: '如果世界由一种负质量粒子构成，那么浮空岛的重力反转就不再是魔法，而是物理。本文尝试建立一个数学模型...',
      coverImage: 'https://picsum.photos/id/234/600/300',
      publishTime: '2025-12-14',
      tags: ['WorldBuild', 'Lore'],
      viewCount: 890, likeCount: 120
    }
  ];
  categories.value = [{name:'UE5', count:45}, {name:'世界观', count:20}];
  hotTags.value = ['UE5', 'Shader', 'Magic', 'Vue3'];
};

onMounted(() => fetchBlogData());
</script>

<style scoped>
/* === 核心背景设定 === */
.page-wrapper {
  min-height: 100vh;
  position: relative;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
  color: #2c3e50;
}

.fixed-bg {
  position: fixed;
  top: 0; left: 0; width: 100%; height: 100%;
  /* 一个很有质感的浅色渐变背景 */
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  z-index: -1;
}

/* === 顶部 Hero Banner === */
.hero-banner {
  height: 360px;
  position: relative;
  /* 换一张更有“异世界”感觉的图 */
  background-image: url('https://images.unsplash.com/photo-1519681393784-d120267933ba?auto=format&fit=crop&w=1920&q=80');
  background-size: cover;
  background-position: center;
  display: flex;
  align-items: center;
  justify-content: center;
}
.banner-overlay {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  background: linear-gradient(to bottom, rgba(0,0,0,0.3), rgba(0,0,0,0.6));
}
.slogan {
  position: relative;
  color: rgba(255,255,255,0.9);
  font-size: 28px;
  font-weight: 300;
  letter-spacing: 2px;
  text-shadow: 0 2px 10px rgba(0,0,0,0.3);
  font-style: italic;
  font-family: "Georgia", serif;
}

/* === 主容器布局 === */
.main-container {
  max-width: 1100px;
  margin: 0 auto;
  padding: 0 20px;
  position: relative;
  margin-top: -80px; /* 关键：让内容向上重叠 Banner */
  padding-bottom: 60px;
}

/* === 通用毛玻璃卡片 === */
.glass-effect {
  background: rgba(255, 255, 255, 0.85); /* 半透明白 */
  backdrop-filter: blur(12px); /* 核心：背景模糊 */
  -webkit-backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.6);
  border-radius: 16px;
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.07);
}

/* === 3. 博主信息卡片 === */
.profile-card {
  padding: 24px 32px;
  margin-bottom: 32px;
  position: relative;
  overflow: hidden;
}
.profile-layout {
  display: flex;
  align-items: flex-start; /* 改为顶部对齐 */
  gap: 24px;
}
.user-avatar {
  border: 4px solid rgba(255,255,255,0.8);
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}
.profile-info {
  flex: 1;
  padding-top: 8px;
}
.name-row {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;
}
.username {
  font-size: 26px;
  font-weight: 800;
  margin: 0;
  color: #1a1a1a;
}
.role-tag {
  border: none;
  font-weight: 600;
}
.bio {
  color: #666;
  margin-bottom: 16px;
  font-size: 15px;
  line-height: 1.5;
}
.stats-row {
  display: flex;
  gap: 32px;
}
.stat-item {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}
.stat-item .num {
  font-size: 18px;
  font-weight: 700;
  color: #1a1a1a;
}
.stat-item .label {
  font-size: 12px;
  color: #999;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.profile-actions {
  padding-top: 10px;
  display: flex;
  gap: 12px;
}
.gradient-btn {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  box-shadow: 0 4px 15px rgba(118, 75, 162, 0.4);
  transition: transform 0.2s, box-shadow 0.2s;
}
.gradient-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(118, 75, 162, 0.6);
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); /* 防止AntD hover覆盖 */
}

/* === 4. 文章卡片 === */
.article-section {
  /* Animation container */
}
.custom-tabs {
  margin-bottom: 20px;
}
:deep(.ant-tabs-nav::before) {
  border-bottom: none; /* 去掉丑陋的灰线 */
}

.article-card {
  display: flex;
  margin-bottom: 24px;
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
  cursor: pointer;
  overflow: hidden;
  height: 200px; /* 固定高度 */
}
.article-card:hover {
  transform: translateY(-5px) scale(1.01);
  box-shadow: 0 15px 40px rgba(0,0,0,0.12);
}

.card-cover {
  width: 280px;
  background-size: cover;
  background-position: center;
  flex-shrink: 0;
}
.card-content {
  padding: 24px;
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}
.card-meta-top {
  display: flex;
  align-items: center;
  font-size: 12px;
  color: #999;
  margin-bottom: 8px;
}
.category-dot {
  width: 6px; height: 6px;
  background: #764ba2;
  border-radius: 50%;
  margin: 0 8px;
}
.card-title {
  font-size: 20px;
  font-weight: 700;
  color: #2c3e50;
  margin-bottom: 8px;
  line-height: 1.4;
  /* 文字过长省略 */
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.card-summary {
  font-size: 14px;
  color: #666;
  line-height: 1.6;
  flex: 1;
  /* 文字过长省略 */
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 12px;
}
.mini-tag {
  color: #667eea;
  background: rgba(102, 126, 234, 0.1);
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 12px;
  margin-right: 8px;
}
.interactions {
  color: #bbb;
  font-size: 13px;
  display: flex;
  gap: 16px;
}

/* === 侧边栏 === */
.sticky-sidebar {
  position: sticky;
  top: 24px;
}
.sidebar-widget {
  padding: 20px;
  margin-bottom: 24px;
}
.search-widget {
  padding: 10px; /* 搜索框紧凑点 */
}
.widget-title {
  font-size: 16px;
  font-weight: 700;
  margin-bottom: 16px;
  position: relative;
  padding-left: 12px;
}
.widget-title::before {
  content: '';
  position: absolute;
  left: 0; top: 4px; bottom: 4px;
  width: 4px;
  background: linear-gradient(to bottom, #667eea, #764ba2);
  border-radius: 2px;
}
.cat-item {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
  border-bottom: 1px dashed #eee;
  color: #555;
  cursor: pointer;
  transition: color 0.2s;
}
.cat-item:hover {
  color: #764ba2;
}
.tag-cloud {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.cloud-tag {
  background: #f0f2f5;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 13px;
  color: #666;
  cursor: pointer;
  transition: all 0.2s;
}
.cloud-tag:hover {
  background: #764ba2;
  color: #fff;
}

/* === 移动端适配 === */
@media (max-width: 768px) {
  .hero-banner { height: 200px; }
  .main-container { margin-top: -40px; }
  .profile-layout { flex-direction: column; align-items: center; text-align: center; }
  .stats-row { justify-content: center; width: 100%; }
  .stat-item { align-items: center; }
  .article-card { flex-direction: column; height: auto; }
  .card-cover { width: 100%; height: 160px; }
  .profile-actions { width: 100%; justify-content: center; }
}
</style>