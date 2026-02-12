<template>
  <div class="main-wrapper">
    
    <div v-if="isUploading" class="global-loading">
      <div class="spinner"></div>
      <p>正在同步头像数据...</p>
    </div>

    <div class="top-section">
      <div v-if="isEditing" class="editor-card">
        <div class="editor-sidebar">
          <div class="editor-header">
            <span class="step-label">步骤 02</span>
            <h3>调整 // ADJUST</h3>
          </div>
          <div class="editor-actions">
            <button class="action-btn confirm-btn" @click="confirmCrop">
              <span class="btn-txt">确定并上传</span>
            </button>
            <button class="action-btn cancel-btn" @click="cancelEdit">
              取消
            </button>
          </div>
        </div>

        <div class="editor-stage" ref="editorContainerRef" @mousemove="onMouseMove" @mouseup="onMouseUp" @mouseleave="onMouseUp">
          <div class="grid-bg"></div>
          <img 
            ref="sourceImgRef" 
            :src="tempImgUrl" 
            class="source-image" 
            draggable="false"
            @load="onImageLoad"
          />
          <div class="overlay"></div>
          
          <div class="crop-box" :style="cropBoxStyle" @mousedown.stop="startDrag">
            <div class="corner-marker tl"></div>
            <div class="corner-marker tr"></div>
            <div class="corner-marker bl"></div>
            <div class="corner-marker br"></div>
            <div class="resize-handle" @mousedown.stop="startResize"></div>
          </div>
        </div>
      </div>

      <div v-else class="preview-card">
        <div class="preview-left">
          <div class="current-avatar-box">
            <img v-if="currentAvatar" :src="currentAvatar" class="avatar-display" />
            <div v-else class="avatar-placeholder">暂无头像</div>
            <div class="scan-line"></div>
          </div>
          <div class="avatar-meta">
            <span class="meta-label">当前头像 // CURRENT</span>
            <span class="meta-status">使用中 // ACTIVE</span>
          </div>
        </div>

        <div 
          class="upload-right"
          :class="{ 'is-dragging': isDragging }"
          @click="triggerUpload"
          @dragover.prevent="handleDragOver"
          @dragleave.prevent="handleDragLeave"
          @drop.prevent="handleDrop"
        >
          <div class="upload-area">
            <input 
              type="file" 
              ref="fileInputRef" 
              accept="image/png, image/jpeg, image/jpg, image/gif" 
              style="display: none" 
              @change="handleFileChange"
            />
            <div class="upload-icon">{{ isDragging ? '⇩' : '+' }}</div>
            <div class="upload-text">
              <h3>{{ isDragging ? '释放文件 // DROP HERE' : '上传新头像 // UPLOAD' }}</h3>
              <p>支持拖拽或点击上传 (Max 5MB)</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="bottom-section">
      <div class="gallery-header">
        <div class="tabs">
          <div 
            class="tab-item" 
            :class="{ active: activeTab === 'default' }"
            @click="activeTab = 'default'"
          >
            系统预设 // SYSTEM_PRESET
          </div>
          <div 
            class="tab-item" 
            :class="{ active: activeTab === 'community' }"
            @click="activeTab = 'community'"
          >
            社区推荐 // COMMUNITY
          </div>
        </div>
      </div>

      <div class="gallery-content">
        <div v-if="activeTab === 'default'" class="avatar-grid">
          <div 
            v-for="item in defaultAvatars" 
            :key="item.id"
            class="avatar-slot"
            :class="{ selected: tempSelectedId === item.id }" 
            @click="preSelectDefaultAvatar(item)"
          >
            <img :src="item.url" :alt="item.name" />
            <div class="selection-indicator"></div>
          </div>
        </div>
        <div v-else class="empty-state">
          <span>// 数据库离线 // DATABASE_OFFLINE</span>
          <p>社区功能暂未开放连接</p>
        </div>
      </div>

      <transition name="slide-up">
        <div v-if="tempSelectedId && activeTab === 'default'" class="floating-bar">
          <div class="bar-info">
            <span class="label">已选预设:</span>
            <span class="val">{{ tempSelectedName }}</span>
          </div>
          <button class="apply-btn" @click="confirmDefaultSelection">
            确认应用 // APPLY
          </button>
        </div>
      </transition>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, reactive, onMounted } from 'vue';
import apiClient from '@/utils/api'; 

