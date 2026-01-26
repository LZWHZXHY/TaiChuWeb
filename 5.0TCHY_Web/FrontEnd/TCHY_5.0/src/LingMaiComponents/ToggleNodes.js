import { Node, mergeAttributes } from '@tiptap/core'
import { VueNodeViewRenderer } from '@tiptap/vue-3'

// 1. 定义外层容器 <details>
export const Details = Node.create({
  name: 'details',
  group: 'block',
  content: 'summary block+', 
  defining: true, 
  draggable: true, 
  isolating: true, // 🔥 关键：防止回车键意外跳出或合并

  addAttributes() {
    return {
      open: {
        default: true,
        parseHTML: element => element.hasAttribute('open'),
        renderHTML: attributes => {
          if (attributes.open) {
            return { open: '' }
          }
          return {}
        },
      },
    }
  },

  parseHTML() {
    return [{ tag: 'details' }]
  },

  renderHTML({ HTMLAttributes }) {
    return ['details', mergeAttributes(HTMLAttributes), 0]
  },

  // 🔥 核心修复：添加交互逻辑
  addNodeView() {
    return ({ node, HTMLAttributes, getPos, editor }) => {
      const dom = document.createElement('details')
      const contentDOM = document.createElement('div')

      // 1. 绑定初始属性
      Object.entries(HTMLAttributes).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          dom.setAttribute(key, value)
        }
      })
      if (node.attrs.open) {
        dom.setAttribute('open', '')
      }


      
      dom.addEventListener('toggle', (e) => {
        if (typeof getPos === 'function') {
          const isOpen = dom.hasAttribute('open')
          // 如果 DOM 状态和 Tiptap 数据不一致，同步数据
          if (editor.isEditable && isOpen !== node.attrs.open) {
            editor.commands.updateAttributes('details', { open: isOpen })
          }
        }
      })


      return {
        dom,
        contentDOM: dom, // 直接把 details 当容器
        ignoreMutation: (mutation) => {
            // 忽略 open 属性的变化，防止 ProseMirror 重新渲染导致闪烁
            if (mutation.type === 'attributes' && mutation.attributeName === 'open') {
                return true
            }
            return false
        },
        update: (updatedNode) => {
            if (updatedNode.type !== node.type) return false
            // 响应外部数据变化 (比如协同编辑)
            if (updatedNode.attrs.open !== dom.hasAttribute('open')) {
               if (updatedNode.attrs.open) dom.setAttribute('open', '')
               else dom.removeAttribute('open')
            }
            return true
        }
      }
    }
  }
})

// 2. 定义标题行 <summary> (保持不变)
export const Summary = Node.create({
  name: 'summary',
  group: 'block',
  content: 'inline*', 
  defining: true,
  selectable: false, // 避免双击全选导致很难选中里面的文字

  parseHTML() {
    return [{ tag: 'summary' }]
  },

  renderHTML({ HTMLAttributes }) {
    return ['summary', mergeAttributes(HTMLAttributes), 0]
  }
})