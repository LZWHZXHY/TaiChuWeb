import { Node, mergeAttributes } from '@tiptap/core'
import { VueNodeViewRenderer } from '@tiptap/vue-3'
import KanbanBoard from './KanbanBoard.vue'

export default Node.create({
  name: 'kanbanBoard',
  group: 'block',
  atom: true,

  addAttributes() {
    return {
      parentId: { default: null }
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