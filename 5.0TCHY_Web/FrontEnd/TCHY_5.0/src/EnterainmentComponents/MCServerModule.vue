<template>
  <div class="cyber-card server-node">
    <div class="card-label-strip">
      <span class="node-id">// NODE_0{{ index || 1 }}</span>
      <span class="node-title">{{ title }}</span>
      <div class="version-tag">{{ version }}</div>
    </div>
    
    <div class="server-preview">
      <div class="image-container">
        <img :src="coverImage" @error="handleImgError" />
        <div class="scanline"></div>
        <div class="noise"></div>
      </div>
      
      <div class="status-overlay">
        <div class="status-indicator" :class="{ 'is-online': isOnline }">
          <span class="pulse-dot"></span>
          <span class="status-text">{{ isOnline ? 'LINK_ESTABLISHED' : 'SIGNAL_LOST' }}</span>
        </div>
      </div>
    </div>

    <div class="card-content">
      <div class="data-grid">
        <div class="data-item">
          <div class="d-label">SYNC_USERS</div>
          <div class="d-value">{{ online }}<span class="slash">/</span>{{ max }}</div>
        </div>
        <div class="data-item">
          <div class="d-label">DATA_PING</div>
          <div class="d-value accent">24MS</div>
        </div>
      </div>

      <div class="ip-terminal">
        <span class="prompt">root@taichu:~#</span>
        <span class="address">connect {{ ip }}</span>
        <button class="terminal-copy" @click="copyIP" title="COPY_ADDRESS">
          <i class="fas fa-terminal"></i>
        </button>
      </div>

      <div class="action-row">
        <button class="cyber-btn" @click="$emit('download', 'modpack')">
          <span class="btn-icon"><i class="fas fa-file-archive"></i></span>
          <div class="btn-text">
            <span class="main">同步整合包</span>
            <span class="sub">SYNC_ARCHIVE</span>
          </div>
        </button>
        
        <button class="cyber-btn red-mode" @click="$emit('download', 'launcher')">
          <span class="btn-icon"><i class="fas fa-bolt"></i></span>
          <div class="btn-text">
            <span class="main">启动中枢</span>
            <span class="sub">BOOT_LINKER</span>
          </div>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  index: Number,
  title: String,
  ip: String,
  version: String,
  online: Number,
  max: Number,
  coverImage: { type: String, default: '/game-default.jpg' },
  isOnline: { type: Boolean, default: true }
});

const emit = defineEmits(['download']);

const copyIP = () => {
  navigator.clipboard.writeText(props.ip).then(() => {
    // 这里可以触发一个全局的赛博风格提示
    alert('>> 传输协议已复制到剪贴板');
  });
};

const handleImgError = (e) => {
  e.target.src = '/土豆.jpg'; // 保持与主站一致的默认图
};
</script>


<style scoped>
/* 确保组件使用固定的排版逻辑 */
.server-node {
  border: 2px solid #111; background: #fff;
  box-shadow: 6px 6px 0 rgba(0,0,0,0.15);
  display: flex; flex-direction: column; margin-bottom: 20px;
}

/* 强制限制图片容器，防止截图中的溢出问题 */
.server-preview {
  height: 180px; width: 100%; position: relative;
  overflow: hidden; border-bottom: 2px solid #111; background: #000;
}
.image-container img {
  width: 100%; height: 100%; object-fit: cover;
  filter: grayscale(20%) contrast(110%); opacity: 0.9;
}

/* 扫描线：增加工业质感 */
.scanline {
  position: absolute; inset: 0; pointer-events: none; z-index: 2;
  background: linear-gradient(to bottom, transparent 50%, rgba(0,0,0,0.1) 50%);
  background-size: 100% 4px;
}

/* 状态指示条 */
.status-overlay { position: absolute; bottom: 10px; left: 10px; z-index: 3; }
.status-indicator {
  background: #111; color: #fff; padding: 4px 10px;
  display: flex; align-items: center; gap: 8px;
  font-family: 'JetBrains Mono', monospace; font-size: 0.7rem; border: 1px solid #444;
}
.pulse-dot { width: 8px; height: 8px; border-radius: 50%; background: #00ff66; box-shadow: 0 0 8px #00ff66; animation: pulse 1.5s infinite; }

/* 内容区数据排版 */
.card-content { padding: 15px; background: #F4F1EA; }
.data-grid {
  display: grid; grid-template-columns: 1fr 1fr; gap: 2px;
  background: #111; border: 2px solid #111; margin-bottom: 15px;
}
.data-item { background: #fff; padding: 10px; }
.d-label { font-family: 'JetBrains Mono', monospace; font-size: 0.6rem; color: #888; }
.d-value { font-family: 'Anton', sans-serif; font-size: 1.5rem; line-height: 1; }

/* 终端样式的 IP 地址 */
.ip-terminal {
  background: #1a1a1a; color: #00ff66; padding: 10px;
  font-family: 'JetBrains Mono', monospace; font-size: 0.8rem;
  display: flex; align-items: center; margin-bottom: 15px; border-left: 4px solid #D92323;
}
.prompt { color: #D92323; margin-right: 8px; font-weight: bold; }
.address { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

/* 动作按钮：直接继承 ComCenter 的工业按钮 */
.action-row { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }
:deep(.cyber-btn) {
  height: 45px; background: #fff; border: 1px solid #111;
  display: flex; cursor: pointer; transition: 0.2s; box-shadow: 2px 2px 0 #111;
}
:deep(.cyber-btn:hover) { transform: translate(-2px, -2px); box-shadow: 4px 4px 0 #111; }
:deep(.btn-icon) { width: 35px; background: #111; color: #fff; display: flex; align-items: center; justify-content: center; }
:deep(.btn-text) { padding-left: 8px; text-align: left; display: flex; flex-direction: column; justify-content: center; }
:deep(.btn-text .main) { font-weight: 800; font-size: 0.8rem; }
:deep(.btn-text .sub) { font-size: 0.6rem; color: #666; font-family: 'JetBrains Mono'; }

@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }
</style>