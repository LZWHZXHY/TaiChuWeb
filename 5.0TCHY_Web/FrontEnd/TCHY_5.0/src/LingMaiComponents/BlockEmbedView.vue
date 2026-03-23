<template>
 <node-view-wrapper 
  class="lingmai-block-embed-container" 
  contenteditable="false"
  @wheel.stop.passive  @mousedown.stop      >
    <div class="embed-header">
      <div class="embed-type-badge">
        <span class="icon">🔄</span> 联动块
      </div>
      <div v-if="blockInfo" class="embed-source">
        来源: <span class="source-link" @click="jumpToSource">{{ blockInfo.noteTitle }}</span>
      </div>
    </div>

    <div class="embed-body" :class="{ 'is-loading': loading }">
      <div v-if="loading" class="placeholder-content">
        <div class="skeleton-line"></div>
        <div class="skeleton-line short"></div>
      </div>
      
      <div v-else-if="error" class="error-content">
        ⚠️ 无法加载该块内容（可能已被删除或无权访问）
      </div>

      <editor-content v-else :editor="readonlyEditor" class="readonly-render-area" />
    </div>

    <div class="embed-footer-actions">
      <button class="action-btn" title="跳转到源文档" @click="jumpToSource">
        <span class="icon">↗️</span> 查看原文
      </button>
    </div>
  </node-view-wrapper>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import { nodeViewProps, NodeViewWrapper, useEditor, EditorContent } from '@tiptap/vue-3'
import StarterKit from '@tiptap/starter-kit'
import TaskList from '@tiptap/extension-task-list'
import TaskItem from '@tiptap/extension-task-item'
// 根据你的编辑器配置引入其他必要的扩展，确保渲染一致
import apiClient from '@/utils/api'

const props = defineProps(nodeViewProps)
const loading = ref(true)
const error = ref(false)
const blockInfo = ref(null)

// 1. 初始化一个只读的编辑器实例
const readonlyEditor = useEditor({
  editable: false, // 强制只读
  extensions: [
    StarterKit,
    TaskList,
    TaskItem.configure({ nested: true }),
    // 确保这里的扩展与 LingMaiEditor.vue 中的核心扩展一致
  ],
  content: '',
})

// 2. 获取数据方法
const fetchBlockData = async () => {
  const targetId = props.node.attrs.targetId
  if (!targetId) return

  loading.value = true
  error.value = false

  try {
    // 调用之前在 LingMaiBlockController 中写的接口
    const res = await apiClient.get(`/Blocks/${targetId}`)
    blockInfo.value = res.data

    if (readonlyEditor.value && res.data.originalJson) {
      // 解析后端存下的局部 JSON 并注入只读编辑器
      const content = JSON.parse(res.data.originalJson)
      readonlyEditor.value.commands.setContent(content)
    }
  } catch (e) {
    console.error('联动块加载失败:', e)
    error.value = true
  } finally {
    loading.value = false
  }
}

// 3. 跳转到源文档逻辑
const jumpToSource = () => {
  if (blockInfo.value?.noteId) {
    // 触发全局导航事件，LingMaiModule.vue 会监听此事件并切换页面
    window.dispatchEvent(new CustomEvent('navigate-note', { 
      detail: blockInfo.value.noteId 
    }))
  }
}

onMounted(() => {
  fetchBlockData()
})

// 如果 targetId 发生变化（比如撤销操作），重新加载
watch(() => props.node.attrs.targetId, () => {
  fetchBlockData()
})

onBeforeUnmount(() => {
  if (readonlyEditor.value) {
    readonlyEditor.value.destroy()
  }
})
</script>

<style lang="scss" scoped>
.lingmai-block-embed-container {
  margin: 1.5rem 0;
  background: #fffcf5; // 暖色调背景，区分原生内容
  border: 1px solid #ffedca;
  border-left: 4px solid #f59e0b; // 经典的联动块左侧线
  border-radius: 6px;
  overflow: hidden;
  transition: all 0.2s ease;

  &:hover {
    box-shadow: 0 4px 12px rgba(245, 158, 11, 0.12);
    border-color: #fcd34d;
  }

  .embed-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 6px 12px;
    background: rgba(245, 158, 11, 0.05);
    border-bottom: 1px dashed #ffedca;
    font-size: 12px;

    .embed-type-badge {
      color: #d97706;
      font-weight: 600;
      display: flex;
      align-items: center;
      gap: 4px;
    }

    .embed-source {
      color: #92400e;
      .source-link {
        cursor: pointer;
        text-decoration: underline;
        &:hover { color: #2563eb; }
      }
    }
  }

  .embed-body {
    padding: 12px;
    min-height: 40px;

    &.is-loading { opacity: 0.6; }

    /* 修改这个部分 */
.readonly-render-area {
  /* 🔥 必须：设置具体的高度限制，否则它会随内容无限撑高，永远不会出现滚动条 */
  max-height: 300px; 
  overflow-y: auto !important; 
  overflow-x: hidden;
  
  /* 🔥 必须：确保内部 ProseMirror 不会反向限制高度 */
  :deep(.ProseMirror) {
    min-height: auto !important;
    height: auto !important;
    padding: 4px !important;
    /* 允许指针交互 */
    pointer-events: auto !important; 
    user-select: text !important;
  }
}

/* 强制显示滚动条（防止某些系统默认隐藏） */
.readonly-render-area::-webkit-scrollbar {
  width: 6px !important;
  display: block !important;
}
.readonly-render-area::-webkit-scrollbar-thumb {
  background-color: #f59e0b !important;
  border-radius: 10px;
}

    .error-content {
      color: #dc2626;
      font-size: 13px;
      padding: 4px 0;
    }

    .placeholder-content {
      .skeleton-line {
        height: 14px;
        background: #f1f5f9;
        margin-bottom: 8px;
        border-radius: 4px;
        &.short { width: 60%; }
      }
    }
  }

  .embed-footer-actions {
    padding: 4px 12px;
    display: flex;
    justify-content: flex-end;
    opacity: 0; // 默认隐藏，悬浮显示
    transition: opacity 0.2s;

    .action-btn {
      background: none;
      border: none;
      font-size: 11px;
      color: #64748b;
      cursor: pointer;
      &:hover { color: #f59e0b; }
    }
  }

  &:hover .embed-footer-actions {
    opacity: 1;
  }
}
</style>