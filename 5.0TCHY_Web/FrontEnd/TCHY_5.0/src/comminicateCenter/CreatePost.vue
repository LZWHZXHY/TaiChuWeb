<template>
  <Teleport to="body">
    <Transition name="modal-fade">
      <div v-if="modelValue" class="cp-overlay" @click.self="closeModal">
        <div class="cp-container md-elevation-5">
          
          <header class="cp-header">
            <div class="header-title">
              <h3>{{ t('CreatePost.create_title') }}</h3>
              <span class="header-sub">NEW_TRANSMISSION //</span>
            </div>
            <button class="close-btn" @click="closeModal">
              <i class="fas fa-times"></i>
            </button>
          </header>

          <div class="cp-body custom-scroll">
            <div class="form-group">
              <label class="input-label">TITLE_</label>
              <input 
                v-model="formData.title" 
                type="text" 
                class="cp-input title-input" 
                :placeholder="t('CreatePost.placeholder_title')" 
                maxlength="50"
              />
            </div>

            <div class="form-group">
              <label class="input-label">CONTENT_</label>
              <textarea 
                v-model="formData.content" 
                class="cp-input content-area custom-scroll" 
                :placeholder="t('CreatePost.placeholder_content')"
              ></textarea>
            </div>

            <div class="media-section">
              <div class="media-grid">
                <div v-for="(img, index) in previewImages" :key="index" class="img-preview-card">
                  <img :src="img.url" />
                  <button class="remove-img-btn" @click="removeImage(index)">
                    <i class="fas fa-trash-alt"></i>
                  </button>
                </div>
                
                <label v-if="previewImages.length < 9" class="upload-btn">
                  <input type="file" accept="image/*" multiple @change="handleFileSelect" hidden />
                  <i class="fas fa-plus"></i>
                  <span>{{ t('CreatePost.add_img') || 'ADD_IMG' }}</span>
                </label>
              </div>
            </div>
          </div>

          <footer class="cp-footer">
            <div class="status-indicator">
              <span :class="['dot', { 'processing': isSubmitting }]"></span>
              {{ isSubmitting ? t('CreatePost.uploading') : t('CreatePost.ready') }}
            </div>
            <button 
              class="submit-btn md-ripple" 
              @click="submitPost" 
              :disabled="isSubmitting || !formData.title || !formData.content"
            >
              <span v-if="isSubmitting"><i class="fas fa-circle-notch fa-spin"></i> {{ t('Register.sending') }}</span>
              <span v-else>{{ t('CreatePost.submit') }}</span>
            </button>
          </footer>

        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, reactive } from 'vue';
import apiClient from '@/utils/api';
import { useI18n } from 'vue-i18n';

// 如果你的项目中还没配置 i18n，这里给一个 fallback，防止报错
const { t } = useI18n({ useScope: 'global', missingWarn: false, fallbackWarn: false }) || { t: (key) => key };

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['update:modelValue', 'success']);

const isSubmitting = ref(false);
const fileList = ref([]); // 实际的 File 对象
const previewImages = ref([]); // 用于预览的 URL

const formData = reactive({
  title: '',
  content: ''
});

// 关闭弹窗
const closeModal = () => {
  if (isSubmitting.value) return;
  emit('update:modelValue', false);
};

// 处理图片选择
const handleFileSelect = (event) => {
  const files = Array.from(event.target.files);
  if (!files.length) return;

  files.forEach(file => {
    // 限制一下大小，比如 5MB
    if (file.size > 5 * 1024 * 1024) {
      alert(`文件 ${file.name} 太大，请上传小于 5MB 的图片`);
      return;
    }
    fileList.value.push(file);
    previewImages.value.push({
      url: URL.createObjectURL(file),
      name: file.name
    });
  });
  
  // 清空 input 使得同名文件可以再次触发 change
  event.target.value = '';
};

// 移除图片
const removeImage = (index) => {
  fileList.value.splice(index, 1);
  URL.revokeObjectURL(previewImages.value[index].url); // 释放内存
  previewImages.value.splice(index, 1);
};

