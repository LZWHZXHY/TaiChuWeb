<template>
  <div ref="menuRef" class="bubble-menu-wrapper">
    <slot />
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { BubbleMenuPlugin } from '@tiptap/extension-bubble-menu'

const props = defineProps({
  editor: {
    type: Object,
    required: true,
  },
  tippyOptions: {
    type: Object,
    default: () => ({}),
  },
  shouldShow: {
    type: Function,
    default: null,
  },
})

const menuRef = ref(null)

onMounted(() => {
  if (!props.editor) return

  // 手动创建气泡菜单插件实例
  // 这绕过了 @tiptap/vue-3 的组件封装，直接使用底层逻辑
  const plugin = BubbleMenuPlugin({
    pluginKey: 'customBubbleMenu',
    editor: props.editor,
    element: menuRef.value,
    tippyOptions: props.tippyOptions,
    shouldShow: props.shouldShow,
  })

  // 将插件注册到编辑器中
  props.editor.registerPlugin(plugin)

  // 组件销毁时，一定要卸载插件，防止内存泄漏
  onBeforeUnmount(() => {
    props.editor.unregisterPlugin('customBubbleMenu')
  })
})
</script>

<style scoped>
.bubble-menu-wrapper {
  /* 初始隐藏，由 Tippy.js 接管显示 */
  visibility: hidden; 
}
</style>