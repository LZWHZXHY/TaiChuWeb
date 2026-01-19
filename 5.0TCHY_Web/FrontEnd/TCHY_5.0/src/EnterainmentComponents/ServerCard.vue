<template>
  <div class="md-server-card">
    <div class="card-media">
      <img :src="coverImage" @error="handleImgError" alt="Server Cover" />
      <div class="media-overlay"></div>
      
      <div class="status-chip" :class="isOnline ? 'online' : 'offline'">
        <span class="status-dot"></span>
        {{ isOnline ? '运行中' : '维护中' }}
      </div>
      
      <div class="version-badge">{{ version }}</div>
    </div>

    <div class="card-content">
      <div class="header-row">
        <h3 class="server-title">{{ title }}</h3>
        <span class="server-id">#{{ index.toString().padStart(2, '0') }}</span>
      </div>

      <div class="ip-action-bar" @click="copyIP" title="点击复制">
        <div class="ip-icon">ADDR</div>
        <span class="ip-text">{{ ip }}</span>
        <div class="copy-hint"><i class="fas fa-copy"></i></div>
      </div>

      <p class="server-desc">点击上方地址快速复制，或下载下方资源文件。</p>
    </div>

    <div class="card-actions">
      <button 
        class="md-btn outlined" 
        @click="$emit('download', 'launcher')"
      >
        启动器
      </button>
      <button 
        class="md-btn contained" 
        @click="$emit('download', 'modpack')"
      >
        同步整合包
      </button>
    </div>
  </div>
</template>

<script setup>
// 定义接收的属性
const props = defineProps({
  index: { type: Number, default: 1 },
  title: { type: String, default: '未知服务器' },
  ip: { type: String, default: '127.0.0.1' },
  version: { type: String, default: 'Latest' },
  coverImage: { type: String, default: '' },
  isOnline: { type: Boolean, default: true }
});

// 定义事件
const emit = defineEmits(['download']);

// 复制连接地址逻辑
const copyIP = () => {
  if (navigator.clipboard && navigator.clipboard.writeText) {
    navigator.clipboard.writeText(props.ip).then(() => {
      alert('连接地址已复制: ' + props.ip);
    }).catch(() => {
      // 兼容性回退方案
      const input = document.createElement('input');
      input.value = props.ip;
      document.body.appendChild(input);
      input.select();
      document.execCommand('copy');
      document.body.removeChild(input);
      alert('连接地址已复制');
    });
  }
};

// 图片加载失败处理 (Base64 纯色占位，防止死循环)
const handleImgError = (e) => {
  const fallback = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=';
  if (e.target.src !== fallback) {
    e.target.src = fallback;
  }
};
</script>

<style scoped>
/* 核心卡片容器 */
.md-server-card {
  background: #fff;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 10px 15px -3px rgba(0,0,0,0.1);
  transition: transform 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex;
  flex-direction: column;
  border: 1px solid #eee;
  height: 100%;
}

.md-server-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 20px 25px -5px rgba(0,0,0,0.15);
}

/* 媒体展示区样式 */
.card-media { 
  position: relative; 
  height: 140px; 
  background: #333; 
  overflow: hidden; 
}
.card-media img { 
  width: 100%; 
  height: 100%; 
  object-fit: cover; 
  transition: transform 0.5s ease; 
}
.md-server-card:hover .card-media img { 
  transform: scale(1.05); 
}

/* 状态标识小标签 (Status Chip) */
.status-chip {
  position: absolute; 
  top: 10px; 
  left: 10px;
  padding: 4px 10px; 
  border-radius: 20px;
  background: rgba(255,255,255,0.95);
  font-size: 12px; 
  font-weight: bold;
  display: flex; 
  align-items: center; 
  gap: 6px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  z-index: 2;
}
.status-dot { width: 8px; height: 8px; border-radius: 50%; }
.status-chip.online { color: #059669; }
.status-chip.online .status-dot { background: #10B981; box-shadow: 0 0 5px #10B981; }
.status-chip.offline { color: #DC2626; }
.status-chip.offline .status-dot { background: #EF4444; }

.version-badge {
  position: absolute; 
  bottom: 0; 
  right: 0;
  background: rgba(0,0,0,0.7); 
  color: #fff;
  padding: 2px 10px; 
  font-size: 11px;
  border-top-left-radius: 8px;
  z-index: 2;
}

/* 内容区域排版 */
.card-content { padding: 16px; flex: 1; display: flex; flex-direction: column; }
.header-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; }
.server-title { margin: 0; font-size: 18px; font-weight: 800; color: #111; line-height: 1.2; }
.server-id { color: #999; font-family: 'JetBrains Mono', monospace; font-size: 14px; }

/* ADDR 复制条样式 */
.ip-action-bar {
  background: #f3f4f6; 
  padding: 8px; 
  border-radius: 6px;
  display: flex; 
  align-items: center; 
  cursor: pointer;
  margin-bottom: 16px; 
  transition: background 0.2s ease;
  border: 1px solid transparent;
}
.ip-action-bar:hover { 
  background: #e5e7eb; 
  border-color: #d1d5db;
}
.ip-icon { 
  font-size: 10px; 
  font-weight: bold; 
  background: #333; 
  padding: 2px 4px; 
  border-radius: 3px; 
  margin-right: 8px; 
  color: #fff; 
}
.ip-text { 
  flex: 1; 
  font-family: 'JetBrains Mono', monospace; 
  color: #333; 
  font-weight: 600; 
  font-size: 13px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.copy-hint { color: #999; font-size: 12px; }

.server-desc { font-size: 12px; color: #666; margin: 0; line-height: 1.5; }

/* 底部操作按钮样式 (Material Design 风格) */
.card-actions { 
  padding: 12px 16px; 
  display: flex; 
  gap: 10px; 
  background: #fafafa; 
  border-top: 1px solid #eee; 
}
.md-btn { 
  flex: 1; 
  padding: 10px 0; 
  border-radius: 8px; 
  border: none; 
  font-size: 13px; 
  cursor: pointer; 
  font-weight: 600; 
  transition: all 0.2s ease; 
  display: flex;
  align-items: center;
  justify-content: center;
}

/* 填充按钮 (Primary) */
.md-btn.contained { 
  background: #2563EB; 
  color: #fff; 
  box-shadow: 0 2px 4px rgba(37, 99, 235, 0.2);
}
.md-btn.contained:hover { 
  background: #1D4ED8; 
  box-shadow: 0 4px 6px rgba(37, 99, 235, 0.3);
}

/* 描边按钮 (Secondary) */
.md-btn.outlined { 
  background: #fff; 
  border: 1px solid #d1d5db; 
  color: #374151; 
}
.md-btn.outlined:hover { 
  border-color: #9ca3af; 
  background: #f9fafb; 
}
</style>