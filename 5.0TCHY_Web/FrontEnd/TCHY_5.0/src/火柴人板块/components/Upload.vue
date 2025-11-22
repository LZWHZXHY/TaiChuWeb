<template>
  <section class="card" aria-labelledby="upload-title">
    <header class="card-head">
      <h3 id="upload-title">2. 人设上传</h3>
    </header>

    <form class="form" @submit.prevent="submitOC">
      <div class="grid two">
        <div class="field">
          <label for="role-name">角色名称<span class="req">*</span></label>
          <input 
            id="role-name" 
            v-model="form.OCName" 
            type="text" 
            class="input" 
            placeholder="例如：炎刃·赤霄" 
            required 
          />
        </div>
        <div class="field">
          <label for="author-name">作者名称<span class="req">*</span></label>
          <input 
            id="author-name" 
            v-model="form.authorName" 
            type="text" 
            class="input" 
            placeholder="你的名字" 
            required 
          />
        </div>
      </div>

      <div class="grid two">
        <div class="field">
          <label for="role-gender">性别<span class="req">*</span></label>
          <select id="role-gender" v-model="form.gender" class="select" required>
            <option value="">请选择性别</option>
            <option value="0">男</option>
            <option value="1">女</option>
            <option value="2">未知</option>
          </select>
        </div>
        <div class="field">
          <label for="role-age">年龄<span class="req">*</span></label>
          <input 
            id="role-age" 
            v-model.number="form.age" 
            type="number" 
            class="input" 
            min="1" 
            max="999" 
            placeholder="1-999" 
            required 
          />
        </div>
      </div>

      <div class="grid two">
        <div class="field">
          <label for="role-species">种族<span class="req">*</span></label>
          <input 
            id="role-species" 
            v-model="form.species" 
            type="text" 
            class="input" 
            placeholder="例如：人类、精灵、机械" 
            required 
          />
        </div>
        <div class="field">
          <label for="role-type">角色类型</label>
          <select id="role-type" v-model="form.roleType" class="select">
            <option value="">未选择</option>
            <option value="近战">近战</option>
            <option value="远程">远程</option>
            <option value="法术">法术</option>
            <option value="辅助">辅助</option>
            <option value="多面手">多面手</option>
          </select>
        </div>
      </div>

      <div class="field">
        <label for="role-ability">能力与设定<span class="req">*</span></label>
        <textarea 
          id="role-ability" 
          v-model="form.ability" 
          class="textarea" 
          rows="6" 
          placeholder="描述角色的能力、限制、招式、战术偏好等…" 
          maxlength="4000"
          required 
        />
        <div class="char-count">{{ form.ability.length }}/4000</div>
      </div>

      <div class="field">
        <label for="role-background">背景故事</label>
        <textarea 
          id="role-background" 
          v-model="form.background" 
          class="textarea" 
          rows="4" 
          placeholder="角色的背景故事、经历、性格等（可选）" 
          maxlength="4000"
        />
        <div class="char-count">{{ form.background?.length || 0 }}/4000</div>
      </div>

      <div class="grid two">
        <div class="field">
          <label for="role-poo">POO<span class="req">*</span></label>
          <input 
            id="role-poo" 
            v-model="form.poo" 
            type="text" 
            class="input" 
            placeholder="POO信息（必填）" 
            required 
          />
        </div>
        <div class="field">
          <label for="role-tier">强度/定位</label>
          <select id="role-tier" v-model="form.tier" class="select">
            <option value="">未选择</option>
            <option value="入门">入门</option>
            <option value="标准">标准</option>
            <option value="高强度">高强度</option>
            <option value="超规格">超规格</option>
          </select>
        </div>
      </div>

      <!-- 立绘上传区域 -->
      <div class="field">
        <label for="character-image">立绘/头像<span class="req">*</span></label>
        <div 
          class="uploader" 
          :class="{ 
            'uploader-dragover': isDragging, 
            'uploader-has-file': characterImage 
          }"
          @dragover.prevent="handleDragOver"
          @dragleave.prevent="handleDragLeave"
          @drop.prevent="handleDrop"
          @click="triggerFileInput"
        >
          <div class="uploader-inner">
            <div class="uploader-icon">🖼️</div>
            <div class="uploader-text">
              <template v-if="characterImage">
                <strong>{{ characterImage.name }}</strong>
                <span class="muted">大小: {{ formatFileSize(characterImage.size) }}</span>
              </template>
              <template v-else>
                <strong>点击选择图片或拖拽到此区域</strong>
                <span class="muted">支持 PNG/JPG/GIF/WebP，最大 5MB</span>
              </template>
            </div>
            <button v-if="characterImage" type="button" class="btn-remove" @click.stop="removeImage">×</button>
          </div>
        </div>
        <input 
          id="character-image"
          ref="fileInput"
          type="file" 
          accept=".jpg,.jpeg,.png,.gif,.webp"
          @change="handleFileSelect"
          style="display: none"
        />
        <div v-if="uploadProgress > 0" class="upload-progress">
          <div class="progress-bar">
            <div class="progress-fill" :style="{ width: uploadProgress + '%' }"></div>
          </div>
          <span class="progress-text">{{ uploadProgress }}%</span>
        </div>
      </div>

      <div class="field check">
        <label class="checkbox">
          <input 
            type="checkbox" 
            v-model="form.agreedToRules" 
            required 
          />
          <span>我已阅读并遵守社区与约战规则</span>
        </label>
      </div>

      <div class="form-actions">
        <button 
          class="btn primary" 
          type="submit" 
          :disabled="!canSubmit || isSubmitting"
        >
          <span v-if="isSubmitting">上传中...</span>
          <span v-else>提交人设</span>
        </button>
        <button 
          class="btn ghost" 
          type="button" 
          @click="resetForm"
          :disabled="isSubmitting"
        >
          清空
        </button>
      </div>

      <!-- 上传结果提示 -->
      <div v-if="uploadResult" class="result-message" :class="uploadResult.success ? 'success' : 'error'">
        <h4>{{ uploadResult.success ? '✅ 上传成功' : '❌ 上传失败' }}</h4>
        <p>{{ uploadResult.message }}</p>
        
        <!-- 显示详细错误信息 -->
        <div v-if="!uploadResult.success" class="error-details">
          <details>
            <summary>查看详细错误信息</summary>
            <div class="error-content">
              <p v-if="uploadResult.statusCode"><strong>状态码:</strong> {{ uploadResult.statusCode }}</p>
              <pre class="error-json">{{ JSON.stringify(uploadResult.details, null, 2) }}</pre>
            </div>
          </details>
        </div>
        
        <div v-if="uploadResult.data" class="result-details">
          <p><strong>角色ID:</strong> {{ uploadResult.data.ocId }}</p>
          <p><strong>角色名称:</strong> {{ uploadResult.data.ocName }}</p>
          <p><strong>POO:</strong> {{ uploadResult.data.poo }}</p>
          <img 
            v-if="uploadResult.data.imageInfo" 
            :src="uploadResult.data.imageInfo.fullUrl" 
            :alt="uploadResult.data.ocName"
            class="preview-image"
          />
        </div>
      </div>
    </form>
  </section>
