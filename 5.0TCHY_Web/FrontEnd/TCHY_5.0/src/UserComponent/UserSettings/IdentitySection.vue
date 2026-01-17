<template>
  <section class="form-section">
    <div class="section-header">
      <span class="sec-num">01</span>
      <span class="sec-title">身份识别 // IDENTITY</span>
    </div>

    <div class="monitor-wrapper">
      <input type="file" ref="fileInput" accept="image/*" style="display: none" @change="handleFileChange" />

      <div class="monitor-frame" @click="triggerUpload">
        <div class="monitor-screen">
          <img :src="avatar" alt="Avatar" class="screen-content" />
          <div class="screen-overlay">
            <div class="scan-line"></div>
            <div class="screen-glare"></div>
            <div class="click-hint">[ CLICK_TO_UPLOAD ]</div>
          </div>
        </div>
        <div class="monitor-base">
          <div class="led-light"></div>
          <span class="model-text">SYS-VISUAL-01</span>
        </div>
      </div>

      <div class="upload-controls">
        <div class="system-hint warning-text">
          <span class="icon">⚠</span>
          LIMIT: 5MB_MAX // 只支持高压缩比格式
        </div>
        <button class="cyber-btn primary full-width" @click="triggerUpload">
          [ ↑ ] UPLOAD_IMAGE_DATA
        </button>
      </div>
    </div>

    <div class="form-group" style="margin-top: 20px;">
      <label>昵称 // UID</label>
      <input type="text" v-model="localNickname" class="cyber-input bold" />
    </div>

    <div class="form-group">
      <label>性别 // GENDER</label>
      <input type="text" v-model="localGender" class="cyber-input bold" />
    </div>

    <transition name="fade">
      <div v-if="uploadModal.visible" class="upload-modal-overlay">
        <div class="upload-terminal-window">
          <div class="term-header">
            <span>>> UPLOAD_PROTOCOL</span>
            <span class="close-btn" @click="cancelUpload">X</span>
          </div>
          <div class="term-body">
            <div class="process-text">
              <p>> INITIATING HANDSHAKE...</p>
              <p>> ANALYZING BIOMETRIC DATA...</p>
              <p v-if="uploadModal.progress > 30" class="success">> FORMAT CHECK: PASSED</p>
              <p v-if="uploadModal.progress > 60">> COMPRESSING STREAM...</p>
              <p v-if="uploadModal.isUploading" class="success">> SYNCING WITH CLOUD STORAGE...</p>
            </div>

            <div class="preview-stage">
              <img :src="uploadModal.tempImg" class="temp-preview" />
              <div class="scan-grid"></div>
            </div>

            <div class="progress-bar-container">
              <div class="progress-fill" :style="{ width: uploadModal.progress + '%' }"></div>
            </div>
            <div class="progress-text">{{ uploadModal.progress }}% // {{ uploadModal.isUploading ? 'UPLOADING' : 'READY' }}</div>

            <button 
              v-if="uploadModal.progress === 100 && !uploadModal.isUploading" 
              class="sys-btn primary full" 
              @click="confirmUpload"
            >
              [ EXECUTE ] APPLY CHANGES
            </button>
            
            <button v-if="uploadModal.isUploading" class="sys-btn primary full" disabled style="opacity: 0.5;">
              TRANSMITTING...
            </button>
          </div>
        </div>
      </div>
    </transition>
  </section>
</template>

<script setup>
import { ref, reactive, watch } from 'vue'
import { useAuthStore } from '@/utils/auth' // 导入您的认证 Store
import apiClient from '@/utils/api'           // 导入 API 客户端

// Props
const props = defineProps({
  avatar: {
    type: String,
    required: true
  },
  nickname: {
    type: String,
    required: true
  },
  gender: {
    type: String,
    required: true
  }
})

// Emits
const emit = defineEmits(['update:avatar', 'update:nickname', 'update:gender'])

// 初始化 Store
const authStore = useAuthStore()

// 本地数据双向绑定 (Nickname)
const localNickname = ref(props.nickname)
watch(localNickname, (val) => {
  emit('update:nickname', val)
})

// 本地数据双向绑定 (Gender)
const localGender = ref(props.gender)
watch(localGender, (val) => {
  emit('update:gender', val)
})

// 上传状态管理
const fileInput = ref(null)
const uploadModal = reactive({
  visible: false,
  progress: 0,
  tempImg: null,
  file: null,
  isUploading: false 
})