// --- 状态定义 ---
const currentAvatar = ref(''); 
const tempImgUrl = ref('');    
const isEditing = ref(false);  
const isDragging = ref(false);
const isUploading = ref(false);

const activeTab = ref('default'); 
const tempSelectedId = ref(null);   
const tempSelectedName = ref('');   
const tempSelectedUrl = ref('');    
const fileInputRef = ref(null);
const sourceImgRef = ref(null);

// 裁剪相关配置
const MIN_BOX_SIZE = 130;
const cropState = reactive({
  startX: 0, startY: 0, startBoxX: 0, startBoxY: 0, startBoxSize: 130,
  isDragging: false, isResizing: false,
  boxX: 0, boxY: 0, boxSize: 130, 
  imgRenderW: 0, imgRenderH: 0, imgRenderX: 0, imgRenderY: 0,
  naturalWidth: 0, naturalHeight: 0
});

// 系统预设头像 (请确保这些 URL 是有效的)
const defaultAvatars = [
  { id: 1, name: '预设_01', url: 'https://img.bianyuzhou.com/uploads/默认头像/默认头像1.png' },
  { id: 2, name: '预设_02', url: 'https://img.bianyuzhou.com/uploads/默认头像/默认头像2.png' },
  { id: 3, name: '预设_03', url: 'https://img.bianyuzhou.com/uploads/默认头像/默认头像3.png' },
  { id: 4, name: '预设_04', url: 'https://img.bianyuzhou.com/uploads/默认头像/默认头像4.png' },
];

const cropBoxStyle = computed(() => ({
  top: `${cropState.boxY}px`, left: `${cropState.boxX}px`, width: `${cropState.boxSize}px`, height: `${cropState.boxSize}px`
}));

// 初始化
const initData = async () => {
  try {
    const res = await apiClient.get('/profile/detail');
    if (res.data && res.data.success) {
      const data = res.data.data;
      // ✅ 修正：直接读取 Avatar (后端已映射为 AvatarUrl)
      if (data.Avatar) {
        currentAvatar.value = data.Avatar;
      } 
    }
  } catch (error) {
    console.error('获取头像失败:', error);
  }
};

onMounted(() => {
  initData();
});

// ==========================================
// 📤 核心上传逻辑 (上传文件)
// ==========================================
const uploadFile = async (fileObj) => {
  if (!fileObj) return;
  
  isUploading.value = true;
  const formData = new FormData();
  formData.append('file', fileObj); 

  try {
    // 对应 ProfileController.cs -> [HttpPost("upload-avatar")]
    const res = await apiClient.post('/profile/upload-avatar', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });

    if (res.data && res.data.success) {
      currentAvatar.value = res.data.url;
      alert('头像更新成功 // SUCCESS');
      
      isEditing.value = false;
      tempSelectedId.value = null;
      tempImgUrl.value = '';
    } else {
      alert(res.data.message || '上传失败');
    }
  } catch (error) {
    console.error(error);
    alert('网络错误，上传失败');
  } finally {
    isUploading.value = false;
  }
};

// ==========================================
// 🎨 系统预设逻辑 (设置 URL)
// ==========================================
const preSelectDefaultAvatar = (item) => { 
  tempSelectedId.value = item.id; 
  tempSelectedName.value = item.name; 
  tempSelectedUrl.value = item.url; 
};

const confirmDefaultSelection = async () => {
  if (!tempSelectedUrl.value) return;
  if (isUploading.value) return; 
  isUploading.value = true;

  try {
    // ✅ 对应我们在后端刚补上的 [HttpPost("set-avatar-url")]
    const res = await apiClient.post('/profile/set-avatar-url', {
      url: tempSelectedUrl.value
    });
    
    if (res.data && res.data.success) {
      currentAvatar.value = tempSelectedUrl.value;
      alert('头像设置成功 // SUCCESS');
      tempSelectedId.value = null;
    } else {
      alert(res.data.message || '设置失败');
    }
    
  } catch (error) {
    console.error('设置头像失败:', error);
    alert('网络错误，请稍后重试');
  } finally {
    isUploading.value = false;
  }
};

