<template>
  <div class="publish-drawing-module">
    <div class="upload-section">
      <div 
        class="upload-zone" 
        @click="triggerFileSelect" 
        :class="{ 'has-file': uploadForm.previewUrl, 'is-dragover': isDragOver }"
        @dragover.prevent="isDragOver = true"
        @dragleave.prevent="isDragOver = false"
        @drop.prevent="handleDrop"
      >
        <input type="file" ref="fileInput" accept="image/*" @change="handleFileChange" style="display:none" />
        
        <div v-if="uploadForm.previewUrl" class="preview-wrap">
          <img :src="uploadForm.previewUrl" />
          <div class="reupload-overlay">
            <span class="btn-replace">REPLACE_SOURCE // 更换源文件</span>
          </div>
          <div class="file-meta">
            FILE_READY: {{ uploadForm.file?.name || 'UNKNOWN' }}
          </div>
        </div>

        <div v-else class="placeholder">
          <div class="scan-grid"></div>
          <div class="center-content">
            <div class="icon-frame"><span class="plus">+</span></div>
            <div class="text-group">
              <p class="main-tip">DRAG & DROP or CLICK TO UPLOAD</p>
              <span class="sub-tip">PROTOCOL: JPG / PNG / WEBP / GIF (MAX 20MB)</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="form-section">
      <div class="cyber-input-group">
        <div class="label-chip">ART_TITLE / 作品标题 (必填)</div>
        <input type="text" v-model="uploadForm.title" placeholder="请输入中枢检索标题..." class="cyber-input" maxlength="50" />
        <div class="input-line"></div>
      </div>

      <div class="cyber-input-group">
        <div class="label-chip">DESCRIPTION / 创作协议 (必填)</div>
        <textarea v-model="uploadForm.desc" placeholder="输入作品背后的设定、故事或参数..." rows="4" class="cyber-input textarea"></textarea>
        <div class="input-line"></div>
      </div>

      <div class="cyber-input-group">
        <div class="label-chip">CREDIT / 署名</div>
        <input type="text" v-model="uploadForm.authorName" placeholder="留空则使用当前操作员 ID" class="cyber-input" />
        <div class="input-line"></div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue';
import apiClient from '@/utils/api';

// 接收父组件传递的全局元数据（标签、权限）
const props = defineProps({
  sharedMeta: { type: Object, required: true }
});

const emit = defineEmits(['success']);

const isDragOver = ref(false);
const fileInput = ref(null);

const uploadForm = reactive({
  title: '',
  desc: '',
  authorName: '',
  file: null,
  previewUrl: ''
});

// --- 文件处理逻辑 (从原 Gallery 迁移) ---

const triggerFileSelect = () => { fileInput.value.click(); };

const handleFileChange = (e) => { processFile(e.target.files[0]); };

const handleDrop = (e) => { 
  isDragOver.value = false; 
  processFile(e.dataTransfer.files[0]); 
};

const processFile = (file) => {
  if (!file) return;
  if (!file.type.startsWith('image/')) return alert("INVALID_PROTOCOL: 仅限图像文件");
  if (file.size > 20 * 1024 * 1024) return alert("DATA_OVERLOAD: 文件超出 20MB 限制");
  
  uploadForm.file = file;
  uploadForm.previewUrl = URL.createObjectURL(file);
};

// --- ✅ 核心：暴露给 UniversalPublisherModal 调用的方法 ---

const validate = () => {
  if (!uploadForm.file) { alert("DATA_MISSING: 请先上传作品文件"); return false; }
  if (!uploadForm.title.trim()) { alert("FIELD_REQUIRED: 标题不可为空"); return false; }
  if (!uploadForm.desc.trim()) { alert("FIELD_REQUIRED: 描述不可为空"); return false; }
  return true;
};

const submit = async () => {
  const formData = new FormData();
  formData.append('Image', uploadForm.file);
  formData.append('Title', uploadForm.title);
  formData.append('Desc', uploadForm.desc);
  
  // 合并父组件传来的全局元数据
  formData.append('Tags', props.sharedMeta.tags); 
  formData.append('Visibility', props.sharedMeta.visibility);
  
  if (uploadForm.authorName) {
    formData.append('AuthorName', uploadForm.authorName);
  }

  const res = await apiClient.post('/Drawing/upload', formData, { 
    headers: { 'Content-Type': 'multipart/form-data' } 
  });
  
  if (res.data.success) {
    emit('success', res.data);
  } else {
    throw new Error(res.data.message || 'UPLOAD_FAILED');
  }
};

// 暴露接口
defineExpose({ validate, submit });
</script>

<style scoped>
/* 继承你原来的样式，并针对组件化做微调 */
.publish-drawing-module {
  display: flex;
  flex-direction: column;
  gap: 30px;
  padding: 20px;
}

.upload-zone {
  height: 300px; /* 增加高度，在全屏模式下更美观 */
  border: 2px dashed #999;
  background: #2a2a2a; /* 稍微调深，增加高级感 */
  position: relative;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  justify-content: center;
  align-items: center;
  overflow: hidden;
}

.upload-zone.has-file { border: 2px solid #111; background: #000; }
.upload-zone:hover { border-color: #D92323; background: #333; }

.scan-grid { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(45deg, #444 25%, transparent 25%, transparent 75%, #444 75%, #444); 
  background-size: 30px 30px; opacity: 0.1; 
}

.icon-frame { width: 80px; height: 80px; border: 2px solid #111; background: #fff; display: flex; align-items: center; justify-content: center; margin: 0 auto 15px; }
.plus { font-size: 50px; color: #111; }
.main-tip { font-weight: 800; font-family: 'Anton'; letter-spacing: 1px; color: #fff; }

.preview-wrap { width: 100%; height: 100%; position: relative; }
.preview-wrap img { width: 100%; height: 100%; object-fit: contain; }

/* 复用原代码的输入框样式 */
.cyber-input-group { position: relative; margin-top: 10px; }
.label-chip { 
  position: absolute; top: -12px; left: 10px; 
  background: #111; color: #fff; font-size: 0.7rem; 
  font-family: 'JetBrains Mono'; padding: 2px 8px; z-index: 2; 
}
.cyber-input { 
  width: 100%; background: #fff; border: 2px solid #ccc; 
  border-bottom: 4px solid #111; padding: 15px; 
  font-family: 'JetBrains Mono'; font-size: 1rem; outline: none; 
}
.cyber-input.textarea { resize: vertical; min-height: 100px; }
.cyber-input:focus { border-color: #111; background: #fffef0; }
.input-line { position: absolute; bottom: -4px; left: 0; height: 4px; width: 0; background: #D92323; transition: width 0.3s; }
.cyber-input:focus + .input-line { width: 100%; }

.file-meta { position: absolute; bottom: 0; right: 0; background: #D92323; color: #fff; font-size: 10px; padding: 4px 10px; font-family: 'JetBrains Mono'; }
</style>