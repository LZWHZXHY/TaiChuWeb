<template>
  <div class="main-wrapper">
    <div class="top-container">
      
      <div v-if="isEditing" class="editor-layout">
        <div class="editor-left" ref="editorContainerRef" @mousemove="onMouseMove" @mouseup="onMouseUp" @mouseleave="onMouseUp">
          <img 
            ref="sourceImgRef" 
            :src="tempImgUrl" 
            class="source-image" 
            draggable="false"
            @load="onImageLoad"
          />
          <div class="overlay"></div>
          
          <div 
            class="crop-box" 
            :style="cropBoxStyle"
            @mousedown.stop="startDrag"
          >
            <div class="resize-handle" @mousedown.stop="startResize"></div>
          </div>
        </div>

        <div class="editor-right">
          <button class="action-btn confirm-btn" @click="confirmCrop">确定</button>
          <button class="action-btn cancel-btn" @click="cancelEdit">取消</button>
        </div>
      </div>

      <div v-else class="upload-view">
        <div class="current-avatar-preview">
          <img v-if="currentAvatar" :src="currentAvatar" class="avatar-display" />
          <div v-else class="avatar-placeholder">暂无头像</div>
        </div>
        
        <div class="upload-action">
          <input 
            type="file" 
            ref="fileInputRef" 
            accept="image/png, image/jpeg, image/jpg, image/gif" 
            style="display: none" 
            @change="handleFileChange"
          />
          <button class="upload-btn" @click="triggerUpload">
            上传头像 (Max 5MB)
          </button>
          <p class="upload-hint">支持 JPG/PNG/GIF，GIF 不支持裁剪</p>
        </div>
      </div>
    </div>

    <div class="bottom-container">
      <div class="nav-bar">
        <div 
          class="nav-item" 
          :class="{ active: activeTab === 'default' }"
          @click="activeTab = 'default'"
        >
          默认头像
        </div>
        <div 
          class="nav-item" 
          :class="{ active: activeTab === 'community' }"
          @click="activeTab = 'community'"
        >
          社区推荐
        </div>
      </div>

      <div class="gallery-wrapper">
        <div class="gallery-content">
          <div v-if="activeTab === 'default'" class="avatar-grid">
            <div 
              v-for="(item, index) in defaultAvatars" 
              :key="item.id"
              class="avatar-item"
              :class="{ selected: tempSelectedId === item.id }" 
              @click="preSelectDefaultAvatar(item)"
            >
              <img :src="item.url" :alt="item.name" />
            </div>
          </div>
          <div v-else class="empty-state">
            社区功能暂未开放
          </div>
        </div>

        <div v-if="tempSelectedId && activeTab === 'default'" class="gallery-footer">
            <div class="footer-info">已选择: {{ tempSelectedName }}</div>
            <button class="confirm-select-btn" @click="confirmDefaultSelection">
              确认使用该头像
            </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, reactive } from 'vue';

// --- 状态管理 ---
const currentAvatar = ref(''); // 最终显示的头像 URL
const tempImgUrl = ref('');    // 编辑中的原始图片 URL
const isEditing = ref(false);  // 是否处于编辑模式
const activeTab = ref('default'); 

// 默认头像逻辑修改：改为“临时选中”和“最终确认”
const tempSelectedId = ref(null);   // 当前点击高亮但未确认的ID
const tempSelectedName = ref('');   // 用于显示名字
const tempSelectedUrl = ref('');    // 暂存URL

const fileInputRef = ref(null);
const editorContainerRef = ref(null);
const sourceImgRef = ref(null);

// --- 裁剪器核心数据 ---
const MIN_BOX_SIZE = 130;
const cropState = reactive({
  startX: 0,
  startY: 0,
  startBoxX: 0,
  startBoxY: 0,
  startBoxSize: 0,
  isDragging: false,
  isResizing: false,
  boxX: 0,
  boxY: 0,
  boxSize: 130, 
  imgRenderW: 0,
  imgRenderH: 0,
  imgRenderX: 0,
  imgRenderY: 0,
  naturalWidth: 0,
  naturalHeight: 0
});

// --- 默认头像数据 ---
const defaultAvatars = [
  { id: 1, name: 'Cat 1', url: 'https://placekitten.com/200/200' },
  { id: 2, name: 'Dog 1', url: 'https://placedog.net/200/200?r' }, 
  { id: 3, name: 'Boy 1', url: 'https://avatar.iran.liara.run/public/boy?username=User1' },
  { id: 4, name: 'Girl 1', url: 'https://avatar.iran.liara.run/public/girl?username=User2' },
  { id: 5, name: 'Cat 2', url: 'https://placekitten.com/201/201' },
  { id: 6, name: 'Dog 2', url: 'https://placedog.net/200/200?id=4' },
  { id: 7, name: 'Boy 2', url: 'https://avatar.iran.liara.run/public/boy?username=User3' },
  { id: 8, name: 'Girl 2', url: 'https://avatar.iran.liara.run/public/girl?username=User4' },
];