</template>

<script setup>
import { ref, computed, reactive, onMounted } from 'vue'
import apiClient, { getAuthToken, clearInvalidToken } from '../../utils/api'

// 表单数据
const form = reactive({
  OCName: '',
  authorName: '',
  gender: '',
  age: '',
  species: '',
  ability: '',
  background: '',
  poo: '', // POO现在是必填
  roleType: '',
  tier: '',
  agreedToRules: false
})

// 响应式数据
const fileInput = ref(null)
const characterImage = ref(null)
const isDragging = ref(false)
const isSubmitting = ref(false)
const uploadProgress = ref(0)
const uploadResult = ref(null)

// 计算属性
const canSubmit = computed(() => {
  return form.OCName && 
         form.authorName && 
         form.gender !== '' && 
         form.age > 0 && 
         form.species && 
         form.ability && 
         form.poo && // POO现在是必填
         characterImage.value && 
         form.agreedToRules
})

// 表单提交函数 - 修复版本
const submitOC = async () => {
  if (!canSubmit.value || isSubmitting.value) return

  isSubmitting.value = true
  uploadProgress.value = 0
  uploadResult.value = null

  try {
    console.log('=== 开始表单验证 ===')
    
    // 1. 验证必填字段
    const validations = [
      { field: '角色名称', value: form.OCName?.trim(), required: true },
      { field: '作者名称', value: form.authorName?.trim(), required: true },
      { field: '性别', value: form.gender, required: true },
      { field: '年龄', value: form.age, required: true, min: 1, max: 999 },
      { field: '种族', value: form.species?.trim(), required: true },
      { field: '能力描述', value: form.ability?.trim(), required: true },
      { field: 'POO', value: form.poo?.trim(), required: true }, // POO必填
      { field: '立绘图片', value: characterImage.value, required: true },
      { field: '用户协议', value: form.agreedToRules, required: true }
    ]

    for (const validation of validations) {
      if (validation.required && !validation.value) {
        throw new Error(`${validation.field}是必须的`)
      }
      if (validation.min && validation.value < validation.min) {
        throw new Error(`${validation.field}不能小于${validation.min}`)
      }
      if (validation.max && validation.value > validation.max) {
        throw new Error(`${validation.field}不能大于${validation.max}`)
      }
    }

    console.log('✅ 前端验证通过')

    // 2. 准备表单数据
    const formDataToSend = new FormData()
    
    // 生成数字类型的时间戳（秒级）
    const currentTime = Math.floor(Date.now() / 1000)
    console.log('⏰ 生成的时间戳:', currentTime, '类型:', typeof currentTime)
    
    // 添加文本字段
    const fields = {
        OCName: form.OCName.trim(),
        authorName: form.authorName.trim(),
        gender: form.gender.toString(),
        age: form.age.toString(),
        species: form.species.trim(),
        ability: form.ability.trim(),
        Background: form.background?.trim() || '',
        POO: form.poo.trim(), // POO是必填
        currentTime: currentTime.toString() // 发送数字时间戳
    }
    
    console.log('📋 准备发送的字段:')
    Object.entries(fields).forEach(([key, value]) => {
        formDataToSend.append(key, value)
        console.log(`  ${key}:`, value, '类型:', typeof value)
    })
    
    // 添加立绘图片文件
    if (characterImage.value) {
        formDataToSend.append('CharacterImage', characterImage.value)
        console.log('🖼️ 图片文件:', {
            name: characterImage.value.name,
            size: characterImage.value.size,
            type: characterImage.value.type
        })
    }

    // 3. 检查认证
    const token = getAuthToken()
    console.log('🔐 认证令牌:', token ? '已找到' : '未找到')
    
    if (!token) {
        throw new Error('未找到认证令牌，请先登录')
    }

    // 4. 发送请求
    console.log('🚀 开始上传OC数据到 /OCBattleField/upload...')
    
    const response = await apiClient.post('/OCBattleField/upload', formDataToSend, {
        headers: {
            'Content-Type': 'multipart/form-data'
        },
        timeout: 30000,
        onUploadProgress: (progressEvent) => {
            if (progressEvent.total) {
                uploadProgress.value = Math.round(
                    (progressEvent.loaded * 100) / progressEvent.total
                )
            }
        }
    })

    console.log('✅ 上传成功:', response.data)
    
    // 5. 处理成功响应
    uploadResult.value = {
        success: true,
        message: response.data.message || '上传成功',
        data: response.data.data || response.data
    }

  } catch (error) {
    console.error('❌ 上传失败详情:')
    console.error('错误消息:', error.message)
    console.error('HTTP状态码:', error.response?.status)
    console.error('响应数据:', error.response?.data)
    console.error('请求配置:', {
        url: error.config?.url,
        method: error.config?.method,
        data: error.config?.data
    })
    
    let errorMessage = '上传失败，请稍后重试'
    let errorDetails = null
    
    // 处理认证错误
    if (error.response?.status === 401) {
        clearInvalidToken()
        errorMessage = '登录已过期，请重新登录'
    }
    
    // 处理400错误 - 显示后端具体验证错误
    else if (error.response?.status === 400) {
        const responseData = error.response?.data
        
        if (responseData) {
            // 尝试提取后端返回的具体错误信息
            if (responseData.message) {
                errorMessage = responseData.message
            }
            
            if (responseData.errors && Array.isArray(responseData.errors)) {
                errorMessage = responseData.errors[0] || errorMessage
            } else if (typeof responseData === 'string') {
                errorMessage = responseData
            }
            
            // 保存详细错误信息用于显示
            errorDetails = responseData
        }
        
        // 如果没有具体错误信息，使用通用提示
        if (errorMessage === '上传失败，请稍后重试') {
            errorMessage = '数据验证失败，请检查表单填写是否正确'
        }
    }
    
    // 处理其他HTTP错误
    else if (error.response?.status === 404) {
        errorMessage = '接口不存在，请检查API路径配置'
    } else if (error.response?.status === 500) {
        errorMessage = '服务器内部错误，请稍后重试'
    }
    
    // 处理网络错误
    else if (error.code === 'ECONNABORTED' || error.message.includes('timeout')) {
        errorMessage = '请求超时，请检查网络连接'
    } else if (error.message.includes('Network Error')) {
        errorMessage = '网络连接失败，请检查API服务是否运行'
    }
    
    uploadResult.value = {
        success: false,
        message: errorMessage,
        details: errorDetails || error.response?.data || error.message,
        statusCode: error.response?.status
    }
    
  } finally {
    isSubmitting.value = false
    uploadProgress.value = 0
  }
}