// ==========================================
// 🖼️ 裁剪与文件处理逻辑 (复用逻辑)
// ==========================================
const processFile = (file) => {
  if (!file) return;
  const validTypes = ['image/jpeg', 'image/png', 'image/jpg', 'image/gif'];
  if (!validTypes.includes(file.type)) {
    alert('不支持的文件格式 (仅限 JPG/PNG/GIF)'); return;
  }
  if (file.size > 5 * 1024 * 1024) { 
    alert('文件过大: 5MB (MAX SIZE)'); return; 
  }

  tempSelectedId.value = null; 

  if (file.type === 'image/gif') {
    const confirmGif = confirm('检测到 GIF 图片，是否直接设为头像？');
    if (confirmGif) uploadFile(file);
  } else {
    const reader = new FileReader();
    reader.onload = (evt) => {
      tempImgUrl.value = evt.target.result;
      isEditing.value = true; 
      // 重置裁剪框位置
      cropState.boxSize = 130; 
      cropState.boxX = 0; 
      cropState.boxY = 0;
    };
    reader.readAsDataURL(file);
  }
};

const confirmCrop = () => {
  if (!sourceImgRef.value) return;

  const canvas = document.createElement('canvas');
  const size = cropState.boxSize;
  const scaleX = cropState.naturalWidth / cropState.imgRenderW;
  
  canvas.width = size * scaleX; 
  canvas.height = canvas.width;
  
  const ctx = canvas.getContext('2d');
  
  ctx.drawImage(
    sourceImgRef.value, 
    (cropState.boxX - cropState.imgRenderX) * scaleX, 
    (cropState.boxY - cropState.imgRenderY) * scaleX, 
    canvas.width, canvas.height, 
    0, 0, canvas.width, canvas.height
  );

  canvas.toBlob((blob) => {
    if (blob) {
      const file = new File([blob], `avatar_crop_${Date.now()}.png`, { type: 'image/png' });
      uploadFile(file);
    }
  }, 'image/png');
};

const cancelEdit = () => { isEditing.value = false; tempImgUrl.value = ''; };
const triggerUpload = () => { fileInputRef.value.click(); };
const handleFileChange = (e) => {
  const file = e.target.files[0];
  processFile(file);
  e.target.value = ''; 
};
const handleDragOver = () => { isDragging.value = true; };
const handleDragLeave = () => { isDragging.value = false; };
const handleDrop = (e) => {
  isDragging.value = false;
  const file = e.dataTransfer.files[0];
  processFile(file);
};

// 裁剪框交互逻辑
const onImageLoad = (e) => {
  const img = e.target;
  cropState.imgRenderW = img.width; cropState.imgRenderH = img.height;
  cropState.imgRenderX = img.offsetLeft; cropState.imgRenderY = img.offsetTop;
  cropState.naturalWidth = img.naturalWidth; cropState.naturalHeight = img.naturalHeight;
  cropState.boxX = cropState.imgRenderX + (cropState.imgRenderW - 130) / 2;
  cropState.boxY = cropState.imgRenderY + (cropState.imgRenderH - 130) / 2;
  checkBoundaries();
};
const startDrag = (e) => { e.preventDefault(); cropState.isDragging = true; cropState.startX = e.clientX; cropState.startY = e.clientY; cropState.startBoxX = cropState.boxX; cropState.startBoxY = cropState.boxY; };
const startResize = (e) => { e.preventDefault(); e.stopPropagation(); cropState.isResizing = true; cropState.startX = e.clientX; cropState.startBoxSize = cropState.boxSize; };
const onMouseMove = (e) => {
  if (cropState.isDragging) {
    cropState.boxX = cropState.startBoxX + (e.clientX - cropState.startX);
    cropState.boxY = cropState.startBoxY + (e.clientY - cropState.startY);
    checkBoundaries();
  } else if (cropState.isResizing) {
    let newSize = cropState.startBoxSize + (e.clientX - cropState.startX);
    if (newSize < MIN_BOX_SIZE) newSize = MIN_BOX_SIZE;
    const maxW = (cropState.imgRenderX + cropState.imgRenderW) - cropState.boxX;
    const maxH = (cropState.imgRenderY + cropState.imgRenderH) - cropState.boxY;
    const maxSize = Math.min(maxW, maxH);
    if (newSize > maxSize) newSize = maxSize;
    cropState.boxSize = newSize;
  }
};
const onMouseUp = () => { cropState.isDragging = false; cropState.isResizing = false; };
const checkBoundaries = () => {
  const minX = cropState.imgRenderX; const minY = cropState.imgRenderY;
  const maxX = cropState.imgRenderX + cropState.imgRenderW - cropState.boxSize;
  const maxY = cropState.imgRenderY + cropState.imgRenderH - cropState.boxSize;
  if (cropState.boxX < minX) cropState.boxX = minX; if (cropState.boxX > maxX) cropState.boxX = maxX;
  if (cropState.boxY < minY) cropState.boxY = minY; if (cropState.boxY > maxY) cropState.boxY = maxY;
};
</script>