// --- 样式计算 ---
const cropBoxStyle = computed(() => ({
  top: `${cropState.boxY}px`,
  left: `${cropState.boxX}px`,
  width: `${cropState.boxSize}px`,
  height: `${cropState.boxSize}px`
}));

// --- 上传逻辑 ---
const triggerUpload = () => {
  fileInputRef.value.click();
};

const handleFileChange = (e) => {
  const file = e.target.files[0];
  if (!file) return;

  if (file.size > 5 * 1024 * 1024) {
    alert('图片大小不能超过 5MB');
    return;
  }

  // 用户主动上传时，清除下方的默认头像选中状态
  tempSelectedId.value = null; 

  if (file.type === 'image/gif') {
    const url = URL.createObjectURL(file);
    currentAvatar.value = url;
    isEditing.value = false; // 确保不显示编辑器
    e.target.value = ''; 
  } else {
    const reader = new FileReader();
    reader.onload = (evt) => {
      tempImgUrl.value = evt.target.result;
      isEditing.value = true; // 开启编辑器
      cropState.boxSize = 130;
      cropState.boxX = 0;
      cropState.boxY = 0;
    };
    reader.readAsDataURL(file);
  }
};

// --- 裁剪器逻辑 ---
const onImageLoad = (e) => {
  const img = e.target;
  cropState.imgRenderW = img.width;
  cropState.imgRenderH = img.height;
  cropState.imgRenderX = img.offsetLeft;
  cropState.imgRenderY = img.offsetTop;
  cropState.naturalWidth = img.naturalWidth;
  cropState.naturalHeight = img.naturalHeight;

  // 居中
  cropState.boxX = cropState.imgRenderX + (cropState.imgRenderW - 130) / 2;
  cropState.boxY = cropState.imgRenderY + (cropState.imgRenderH - 130) / 2;
  checkBoundaries();
};

const startDrag = (e) => {
  e.preventDefault();
  cropState.isDragging = true;
  cropState.startX = e.clientX;
  cropState.startY = e.clientY;
  cropState.startBoxX = cropState.boxX;
  cropState.startBoxY = cropState.boxY;
};

const startResize = (e) => {
  e.preventDefault();
  e.stopPropagation();
  cropState.isResizing = true;
  cropState.startX = e.clientX;
  cropState.startBoxSize = cropState.boxSize;
};

const onMouseMove = (e) => {
  if (cropState.isDragging) {
    const deltaX = e.clientX - cropState.startX;
    const deltaY = e.clientY - cropState.startY;
    cropState.boxX = cropState.startBoxX + deltaX;
    cropState.boxY = cropState.startBoxY + deltaY;
    checkBoundaries();
  } else if (cropState.isResizing) {
    const delta = e.clientX - cropState.startX; 
    let newSize = cropState.startBoxSize + delta;
    if (newSize < MIN_BOX_SIZE) newSize = MIN_BOX_SIZE;
    
    const maxW = (cropState.imgRenderX + cropState.imgRenderW) - cropState.boxX;
    const maxH = (cropState.imgRenderY + cropState.imgRenderH) - cropState.boxY;
    const maxSize = Math.min(maxW, maxH);
    if (newSize > maxSize) newSize = maxSize;
    
    cropState.boxSize = newSize;
  }
};

const onMouseUp = () => {
  cropState.isDragging = false;
  cropState.isResizing = false;
};

const checkBoundaries = () => {
  const minX = cropState.imgRenderX;
  const minY = cropState.imgRenderY;
  const maxX = cropState.imgRenderX + cropState.imgRenderW - cropState.boxSize;
  const maxY = cropState.imgRenderY + cropState.imgRenderH - cropState.boxSize;

  if (cropState.boxX < minX) cropState.boxX = minX;
  if (cropState.boxX > maxX) cropState.boxX = maxX;
  if (cropState.boxY < minY) cropState.boxY = minY;
  if (cropState.boxY > maxY) cropState.boxY = maxY;
};

// --- 确定与取消 (裁剪) ---
const cancelEdit = () => {
  isEditing.value = false;
  tempImgUrl.value = '';
  fileInputRef.value.value = '';
};

const confirmCrop = () => {
  const canvas = document.createElement('canvas');
  const size = cropState.boxSize;
  const scaleX = cropState.naturalWidth / cropState.imgRenderW;
  const scaleY = cropState.naturalHeight / cropState.imgRenderH;

  canvas.width = size * scaleX;
  canvas.height = size * scaleY;
  
  const ctx = canvas.getContext('2d');
  const sourceX = (cropState.boxX - cropState.imgRenderX) * scaleX;
  const sourceY = (cropState.boxY - cropState.imgRenderY) * scaleY;
  const sourceSize = size * scaleX;

  ctx.drawImage(sourceImgRef.value, sourceX, sourceY, sourceSize, sourceSize, 0, 0, canvas.width, canvas.height);

  currentAvatar.value = canvas.toDataURL('image/png');
  isEditing.value = false;
  fileInputRef.value.value = '';
  // 确认裁剪后，清除下方默认头像的选中状态
  tempSelectedId.value = null;
};

// --- 底部逻辑 (新) ---

