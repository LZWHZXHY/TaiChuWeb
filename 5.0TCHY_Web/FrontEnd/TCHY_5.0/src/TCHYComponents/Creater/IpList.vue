<template>
  <div class="ip-list-container" :class="{ 'full-screen-mode': isProjectMode }">
    
    <div v-if="isProjectMode" class="project-view">
      <button class="back-floating-btn" @click="isProjectMode = false">
        ↩ 退出工坊
      </button>
      
      <ProjectManager />
    </div>

    <div v-else class="dashboard-view">
      <div class="header">
        <h2>太初寰宇IP世界观</h2>
        <button class="create-btn" @click="$emit('go-create')">+ 新建宇宙</button>
      </div>

      <Infos @open-project-manager="isProjectMode = true" />

      <div v-if="isLoading" class="loading">
        加载中...
      </div>

      <div v-else-if="ips.length === 0" class="empty-state">
        <p>还没有创建任何世界观，快去创建一个吧！</p>
      </div>

      <div v-else class="grid-layout">
        <div 
          v-for="ip in ips" 
          :key="ip.id" 
          class="ip-card"
          @click="handleEnterWorld(ip.id)"
        >
          <div class="card-cover" :style="getCoverStyle(ip.cover_url)">
            <span v-if="!ip.cover_url" class="placeholder-text">{{ ip.name[0] }}</span>
          </div>
          
          <div class="card-info">
            <h3>{{ ip.name }}</h3>
            <p class="tagline">{{ ip.tagline || '暂无标语' }}</p>
            <p class="summary">{{ ip.summary || '暂无简介...' }}</p>
            <div class="card-footer">
              <span class="date">更新于: {{ formatDate(ip.updateTime) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
    
  </div>
</template>

<script>
import apiClient from '@/utils/api';
import Infos from './Components/创作相关信息.vue';
import ProjectManager from './Components/ProjectManager.vue';

export default {
  name: 'IpList',
  components: {
    Infos,
    ProjectManager
  },
  data() {
    return {
      ips: [],
      isLoading: true,
      isProjectMode: false // 控制是否处于“项目管理模式”
    };
  },
  mounted() {
    this.fetchIps();
  },
  methods: {
    async fetchIps() {
      try {
        const response = await apiClient.get('/IP'); 
        this.ips = response.data;
      } catch (error) {
        console.error('获取列表失败:', error);
        alert('无法加载世界观列表');
      } finally {
        this.isLoading = false;
      }
    },
    handleEnterWorld(id) {
      this.$emit('enter-world', id);
    },
    formatDate(dateStr) {
      if (!dateStr) return '';
      const date = new Date(dateStr);
      return date.toLocaleDateString();
    },
    getCoverStyle(url) {
      if (url) {
        return { backgroundImage: `url(${url})` };
      }
      return { backgroundColor: '#34495e' };
    }
  }
};
</script>

<style scoped>
/* 默认容器样式：限制宽度，居中显示 */
.ip-list-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
  transition: all 0.3s ease; /* 增加平滑过渡 */
}

/* === 3. 新增：全屏模式样式 === */
/* 当加上这个类时，强制撑满屏幕，去掉边距 */
.ip-list-container.full-screen-mode {
  max-width: 100vw;   /* 宽度 100% 视口 */
  height: 100vh;      /* 高度 100% 视口 */
  padding: 0;         /* 去掉内边距 */
  margin: 0;          /* 去掉外边距 */
  background: #f4f6f8;/* 统一背景色 */
  overflow: hidden;   /* 禁止主页面滚动，交给内部组件滚动 */
}

/* 4. 新增：全屏模式下的内容容器 */
.project-view {
  width: 100%;
  height: 100%;
  position: relative; /* 为了定位悬浮按钮 */
}

/* 5. 新增：悬浮返回按钮样式 */
.back-floating-btn {
  position: absolute;
  bottom: 20px;       /* 放在左下角或者右上角都可以，这里放左下角 */
  left: 20px;
  z-index: 1000;      /* 保证在最上层 */
  background: rgba(44, 62, 80, 0.8); /* 半透明深色背景 */
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 30px;
  cursor: pointer;
  font-size: 0.9rem;
  font-weight: bold;
  box-shadow: 0 4px 12px rgba(0,0,0,0.2);
  backdrop-filter: blur(4px); /* 毛玻璃效果 */
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 8px;
}

.back-floating-btn:hover {
  background: #2c3e50;
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(0,0,0,0.3);
}

/* === 以下保持原有的 Dashboard 样式不变 === */

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.create-btn {
  background: #2ecc71;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: bold;
}

.grid-layout {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 20px;
}

.ip-card {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0,0,0,0.08);
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
  display: flex;
  flex-direction: column;
}

.ip-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 20px rgba(0,0,0,0.15);
}

.card-cover {
  height: 160px;
  background-size: cover;
  background-position: center;
  display: flex;
  align-items: center;
  justify-content: center;
  color: rgba(255,255,255,0.2);
  font-size: 3rem;
  font-weight: bold;
}

.card-info {
  padding: 1.2rem;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.card-info h3 {
  margin: 0 0 0.5rem 0;
  color: #2c3e50;
}

.tagline {
  font-size: 0.9rem;
  color: #e67e22;
  margin-bottom: 0.8rem;
  font-weight: 500;
}

.summary {
  font-size: 0.85rem;
  color: #7f8c8d;
  line-height: 1.5;
  margin-bottom: 1rem;
  flex: 1;
  display: -webkit-box;
  line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.card-footer {
  border-top: 1px solid #eee;
  padding-top: 10px;
  font-size: 0.8rem;
  color: #bdc3c7;
  text-align: right;
}

.empty-state {
  text-align: center;
  color: #999;
  padding: 4rem 0;
}
</style>