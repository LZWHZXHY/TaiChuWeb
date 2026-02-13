<template>
  <div class="publish-post-module">
    <div class="input-section">
      <div class="field-group">
        <span class="field-prefix">>> DATA_TITLE</span>
        <input 
          v-model="postData.title" 
          type="text" 
          class="cyber-input title-field" 
          placeholder="输入传输标题..." 
          maxlength="50"
        />
      </div>

      <div class="field-group">
        <span class="field-prefix">>> CONTENT_LOAD</span>
        <textarea 
          v-model="postData.content" 
          class="cyber-input content-field custom-scroll" 
          placeholder="正在监听实时信号..."
        ></textarea>
      </div>
    </div>

    <div class="media-section">
      <div class="section-tag">// VISUAL_ATTACHMENTS ({{ previews.length }}/9)</div>
      <div class="media-grid">
        <div v-for="(img, index) in previews" :key="index" class="img-preview-card">
          <img :src="img.url" />
          <button class="remove-btn" @click="removeImage(index)">×</button>
          <div class="card-deco">IMG_{{ index + 1 }}</div>
        </div>
        
        <label v-if="previews.length < 9" class="upload-trigger">
          <input type="file" accept="image/*" multiple @change="handleFileSelect" hidden />
          <div class="plus-icon">+</div>
          <div class="trigger-text">UPLOAD_DATA</div>
        </label>
      </div>
    </div>

    <div class="module-footer">
      <span class="protocol">ENCRYPTION_NONE // COMPRESSION_AUTO</span>
      <span class="char-count">{{ postData.content.length }} CHARS</span>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue';
import apiClient from '@/utils/api';

// 接收来自主容器的元数据 (标签、可见性)
const props = defineProps({
  sharedMeta: { type: Object, required: true }
});

const emit = defineEmits(['success']);

// 内部状态管理
const postData = reactive({
  title: '',
  content: ''
});

const fileList = ref([]);
const previews = ref([]);

/**
 * 处理文件选择逻辑
 */
const handleFileSelect = (event) => {
  const files = Array.from(event.target.files);
  if (!files.length) return;

  files.forEach(file => {
    // 限制 10MB
    if (file.size > 10 * 1024 * 1024) return; 
    
    fileList.value.push(file);
    previews.value.push({
      url: URL.createObjectURL(file),
      name: file.name
    });
  });
  event.target.value = ''; // 重置 input 方便连选
};

/**
 * 移除已选图片
 */
const removeImage = (index) => {
  fileList.value.splice(index, 1);
  URL.revokeObjectURL(previews.value[index].url);
  previews.value.splice(index, 1);
};

/**
 * ✅ 核心：暴露给父组件执行的校验逻辑
 */
const validate = () => {
  if (!postData.title.trim()) {
    alert('TRANSMISSION_REJECTED: TITLE_MISSING');
    return false;
  }
  return true;
};

/**
 * ✅ 核心：暴露给父组件执行的提交逻辑
 */
const submit = async () => {
  try {
    const submission = new FormData();
    submission.append('Title', postData.title.trim());
    submission.append('Content', postData.content.trim());
    submission.append('Tags', props.sharedMeta.tags);
    submission.append('Visibility', props.sharedMeta.visibility);
    submission.append('PostType', 0); // 默认为动态帖

    fileList.value.forEach(file => {
      submission.append('Images', file); 
    });

    const res = await apiClient.post('/ThePost/create', submission);
    
    if (res.data.success) {
      emit('success', res.data);
      return res.data;
    }
  } catch (error) {
    console.error('PROTOCOL_ERROR:', error);
    throw error;
  }
};

// 必须暴露方法，否则父组件无法通过 ref 调用
defineExpose({ validate, submit });
</script>

<style scoped>
/* 继承太初控制台的排版风格 */
.publish-post-module {
  display: flex;
  flex-direction: column;
  gap: 25px;
  animation: slide-in 0.3s ease-out;
}

.field-group { margin-bottom: 20px; }
.field-prefix {
  display: block;
  font-family: 'JetBrains Mono', monospace;
  font-size: 10px;
  color: #D92323;
  font-weight: bold;
  margin-bottom: 6px;
}

.cyber-input {
  width: 100%;
  background: #fff;
  border: 2px solid #111;
  padding: 12px;
  font-family: inherit;
  outline: none;
  transition: 0.2s;
  color:#000
}

.title-field {
  font-family: 'Anton', sans-serif;
  font-size: 24px;
  font-weight: 900;
  text-transform: uppercase;
}

.content-field {
  min-height: 140px;
  font-size: 16px;
  line-height: 1.5;
}

.cyber-input:focus {
  border-color: #D92323;
  box-shadow: 6px 6px 0 rgba(217, 35, 35, 0.1);
}

/* 媒体矩阵样式 */
.section-tag { font-family: 'JetBrains Mono'; font-size: 10px; font-weight: bold; margin-bottom: 10px; color: #888; }
.media-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  gap: 12px;
}

.img-preview-card {
  aspect-ratio: 1;
  border: 2px solid #111;
  position: relative;
  background: #000;
  overflow: hidden;
}

.img-preview-card img { width: 100%; height: 100%; object-fit: cover; }

.remove-btn {
  position: absolute; top: 0; right: 0;
  background: #D92323; color: white; border: none;
  padding: 2px 6px; cursor: pointer; z-index: 2;
}

.card-deco {
  position: absolute; bottom: 2px; left: 4px;
  font-size: 8px; color: rgba(255,255,255,0.5); font-family: 'JetBrains Mono';
}

.upload-trigger {
  aspect-ratio: 1;
  border: 2px dashed #999;
  background: #f0f0f0;
  display: flex; flex-direction: column;
  align-items: center; justify-content: center;
  cursor: pointer; transition: 0.2s;
}

.upload-trigger:hover { border-color: #D92323; background: #fff; }
.plus-icon { font-size: 24px; color: #999; }
.upload-text { font-family: 'JetBrains Mono'; font-size: 8px; font-weight: bold; margin-top: 4px; }

.module-footer {
  margin-top: auto;
  padding-top: 15px;
  border-top: 1px dashed #ccc;
  display: flex; justify-content: space-between;
  font-family: 'JetBrains Mono'; font-size: 9px; color: #aaa;
}

@keyframes slide-in {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>