// 步骤1：预选中
const preSelectDefaultAvatar = (item) => {
  tempSelectedId.value = item.id;
  tempSelectedName.value = item.name;
  tempSelectedUrl.value = item.url;
  // 注意：这里不再直接修改 currentAvatar
};

// 步骤2：确认选择
const confirmDefaultSelection = () => {
  if (!tempSelectedId.value) return;

  // 1. 设置上方头像
  currentAvatar.value = tempSelectedUrl.value;
  
  // 2. 强制关闭可能存在的上方编辑框
  if (isEditing.value) {
    isEditing.value = false;
    tempImgUrl.value = '';
    fileInputRef.value.value = '';
  }

  // 3. (可选) 是否要清除选中状态？通常保留表示"当前选中了默认的这个"
  // 这里我们保留 tempSelectedId 以维持边框高亮，表示当前生效的是这个
};

</script>

<style scoped>
/* 全局容器 */
.main-wrapper {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  background-color: #F4F1EA; 
  font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
}

/* --- 上半部分 (40%) --- */
.top-container {
  height: 40%;
  border-bottom: 1px solid #ccc;
  background-color: #fff;
  position: relative;
  overflow: hidden;
}

/* 上传视图 */
.upload-view {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 20px;
}

.current-avatar-preview {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  border: 4px solid #e0e0e0;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fafafa;
}

.avatar-display {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.avatar-placeholder {
  color: #999;
  font-size: 14px;
}

.upload-btn {
  padding: 10px 24px;
  background-color: #333;
  color: #fff;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 600;
  transition: opacity 0.2s;
}

.upload-btn:hover {
  opacity: 0.8;
}

.upload-hint {
  font-size: 12px;
  color: #888;
}

/* 编辑视图布局 */
.editor-layout {
  width: 100%;
  height: 100%;
  display: flex;
}

.editor-left {
  flex: 1;
  background-color: #1a1a1a;
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  user-select: none;
}

.source-image {
  max-width: 90%;
  max-height: 90%;
  display: block;
  pointer-events: none; 
}

.crop-box {
  position: absolute;
  border: 2px solid #fff;
  box-shadow: 0 0 0 9999px rgba(0, 0, 0, 0.6);
  cursor: move;
  z-index: 10;
}

.resize-handle {
  width: 16px;
  height: 16px;
  background-color: #1890ff;
  position: absolute;
  bottom: -8px;
  right: -8px;
  cursor: se-resize;
  border-radius: 50%;
  border: 2px solid #fff;
}

.editor-right {
  width: 120px;
  background-color: #fff;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  gap: 20px;
  border-left: 1px solid #eee;
}

.action-btn {
  width: 80px;
  height: 36px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-weight: bold;
}

.confirm-btn {
  background-color: #1890ff;
  color: #fff;
}

.cancel-btn {
  background-color: #f5f5f5;
  color: #333;
}

/* --- 下半部分 (60%) --- */
.bottom-container {
  height: 60%;
  display: flex;
  flex-direction: column;
  position: relative; /* 为绝对定位的 footer 做参考 */
}

.nav-bar {
  height: 50px;
  display: flex;
  border-bottom: 1px solid #e0e0e0;
  background: #fff;
  flex-shrink: 0;
}

.nav-item {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  font-size: 16px;
  color: #666;
  position: relative;
  transition: all 0.2s;
}

.nav-item:hover {
  background-color: #fafafa;
}

.nav-item.active {
  color: #333;
  font-weight: bold;
  background-color: #F4F1EA;
}

.nav-item.active::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  width: 100%;
  height: 3px;
  background-color: #333;
}

/* Gallery Wrapper: 包含内容区和底部的固定按钮 */
.gallery-wrapper {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden; /* 内部滚动 */
  position: relative;
}

.gallery-content {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  padding-bottom: 80px; /* 预留底部按钮空间，防止遮挡最后一行 */
}

.avatar-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(80px, 1fr));
  gap: 15px;
}

.avatar-item {
  aspect-ratio: 1;
  border-radius: 8px;
  overflow: hidden;
  cursor: pointer;
  border: 3px solid transparent;
  transition: all 0.2s;
}

.avatar-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}

/* 选中高亮 */
.avatar-item.selected {
  border-color: #1890ff;
  box-shadow: 0 0 0 2px rgba(24, 144, 255, 0.2);
}

.avatar-item img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.empty-state {
  text-align: center;
  color: #999;
  margin-top: 40px;
}

/* 底部浮动确认条 */
.gallery-footer {
  position: absolute;
  bottom: 0;
  left: 0;
  width: 100%;
  height: 70px;
  background-color: rgba(255, 255, 255, 0.95);
  border-top: 1px solid #eee;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  box-sizing: border-box;
  box-shadow: 0 -4px 12px rgba(0,0,0,0.05);
  backdrop-filter: blur(5px);
}

.footer-info {
  font-size: 14px;
  color: #333;
  font-weight: 500;
}

.confirm-select-btn {
  padding: 8px 20px;
  background-color: #1890ff;
  color: #fff;
  border: none;
  border-radius: 4px;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.2s;
}

.confirm-select-btn:hover {
  background-color: #096dd9;
}
</style>