// 测试API连接
const testApiConnection = async () => {
  try {
    console.log('🔍 测试API连接...')
    const response = await apiClient.get('/health', {
      validateStatus: (status) => status < 500 // 允许400错误，只测试连接
    })
    console.log('📡 API连接状态:', response.status)
    return true
  } catch (error) {
    console.error('❌ API连接测试失败:', error.message)
    return false
  }
}

// 页面加载时检查
onMounted(async () => {
  console.log('🔄 页面加载完成，检查API状态...')
  const isConnected = await testApiConnection()
  if (!isConnected) {
    console.warn('⚠️ API服务可能未启动，请检查后端服务')
  }
})

// 其他辅助函数
const triggerFileInput = () => {
  fileInput.value?.click()
}

const handleFileSelect = (event) => {
  const file = event.target.files[0]
  if (file) {
    validateAndSetImage(file)
  }
}

const handleDragOver = () => {
  isDragging.value = true
}

const handleDragLeave = () => {
  isDragging.value = false
}

const handleDrop = (event) => {
  isDragging.value = false
  const file = event.dataTransfer.files[0]
  if (file) {
    validateAndSetImage(file)
  }
}

const validateAndSetImage = (file) => {
  const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp']
  if (!allowedTypes.includes(file.type)) {
    alert('不支持的文件格式，请选择 JPG、PNG、GIF 或 WebP 格式的图片')
    return
  }

  const maxSize = 5 * 1024 * 1024
  if (file.size > maxSize) {
    alert('文件大小超过 5MB 限制')
    return
  }

  characterImage.value = file
}

