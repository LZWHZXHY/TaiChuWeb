<template>
  <Teleport to="body">
    <Transition name="modal-scale">
      <div v-if="modelValue" class="cp-overlay" @click.self="closeModal">
        <div class="cp-container">
          
          <header class="cp-header">
            <div class="header-title">
              <span class="deco-box">■</span>
              <div class="title-wrap">
                <h3>{{ t('CreatePost.create_title') }}</h3>
                <span class="header-sub">NEW_TRANSMISSION // DATA_ENTRY</span>
              </div>
            </div>
            <button class="close-btn" @click="closeModal">
              CLOSE [X]
            </button>
          </header>

          <div class="cp-body custom-scroll">
            <div class="form-group">
              <label class="input-label">// TITLE_FIELD</label>
              <input 
                v-model="formData.title" 
                type="text" 
                class="cp-input title-input" 
                :placeholder="t('CreatePost.placeholder_title')" 
                maxlength="50"
              />
            </div>

            <div class="form-group">
              <label class="input-label">// CONTENT_STREAM (OPTIONAL)</label>
              <textarea 
                v-model="formData.content" 
                class="cp-input content-area custom-scroll" 
                :placeholder="t('CreatePost.placeholder_content')"
              ></textarea>
            </div>

            <div class="media-section">
              <label class="input-label">// ATTACHED_MEDIA</label>
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
                  <span>UPLOAD</span>
                </label>
              </div>
            </div>
          </div>

          <footer class="cp-footer">
            <div class="status-indicator">
              <span :class="['dot', { 'processing': isSubmitting }]"></span>
              <span class="status-text">
                {{ isSubmitting ? 'UPLOADING_DATA...' : 'SYSTEM_READY' }}
              </span>
            </div>
            
            <button 
              class="submit-btn" 
              @click="submitPost" 
              :disabled="isSubmitting || !formData.title.trim()"
            >
              <span v-if="isSubmitting">SENDING...</span>
              <span v-else>TRANSMIT_DATA</span>
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

const { t } = useI18n({ useScope: 'global', missingWarn: false, fallbackWarn: false }) || { t: (key) => key };

const props = defineProps({
  modelValue: { type: Boolean, default: false }
});

const emit = defineEmits(['update:modelValue', 'success']);

const isSubmitting = ref(false);
const fileList = ref([]);
const previewImages = ref([]);

const formData = reactive({
  title: '',
  content: ''
});

const closeModal = () => {
  if (isSubmitting.value) return;
  emit('update:modelValue', false);
};

const handleFileSelect = (event) => {
  const files = Array.from(event.target.files);
  if (!files.length) return;

  files.forEach(file => {
    if (file.size > 10 * 1024 * 1024) { // 配合后端 10MB 限制
      alert(`文件过大: ${file.name}`);
      return;
    }
    fileList.value.push(file);
    previewImages.value.push({
      url: URL.createObjectURL(file),
      name: file.name
    });
  });
  event.target.value = '';
};

const removeImage = (index) => {
  fileList.value.splice(index, 1);
  URL.revokeObjectURL(previewImages.value[index].url);
  previewImages.value.splice(index, 1);
};

// 提交逻辑修复
const submitPost = async () => {
  // 修改：内容可以为空，只校验标题
  if (!formData.title.trim()) return;

  isSubmitting.value = true;
  
  try {
    const submission = new FormData();
    submission.append('Title', formData.title.trim());
    submission.append('Content', formData.content.trim()); // 传空字符串后端也支持
    submission.append('PostType', 0); // 分类固定为 0
    
    fileList.value.forEach(file => {
      submission.append('Images', file); // 匹配后端 CreatePostDto 字段名
    });

    // 路径根据你的控制器调整
    const res = await apiClient.post('/ThePost/create', submission, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });

    if (res.data.success) {
      formData.title = '';
      formData.content = '';
      fileList.value = [];
      previewImages.value = [];
      emit('success');
      closeModal();
    }
  } catch (e) {
    console.error(e);
    alert('TRANSMISSION_ERROR // 信号中断');
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&family=Inter:wght@400;800&display=swap');

.cp-overlay {
  --red: #D92323;
  --black: #111111;
  --off-white: #F4F1EA;
  --gray: #E0DDD5;
  --mono: 'JetBrains Mono', monospace;
  --heading: 'Anton', sans-serif;

  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.85);
  backdrop-filter: blur(4px);
  z-index: 9999;
  display: flex;
  justify-content: center;
  align-items: center;
}

