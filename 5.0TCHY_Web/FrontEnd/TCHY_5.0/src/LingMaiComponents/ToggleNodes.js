// ToggleNodes.js
import { Node, mergeAttributes } from '@tiptap/core' // 🔴 必须确保这一行存在

export const Details = Node.create({
  name: 'details',
  group: 'block',
  content: 'summary block+', 
  defining: true, 
  isolating: false, // 允许框选穿透

  addAttributes() {
    return {
      open: {
        default: true,
        renderHTML: attributes => ({
          'data-open': attributes.open,
          class: `details-node ${attributes.open ? 'is-open' : 'is-closed'}`
        }),
        parseHTML: element => element.getAttribute('data-open') === 'true',
      },
    }
  },

  renderHTML({ HTMLAttributes }) {
    return ['div', mergeAttributes(HTMLAttributes, { 'data-type': 'details' }), 0]
  },

  addNodeView() {
    return ({ node, getPos, editor }) => {
      // 创建主容器
      const dom = document.createElement('div')
      dom.setAttribute('data-type', 'details')
      dom.className = `details-node ${node.attrs.open ? 'is-open' : 'is-closed'}`
      dom.setAttribute('data-open', node.attrs.open)

      // 核心：点击事件拦截
      dom.onclick = (e) => {
        // 查找点击的是否是标题区域
        if (e.target.closest('[data-type="summary"]')) {
          if (typeof getPos === 'function' && editor.isEditable) {
            editor.commands.updateAttributes('details', { open: !node.attrs.open })
          }
        }
      }

      // 创建内容容器（ProseMirror 会将所有 content 渲染到这里，包括 summary）
      const contentDOM = document.createElement('div')
      contentDOM.className = 'details-content'
      dom.appendChild(contentDOM)

      return {
        dom,
        contentDOM,
      }
    }
  }
})

export const Summary = Node.create({
  name: 'summary',
  group: 'block',
  content: 'inline*', 
  defining: true,
  isolating: false,
  selectable: false,

  renderHTML({ HTMLAttributes }) {
    return ['div', mergeAttributes(HTMLAttributes, { 'data-type': 'summary', class: 'summary-node' }), 0]
  }
})