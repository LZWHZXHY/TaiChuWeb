import { Node, mergeAttributes } from '@tiptap/core'
import { VueNodeViewRenderer } from '@tiptap/vue-3'
import KanbanBoard from './KanbanBoard.vue'

export default Node.create({
  name: 'kanbanBoard',
  group: 'block',
  atom: true,

  addAttributes() {
    return {
      parentId: { default: null },
      // 新增 columns 属性，默认给三个基础列，并且支持解析和存储
      columns: {
        default: ['待办', '进行中', '已完成'],
        parseHTML: element => {
          const cols = element.getAttribute('data-columns')
          return cols ? JSON.parse(cols) : ['待办', '进行中', '已完成']
        },
        renderHTML: attributes => {
          return {
            'data-columns': JSON.stringify(attributes.columns)
          }
        }
      }
    }
  },

  parseHTML() {
    return [{ tag: 'kanban-board' }]
  },

  renderHTML({ HTMLAttributes }) {
    return ['kanban-board', mergeAttributes(HTMLAttributes)]
  },

  addNodeView() {
    return VueNodeViewRenderer(KanbanBoard)
  }
})