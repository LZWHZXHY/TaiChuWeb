import TaskItem from '@tiptap/extension-task-item'

export const CustomTaskItem = TaskItem.extend({
  // 保持你原有的允许嵌套块级内容的配置
  content: 'block+',

  addKeyboardShortcuts() {
    return {
      // 按下 Tab 键：向内进位 (作为子任务)
      'Tab': () => this.editor.commands.sinkListItem(this.name),
      
      // 按下 Shift + Tab 键：向外退位 (提升层级)
      'Shift-Tab': () => this.editor.commands.liftListItem(this.name),

      // 保留原有的回车分割逻辑（可选，通常继承自父类，但显式声明更稳妥）
      'Enter': () => this.editor.commands.splitListItem(this.name),
    }
  },
})