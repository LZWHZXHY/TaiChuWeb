<template>
  <SimpleBubbleMenu
    v-if="editor"
    :editor="editor"
    :tippy-options="{ duration: 100, maxWidth: 800, zIndex: 999 }"
    class="bubble-menu-container"
  >
    <div class="section-group">
      <div class="btn-group">
        <button 
          class="menu-btn" 
          :class="{ 'is-active': editor.isActive('bold') }" 
          @click="editor.chain().focus().toggleBold().run()"
          title="加粗 (Ctrl+B)"
        >
          <strong>B</strong>
        </button>
        <button 
          class="menu-btn italic" 
          :class="{ 'is-active': editor.isActive('italic') }" 
          @click="editor.chain().focus().toggleItalic().run()"
          title="倾斜 (Ctrl+I)"
        >
          <em>i</em>
        </button>
        <button 
          class="menu-btn underline" 
          :class="{ 'is-active': editor.isActive('underline') }" 
          @click="editor.chain().focus().toggleUnderline().run()"
          title="下划线 (Ctrl+U)"
        >
          <u>U</u>
        </button>
        <button 
          class="menu-btn strike" 
          :class="{ 'is-active': editor.isActive('strike') }" 
          @click="editor.chain().focus().toggleStrike().run()"
          title="删除线"
        >
          <s>S</s>
        </button>
      </div>
    </div>

    <div class="divider"></div>

    <div class="section-group">
      <div class="btn-group font-size-group">
        <button 
          class="menu-btn fs-btn" 
          :class="{ 'is-active': editor.isActive('textStyle', { fontSize: '12px' }) }" 
          @click="editor.chain().focus().setFontSize('12px').run()"
          title="小号"
        >
          <span style="font-size: 12px">A</span>
        </button>
        <button 
          class="menu-btn fs-btn" 
          @click="editor.chain().focus().unsetFontSize().run()"
          title="默认大小"
        >
          <span style="font-size: 15px">A</span>
        </button>
        <button 
          class="menu-btn fs-btn" 
          :class="{ 'is-active': editor.isActive('textStyle', { fontSize: '20px' }) }" 
          @click="editor.chain().focus().setFontSize('20px').run()"
          title="大号"
        >
          <span style="font-size: 18px">A</span>
        </button>
        <button 
          class="menu-btn fs-btn" 
          :class="{ 'is-active': editor.isActive('textStyle', { fontSize: '24px' }) }" 
          @click="editor.chain().focus().setFontSize('24px').run()"
          title="特大"
        >
          <span style="font-size: 22px; font-weight: bold;">A</span>
        </button>
      </div>
    </div>

    <div class="divider"></div>

    <div class="color-section-group">
      <div class="color-row">
        <span class="color-label">字</span>
        <div 
          v-for="color in textColors" 
          :key="color.val"
          class="color-dot"
          :style="{ background: color.val }"
          :class="{ 'is-active': editor.isActive('textStyle', { color: color.val }) }"
          @click="setTextColor(color.val)"
          :title="'字体: ' + color.name"
        ></div>
        <div class="color-dot remove" @click="editor.chain().focus().unsetColor().run()" title="清除">✕</div>
      </div>

      <div class="color-row">
        <span class="color-label">底</span>
        <div 
          v-for="color in bgColors" 
          :key="color.val"
          class="color-dot"
          :style="{ background: color.val }"
          :class="{ 'is-active': editor.isActive('highlight', { color: color.val }) }"
          @click="setBgColor(color.val)"
          :title="'背景: ' + color.name"
        ></div>
        <div class="color-dot remove" @click="editor.chain().focus().unsetHighlight().run()" title="清除">✕</div>
      </div>
    </div>

  </SimpleBubbleMenu>
</template>

<script setup>
import SimpleBubbleMenu from './SimpleBubbleMenu.vue'

const props = defineProps(['editor'])

const textColors = [
  { name: '红', val: '#e03e3e' },
  { name: '蓝', val: '#0b6e99' },
  { name: '绿', val: '#0f7b6c' },
  { name: '橙', val: '#d9730d' },
  { name: '紫', val: '#6940a5' },
]

const bgColors = [
  { name: '浅红', val: '#ffe2dd' },
  { name: '浅黄', val: '#fdecc8' },
  { name: '浅绿', val: '#dbeddb' },
  { name: '浅蓝', val: '#d3e5ef' },
  { name: '浅紫', val: '#e8deee' },
]

const setTextColor = (color) => {
  props.editor.chain().focus().setColor(color).run()
}

const setBgColor = (color) => {
  props.editor.chain().focus().toggleHighlight({ color: color }).run()
}
</script>

<style lang="scss" scoped>
.bubble-menu-container {
  display: flex;
  align-items: center;
  background: #fff;
  border: 1px solid #e2e8f0;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  border-radius: 8px;
  padding: 6px 8px;
  gap: 8px;
  pointer-events: auto; 
}

.section-group { display: flex; align-items: center; gap: 4px; }
.btn-group { display: flex; gap: 2px; }

.menu-btn {
  background: transparent;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  padding: 4px 8px;
  font-size: 14px;
  color: #555;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 28px;
  height: 28px;

  &:hover { background: #f1f5f9; color: #000; }
  &.is-active { background: #e0f2fe; color: #0284c7; }
  
  &.italic { font-family: serif; }
  &.underline { text-decoration: underline; }
}

.fs-btn { min-width: 32px; }

.divider { width: 1px; height: 24px; background: #e2e8f0; }

.color-section-group {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding-left: 4px;
}

.color-row { display: flex; align-items: center; gap: 4px; }
.color-label { font-size: 10px; color: #999; width: 12px; margin-right: 2px; }

.color-dot {
  width: 16px; height: 16px; border-radius: 4px; cursor: pointer; border: 1px solid rgba(0,0,0,0.1); transition: transform 0.1s;
  &:hover { transform: scale(1.1); border-color: #666; }
  &.is-active { border: 2px solid #000; box-shadow: 0 0 0 1px #fff inset; }
  &.remove { background: #fff; border: 1px solid #ccc; display: flex; align-items: center; justify-content: center; font-size: 10px; color: #666; &:hover { color: #e03e3e; border-color: #e03e3e; } }
}
</style>