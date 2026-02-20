<template>
  <router-link :to="`/art/${data.id}`" class="md-link">
    <div class="md-art-container">
      
      <div class="md-file-header">
        <div class="md-file-info">
          <span class="md-icon">📄</span>
          <span class="md-filename">ATTACHMENT_IMG_{{ data.id || 'NUL' }}.PNG</span>
        </div>
        <span class="md-type">[VISUAL_ART]</span>
      </div>

      <div class="md-image-wrapper">
        <img 
          :src="data.imgUrl || 'https://via.placeholder.com/800x400/111/333?text=MISSING_ASSET'" 
          alt="Art Cover" 
          class="md-raw-img" 
        />
      </div>
      
      <div class="md-caption">
        <span class="md-title">>> {{ data.title || 'untitled_asset' }}</span>
        <span class="md-author">SRC: @{{ data.author }}</span>
      </div>

    </div>
  </router-link>
</template>

<script setup lang="ts">
// 使用 any 绕过 TS 检查
defineProps<{
  data: any
}>();
</script>

<style scoped>
.md-link { 
  text-decoration: none; 
  display: block; 
}

/* 移除圆角和阴影，改为硬朗的文件框 */
.md-art-container {
  border: 1px solid rgba(255, 255, 255, 0.1);
  background: rgba(0, 0, 0, 0.2);
  margin: 10px 0;
  transition: border-color 0.2s ease;
}

.md-art-container:hover {
  border-color: rgba(170, 0, 255, 0.4); /* 保留一点点极简的霓虹紫反馈 */
}

/* 附件头部：等宽字体，模拟代码编辑器里的文件栏 */
.md-file-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 16px;
  background: rgba(255, 255, 255, 0.03);
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
  font-family: 'JetBrains Mono', monospace;
  font-size: 11px;
  color: #888;
}

.md-file-info {
  display: flex;
  gap: 8px;
  align-items: center;
}

.md-filename {
  color: #ccc;
  letter-spacing: 1px;
}

.md-type {
  color: #AA00FF; /* 艺术类专属的紫色标签 */
  opacity: 0.8;
}

/* 图片容器：取消遮罩渐变 */
.md-image-wrapper {
  width: 100%;
  overflow: hidden;
  background: #050505;
}

.md-raw-img {
  width: 100%;
  display: block;
  /* 加一点点灰度，让它看起来更像档案/复印件，悬浮时恢复全彩 */
  filter: grayscale(15%); 
  transition: filter 0.3s ease;
}

.md-art-container:hover .md-raw-img {
  filter: grayscale(0%);
}

/* 底部注脚 */
.md-caption {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  font-family: 'JetBrains Mono', monospace;
  font-size: 12px;
  color: #666;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
}

.md-title {
  color: #fff;
  font-family: 'Inter', sans-serif;
  font-size: 14px;
}

.md-author {
  color: #999;
  font-size: 11px;
}
</style>