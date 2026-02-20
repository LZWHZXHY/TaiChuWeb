<template>
  <section class="view-section">
    <div class="section-header">
      <h2>INITIATE_CREATION // 创造矩阵</h2>
      <p class="sub-text">选择要接入的数据类型以开始构建</p>
    </div>

    <div class="type-selector">
      <div 
        class="type-card" 
        :class="{ active: currentType === 'post' }"
        @click="currentType = 'post'"
      >
        <span class="icon">💬</span>
        <span class="title">动态帖子 // POST</span>
        <span class="desc">发布轻量级的短讯或交流讨论</span>
      </div>
      <div 
        class="type-card" 
        :class="{ active: currentType === 'art' }"
        @click="currentType = 'art'"
      >
        <span class="icon">🎨</span>
        <span class="title">视觉绘画 // ART</span>
        <span class="desc">上传单张或多张原创图像作品</span>
      </div>
      <div 
        class="type-card" 
        :class="{ active: currentType === 'blog' }"
        @click="currentType = 'blog'"
      >
        <span class="icon">📜</span>
        <span class="title">深度博客 // BLOG</span>
        <span class="desc">撰写富文本长篇文章与教程</span>
      </div>
    </div>

    <div class="form-workspace">
      <div class="workspace-header">
        <span class="indicator"></span>
        CURRENT_MODULE: {{ currentType.toUpperCase() }}_EDITOR
      </div>

      <form @submit.prevent="handleSubmit" class="cyber-form">
        <div class="form-group">
          <label>数据代号 // TITLE</label>
          <input type="text" v-model="formData.title" placeholder="输入标题..." required />
        </div>

        <div v-if="currentType === 'post' || currentType === 'blog'" class="form-group">
          <label>核心数据 // CONTENT</label>
          <textarea v-model="formData.content" rows="6" placeholder="在此输入内容正文..." :required="currentType === 'blog'"></textarea>
        </div>
        
        <div v-if="currentType === 'art'" class="form-group">
          <label>补充描述 // DESCRIPTION</label>
          <textarea v-model="formData.desc" rows="3" placeholder="作品背后的故事..."></textarea>
        </div>

        <div class="form-group">
          <label>{{ currentType === 'blog' ? '封面协议 // COVER IMAGE' : '资源载入 // UPLOAD FILES' }}</label>
          <div class="file-drop-zone">
            <input type="file" @change="handleFileUpload" :multiple="currentType !== 'blog'" accept="image/*" />
            <div class="drop-text">点击或拖拽文件至此区域</div>
          </div>
          <div class="file-preview" v-if="selectedFiles.length > 0">
            已选择 {{ selectedFiles.length }} 个文件
          </div>
        </div>

        <div class="form-group">
          <label>特征标签 // TAGS (用逗号分隔)</label>
          <input type="text" v-model="formData.tags" placeholder="例如: 赛博朋克, 教程, 吐槽" />
        </div>

        <div class="form-actions">
          <button type="submit" class="cyber-submit-btn" :disabled="isSubmitting">
            <span v-if="!isSubmitting">EXECUTE // 发布执行</span>
            <span v-else>TRANSMITTING...</span>
          </button>
        </div>
      </form>
    </div>
  </section>
</template>

<script setup>
import { ref, reactive } from 'vue'
import apiClient from '@/utils/api'

const currentType = ref('post')
const isSubmitting = ref(false)

// 统一的表单数据模型
const formData = reactive({
  title: '',
  content: '',
  desc: '',
  tags: '',
})
const selectedFiles = ref([])

const handleFileUpload = (event) => {
  selectedFiles.value = Array.from(event.target.files)
}

