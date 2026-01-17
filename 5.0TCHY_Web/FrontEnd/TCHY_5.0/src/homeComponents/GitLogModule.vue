<template>
  <div class="cyber-card git-log-module">
    <div class="card-label-strip">
      <span class="node-id">// SYS_LOG_STREAM</span>
      <span class="node-title">GITHUB_PUSH_HISTORY</span>
    </div>

    <div class="log-terminal custom-scroll">
      <div v-if="loading" class="log-line loading">
        <span class="prompt">></span> [ SCANNING_REMOTE_REPOSITORY... ]
      </div>
      
      <div v-else-if="logs.length === 0" class="log-line error">
        <span class="prompt">></span> [ ACCESS_DENIED_OR_NO_DATA ]
      </div>

      <div v-for="log in filteredLogs" :key="log.sha" class="log-entry">
        <div class="log-meta-top">
          <div class="author-info">
            <div class="mini-avatar">
              <img :src="log.author?.avatar_url || '/土豆.jpg'" @error="handleImgError" />
            </div>
            <span class="author-name">{{ log.author?.login || log.commit.author.name }}</span>
          </div>
          <div class="log-time">{{ formatDate(log.commit.author.date) }}</div>
        </div>

        <div class="log-main">
          <span class="log-hash">#{{ log.sha.substring(0, 7) }}</span>
          <span class="log-type" :class="getLogType(log.commit.message).class">
            [{{ getLogType(log.commit.message).label }}]
          </span>
          <span class="log-msg">{{ getPureMessage(log.commit.message) }}</span>
        </div>
      </div>

      <div class="log-footer">
        <span class="blink">_</span> TERMINAL_ACTIVE // {{ repoName }}
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';

const props = defineProps({
  repo: { type: String, required: true },
  limit: { type: Number, default: 15 }
});

const logs = ref([]);
const loading = ref(true);
const repoName = computed(() => props.repo.split('/')[1]?.toUpperCase() || 'UNKNOWN');

const filteredLogs = computed(() => {
  const excludeTypes = ['chore', 'style', 'merge'];
  return logs.value.filter(log => {
    const msg = log.commit.message.toLowerCase();
    return !excludeTypes.some(t => msg.startsWith(t));
  });
});

const fetchLogs = async () => {
  loading.value = true;
  try {
    const response = await fetch(`https://api.github.com/repos/${props.repo}/commits?per_page=${props.limit * 2}`);
    const data = await response.json();
    if (Array.isArray(data)) {
      logs.value = data;
    }
  } catch (e) {
    console.error("Git Log Fetch Error", e);
  } finally {
    loading.value = false;
  }
};

const getLogType = (message) => {
  const m = message.toLowerCase();
  if (m.startsWith('feat')) return { label: 'FEAT', class: 't-feat' };
  if (m.startsWith('fix')) return { label: 'FIX', class: 't-fix' };
  if (m.startsWith('perf')) return { label: 'PERF', class: 't-perf' };
  if (m.startsWith('refactor')) return { label: 'REFACT', class: 't-refact' };
  return { label: 'LOG', class: 't-default' };
};

const getPureMessage = (message) => {
  // 处理 Merge 信息和规范前缀
  return message.split('\n')[0].replace(/^(feat|fix|perf|refactor|chore|style|docs|merge)(\(.*\))?:\s*/i, '');
};

const formatDate = (dateStr) => {
  const d = new Date(dateStr);
  return `${d.getMonth() + 1}/${d.getDate()} ${d.getHours()}:${d.getMinutes().toString().padStart(2, '0')}`;
};

const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };

onMounted(fetchLogs);
</script>

<style scoped>
/* --- 基础容器 --- */
.git-log-module { 
  height: 100%; 
  display: flex; 
  flex-direction: column; 
  background: #fff; 
}

/* --- 终端显示区域 --- */
.log-terminal {
  flex: 1;
  background: #0a0a0a; /* 深黑底色，模拟命令行终端 */
  color: #aaa;
  padding: 18px;
  font-family: 'JetBrains Mono', monospace; /* 强制使用等宽字体 */
  font-size: 0.75rem;
  line-height: 1.6;
  overflow-y: auto;
  min-height: 300px;
}

/* --- 日志条目 --- */
.log-entry { 
  margin-bottom: 18px; 
  padding-bottom: 12px; 
  border-bottom: 1px solid #1a1a1a; 
  display: flex; 
  flex-direction: column;
}

/* --- 顶部元数据：人员与时间 --- */
.log-meta-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.author-info {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* --- 头像优化：恢复颜色并增加细节 --- */
.mini-avatar {
  width: 20px;
  height: 20px;
  border: 1px solid #444; /* 保持硬核边框 */
  overflow: hidden;
  background: #222;
  flex-shrink: 0;
  box-shadow: 0 0 5px rgba(0,0,0,0.5);
  transition: 0.2s ease;
}

.mini-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  /* 关键点：已移除 grayscale(100%)，现在会显示原始色彩 */
  filter: contrast(1.1); /* 仅轻微增强对比度，保持清晰 */
  transition: 0.2s ease;
}

/* 悬浮效果 */
.log-entry:hover .mini-avatar {
  border-color: var(--red, #D92323);
  transform: scale(1.1);
}

.author-name {
  color: #ccc; /* 提高文字亮度 */
  font-weight: 700;
  font-size: 0.75rem;
  letter-spacing: 0.5px;
}

.log-time { 
  color: #555; 
  font-size: 0.65rem; 
}

/* --- 消息内容布局 --- */
.log-main { 
  display: flex; 
  gap: 10px; 
  align-items: flex-start; 
}

.log-hash { 
  color: #666; 
  font-weight: bold; 
  font-size: 0.7rem;
}

.log-type { 
  font-weight: 800; 
  flex-shrink: 0; 
  font-size: 0.7rem;
}

/* --- 状态颜色：符合工业赛博调色盘 --- */
.t-feat { color: #00ff66; text-shadow: 0 0 5px rgba(0,255,102,0.3); } /* 绿色：功能 */
.t-fix { color: #D92323; text-shadow: 0 0 5px rgba(217,35,35,0.3); }  /* 红色：修复 */
.t-perf { color: #ffcc00; } /* 黄色：性能 */
.t-refact { color: #00ccff; } /* 蓝色：重构 */
.t-default { color: #eee; }

.log-msg { 
  color: #ddd; 
  word-break: break-all; 
  line-height: 1.4;
}

/* --- 底部状态栏 --- */
.log-footer { 
  margin-top: 12px; 
  color: #444; 
  font-size: 0.6rem; 
  text-transform: uppercase; 
  letter-spacing: 1px;
}

.blink { 
  animation: blink 1.2s infinite; 
  color: var(--red, #D92323);
}

@keyframes blink { 
  0%, 100% { opacity: 1; } 
  50% { opacity: 0; } 
}

/* --- 滚动条优化 --- */
.custom-scroll::-webkit-scrollbar { 
  width: 4px; 
}
.custom-scroll::-webkit-scrollbar-thumb { 
  background: #333; 
  border-radius: 0;
}
.custom-scroll::-webkit-scrollbar-thumb:hover { 
  background: #444; 
}
.custom-scroll::-webkit-scrollbar-track { 
  background: #050505; 
}
</style>