const removeImage = () => {
  characterImage.value = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}

const formatFileSize = (bytes) => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const resetForm = () => {
  Object.keys(form).forEach(key => {
    if (key !== 'agreedToRules') {
      form[key] = ''
    } else {
      form[key] = false
    }
  })
  characterImage.value = null
  uploadResult.value = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}
</script>

<style scoped>
.card { 
  background: #fff; 
  border: 1px solid #e6ebf3; 
  border-radius: 16px; 
  box-shadow: 0 2px 10px rgba(2,6,23,.06); 
  padding: 20px; 
  margin-bottom: 20px;
}

.card-head { 
  border-bottom: 1px solid #e6ebf3; 
  padding-bottom: 12px; 
  margin-bottom: 20px; 
}

.card-head h3 { 
  margin: 0; 
  font-size: 20px; 
  font-weight: 700; 
  color: #1e293b;
}

.form { display: grid; gap: 16px; }

.grid.two { 
  display: grid; 
  grid-template-columns: 1fr 1fr; 
  gap: 16px; 
}

@media (max-width: 640px) { 
  .grid.two { grid-template-columns: 1fr; } 
}

.field { display: grid; gap: 8px; }

label { 
  font-weight: 600; 
  color: #374151; 
  font-size: 14px;
}

.req { color: #ef4444; margin-left: 2px; }

.input, .select, .textarea {
  background: #fff; 
  border: 1px solid #e6ebf3; 
  border-radius: 8px;
  padding: 12px; 
  font-size: 14px;
  outline: none; 
  transition: border-color 0.2s, box-shadow 0.2s;
  width: 100%;
  box-sizing: border-box;
}

.input:focus, .select:focus, .textarea:focus { 
  border-color: #2563eb; 
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1); 
}

