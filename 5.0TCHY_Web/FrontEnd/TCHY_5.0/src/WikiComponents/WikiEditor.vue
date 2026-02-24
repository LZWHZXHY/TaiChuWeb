<template>
  <div class="wiki-editor">
    <div class="editor-header">
      <input type="text" v-model="localTitle" class="title-input" placeholder="输入文章标题..." />
      <div class="editor-actions">
        <button class="btn-cancel" @click="$emit('cancel')">取消</button>
        <button class="btn-save" @click="saveChanges">保存修改</button>
      </div>
    </div>
    
    <div class="editor-body">
      <textarea 
        v-model="localContent" 
        class="markdown-input"
        placeholder="在这里输入 Markdown 格式的内容..."
      ></textarea>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  article: Object
})

const emit = defineEmits(['save', 'cancel'])

// 拷贝一份本地数据用于编辑
const localTitle = ref(props.article.title)
const localContent = ref(props.article.rawMarkdown || props.article.contentHtml)

// 当父组件传来的文章变化时（极少出现，但以防万一），重置本地状态
watch(() => props.article, (newVal) => {
  localTitle.value = newVal.title
  localContent.value = newVal.rawMarkdown || newVal.contentHtml
})

const saveChanges = () => {
  // 把修改后的数据打包发给父组件
  emit('save', {
    title: localTitle.value,
    content: localContent.value
  })
}
</script>

<style scoped>
.wiki-editor {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 32px 40px;
  overflow-y: auto;
  background: #fafafa;
}

.editor-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.title-input {
  flex: 1;
  font-size: 2rem;
  font-weight: 700;
  border: none;
  border-bottom: 2px solid #e2e8f0;
  background: transparent;
  padding: 8px 0;
  margin-right: 24px;
  outline: none;
  transition: border-color 0.2s;
}
.title-input:focus { border-bottom-color: #2563eb; }

.editor-actions { display: flex; gap: 12px; }
.btn-cancel { padding: 8px 16px; border: 1px solid #cbd5e1; background: white; border-radius: 6px; cursor: pointer; color: #64748b; }
.btn-cancel:hover { background: #f8fafc; }
.btn-save { padding: 8px 16px; border: none; background: #2563eb; color: white; border-radius: 6px; cursor: pointer; font-weight: 500; }
.btn-save:hover { background: #1d4ed8; }

.editor-body { flex: 1; display: flex; flex-direction: column; }
.markdown-input {
  flex: 1;
  width: 100%;
  min-height: 500px;
  padding: 20px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-family: 'JetBrains Mono', 'Courier New', monospace;
  font-size: 15px;
  line-height: 1.6;
  resize: vertical;
  background: white;
  outline: none;
}
.markdown-input:focus { border-color: #93c5fd; box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1); }
</style>