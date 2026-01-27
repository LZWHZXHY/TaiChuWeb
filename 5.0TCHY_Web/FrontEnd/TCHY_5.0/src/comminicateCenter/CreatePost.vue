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
            
            <div v-if="!submitResult" class="form-view">
              <div class="form-group">
                <label class="input-label">// 帖子标题</label>
                <input 
                  v-model="formData.title" 
                  type="text" 
                  class="cp-input title-input" 
                  :placeholder="t('CreatePost.placeholder_title')" 
                  maxlength="50"
                />
              </div>

              <div class="form-group">
                <label class="input-label">// 帖子内容 (可选)</label>
                <textarea 
                  v-model="formData.content" 
                  class="cp-input content-area custom-scroll" 
                  :placeholder="t('CreatePost.placeholder_content')"
                ></textarea>
              </div>

              <div class="media-section">
                <label class="input-label">// 图片附加</label>
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

            <div v-else class="result-view">
              <div class="result-icon">✔ TRANSMISSION COMPLETE</div>
              <p class="result-sub">Data synchronized with Core System.</p>
              
              <div class="reward-ticket">
                <div class="ticket-header">:: TRANSACTION RECEIPT ::</div>
                
                <div class="ticket-row">
                  <span class="label">STATUS:</span>
                  <span class="value success">UPLOADED [200]</span>
                </div>

                <div v-if="submitResult.reward && (submitResult.reward.success || submitResult.reward.Success)" class="reward-content">
                  <div class="dashed-line">--------------------------------</div>
                  
                  <div class="ticket-row highlight">
                    <span class="label">REWARD:</span>
                    <span class="value">{{ submitResult.reward.message || submitResult.reward.Message }}</span>
                  </div>

                  <div v-if="submitResult.reward.leveledUp || submitResult.reward.LeveledUp" class="level-up-box">
                    <span class="blink">⚠️ LEVEL UP DETECTED</span>
                    <div class="new-level">CURRENT GRADE: LV.{{ submitResult.reward.currentLevel || submitResult.reward.CurrentLevel }}</div>
                  </div>
                </div>

                <div v-else class="ticket-row">
                  <span class="label">INFO:</span>
                  <span class="value">NO EXTRA REWARD</span>
                </div>
                
                <div class="dashed-line">--------------------------------</div>
                <div class="ticket-footer">END OF LINE_</div>
              </div>
            </div>

          </div>

          <footer class="cp-footer">
            <div class="status-indicator">
              <span :class="['dot', { 'processing': isSubmitting, 'done': submitResult }]"></span>
              <span class="status-text">
                {{ submitResult ? 'STANDBY' : (isSubmitting ? 'UPLOADING...' : 'SYSTEM_READY') }}
              </span>
            </div>
            
            <button 
              v-if="!submitResult"
              class="submit-btn" 
              @click="submitPost" 
              :disabled="isSubmitting || !formData.title.trim()"
            >
              <span v-if="isSubmitting">SENDING...</span>
              <span v-else>TRANSMIT_DATA</span>
            </button>

            <button 
              v-else 
              class="submit-btn confirm-btn" 
              @click="closeModal"
            >
              CONFIRM [OK]
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
const submitResult = ref(null); // 新增：用于存储成功后的返回结果

const formData = reactive({
  title: '',
  content: ''
});

// 关闭弹窗并重置状态
const closeModal = () => {
  if (isSubmitting.value) return;
  emit('update:modelValue', false);

  // 延迟重置，避免动画闪烁，保证下次打开是干净的表单
  setTimeout(() => {
    submitResult.value = null;
    formData.title = '';
    formData.content = '';
    fileList.value = [];
    previewImages.value = [];
  }, 300);
};

