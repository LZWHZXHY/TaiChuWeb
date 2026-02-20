<template>
  <router-link :to="`/post/${data.id}`" class="md-link">
    <div class="md-post-container">
      
      <div class="md-meta">
        <div class="md-user">
          <Avatar :userId="data.authorId" :showLevel="false" :allowLink="false" class="mini-avatar" />
          <span class="md-author">@{{ data.author }}</span>
        </div>
        <span class="md-type">[LOG_ENTRY]</span>
        <span class="md-date">{{ formatDate(data.timestamp) }}</span>
      </div>

      <div class="md-body">
        <h3 v-if="data.title" class="md-title"># {{ data.title }}</h3>
        <div class="md-quote">
          <p class="md-text">{{ data.content }}</p>
        </div>
      </div>
      
      <div class="md-footer">
        <span class="md-ref">REF: {{ data.id }}</span>
      </div>
      
    </div>
  </router-link>
</template>

<script setup lang="ts">
// 引入你的通用头像组件（确保路径正确）
import Avatar from '@/GeneralComponents/UserAvatar.vue'; 

// 定义 Props，使用 any 绕过严苛的 TS 检查，保证灵活性
defineProps<{
  data: any
}>();

// 格式化时间的辅助函数，加上了 string 类型声明
const formatDate = (dateString: string) => {
  if (!dateString) return '';
  const d = new Date(dateString);
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  const hours = String(d.getHours()).padStart(2, '0');
  const minutes = String(d.getMinutes()).padStart(2, '0');
  return `${year}-${month}-${day} ${hours}:${minutes}`;
};
</script>

<style scoped>
.md-link { text-decoration: none; color: inherit; }

.md-post-container { font-family: 'Inter', sans-serif; }

.md-meta {
  display: flex; 
  align-items: center; 
  gap: 12px;
  font-family: 'JetBrains Mono', monospace;
  font-size: 12px;
  color: #666;
  margin-bottom: 16px;
}

.md-user {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* 限制头像大小，匹配极简的文字流 */
.mini-avatar { width: 20px; height: 20px; border-radius: 2px; }

.md-type { color: #00F0FF; opacity: 0.8; }

.md-title {
  color: #fff;
  font-size: 18px;
  margin-bottom: 12px;
  font-weight: 700;
}

/* 核心：MD 引用竖线样式 */
.md-quote {
  padding: 4px 20px;
  border-left: 3px solid #333; 
  background: rgba(255, 255, 255, 0.02);
}

.md-text {
  color: #ccc;
  line-height: 1.7;
  font-size: 15px;
  margin: 0;
  white-space: pre-wrap; /* 保证换行符能正常渲染 */
}

.md-footer {
  margin-top: 16px;
  font-family: 'JetBrains Mono', monospace;
  font-size: 10px;
  color: #333;
}
</style>