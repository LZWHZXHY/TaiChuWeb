<template>
  <div class="settings-container">
    
    <div v-if="!selectedIpId" class="ip-selection-view">
      <div class="header"><h2>📚 设定资料库</h2></div>
      <div v-if="loading" class="loading-text">加载中...</div>
      <div v-else class="ip-grid">
        <div v-for="ip in ipList" :key="ip.id" class="ip-card" @click="selectIp(ip)">
           <h3>{{ ip.name }}</h3>
        </div>
      </div>
    </div>

    <div v-else class="detail-view-wrapper">
      
      <div class="main-toolbar">
        <div class="left">
          <button class="nav-btn back" @click="backToList">⬅️ 返回列表</button>
          <span class="world-name">🌌 {{ selectedIpName }}</span>
        </div>
        
        <div class="view-switcher">
          <button 
            :class="['switch-btn', { active: viewType === 'graph' }]" 
            @click="viewType = 'graph'"
          >
            🕸️ 全息图谱
          </button>
          <button 
            :class="['switch-btn', { active: viewType === 'doc' }]" 
            @click="viewType = 'doc'"
          >
            📄 档案文档
          </button>
          <button 
              :class="['switch-btn', { active: viewType === 'timeline' }]" 
              @click="viewType = 'timeline'"
            >⏳ 编年史记</button>
        </div>
      </div>

      <div class="view-content">
        <keep-alive>
          <GraphView v-if="viewType === 'graph'" :ipId="selectedIpId" />
        </keep-alive>
        
        <DocView v-if="viewType === 'doc'" :ipId="selectedIpId" />
        
        <ChronicleView v-if="viewType === 'timeline'" :ipId="selectedIpId" />
      </div>

    </div>

  </div>
</template>

<script>
import apiClient from '@/utils/api';
import GraphView from '@/TCHYComponents/Library/GraphView.vue'; // 引入新组件
import DocView from '@/TCHYComponents/Library/DocView.vue';     // 引入新组件
import ChronicleView from './Library/ChronicleView.vue';

export default {
  name: 'SettingsComponent',
  components: {
    GraphView,
    DocView,
    ChronicleView
  },
  data() {
    return {
      loading: false,
      ipList: [],
      selectedIpId: null,
      selectedIpName: '',
      
      // 控制当前的子视图 'graph' | 'doc'
      viewType: 'graph' 
    };
  },
  mounted() {
    this.fetchIpList();
  },
  methods: {
    async fetchIpList() {
      this.loading = true;
      try {
        const res = await apiClient.get('/IP');
        this.ipList = res.data;
      } catch (error) { console.error(error); } 
      finally { this.loading = false; }
    },
    // ... getCoverStyle 等辅助函数 ...
    
    selectIp(ip) {
      this.selectedIpId = ip.id;
      this.selectedIpName = ip.name;
      this.viewType = 'graph'; // 默认进入图谱模式
    },
    backToList() {
      this.selectedIpId = null;
      this.selectedIpName = '';
    }
  }
};
</script>

<style scoped>
.settings-container { width: 100%; height: 90vh; background: #f4f6f9; display: flex; flex-direction: column; }

/* 详情页包装器 */
.detail-view-wrapper { width: 100%; height: 100%; display: flex; flex-direction: column; }

.main-toolbar {
  height: 60px; background: white; border-bottom: 1px solid #ddd;
  display: flex; align-items: center; justify-content: space-between; padding: 0 20px;
  flex-shrink: 0; /* 防止被挤压 */
}

.left { display: flex; align-items: center; gap: 15px; }
.world-name { font-weight: bold; font-size: 1.1rem; color: #333; }
.nav-btn { border: 1px solid #ddd; background: transparent; padding: 6px 12px; border-radius: 4px; cursor: pointer; }
.nav-btn:hover { background: #f5f5f5; }

/* 切换器样式 */
.view-switcher { background: #f0f2f5; padding: 4px; border-radius: 6px; display: flex; gap: 4px; }
.switch-btn { border: none; background: transparent; padding: 6px 16px; border-radius: 4px; cursor: pointer; font-size: 0.9rem; color: #666; font-weight: 500; transition: 0.2s; }
.switch-btn.active { background: white; color: #1890ff; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }

.view-content { flex: 1; overflow: hidden; position: relative; }

/* 复用之前的 IP 列表样式 */
.ip-grid { padding: 20px; display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 20px; }
.ip-card { background: white; border-radius: 8px; cursor: pointer; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.05); }
.card-cover { height: 120px; background: #34495e; }
.card-info { padding: 15px; text-align: center; }
</style>