<template>
  <form @submit.prevent="handleSubmit" class="cyber-form post-theme">
    <div class="form-decorative-bar"></div>

    <div class="form-group">
      <label><span class="label-decor"></span>数据代号 <span class="dim-text">// TITLE</span></label>
      <input type="text" v-model="formData.title" placeholder="输入帖子标题..." required />
    </div>

    <div class="form-group">
      <label><span class="label-decor"></span>核心数据 <span class="dim-text">// CONTENT</span></label>
      <div class="textarea-wrapper">
        <textarea 
          v-model="formData.content" 
          rows="5" 
          maxlength="500"
          placeholder="分享你的最新动态或想法..."
        ></textarea>
        <span class="char-counter" :class="{ limit: formData.content.length >= 500 }">
          {{ formData.content.length }} / 500
        </span>
      </div>
    </div>

    <div class="form-group">
      <label><span class="label-decor"></span>影像载入 <span class="dim-text">// UPLOAD IMAGES</span></label>
      <div class="file-drop-zone">
        <input type="file" @change="handleFileUpload" multiple accept="image/*" title="" />
        <div class="drop-text">
          <span class="icon">📸</span> 
          <span>点击或拖拽图像文件至此区域 <br><small class="limit-hint">(最多 5 张，单图限 5MB)</small></span>
        </div>
      </div>
      
      <div class="image-preview-matrix" v-if="previewUrls.length > 0">
        <div class="preview-node" v-for="(url, index) in previewUrls" :key="index">
          <img :src="url" alt="preview" />
          <div class="delete-btn" @click="removeFile(index)">✖</div>
        </div>
      </div>
    </div>

    <div class="form-group">
      <label><span class="label-decor"></span>特征标签 <span class="dim-text">// TAGS</span></label>
      <CyberTagInput v-model="formData.tags" :max-tags="5" />
    </div>

    <div class="form-actions">
      <button type="submit" class="cyber-submit-btn" :disabled="isSubmitting">
        <span class="btn-decor"></span>
        <span v-if="!isSubmitting">BROADCAST // 广播发射</span>
        <span v-else class="glitch-text">TRANSMITTING...</span>
      </button>
    </div>
  </form>
</template>

<script setup>
import { ref, reactive, onBeforeUnmount } from 'vue'
import apiClient from '@/utils/api' 
// 🚀 引入我们刚才写的组件
import CyberTagInput from '@/GeneralComponents/CyberTagInput.vue'

const isSubmitting = ref(false)
const selectedFiles = ref([])
const previewUrls = ref([])
const formData = reactive({ title: '', content: '', tags: '' })

// 处理文件上传并生成预览图
const handleFileUpload = (event) => {
  const files = Array.from(event.target.files)
  
  // 1. 限制最多 5 张图
  if (selectedFiles.value.length + files.length > 5) {
    alert('系统拦截：单个帖子最多只能承载 5 张影像数据')
    event.target.value = ''
    return
  }

  // 2. 限制单图最大 5MB
  const maxSize = 5 * 1024 * 1024; // 5MB in bytes
  for (let file of files) {
    if (file.size > maxSize) {
      alert(`协议拒绝：影像 [${file.name}] 体积过大，上限为 5MB`);
      event.target.value = ''
      return;
    }
  }

  // 校验通过，载入预览矩阵
  files.forEach(file => {
    selectedFiles.value.push(file)
    previewUrls.value.push(URL.createObjectURL(file))
  })
  
  event.target.value = ''
}

// 移除单张图片
const removeFile = (index) => {
  selectedFiles.value.splice(index, 1)
  URL.revokeObjectURL(previewUrls.value[index]) 
  previewUrls.value.splice(index, 1)
}

// 组件卸载时清理内存
onBeforeUnmount(() => {
  previewUrls.value.forEach(url => URL.revokeObjectURL(url))
})

