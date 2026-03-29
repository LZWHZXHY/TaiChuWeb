<script setup lang="ts">
import { onMounted, watch } from 'vue';
import { useOnlineStore } from '@/stores/online'; 
import apiClient from '@/utils/api'; 

// 引入通用头像组件
import UserAvatar from './UserAvatar.vue';

// --- 定义向父组件传递事件 ---
const emit = defineEmits<{
  (e: 'update-count', count: number): void;
}>();

// 实例化全局的在线状态 Store
const onlineStore = useOnlineStore();

// 监听全局在线人数的变化，同步给父组件（用于控制台终端同步显示）
watch(
  () => onlineStore.onlineCount, 
  (newCount) => {
    emit('update-count', newCount);
  },
  { immediate: true } // 如果 store 已经有数据，立即触发一次
);

onMounted(() => {
  const baseUrl = apiClient.defaults.baseURL || 'https://localhost:44359';
  
  // 触发全局连接（store 内部已经做了防重复连接的判断）
  onlineStore.startSignalR(baseUrl);
});
</script>

<template>
  <div class="sidebar-widget online-widget">
    <h5 class="widget-title">ONLINE_NODES // 活跃节点</h5>
    
    <div class="online-stats-header">
      <span class="online-pulse"></span>
      <span class="online-number">{{ onlineStore.onlineCount }} 名探索者正在连接</span>
    </div>
    
    <div class="online-avatars-container" v-if="onlineStore.onlineUsersList.length > 0">
      
      <div 
        v-for="user in onlineStore.onlineUsersList.slice(0, 12)" 
        :key="user.userId" 
        class="online-avatar-wrapper"
        :title="user.userName"
      >
        <UserAvatar 
          :userId="user.userId"
          :avatarUrl="user.avatar"
          :userName="user.userName"
          :size="32"
          :showLevel="false"
        />
      </div>
      
      <div v-if="onlineStore.onlineCount > 12" class="online-avatar-wrapper more-indicator">
        +{{ onlineStore.onlineCount - 12 }}
      </div>
    </div>
  </div>
</template>

<style scoped>
.sidebar-widget { 
  background: #fff; 
  padding: 24px; 
  border: 1px solid #eee; 
  width: 100%; 
  box-sizing: border-box; 
  margin-bottom: 30px; 
}

.widget-title { 
  font-family: 'JetBrains Mono', monospace; 
  font-size: 13px; 
  border-left: 3px solid #000; 
  padding-left: 10px; 
  margin-bottom: 20px; 
}

.online-stats-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 16px;
  font-family: 'JetBrains Mono', sans-serif;
  font-size: 13px;
  font-weight: bold;
  color: #000;
}

.online-pulse {
  width: 10px;
  height: 10px;
  background-color: #00ff41;
  border-radius: 50%;
  box-shadow: 0 0 10px #00ff41;
  animation: signalPulse 1.5s infinite;
}

.online-avatars-container {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

/* 将原来的 .online-avatar-circle 改名为 wrapper，更符合包裹器的语义 */
.online-avatar-wrapper {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  /* overflow: hidden; */
  border: 1px solid #ddd;
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
  background-color: #ffffff;
}

.online-avatar-wrapper:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  border-color: #0047ff;
}

/* 提示多余人数的特殊样式 */
.more-indicator {
  background-color: #f8f9fa;
  font-family: 'JetBrains Mono', sans-serif;
  font-size: 10px;
  font-weight: bold;
  color: #555;
}

@keyframes signalPulse {
  0% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(0, 255, 65, 0.7); }
  70% { transform: scale(1); box-shadow: 0 0 0 6px rgba(0, 255, 65, 0); }
  100% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(0, 255, 65, 0); }
}
</style>