<style scoped>
/* 样式部分保持不变，直接复用你原有的即可 */
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;700;900&display=swap');

.main-wrapper {
  width: 100%; height: 100%;
  display: flex; flex-direction: column;
  background-color: transparent;
  font-family: 'Noto Sans SC', sans-serif;
  gap: 20px; padding: 30px; box-sizing: border-box;
  position: relative;
}

.global-loading {
  position: absolute; inset: 0;
  background: rgba(255,255,255,0.85);
  z-index: 100;
  display: flex; flex-direction: column;
  align-items: center; justify-content: center;
  gap: 15px; font-weight: bold;
}
.spinner {
  width: 30px; height: 30px;
  border: 4px solid #eee; border-top: 4px solid #000;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}
@keyframes spin { 100% { transform: rotate(360deg); } }

.top-section { flex: 0 0 auto; min-height: 220px; }

.preview-card {
  width: 100%; height: 100%;
  background: #F4F1EA; border-radius: 24px;
  display: flex; padding: 24px; box-sizing: border-box; gap: 40px;
}

.preview-left {
  display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 12px;
}
.current-avatar-box {
  width: 140px; height: 140px; border-radius: 50%;
  border: 4px solid #fff; overflow: hidden; position: relative;
  box-shadow: 0 8px 20px rgba(0,0,0,0.05); background: #ddd;
}
.avatar-display { width: 100%; height: 100%; object-fit: cover; }
.avatar-placeholder { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; color: #999; font-size: 12px; background: #eee; }
.scan-line {
  position: absolute; top: 0; left: 0; width: 100%; height: 100%;
  background: linear-gradient(to bottom, transparent 50%, rgba(255,255,255,0.3) 50%, transparent);
  background-size: 100% 4px; pointer-events: none;
}
.avatar-meta { display: flex; flex-direction: column; align-items: center; font-family: 'Roboto Mono', monospace; }
.meta-label { font-size: 10px; color: #999; }
.meta-status { font-size: 12px; font-weight: bold; color: #000; letter-spacing: 1px; }

.upload-right {
  flex: 1; border: 2px dashed rgba(0,0,0,0.1); border-radius: 16px;
  transition: all 0.2s cubic-bezier(0.25, 0.46, 0.45, 0.94);
  background: rgba(255,255,255,0.4);
}
.upload-right:hover { border-color: #000; background: #fff; }
.upload-right.is-dragging {
  border-color: #000; border-style: solid; background-color: #e6f7ff;
  transform: scale(0.98); box-shadow: inset 0 0 20px rgba(0,0,0,0.05);
}
.upload-area {
  width: 100%; height: 100%; display: flex; align-items: center; justify-content: center;
  cursor: pointer; gap: 20px;
}
.upload-icon { font-size: 40px; font-weight: 300; color: #000; }
.upload-text h3 { margin: 0; font-size: 16px; font-weight: 800; }
.upload-text p { margin: 4px 0 0 0; font-size: 12px; color: #666; }

.editor-card {
  width: 100%; height: 300px; background: #1a1a1a; border-radius: 24px;
  display: flex; overflow: hidden; color: #fff;
}
.editor-stage {
  flex: 1; position: relative; display: flex; align-items: center; justify-content: center;
  overflow: hidden; background: #000;
}
.grid-bg {
  position: absolute; top:0; left:0; width:100%; height:100%;
  background-image: radial-gradient(#333 1px, transparent 1px);
  background-size: 20px 20px; opacity: 0.3; pointer-events: none;
}
.source-image { max-width: 90%; max-height: 90%; display: block; opacity: 0.6; }
.overlay { position: absolute; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0); pointer-events: none; }
.crop-box {
  position: absolute; border-radius: 50%; box-shadow: 0 0 0 9999px rgba(0, 0, 0, 0.8);
  z-index: 10; border: 2px solid #fff; cursor: move;
}
.corner-marker { position: absolute; width: 10px; height: 10px; border-color: #fff; border-style: solid; }
.tl { top: 0; left: 0; border-width: 2px 0 0 2px; border-top-left-radius: 50%; }
.tr { top: 0; right: 0; border-width: 2px 2px 0 0; border-top-right-radius: 50%; }
.bl { bottom: 0; left: 0; border-width: 0 0 2px 2px; border-bottom-left-radius: 50%; }
.br { bottom: 0; right: 0; border-width: 0 2px 2px 0; border-bottom-right-radius: 50%; }
.resize-handle {
  width: 16px; height: 16px; background-color: #d35400; position: absolute;
  bottom: 14%; right: 14%; cursor: se-resize; border-radius: 50%; border: 2px solid #fff;
}
.editor-sidebar {
  width: 200px; background: #222; display: flex; flex-direction: column;
  justify-content: space-between; padding: 30px 20px; border-right: 1px solid #333;
}
.editor-header .step-label { font-size: 10px; color: #666; }
.editor-header h3 { margin: 5px 0; font-size: 14px; color: #fff; }
.editor-actions { display: flex; flex-direction: column; gap: 10px; }
.action-btn { height: 40px; border: none; cursor: pointer; font-weight: bold; font-size: 12px; transition: all 0.2s; }
.confirm-btn { background: #fff; color: #000; border-radius: 4px; }
.confirm-btn:hover { background: #e6e6e6; }
.cancel-btn { background: #000; color: #666; border-radius: 4px;}
.cancel-btn:hover { color: #fff; }

.bottom-section { flex: 1; display: flex; flex-direction: column; position: relative; }
.gallery-header { margin-bottom: 16px; border-bottom: 1px solid rgba(0,0,0,0.05); }
.tabs { display: flex; gap: 30px; }
.tab-item {
  padding-bottom: 12px; cursor: pointer; font-size: 12px; font-weight: 700; color: #999;
  position: relative; transition: color 0.2s;
}
.tab-item.active { color: #000; }
.tab-item.active::after {
  content: ''; position: absolute; bottom: -1px; left: 0; width: 100%; height: 2px; background: #000;
}
.gallery-content { flex: 1; overflow-y: auto; padding: 16px; }
.gallery-content::-webkit-scrollbar { width: 4px; }
.gallery-content::-webkit-scrollbar-thumb { background: #eee; border-radius: 2px; }
.avatar-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(90px, 1fr)); gap: 20px; }
.avatar-slot {
  aspect-ratio: 1; border-radius: 50%; cursor: pointer; position: relative;
  border: 4px solid transparent; transition: all 0.2s; padding: 4px;
}
.avatar-slot:hover { transform: scale(1.05); }
.avatar-slot img { width: 100%; height: 100%; border-radius: 50%; object-fit: cover; background: #f0f0f0; }
.avatar-slot.selected { border-color: #000; }
.selection-indicator {
  position: absolute; bottom: 0; right: 0; width: 24px; height: 24px;
  background: #000; border-radius: 50%; color: #fff; display: none;
  align-items: center; justify-content: center; font-size: 12px;
}
.avatar-slot.selected .selection-indicator { display: flex; }
.avatar-slot.selected .selection-indicator::after { content: '✓'; }
.empty-state {
  height: 100px; display: flex; flex-direction: column;
  align-items: center; justify-content: center; color: #ccc; font-family: 'Roboto Mono';
}

.floating-bar {
  position: absolute; bottom: 0; left: 0; right: 0; height: 60px;
  background: #000; border-radius: 30px; display: flex; align-items: center;
  justify-content: space-between; padding: 0 8px 0 30px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.15); color: #fff;
}
.bar-info { display: flex; gap: 10px; font-size: 12px; }
.bar-info .label { opacity: 0.6; }
.bar-info .val { font-weight: bold; color: #d35400; }
.apply-btn {
  background: #fff; color: #000; border: none; padding: 10px 24px;
  border-radius: 20px; font-weight: 900; cursor: pointer; transition: transform 0.2s;
}
.apply-btn:hover { transform: scale(1.05); }

.slide-up-enter-active, .slide-up-leave-active { transition: all 0.3s ease; }
.slide-up-enter-from, .slide-up-leave-to { transform: translateY(100%); opacity: 0; }
.upload-text{color:black}
</style>