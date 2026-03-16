<script setup lang="ts">
import { computed } from 'vue';

// --- 类型定义 ---
export interface ActivityItem {
  id: number;
  name: string;
  host: string;
  userId: number;
  category: string;
  type: number;
  startdate: string;
  enddate: string;
  verify: number;
  phase: number;
  coverUrl: string;
  desc: string;
  tags: string[];
}

const props = defineProps<{
  data: ActivityItem;
}>();

// --- 计算活动状态 ---
const activityStatus = computed(() => {
  if (!props.data.startdate || !props.data.enddate) return '状态未知';
  const now = new Date();
  const startDate = new Date(props.data.startdate);
  const endDate = new Date(props.data.enddate);

  if (now < startDate) return '即将开始';
  if (now >= startDate && now <= endDate) return '进行中';
  return '已结束';
});

// --- 格式化截止时间 ---
const formattedDeadline = computed(() => {
  if (!props.data.enddate) return '';
  const d = new Date(props.data.enddate);
  return `${d.getFullYear()}.${(d.getMonth() + 1).toString().padStart(2, '0')}.${d.getDate().toString().padStart(2, '0')}`;
});
</script>

<template>
  <article class="project-card">
    <div class="proj-status-bar">
      <span class="proj-status" :class="{'urgent': activityStatus === '进行中'}">
        {{ activityStatus }}
      </span>
      <span class="proj-category" v-if="data.category">{{ data.category }}</span>
    </div>
    
    <h4 class="proj-title" :title="data.name">{{ data.name }}</h4>
    
    <div class="proj-footer">
      <span class="proj-initiator" :title="'@' + data.host">发起: @{{ data.host }}</span>
      <span class="proj-participants">
        <span class="icon">⏳</span> {{ formattedDeadline }} 截止
      </span>
    </div>
  </article>
</template>

<style scoped>
.project-card {
  background: #fff;
  border: 1px solid #ddd;
  padding: 20px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  transition: 0.2s;
  position: relative;
  overflow: hidden;
  min-height: 140px;
  box-sizing: border-box;
  cursor: pointer;
}

/* 左侧科技感装饰条 */
.project-card::before {
  content: "";
  position: absolute;
  top: 0; left: 0; width: 4px; height: 100%;
  background: #1a1a1a;
  transition: 0.2s;
}

.project-card:hover { 
  border-color: #000; 
  transform: translateY(-3px); 
  box-shadow: 4px 4px 0px rgba(0,0,0,0.05); 
}

.project-card:hover::before { 
  background: #0047ff; 
}

.proj-status-bar { 
  margin-bottom: 12px; 
  display: flex; 
  justify-content: space-between; 
  align-items: center; 
}

.proj-status {
  font-family: 'JetBrains Mono', monospace; 
  font-size: 10px; 
  font-weight: bold;
  padding: 3px 8px; 
  background: #eee; 
  color: #555;
}

/* 进行中的高亮显示 */
.proj-status.urgent { 
  background: #0047ff; 
  color: #fff; 
}

.proj-category {
  font-family: 'JetBrains Mono', monospace;
  font-size: 10px;
  color: #888;
  border: 1px solid #eee;
  padding: 2px 6px;
  border-radius: 2px;
}

.proj-title { 
  font-size: 15px; 
  font-weight: 800; 
  line-height: 1.4; 
  margin: 0 0 16px 0; 
  color: #111;
  /* 标题防溢出截断 */
  display: -webkit-box;
  -webkit-line-clamp: 2; 
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
}

.proj-footer {
  display: flex; 
  flex-direction: column; 
  gap: 6px;
  font-family: 'JetBrains Mono', monospace; 
  font-size: 11px; 
  border-top: 1px dashed #eee; 
  padding-top: 12px;
}

.proj-initiator { 
  color: #666; 
  white-space: nowrap; 
  overflow: hidden; 
  text-overflow: ellipsis; 
}

.proj-participants { 
  font-weight: bold; 
  color: #1a1a1a; 
  display: flex; 
  align-items: center; 
  gap: 4px; 
}

.proj-participants .icon { 
  color: #0047ff; 
}
</style>