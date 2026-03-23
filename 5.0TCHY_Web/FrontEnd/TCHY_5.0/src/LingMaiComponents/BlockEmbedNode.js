import { Node, mergeAttributes } from '@tiptap/core'
import { VueNodeViewRenderer } from '@tiptap/vue-3'
import BlockEmbedView from './BlockEmbedView.vue'

/**
 * BlockEmbed Node
 * 用于实现“块联动”功能。它不存储实际内容，仅存储指向后端 blocks 表的 ID。
 * 渲染时，由 BlockEmbedView.vue 根据 targetId 实时拉取最新内容。
 */
export default Node.create({
  name: 'blockEmbed',

  // 1. 定义为块级元素
  group: 'block',

  // 2. 设置为原子节点，防止用户直接在内部输入，保持只读联动特性
  atom: true,

    draggable: true, // 允许拖拽

    // 🔥 核心修改：允许特定事件通过 NodeView 传给内部组件
    stopEvent: ({ event }) => {
        // 如果是滚轮事件、点击事件或输入事件，不要停止它，让它传进 Vue 组件
        const isWheel = event.type === 'wheel'
        const isClick = event.type === 'mousedown' || event.type === 'click'
        return !(isWheel || isClick) 
    },




  // 3. 定义属性：存储目标块的唯一 ID
  addAttributes() {
    return {
      targetId: {
        default: null,
        // 确保在 HTML 中渲染出 data-target-id 方便调试或解析
        parseHTML: element => element.getAttribute('data-target-id'),
        renderHTML: attributes => {
          if (!attributes.targetId) return {}
          return { 'data-target-id': attributes.targetId }
        },
      },
    }
  },

  // 4. 解析 HTML：识别包含 data-type="block-embed" 的 div
  parseHTML() {
    return [
      {
        tag: 'div[data-type="block-embed"]',
      },
    ]
  },

  // 5. 渲染 HTML：在导出 HTML 或保存时生成的结构
  renderHTML({ HTMLAttributes }) {
    return ['div', mergeAttributes(HTMLAttributes, { 'data-type': 'block-embed' }), 0]
  },

  // 6. 🔥 核心：指定使用 Vue 组件进行交互式渲染
  addNodeView() {
    return VueNodeViewRenderer(BlockEmbedView)
  },

  // 7. 自定义输入规则（可选）：支持通过 (( 快速插入，但在 Suggestion 逻辑中处理更佳
  addCommands() {
    return {
      insertBlockEmbed: attributes => ({ commands }) => {
        return commands.insertContent({
          type: this.name,
          attrs: attributes,
        })
      },
    }
  },
})