// 触发文件选择框
const triggerUpload = () => {
  if (uploadModal.isUploading) return
  fileInput.value.click()
}

// 处理文件选择逻辑
const handleFileChange = (event) => {
  const file = event.target.files[0]
  if (!file) return

  if (file.size > 5 * 1024 * 1024) {
    alert('SYSTEM_ERROR: FILE_SIZE_EXCEEDED (MAX 5MB)')
    return
  }

  const reader = new FileReader()
  reader.onload = (e) => {
    uploadModal.tempImg = e.target.result 
    uploadModal.file = file              
    uploadModal.visible = true
    uploadModal.progress = 0
    uploadModal.isUploading = false
    simulateInitialProgress()
  }
  reader.readAsDataURL(file)
  event.target.value = ''
}

const simulateInitialProgress = () => {
  const interval = setInterval(() => {
    if (uploadModal.progress < 100) {
      uploadModal.progress += Math.floor(Math.random() * 20)
      if (uploadModal.progress > 100) uploadModal.progress = 100
    } else {
      clearInterval(interval)
    }
  }, 100)
}

/**
 * 核心：确认上传并实现持久化
 */
// IdentitySection.vue 中的 confirmUpload 函数
const confirmUpload = async () => {
  if (!uploadModal.file || uploadModal.isUploading) return

  const currentUserId = authStore.user?.id
  if (!currentUserId) {
    alert('CRITICAL_ERROR: 无法获取 UID，请重新登录')
    return
  }

  uploadModal.isUploading = true
  uploadModal.progress = 0 

  try {
    const formData = new FormData()
    formData.append('file', uploadModal.file)
    formData.append('userId', currentUserId)

    const response = await apiClient.post('/Profile/upload-avatar', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (progressEvent) => {
        uploadModal.progress = Math.round((progressEvent.loaded * 100) / progressEvent.total)
      }
    })

    if (response.data && response.data.success) {
      const newAvatarUrl = response.data.url;

      // --- 👇 核心修改：确保切换页面不还原 👇 ---
      
      // 1. 修改父组件当前显示的变量
      emit('update:avatar', newAvatarUrl)
      
      // 2. 修改全局 Store 里的数据
      if (authStore.user) {
        authStore.user.logo = newAvatarUrl;
        
        // 3. 修改本地持久化缓存（这是刷新页面也不还原的关键）
        localStorage.setItem('user', JSON.stringify(authStore.user));
      }
      
      // --- 👆 修改结束 👆 ---

      setTimeout(() => {
        uploadModal.visible = false
        uploadModal.isUploading = false
      }, 600)
    }
  } catch (error) {
    console.error('上传失败:', error)
    uploadModal.isUploading = false
  }
}

const cancelUpload = () => {
  if (uploadModal.isUploading) return 
  uploadModal.visible = false
  uploadModal.tempImg = null
  uploadModal.file = null
}
</script>

<style scoped>
/* 保持所有 CSS 样式不动 */
.monitor-wrapper {
  display: flex;
  gap: 25px;
  align-items: center;
}

.monitor-frame {
  width: 140px;
  background: #2a2a2a;
  padding: 2px 2px 16px 2px;
  border-radius: 0px 0px 0px 0px;
  box-shadow: 
    0 0 0 2px #111,
    4px 4px 10px rgba(0,0,0,0.3);
  cursor: pointer;
  transition: transform 0.2s;
}

.monitor-frame:hover {
  transform: scale(1.02);
}

.monitor-screen {
  background: #000;
  width: 100%;
  aspect-ratio: 1;
  border-radius: 4px;
  position: relative;
  overflow: hidden;
  border: 2px solid #000;
  box-shadow: inset 0 0 10px rgba(255,255,255,0.1);
}

.screen-content {
  width: 100%;
  height: 100%;
  object-fit: cover;
  filter: contrast(1.1) brightness(0.9);
}

.screen-overlay {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  background: linear-gradient(rgba(18, 16, 16, 0) 50%, rgba(0, 0, 0, 0.25) 50%), linear-gradient(90deg, rgba(255, 0, 0, 0.06), rgba(0, 255, 0, 0.02), rgba(0, 0, 255, 0.06));
  background-size: 100% 2px, 3px 100%;
  pointer-events: none;
  display: flex;
  align-items: center;
  justify-content: center;
}

