import { Node, mergeAttributes } from '@tiptap/core'
import { VueNodeViewRenderer } from '@tiptap/vue-3'
import CalendarNodeView from './CalendarNodeView.vue'

export default Node.create({
  name: 'calendarBlock',
  group: 'block',
  atom: true, // 重要：让整个日历在光标移动时被视为一个不可分割的单一块

  // 这里的属性将被存储到你数据库的 ContentJson 中
  addAttributes() {
    return {
      dayData: {
        default: {} // 结构例如: { "2026-04-14": [{ id: "1", text: "会议", linkId: "note-uuid" }] }
      }
    }
  },

  parseHTML() {
    return [{ tag: 'div[data-type="calendar-block"]' }]
  },

  renderHTML({ HTMLAttributes }) {
    return ['div', mergeAttributes(HTMLAttributes, { 'data-type': 'calendar-block' })]
  },

  addNodeView() {
    return VueNodeViewRenderer(CalendarNodeView)
  }
})