const handleSubmit = async () => {
  if (!formData.title) return alert('必须提供标题')
  
  isSubmitting.value = true
  try {
    const payload = new FormData()
    payload.append('Title', formData.title)
    // 💡 CyberTagInput 已经帮我们把数据处理成干净的字符串了，直接用！
    if (formData.tags) payload.append('Tags', formData.tags)
    if (formData.content) payload.append('Content', formData.content)
    payload.append('PostType', 0)
    
    // 挂载所有图片文件
    selectedFiles.value.forEach(file => payload.append('Images', file))

    // 🚀 执行真实的 API 发布请求
    const response = await apiClient.post('/ThePost/create', payload, {
      headers: {
        'Content-Type': 'multipart/form-data' // 显式声明表单类型
      }
    })

    // 根据后端的返回结构判断是否成功
    if (response.data && response.data.success) {
      alert(`POST 广播发射成功！\n系统提示: ${response.data.message}`)
      
      // 重置表单
      formData.title = ''
      formData.content = ''
      formData.tags = '' // 清空字符串，标签组件会自动清空视觉块
      selectedFiles.value = []
      previewUrls.value.forEach(url => URL.revokeObjectURL(url))
      previewUrls.value = []
    } else {
      // 后端拦截返回的错误信息（比如标题为空、图片太大等）
      alert(`发射失败: ${response.data?.message || '未知错误'}`)
    }

  } catch (error) {
    // 网络异常或 HTTP 500 等情况
    alert(`网络传输崩溃: ${error.response?.data?.message || error.message}`)
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped>
/* POST 模块专属变量：赛博蓝 */
.post-theme {
  --theme-color: #00f3ff;
  --theme-color-rgb: 0, 243, 255;
  --theme-color-dark: #00b0b8;
  --bg-dark: #111;
  --border-dim: #ddd;
}

.cyber-form { 
  display: flex; flex-direction: column; gap: 24px; 
  padding-top: 10px; position: relative;
}

/* 顶部装饰条 */
.form-decorative-bar {
  position: absolute; top: -10px; left: 0; width: 60px; height: 4px;
  background: var(--theme-color);
  box-shadow: 0 0 10px rgba(var(--theme-color-rgb), 0.5);
}

.form-group { display: flex; flex-direction: column; gap: 10px; }

/* 高级感 Label */
.form-group label { 
  display: flex; align-items: center; gap: 8px;
  font-weight: 800; font-size: 0.85rem; color: #111; letter-spacing: 1px; 
}
.label-decor {
  display: inline-block; width: 6px; height: 6px; background: #111;
  transform: rotate(45deg);
}
.dim-text { color: #888; font-family: 'JetBrains Mono', monospace; font-size: 0.75rem; font-weight: normal; }

/* 硬核输入框设计 */
.cyber-form input[type="text"],
.cyber-form textarea {
  width: 100%; padding: 14px 16px; 
  border: 1px solid var(--border-dim); background: #fdfdfd;
  font-family: 'JetBrains Mono', monospace; font-size: 0.9rem; transition: all 0.3s ease;
  outline: none; resize: vertical; box-sizing: border-box;
  border-left: 4px solid var(--border-dim); /* 左侧边框加粗 */
}
.cyber-form input[type="text"]:hover,
.cyber-form textarea:hover {
  background: #f4f5f7; border-color: #ccc;
}
.cyber-form input[type="text"]:focus,
.cyber-form textarea:focus {
  border-color: var(--bg-dark); background: #fff; 
  border-left-color: var(--theme-color); /* 聚焦时左侧亮起赛博蓝 */
  box-shadow: 4px 4px 0 rgba(var(--theme-color-rgb), 0.2); 
}

/* 文本域字数统计 */
.textarea-wrapper { position: relative; }
.char-counter {
  position: absolute; bottom: 10px; right: 10px;
  font-size: 0.75rem; color: #aaa; font-family: 'JetBrains Mono', monospace;
  background: rgba(255,255,255,0.9); padding: 2px 6px; pointer-events: none;
  border-radius: 2px;
}
.char-counter.limit { color: #D92323; font-weight: bold; background: rgba(217, 35, 35, 0.1); }

/* 文件上传区域 (带有科技点阵背景) */
.file-drop-zone { 
  position: relative; border: 1px dashed #bbb; background-color: #f9f9f9; 
  /* CSS 绘制科技点阵背景 */
  background-image: radial-gradient(#e0e0e0 1px, transparent 1px);
  background-size: 15px 15px;
  padding: 35px 20px; text-align: center; cursor: pointer; transition: all 0.3s; 
}
.file-drop-zone:hover { 
  border-color: var(--theme-color); background-color: #f0fcff; 
  background-image: radial-gradient(rgba(var(--theme-color-rgb), 0.2) 1px, transparent 1px);
}
.file-drop-zone input[type="file"] { position: absolute; inset: 0; opacity: 0; cursor: pointer; width: 100%; }
.drop-text { 
  font-weight: bold; color: #555; font-size: 0.95rem; pointer-events: none; 
  display: flex; flex-direction: column; align-items: center; gap: 8px;
}
.drop-text .icon { font-size: 1.8rem; margin-bottom: 5px; opacity: 0.8; }
.limit-hint { font-size: 0.75rem; color: #999; font-family: 'JetBrains Mono', monospace; }

/* 缩略图矩阵 */
.image-preview-matrix {
  display: grid; grid-template-columns: repeat(auto-fill, minmax(90px, 1fr)); gap: 12px; margin-top: 15px;
}
.preview-node {
  position: relative; aspect-ratio: 1; background: #eee; overflow: hidden;
  /* 图像切角设计 */
  clip-path: polygon(0 0, 100% 0, 100% calc(100% - 10px), calc(100% - 10px) 100%, 0 100%);
  border-bottom: 2px solid var(--theme-color);
}
.preview-node img { width: 100%; height: 100%; object-fit: cover; transition: 0.3s; }
.preview-node:hover img { transform: scale(1.05); }

.delete-btn {
  position: absolute; top: 0; right: 0; background: rgba(17,17,17,0.85); color: #fff;
  width: 28px; height: 28px; display: flex; align-items: center; justify-content: center;
  font-size: 0.85rem; cursor: pointer; transition: 0.2s;
  border-bottom-left-radius: 4px; /* 稍微柔和一点的内角 */
}
.delete-btn:hover { background: #D92323; }

/* 提交按钮的高级感设计 */
.form-actions { margin-top: 20px; display: flex; justify-content: flex-end; }
.cyber-submit-btn {
  position: relative; background: var(--bg-dark); color: #fff; border: none; 
  padding: 16px 45px; font-family: 'Anton'; font-size: 1.2rem; 
  cursor: pointer; transition: 0.2s; letter-spacing: 2px;
  /* 经典的科技斜切角 */
  clip-path: polygon(0 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%);
}
/* 按钮内部装饰方块 */
.btn-decor {
  position: absolute; bottom: 5px; right: 5px; width: 6px; height: 6px;
  background: rgba(255,255,255,0.2);
}
.cyber-submit-btn:hover:not(:disabled) { 
  background: var(--theme-color); color: #000; 
  transform: translateY(-2px);
}
.cyber-submit-btn:hover:not(:disabled) .btn-decor { background: #000; }
.cyber-submit-btn:disabled { background: #555; color: #999; cursor: not-allowed; }

/* 简单的文字抖动动画，用于上传中 */
@keyframes glitch {
  0% { opacity: 1; transform: translateX(0); } 
  25% { opacity: 0.8; transform: translateX(-1px); } 
  50% { opacity: 0.9; transform: translateX(1px); } 
  100% { opacity: 1; transform: translateX(0); }
}
.glitch-text { display: inline-block; animation: glitch 0.3s infinite; }
</style>