.click-hint {
  color: #00ff41;
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.6rem;
  opacity: 0;
  background: rgba(0,0,0,0.8);
  padding: 2px 4px;
  transition: opacity 0.2s;
}
.monitor-frame:hover .click-hint { opacity: 1; }

.monitor-base {
  height: 0;
  position: relative;
}
.led-light {
  width: 4px; height: 4px; background: #0f0;
  position: absolute; bottom: -8px; right: 5px;
  border-radius: 50%;
  box-shadow: 0 0 4px #0f0;
}
.model-text {
  position: absolute; bottom: -10px; left: 2px;
  font-size: 0.5rem; color: #666; font-family: 'JetBrains Mono', monospace;
}

.upload-controls {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.system-hint {
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.7rem;
  color: #666;
  border-left: 2px solid #ff9900;
  padding-left: 8px;
  line-height: 1.4;
}
.cyber-btn.primary {
  background: #111111;
  color: #D92323;
  border: 1vh black;
  padding: 10px;
  font-weight: bold;
  font-family: 'JetBrains Mono', monospace;
}
.cyber-btn.primary:hover {
  background: #000000;
  color: #ffffff;
}

.upload-modal-overlay {
  position: fixed;
  top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(0,0,0,0.85);
  backdrop-filter: blur(4px);
  z-index: 100;
  display: flex;
  align-items: center;
  justify-content: center;
}

.upload-terminal-window {
  width: 500px;
  max-width: 90%;
  background: #0d0d0d;
  border: 1px solid #444;
  box-shadow: 0 20px 50px rgba(0,0,0,0.8);
  font-family: 'JetBrains Mono', monospace;
  color: #ddd;
}

.term-header {
  background: #333;
  padding: 8px 12px;
  font-size: 0.8rem;
  display: flex; justify-content: space-between;
  border-bottom: 1px solid #555;
  color: #fff;
}
.close-btn { cursor: pointer; }
.close-btn:hover { color: #D92323; }

.term-body { padding: 20px; display: flex; flex-direction: column; gap: 15px; }

.process-text {
  min-height: 80px;
  font-size: 0.75rem;
  color: #888;
}
.process-text p { margin: 4px 0; }
.process-text .success { color: #00ff41; }

.preview-stage {
  height: 150px;
  border: 1px dashed #444;
  background: #111;
  position: relative;
  display: flex; align-items: center; justify-content: center;
  overflow: hidden;
}
.temp-preview { height: 100%; opacity: 0.8; }
.scan-grid {
  position: absolute; top:0; left:0; width:100%; height:100%;
  background: 
    linear-gradient(90deg, rgba(0,255,0,0.1) 1px, transparent 1px),
    linear-gradient(rgba(0,255,0,0.1) 1px, transparent 1px);
  background-size: 20px 20px;
}

.progress-bar-container {
  height: 6px;
  background: #222;
  border: 1px solid #444;
  width: 100%;
}
.progress-fill {
  height: 100%;
  background: #00ff41;
  transition: width 0.2s;
  box-shadow: 0 0 10px #00ff41;
}
.progress-text { text-align: right; font-size: 0.7rem; color: #00ff41; }

.sys-btn.full { width: 100%; padding: 12px; margin-top: 10px; }

.form-section { margin-bottom: 50px; }
.section-header { display: flex; align-items: baseline; border-bottom: 2px solid #111; padding-bottom: 10px; margin-bottom: 20px; }
.sec-num { font-size: 2.5rem; font-weight: bold; color: #ccc; margin-right: 15px; line-height: 0.8; font-family: 'Noto Sans SC', sans-serif; }
.sec-title { font-size: 1.1rem; font-weight: bold; font-family: 'JetBrains Mono', monospace; }
.form-group { margin-bottom: 15px; display: flex; flex-direction: column; padding-top: 10px; }
.form-group label { font-size: 0.7rem; color: #666; margin-bottom: 5px; font-weight: bold; font-family: 'JetBrains Mono', monospace; }
.cyber-input, .cyber-textarea { border: 1px solid #999; background: #fff; padding: 10px; font-family: 'Noto Sans SC', sans-serif; font-size: 0.9rem; outline: none; transition: 0.2s; width: 100%; display: block; }
.cyber-input:focus, .cyber-textarea:focus { border-color: #D92323; box-shadow: 2px 2px 0 rgba(0,0,0,0.1); }

.fade-enter-active, .fade-leave-active { transition: opacity 0.3s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>