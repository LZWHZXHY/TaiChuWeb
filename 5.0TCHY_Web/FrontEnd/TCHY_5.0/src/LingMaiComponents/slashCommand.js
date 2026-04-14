import { Extension } from '@tiptap/core'
import Suggestion from '@tiptap/suggestion'
import { VueRenderer } from '@tiptap/vue-3'
import tippy from 'tippy.js'
import CommandList from './CommandList.vue'
import apiClient from '@/utils/api'


// 定义菜单里有哪些功能
const getSuggestionItems = ({ query }) => {
  return [
    {
      title: '图片',
      icon: '🖼️',
      command: ({ editor, range }) => {
        const input = document.createElement('input')
        input.type = 'file'
        input.accept = 'image/*'
        
        input.onchange = async () => {
          if (input.files.length === 0) return
          const file = input.files[0]

          const formData = new FormData()
          formData.append('file', file)

          try {
            // 🔥 修复点：删除了 headers 配置，让浏览器自动处理 boundary
            const res = await apiClient.post('/Upload/image', formData)
            
            const url = res.data.url

            if (url) {
              // 删除输入的 "/" 字符，并插入图片
              editor.chain().focus().deleteRange(range).setImage({ src: url }).run()
            }
          } catch (e) {
            // 🔥 打印详细错误，方便排查
            console.error('上传失败详情:', e.response || e)
            alert(`图片上传失败: ${e.response?.data?.message || '请检查网络或后端接口'}`)
          }
        }
        
        input.click()
      },
    },
    {
  title: '日历视图',
  description: '插入一个交互式日历，管理每日事项与双向链接',
  icon: '📅',
  command: ({ editor, range }) => {
    editor.chain().focus().deleteRange(range).insertContent({ type: 'calendarBlock' }).run()
  }
},
    {
      title: '折叠列表',
      icon: '▶',
      command: ({ editor, range }) => {
        editor.chain().focus().deleteRange(range)
          .insertContent({
            type: 'details',
            attrs: { open: true }, // 默认展开
            content: [
              {
                type: 'summary',
                content: [{ type: 'text', text: '折叠标题' }] // 默认标题
              },
              {
                type: 'paragraph', // 默认在这个折叠块里放一个空段落
                content: [] 
              }
            ]
          })
          .run()
      },
    },
    // ... 在 items 数组中添加
    {
      title: '看板块',
      description: '插入当前页面的任务看板',
      icon: '📊', // 如果你有图标库可以使用
      command: ({ editor, range }) => {
        // 获取当前编辑器的 noteId 属性
        const noteId = editor.options.editorProps.noteId 
        editor
          .chain()
          .focus()
          .deleteRange(range)
          .insertContent({
            type: 'kanbanBoard',
            attrs: { parentId: noteId }
          })
          .run()
      },
    },
    {
      title: '项目热力图',
      description: '展示长期项目的每日贡献热度',
      icon: '🔥', 
      command: ({ editor, range }) => {
        // 获取当前编辑器的 noteId 属性 (这需要在 LingMaiEditor.vue 初始化 editor 时传入)
        const noteId = editor.options.editorProps.noteId
        
        editor
          .chain()
          .focus()
          .deleteRange(range) // 删除输入的 "/"
          .insertContent({
            type: 'projectHeatmap', // 🔥 对应 HeatmapNode.js 里的 name
            attrs: { parentId: noteId } // 绑定当前页面ID
          })
          .run()
      },
    },
    {
      title: '待办列表',
      icon: '☑️',
      command: ({ editor, range }) => {
        editor.chain().focus().deleteRange(range).toggleTaskList().run()
      },
    },
    {
      title: '插入表格',
      description: '创建一个 3x3 的表格',
      // 你可以使用图标组件或者 svg 字符串
      icon: '', 
      
      // 🔥 核心执行逻辑
      command: ({ editor, range }) => {
        editor
          .chain()
          .focus()
          .deleteRange(range) // 删除输入的 "/" 字符
          .insertTable({ rows: 3, cols: 3, withHeaderRow: true }) // 插入表格
          .run()
      },
    },
    {
      title: '一级标题',
      icon: 'H1',
      command: ({ editor, range }) => {
        editor.chain().focus().deleteRange(range).setNode('heading', { level: 1 }).run()
      },
    },
    {
      title: '二级标题',
      icon: 'H2',
      command: ({ editor, range }) => {
        editor.chain().focus().deleteRange(range).setNode('heading', { level: 2 }).run()
      },
    },
    {
      title: '三级标题',
      icon: 'H3',
      command: ({ editor, range }) => {
        editor.chain().focus().deleteRange(range).setNode('heading', { level: 3 }).run()
      },
    },
    {
      title: '普通文本',
      icon: 'T',
      command: ({ editor, range }) => {
        editor.chain().focus().deleteRange(range).setParagraph().run()
      },
    },
    {
      title: '无序列表',
      icon: '•',
      command: ({ editor, range }) => {
        editor.chain().focus().deleteRange(range).toggleBulletList().run()
      },
    },
    {
      title: '有序列表',
      icon: '1.',
      command: ({ editor, range }) => {
        editor.chain().focus().deleteRange(range).toggleOrderedList().run()
      },
    },
    {
      title: '引用',
      icon: '❝',
      command: ({ editor, range }) => {
        editor.chain().focus().deleteRange(range).toggleBlockquote().run()
      },
    },
    {
      title: '代码块',
      icon: '{}',
      command: ({ editor, range }) => {
        editor.chain().focus().deleteRange(range).toggleCodeBlock().run()
      },
    },
    {
      title: '分割线',
      icon: '—',
      command: ({ editor, range }) => {
        editor.chain().focus().deleteRange(range).setHorizontalRule().run()
      },
    },
  ].filter(item => item.title.toLowerCase().includes(query.toLowerCase()))
}

// 渲染逻辑 (和你的 Mention 逻辑很像)
const renderSuggestion = () => {
  let component
  let popup

  return {
    onStart: props => {
      component = new VueRenderer(CommandList, {
        props,
        editor: props.editor,
      })

      if (!props.clientRect) return

      popup = tippy('body', {
        getReferenceClientRect: props.clientRect,
        appendTo: () => document.body,
        content: component.element,
        showOnCreate: true,
        interactive: true,
        trigger: 'manual',
        placement: 'bottom-start',
      })
    },
    onUpdate(props) {
      component.updateProps(props)
      if (!props.clientRect) return
      popup[0].setProps({
        getReferenceClientRect: props.clientRect,
      })
    },
    onKeyDown(props) {
      if (props.event.key === 'Escape') {
        popup[0].hide()
        return true
      }
      return component.ref?.onKeyDown(props)
    },
    onExit() {
      popup[0].destroy()
      component.destroy()
    },
  }
}

export default Extension.create({
  name: 'slashCommand',

  addProseMirrorPlugins() {
    return [
      Suggestion({
        editor: this.editor,
        char: '/', // 🔥 核心：监听 / 键
        command: ({ editor, range, props }) => {
          props.command({ editor, range })
        },
        items: getSuggestionItems,
        render: renderSuggestion,
      }),
    ]
  },
})