.cp-container {
  width: 95%;
  max-width: 650px;
  background: var(--off-white);
  border: 4px solid var(--black);
  box-shadow: 10px 10px 0 rgba(0, 0, 0, 0.3);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  animation: modalIn 0.3s cubic-bezier(0.18, 0.89, 0.32, 1.28);
}

/* Header: 黑色粗犷风 */
.cp-header {
  padding: 15px 20px;
  background: var(--black);
  color: var(--off-white);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-title {
  display: flex;
  align-items: center;
  gap: 12px;
}

.deco-box {
  color: var(--red);
  font-size: 20px;
}

.title-wrap h3 {
  font-family: var(--heading);
  font-size: 1.5rem;
  margin: 0;
  letter-spacing: 1px;
}

.header-sub {
  font-family: var(--mono);
  font-size: 10px;
  color: #666;
  display: block;
}

.close-btn {
  background: transparent;
  border: 1px solid #444;
  color: #888;
  font-family: var(--mono);
  font-size: 12px;
  padding: 5px 10px;
  cursor: pointer;
}

.close-btn:hover {
  background: var(--red);
  color: white;
  border-color: var(--red);
}

/* Body: 工业灰白背景 */
.cp-body {
  padding: 25px;
  overflow-y: auto;
  max-height: 60vh;
}

.input-label {
  font-family: var(--mono);
  font-size: 12px;
  font-weight: 700;
  color: var(--black);
  margin-bottom: 8px;
  display: block;
}

.cp-input {
  width: 100%;
  background: #fff;
  border: 2px solid var(--black);
  padding: 12px;
  font-family: var(--mono);
  font-size: 14px;
  outline: none;
  transition: all 0.2s;
  box-sizing: border-box;
}

.cp-input:focus {
  background: #fff;
  border-color: var(--red);
  box-shadow: 4px 4px 0 rgba(217, 35, 35, 0.1);
}

.title-input {
  font-weight: 800;
  font-size: 18px;
  text-transform: uppercase;
}

.content-area {
  min-height: 120px;
  resize: vertical;
}

/* Media Grid: 工业缩略图 */
.media-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(80px, 1fr));
  gap: 10px;
}

.img-preview-card {
  aspect-ratio: 1;
  border: 1px solid var(--black);
  position: relative;
  background: #000;
}

.img-preview-card img {
  width: 100%; height: 100%; object-fit: cover;
}

.remove-img-btn {
  position: absolute;
  top: 0; right: 0;
  background: var(--red);
  color: white;
  border: none;
  padding: 4px;
  cursor: pointer;
}

.upload-btn {
  aspect-ratio: 1;
  border: 2px dashed #999;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  background: #eee;
  color: #666;
}

.upload-btn:hover {
  border-color: var(--red);
  color: var(--red);
}

.upload-btn i { font-size: 20px; }
.upload-btn span { font-family: var(--mono); font-size: 9px; margin-top: 4px; font-weight: 700; }

/* Footer: 状态与按钮 */
.cp-footer {
  padding: 15px 25px;
  border-top: 2px solid var(--black);
  background: var(--gray);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 10px;
  font-family: var(--mono);
}

.dot {
  width: 8px; height: 8px;
  background: #009966;
  border-radius: 50%;
}

.dot.processing {
  background: var(--red);
  animation: blink 0.8s infinite;
}

.status-text {
  font-size: 11px;
  color: #555;
  font-weight: 700;
}

.submit-btn {
  background: var(--black);
  color: var(--off-white);
  border: none;
  padding: 10px 25px;
  font-family: var(--heading);
  font-size: 1.1rem;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 4px 4px 0 rgba(0,0,0,0.2);
}

.submit-btn:hover:not(:disabled) {
  background: var(--red);
  transform: translate(-2px, -2px);
  box-shadow: 6px 6px 0 rgba(0,0,0,0.3);
}

.submit-btn:disabled {
  background: #999;
  cursor: not-allowed;
}

@keyframes modalIn {
  from { opacity: 0; transform: translateY(30px); }
  to { opacity: 1; transform: translateY(0); }
}

@keyframes blink { 50% { opacity: 0.3; } }

.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
</style>