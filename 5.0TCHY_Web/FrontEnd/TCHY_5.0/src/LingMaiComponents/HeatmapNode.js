import { Node, mergeAttributes } from '@tiptap/core'
import { VueNodeViewRenderer } from '@tiptap/vue-3'
import ProjectHeatmap from './ProjectHeatmap.vue' // 确保路径指向你刚才创建的组件

export default Node.create({
  name: 'projectHeatmap', // 🔥 这个名字要和 slashCommand.js 里的 type 一致

  group: 'block', // 它是一个块级元素，独占一行

  atom: true, // 原子节点：它是一个整体，光标不能进入其内部编辑文本

  addAttributes() {
    return {
      parentId: {
        default: null, // 用来存储当前页面的 ID，以便热力图知道去请求哪个项目的数据
      },
    }
  },

  parseHTML() {
    return [
      {
        tag: 'project-heatmap', // 解析 HTML 时识别这个标签
      },
    ]
  },

  renderHTML({ HTMLAttributes }) {
    // 导出 HTML 时生成这个标签，并带上属性
    return ['project-heatmap', mergeAttributes(HTMLAttributes)]
  },

  addNodeView() {
    // 核心：用 Vue 组件来渲染这个节点
    return VueNodeViewRenderer(ProjectHeatmap)
  },
})