const handleFileSelect = (event) => {
  const files = Array.from(event.target.files);
  if (!files.length) return;

  files.forEach(file => {
    if (file.size > 10 * 1024 * 1024) { 
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

// 提交逻辑
const submitPost = async () => {
  if (!formData.title.trim()) return;

  isSubmitting.value = true;
  
  try {
    const submission = new FormData();
    submission.append('Title', formData.title.trim());
    submission.append('Content', formData.content.trim()); 
    submission.append('PostType', 0); 
    
    fileList.value.forEach(file => {
      submission.append('Images', file); 
    });

    const res = await apiClient.post('/ThePost/create', submission, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });

    if (res.data.success) {
      // ✅ 成功后，不再弹窗，而是设置 result，切换视图
      submitResult.value = res.data;
      
      // 可以在这里立即通知列表刷新，用户点击 Confirm 关闭弹窗时体验更流畅
      emit('success');
    }
  } catch (e) {
    console.error(e);
    alert('TRANSMISSION_ERROR // 信号中断'); // 出错保留 alert 作为降级处理
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
  --green: #009966;
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
  min-height: 300px; /* 保持高度稳定，防止切换视图时跳动 */
}

/* Form Styles */
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
  background: #ffffff;
  border: 2px solid var(--black);
  padding: 12px;
  font-family: var(--mono);
  font-size: 14px;
  outline: none;
  transition: all 0.2s;
  box-sizing: border-box;
}

.cp-input:focus {
  background: #ffffff;
  border-color: var(--red);
  box-shadow: 4px 4px 0 rgba(217, 35, 35, 0.1);
}

.title-input {
  font-weight: 800;
  font-size: 18px;
  text-transform: uppercase;
  color:#000
}

.content-area {
  min-height: 120px;
  resize: vertical;
  color:#000
}

/* Media Grid */
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

/* 🔥🔥🔥 Result View Styles (新增加的结算界面样式) 🔥🔥🔥 */
.result-view {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 20px 0;
  animation: slideUp 0.4s ease-out;
}

.result-icon {
  font-family: var(--heading);
  font-size: 2rem;
  color: var(--green);
  margin-bottom: 5px;
  letter-spacing: 1px;
}

.result-sub {
  font-family: var(--mono);
  font-size: 12px;
  color: #666;
  margin-bottom: 30px;
}

/* 结算小票样式 */
.reward-ticket {
  width: 100%;
  max-width: 400px;
  background: #E8E5DE;
  border: 2px dashed var(--black);
  padding: 20px;
  font-family: var(--mono);
  position: relative;
}

.ticket-header {
  text-align: center;
  font-weight: 700;
  margin-bottom: 15px;
  font-size: 14px;
  color: #333;
}

.ticket-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
  font-size: 14px;
}

.ticket-row.highlight {
  margin-top: 5px;
  font-size: 15px;
}

.label { color: #555; font-weight: 700; }
.value { font-weight: 400; color: #000; }
.value.success { color: var(--green); font-weight: 700; }

.dashed-line {
  color: #aaa;
  overflow: hidden;
  white-space: nowrap;
  margin: 10px 0;
  font-size: 12px;
}

/* 升级特效 */
.level-up-box {
  background: var(--red);
  color: white;
  padding: 10px;
  text-align: center;
  margin-top: 15px;
  border: 2px solid var(--black);
}

.blink {
  font-weight: 800;
  font-size: 14px;
  animation: blink 1s infinite;
  display: block;
}

.new-level {
  font-size: 12px;
  margin-top: 4px;
}

.ticket-footer {
  text-align: right;
  font-size: 10px;
  color: #999;
}


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
  background: #999;
  border-radius: 50%;
}

.dot.processing {
  background: var(--red);
  animation: blink 0.5s infinite;
}

.dot.done {
  background: var(--green);
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

/* 确认按钮颜色 */
.submit-btn.confirm-btn {
  background: var(--green); 
  color: white;
}
.submit-btn.confirm-btn:hover {
  background: #007744;
}

.submit-btn:disabled {
  background: #999;
  cursor: not-allowed;
}

@keyframes modalIn {
  from { opacity: 0; transform: translateY(30px); }
  to { opacity: 1; transform: translateY(0); }
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(15px); }
  to { opacity: 1; transform: translateY(0); }
}

@keyframes blink { 50% { opacity: 0.3; } }

.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); }
</style>