// 模拟提交逻辑，未来需要对接真实 API
const handleSubmit = async () => {
  if (!formData.title) {
    alert('必须提供标题')
    return
  }

  isSubmitting.value = true
  try {
    const payload = new FormData()
    payload.append('Title', formData.title)
    if (formData.tags) payload.append('Tags', formData.tags)

    let endpoint = ''

    // 根据类型组装不同的 Payload 和 Endpoint
    if (currentType.value === 'post') {
      endpoint = 'ThePost/create'
      payload.append('Content', formData.content)
      payload.append('PostType', 0) // 假设默认类型为0
      selectedFiles.value.forEach(file => payload.append('Images', file))

    } else if (currentType.value === 'art') {
      endpoint = 'Drawing/upload'
      payload.append('Desc', formData.desc)
      if (selectedFiles.value.length === 0) throw new Error("绘画作品必须包含图像")
      payload.append('Image', selectedFiles.value[0]) // 假设单图

    } else if (currentType.value === 'blog') {
      endpoint = 'Blog/articles' // 注：博客通常传 JSON，但如果带封面可能需要分步上传
      // 这里仅为演示流程
      payload.append('Content', formData.content)
      payload.append('Status', 1) 
    }

    // 执行 API 请求 (这里暂时注销，避免你测试时报错)
    /*
    const res = await apiClient.post(endpoint, payload, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    console.log(res.data)
    */
   
    // 模拟网络延迟
    await new Promise(r => setTimeout(r, 1000))

    alert(`${currentType.value.toUpperCase()} 序列发布成功！数据已写入矩阵。`)
    
    // 清空表单
    formData.title = ''
    formData.content = ''
    formData.desc = ''
    formData.tags = ''
    selectedFiles.value = []

  } catch (error) {
    alert(`传输失败: ${error.message}`)
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped>
.view-section { position: relative; z-index: 1; max-width: 900px; margin: 0 auto; }

.section-header { margin-bottom: 30px; border-bottom: 3px solid #111; padding-bottom: 10px; }
.section-header h2 { font-family: 'Anton'; font-size: 2rem; margin: 0; }
.sub-text { font-weight: bold; color: #666; margin-top: 5px; font-size: 0.9rem; }

/* 类型选择器卡片 */
.type-selector { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; margin-bottom: 30px; }
.type-card { background: #fff; border: 2px solid #111; padding: 20px; cursor: pointer; transition: 0.2s; display: flex; flex-direction: column; align-items: flex-start; }
.type-card:hover { transform: translateY(-3px); box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }
.type-card.active { background: #111; color: #fff; border-color: #111; box-shadow: 6px 6px 0 #D92323; transform: translateY(-3px); }

.type-card .icon { font-size: 2rem; margin-bottom: 10px; }
.type-card .title { font-family: 'Anton'; font-size: 1.2rem; margin-bottom: 5px; }
.type-card .desc { font-size: 0.75rem; color: #888; line-height: 1.4; font-weight: bold; }
.type-card.active .desc { color: #ccc; }

/* 表单工作区 */
.form-workspace { background: #fff; border: 2px solid #111; padding: 30px; position: relative; }
.workspace-header { font-family: 'Anton'; font-size: 0.9rem; color: #111; margin-bottom: 25px; display: flex; align-items: center; gap: 10px; border-bottom: 1px dashed #ccc; padding-bottom: 10px; }
.indicator { width: 10px; height: 10px; background: #2ecc71; border-radius: 50%; display: inline-block; box-shadow: 0 0 5px #2ecc71; }

.cyber-form { display: flex; flex-direction: column; gap: 20px; }
.form-group { display: flex; flex-direction: column; gap: 8px; }
.form-group label { font-weight: bold; font-size: 0.85rem; color: #333; }

/* 输入框硬核样式 */
.cyber-form input[type="text"],
.cyber-form textarea {
  width: 100%; padding: 12px; border: 2px solid #ccc; background: #f9f9f9;
  font-family: 'JetBrains Mono', monospace; font-size: 0.9rem; transition: 0.2s;
  outline: none; resize: vertical;
}
.cyber-form input[type="text"]:focus,
.cyber-form textarea:focus {
  border-color: #111; background: #fff; box-shadow: 4px 4px 0 rgba(217, 35, 35, 0.2);
}

/* 文件上传框 */
.file-drop-zone { position: relative; border: 2px dashed #ccc; background: #f9f9f9; padding: 30px; text-align: center; cursor: pointer; transition: 0.2s; }
.file-drop-zone:hover { border-color: #111; background: #eee; }
.file-drop-zone input[type="file"] { position: absolute; inset: 0; opacity: 0; cursor: pointer; width: 100%; }
.drop-text { font-weight: bold; color: #666; font-size: 0.9rem; pointer-events: none; }
.file-preview { margin-top: 10px; font-size: 0.8rem; font-weight: bold; color: #D92323; }

/* 提交按钮 */
.form-actions { margin-top: 10px; display: flex; justify-content: flex-end; }
.cyber-submit-btn {
  background: #111; color: #fff; border: none; padding: 15px 40px; font-family: 'Anton'; font-size: 1.2rem; cursor: pointer; transition: 0.2s; letter-spacing: 1px;
}
.cyber-submit-btn:hover:not(:disabled) { background: #D92323; box-shadow: 5px 5px 0 rgba(0,0,0,0.1); transform: translate(-2px, -2px); }
.cyber-submit-btn:disabled { background: #666; cursor: not-allowed; }
</style>