<template>
  <div class="md-server-card">
    <div class="card-media">
      <img :src="coverImage" @error="handleImgError" alt="Server Cover" loading="lazy" />
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

      <div class="ip-group">
        <div class="ip-action-bar" @click="copyIP(ip1)" title="点击复制主线路">
          <div class="ip-icon primary">主线</div>
          <span class="ip-text">{{ ip1 }}</span>
          <div class="copy-hint"><i class="icon">⧉</i></div>
        </div>

        <div class="ip-action-bar alt" v-if="ip2" @click="copyIP(ip2)" title="点击复制备用线路">
          <div class="ip-icon secondary">备用</div>
          <span class="ip-text">{{ ip2 }}</span>
          <div class="copy-hint"><i class="icon">⧉</i></div>
        </div>
      </div>

      <p class="server-desc">点击地址即可复制到剪贴板。</p>
    </div>

    <div class="card-actions">
      <button class="md-btn outlined" @click="$emit('download', 'launcher')">
        启动器
      </button>
      <button class="md-btn contained" @click="$emit('download', 'modpack')">
        下载整合包
      </button>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  index: { type: Number, default: 1 },
  title: String,
  ip1: String, // 对应数据源的 ip1
  ip2: String, // 对应数据源的 ip2
  version: String,
  coverImage: String,
  isOnline: Boolean
});

const emit = defineEmits(['download']);

// 复制逻辑
const copyIP = async (text) => {
  if (!text) return;
  try {
    await navigator.clipboard.writeText(text);
    alert(`复制成功: ${text}`);
  } catch (err) {
    // 简单回退
    const input = document.createElement('input');
    input.value = text;
    document.body.appendChild(input);
    input.select();
    document.execCommand('copy');
    document.body.removeChild(input);
    alert('复制成功');
  }
};

// 图片容错
const handleImgError = (e) => {
  e.target.src = 'https://via.placeholder.com/400x200?text=NO+IMAGE';
};
</script>

<style scoped>
/* 引入漂亮字体 */
@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@500&display=swap');

.md-server-card {
  background: #fff;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1);
  transition: all 0.3s ease;
  display: flex;
  flex-direction: column;
  height: 100%; /* 确保卡片等高 */
  border: 1px solid #f0f0f0;
}

.md-server-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 10px 15px -3px rgba(0,0,0,0.1);
}

/* 媒体图片 */
.card-media { position: relative; height: 150px; background: #eee; }
.card-media img { width: 100%; height: 100%; object-fit: cover; }
.media-overlay { position: absolute; inset: 0; background: linear-gradient(to top, rgba(0,0,0,0.4), transparent); }

/* 状态 Chip */
.status-chip {
  position: absolute; top: 12px; left: 12px;
  padding: 4px 12px; border-radius: 20px;
  background: rgba(255,255,255,0.95);
  font-size: 12px; font-weight: bold;
  display: flex; align-items: center; gap: 6px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.15);
}
.status-dot { width: 8px; height: 8px; border-radius: 50%; }
.status-chip.online { color: #059669; }
.status-chip.online .status-dot { background: #10B981; box-shadow: 0 0 0 2px rgba(16, 185, 129, 0.2); }
.status-chip.offline { color: #DC2626; }
.status-chip.offline .status-dot { background: #EF4444; }

.version-badge {
  position: absolute; bottom: 8px; right: 8px;
  background: rgba(0,0,0,0.6); color: #fff;
  padding: 2px 8px; font-size: 11px; border-radius: 4px;
  backdrop-filter: blur(4px);
}

/* 内容 */
.card-content { padding: 16px; flex: 1; display: flex; flex-direction: column; }
.header-row { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 16px; }
.server-title { margin: 0; font-size: 18px; font-weight: 700; color: #1f2937; line-height: 1.3; }
.server-id { color: #9ca3af; font-family: 'JetBrains Mono'; font-size: 14px; }

/* IP 列表组 */
.ip-group { display: flex; flex-direction: column; gap: 8px; margin-bottom: 12px; }

.ip-action-bar {
  background: #f3f4f6; padding: 8px 10px; border-radius: 6px;
  display: flex; align-items: center; cursor: pointer;
  transition: all 0.2s; border: 1px solid transparent;
}
.ip-action-bar:hover { background: #e5e7eb; border-color: #d1d5db; }
.ip-action-bar:active { background: #d1d5db; }

.ip-icon {
  font-size: 10px; padding: 2px 6px; border-radius: 4px;
  color: #fff; margin-right: 8px; font-weight: bold;
}
.ip-icon.primary { background: #2563EB; }
.ip-icon.secondary { background: #6B7280; }

.ip-text {
  flex: 1; font-family: 'JetBrains Mono'; color: #374151;
  font-size: 13px; font-weight: 600;
}
.copy-hint .icon { font-style: normal; font-size: 14px; color: #9ca3af; }

.server-desc { font-size: 12px; color: #6b7280; margin-top: auto; padding-top: 10px; }

/* 按钮 */
.card-actions {
  padding: 12px 16px; background: #f9fafb;
  border-top: 1px solid #f3f4f6; display: flex; gap: 10px;
}
.md-btn {
  flex: 1; padding: 8px; border-radius: 6px; border: none;
  font-size: 13px; font-weight: 600; cursor: pointer;
  transition: all 0.2s;
}
.md-btn.contained { background: #2563EB; color: #fff; box-shadow: 0 1px 2px rgba(37, 99, 235, 0.3); }
.md-btn.contained:hover { background: #1d4ed8; }
.md-btn.outlined { background: #fff; border: 1px solid #d1d5db; color: #374151; }
.md-btn.outlined:hover { border-color: #9ca3af; background: #f3f4f6; }
</style>