.char-count {
  font-size: 12px;
  color: #5a6478;
  text-align: right;
}

.uploader {
  border: 2px dashed #d6deea; 
  border-radius: 12px; 
  background: #f8fafc;
  padding: 20px; 
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
}

.uploader:hover {
  border-color: #2563eb;
  background: #f1f5f9;
}

.uploader-dragover {
  border-color: #2563eb;
  background: #e0f2fe;
  transform: scale(1.02);
}

.uploader-has-file {
  border-style: solid;
  border-color: #10b981;
  background: #f0fdf4;
}

.uploader-inner { 
  display: flex; 
  align-items: center; 
  gap: 12px;
  justify-content: space-between;
}

.uploader-icon { font-size: 24px; }

.uploader-text { flex: 1; }

.uploader-text strong { 
  display: block; 
  margin-bottom: 4px;
  color: #1e293b;
}

.uploader-text .muted { 
  font-size: 12px; 
  color: #5a6478; 
}

.btn-remove {
  background: #ef4444;
  color: white;
  border: none;
  border-radius: 50%;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  font-size: 16px;
  line-height: 1;
}

.upload-progress {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 8px;
}

.progress-bar {
  flex: 1;
  height: 6px;
  background: #e6ebf3;
  border-radius: 3px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: #2563eb;
  transition: width 0.3s ease;
}

.progress-text {
  font-size: 12px;
  color: #5a6478;
  min-width: 40px;
}

.field.check { margin-top: 8px; }

.checkbox { 
  display: flex; 
  align-items: center; 
  gap: 8px; 
  cursor: pointer;
  font-size: 14px;
}

.form-actions { 
  display: flex; 
  gap: 12px; 
  align-items: center;
  margin-top: 20px;
}

.btn { 
  appearance: none; 
  border: 1px solid #e6ebf3; 
  border-radius: 8px; 
  padding: 12px 20px; 
  font-weight: 600; 
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s ease;
  min-width: 100px;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn.primary { 
  background: #2563eb; 
  color: #fff; 
  border-color: transparent; 
}

.btn.primary:hover:not(:disabled) {
  background: #1d4ed8;
  transform: translateY(-1px);
}

.btn.ghost { 
  background: #fff; 
  color: #64748b;
}

.btn.ghost:hover:not(:disabled) {
  background: #f8fafc;
  color: #374151;
}

.result-message {
  padding: 16px;
  border-radius: 8px;
  margin-top: 16px;
}

.result-message.success {
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
  color: #166534;
}

.result-message.error {
  background: #fef2f2;
  border: 1px solid #fecaca;
  color: #dc2626;
}

.result-message h4 {
  margin: 0 0 8px 0;
  font-size: 16px;
  font-weight: 600;
}

.error-details {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid rgba(220, 38, 38, 0.2);
}

.error-details summary {
  cursor: pointer;
  font-weight: 600;
  color: #dc2626;
  margin-bottom: 8px;
}

.error-content {
  background: rgba(220, 38, 38, 0.05);
  border-radius: 4px;
  padding: 12px;
  margin-top: 8px;
}

.error-json {
  font-family: 'Courier New', monospace;
  font-size: 12px;
  background: white;
  padding: 8px;
  border-radius: 4px;
  overflow-x: auto;
  max-height: 200px;
  overflow-y: auto;
  margin: 0;
}

.result-details {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid rgba(0,0,0,0.1);
}

.preview-image {
  max-width: 200px;
  max-height: 200px;
  border-radius: 8px;
  margin-top: 8px;
}
</style>