// 提交逻辑
const submitPost = async () => {
  if (!formData.title.trim() || !formData.content.trim()) return;

  isSubmitting.value = true;
  
  try {
    // 构建 FormData，因为涉及到文件上传
    const submission = new FormData();
    submission.append('Title', formData.title);
    submission.append('Content', formData.content); // 注意：后端字段名需确认 (Content vs Body)
    
    fileList.value.forEach(file => {
      submission.append('images', file); // 注意：后端接收文件的字段名 (images, files, etc.)
    });

    // 假设 API 路径
    const res = await apiClient.post('/Posts/create', submission, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });

    if (res.data.success || res.status === 200) {
      // 重置表单
      formData.title = '';
      formData.content = '';
      fileList.value = [];
      previewImages.value = [];
      
      emit('success'); // 通知父组件刷新列表
      closeModal();
    } else {
      alert('发布失败: ' + (res.data.message || '未知错误'));
    }
  } catch (e) {
    console.error(e);
    alert('网络连接中断 // CONNECTION_LOST');
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600;800&display=swap');

/* 变量定义，保持与主仪表盘一致 */
.cp-overlay {
  --accent-blue: #3b82f6;
  --bg-overlay: rgba(15, 23, 42, 0.6);
  --bg-card: #ffffff;
  --text-main: #1e293b;
  --text-sub: #64748b;
  --border: #e2e8f0;
  
  position: fixed;
  inset: 0;
  background: var(--bg-overlay);
  backdrop-filter: blur(8px);
  z-index: 9999;
  display: flex;
  justify-content: center;
  align-items: center;
  font-family: 'Inter', sans-serif;
}

.cp-container {
  width: 90%;
  max-width: 600px;
  max-height: 85vh;
  background: var(--bg-card);
  border-radius: 20px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  border: 1px solid rgba(255, 255, 255, 0.8);
}

/* Header */
.cp-header {
  padding: 20px 24px;
  border-bottom: 1px solid var(--border);
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #f8fafc;
}

.header-title h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 800;
  color: var(--text-main);
}

.header-sub {
  font-size: 10px;
  color: var(--accent-blue);
  font-weight: 700;
  letter-spacing: 1px;
}

.close-btn {
  background: transparent;
  border: none;
  font-size: 20px;
  color: var(--text-sub);
  cursor: pointer;
  transition: transform 0.2s, color 0.2s;
  width: 32px; height: 32px;
  display: flex; align-items: center; justify-content: center;
  border-radius: 50%;
}

.close-btn:hover {
  color: #ef4444;
  background: #fee2e2;
  transform: rotate(90deg);
}

/* Body */
.cp-body {
  padding: 24px;
  overflow-y: auto;
  flex: 1;
}

.form-group {
  margin-bottom: 20px;
}

.input-label {
  display: block;
  font-size: 11px;
  font-weight: 800;
  color: var(--text-sub);
  margin-bottom: 8px;
  letter-spacing: 0.5px;
}

.cp-input {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid var(--border);
  border-radius: 12px;
  font-size: 14px;
  color: var(--text-main);
  background: #f8fafc;
  transition: all 0.3s ease;
  box-sizing: border-box;
  font-family: inherit;
}

.cp-input:focus {
  outline: none;
  background: #fff;
  border-color: var(--accent-blue);
  box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.1);
}

.title-input {
  font-weight: 700;
  font-size: 16px;
}

.content-area {
  min-height: 150px;
  resize: vertical;
  line-height: 1.6;
}

/* Media Grid */
.media-section {
  margin-top: 10px;
}
.media-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.img-preview-card {
  width: 80px;
  height: 80px;
  border-radius: 12px;
  overflow: hidden;
  position: relative;
  border: 1px solid var(--border);
}

.img-preview-card img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.remove-img-btn {
  position: absolute;
  top: 4px;
  right: 4px;
  background: rgba(0,0,0,0.6);
  color: #fff;
  border: none;
  border-radius: 4px;
  width: 20px;
  height: 20px;
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  font-size: 10px;
}
.remove-img-btn:hover { background: #ef4444; }

.upload-btn {
  width: 80px;
  height: 80px;
  border: 2px dashed var(--border);
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: var(--text-sub);
  cursor: pointer;
  transition: all 0.2s;
  background: #f8fafc;
}

.upload-btn:hover {
  border-color: var(--accent-blue);
  color: var(--accent-blue);
  background: #eff6ff;
}

.upload-btn i { font-size: 18px; margin-bottom: 4px; }
.upload-btn span { font-size: 9px; font-weight: 700; }

/* Footer */
.cp-footer {
  padding: 16px 24px;
  border-top: 1px solid var(--border);
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #f8fafc;
}

.status-indicator {
  font-size: 11px;
  font-weight: 600;
  color: var(--text-sub);
  display: flex;
  align-items: center;
  gap: 6px;
}

.dot {
  width: 6px; height: 6px;
  background: #10b981;
  border-radius: 50%;
}
.dot.processing { background: #f59e0b; animation: blink 1s infinite; }

.submit-btn {
  padding: 10px 24px;
  background: var(--text-main);
  color: #fff;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  font-size: 13px;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 8px;
}

.submit-btn:hover:not(:disabled) {
  background: var(--accent-blue);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
}

.submit-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Animations */
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.3s ease;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

.modal-fade-enter-active .cp-container,
.modal-fade-leave-active .cp-container {
  transition: transform 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.modal-fade-enter-from .cp-container {
  transform: scale(0.95) translateY(20px);
}
.modal-fade-leave-to .cp-container {
  transform: scale(0.95) translateY(20px);
}

@keyframes blink { 50% { opacity: 0.4; } }

/* 滚动条美化 */
.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }
</style>