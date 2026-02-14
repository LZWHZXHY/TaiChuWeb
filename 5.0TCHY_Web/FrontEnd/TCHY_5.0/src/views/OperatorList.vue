<template>
  <div class="operators-page">
    <div class="page-header">
      <h1 class="title">// 成员数据库</h1>
      <div class="stats-group">
        <div class="stat-item active">
          <span class="label">ACTIVE:</span>
          <span class="value">{{ activeUsers.length }}</span>
        </div>
        <div class="stat-item separator">/</div>
        <div class="stat-item inactive">
          <span class="label">INACTIVE:</span>
          <span class="value">{{ inactiveUsers.length }}</span>
        </div>
      </div>
    </div>

    <div v-if="loading" class="loading-state">
      <div class="scanner">SCANNING_NETWORK_NODES...</div>
      <div class="progress-bar"></div>
    </div>

    <div v-else class="content-wrapper">
      <section v-if="activeUsers.length > 0" class="operator-section">
        <h2 class="section-title">/// 活跃操作员_ACTIVE_NODES</h2>
        <div class="avatar-grid">
          <div v-for="user in activeUsers" :key="user.Id" class="avatar-item active-node">
            <CyberAvatar 
              :userId="user.Id" 
              :passedAvatar="user.Avatar"
              :passedLevel="user.Level"
              :showLevel="true"
            />
            <div class="user-info">
              <span class="u-name">{{ user.Username }}</span>
              <span class="u-id">ID: #{{ String(user.Id).padStart(4, '0') }}</span>
            </div>
          </div>
        </div>
      </section>

      <section v-if="inactiveUsers.length > 0" class="operator-section inactive">
        <h2 class="section-title">/// 沉寂节点_INACTIVE_NODES</h2>
        <div class="avatar-grid mini">
          <div v-for="user in inactiveUsers" :key="user.Id" class="avatar-item dimmed-node">
            <CyberAvatar 
              :userId="user.Id" 
              :passedAvatar="user.Avatar"
              :passedLevel="user.Level"
              :showLevel="false" 
              :allowLink="true"
            />
            <div class="user-info">
              <span class="u-name">{{ user.Username }}</span>
              <span class="u-id">OFFLINE</span>
            </div>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import apiClient from '@/utils/api'
import CyberAvatar from '@/GeneralComponents/UserAvatar.vue' // 请根据实际路径调整

const users = ref([])
const loading = ref(true)

// 计算属性：归类头像不为空的活跃用户
const activeUsers = computed(() => {
  return users.value.filter(u => u.Avatar && u.Avatar.trim() !== '')
})

// 计算属性：归类头像为空的不活跃用户
const inactiveUsers = computed(() => {
  return users.value.filter(u => !u.Avatar || u.Avatar.trim() === '')
})

const fetchAllUsers = async () => {
  try {
    const res = await apiClient.get('/Tool/all-operators')
    if (res.data.success) {
      users.value = res.data.data
    }
  } catch (error) {
    console.error('Failed to load operators:', error)
  } finally {
    loading.value = false
  }
}

onMounted(fetchAllUsers)
</script>

<style scoped>
/* 基础容器 */
.operators-page {
  padding: 100px 40px 60px;
  min-height: 100vh;
  background: #F4F1EA;
  font-family: 'JetBrains Mono', monospace;
  color: #111;
}

/* 头部样式 */
.page-header {
  border-bottom: 4px solid #111;
  margin-bottom: 50px;
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  padding-bottom: 10px;
}

.title { font-size: 28px; margin: 0; font-weight: 800; letter-spacing: -1px; }

.stats-group {
  display: flex;
  gap: 15px;
  font-weight: 700;
  font-size: 14px;
}

.stat-item.active { color: #D92323; }
.stat-item.inactive { color: #888; }

/* 区域通用样式 */
.operator-section { margin-bottom: 80px; }
.section-title {
  font-size: 12px;
  font-weight: 700;
  margin-bottom: 30px;
  padding: 4px 12px;
  background: #111;
  color: #fff;
  display: inline-block;
  clip-path: polygon(0 0, 100% 0, 95% 100%, 0% 100%);
}

/* 网格布局 */
.avatar-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(130px, 1fr));
  gap: 40px 20px;
}

/* 活跃节点样式 */
.active-node :deep(.avatar-frame) {
  border-color: #D92323 !important;
  box-shadow: 0 0 15px rgba(217, 35, 35, 0.2);
}

/* 不活跃网格缩小 */
.avatar-grid.mini {
  grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  gap: 30px 15px;
}

.avatar-grid.mini :deep(.avatar-wrapper) {
  width: 60px;
  height: 60px;
}

/* 沉寂节点视觉效果 */
.dimmed-node {
  opacity: 0.4;
  filter: grayscale(1);
  transition: all 0.4s ease;
}

.dimmed-node:hover {
  opacity: 1;
  filter: grayscale(0);
  transform: translateY(-5px);
}

/* 用户信息 */
.user-info {
  margin-top: 12px;
  text-align: center;
  display: flex;
  flex-direction: column;
}

.u-name { font-size: 13px; font-weight: 700; margin-bottom: 2px; }
.u-id { font-size: 10px; color: #888; letter-spacing: 1px; }

/* 加载动画 */
.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding-top: 15vh;
}

.scanner {
  font-weight: 800;
  letter-spacing: 2px;
  animation: pulse 1.5s infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.3; }
}

.progress-bar {
  width: 200px;
  height: 2px;
  background: #ddd;
  margin-top: 15px;
  position: relative;
  overflow: hidden;
}

.progress-bar::after {
  content: '';
  position: absolute;
  left: -100%;
  width: 100%;
  height: 100%;
  background: #D92323;
  animation: scan 2s infinite;
}

@keyframes scan {
  100% { left: 100%; }
}
</style>