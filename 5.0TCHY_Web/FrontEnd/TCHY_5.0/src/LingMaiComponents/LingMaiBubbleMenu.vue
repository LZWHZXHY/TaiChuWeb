<template>
  <SimpleBubbleMenu
    v-if="editor"
    :editor="editor"
    :tippy-options="{ duration: 100, maxWidth: 600, zIndex: 999 }"
    class="bubble-menu-wrapper"
  >
    <div v-if="editor.isActive('table')" class="toolbar-row table-toolbar">
      <div class="label-tag">表格工具</div>
      
      <div class="btn-group">
        <button class="menu-btn" @click="editor.chain().focus().addColumnBefore().run()" title="左侧加列">⬅️列</button>
        <button class="menu-btn" @click="editor.chain().focus().addColumnAfter().run()" title="右侧加列">列➡️</button>
        <button class="menu-btn danger-hover" @click="editor.chain().focus().deleteColumn().run()" title="删除选中列">❌列</button>
      </div>

      <div class="divider"></div>

      <div class="btn-group">
        <button class="menu-btn" @click="editor.chain().focus().addRowBefore().run()" title="上方加行">⬆️行</button>
        <button class="menu-btn" @click="editor.chain().focus().addRowAfter().run()" title="下方加行">行⬇️</button>
        <button class="menu-btn danger-hover" @click="editor.chain().focus().deleteRow().run()" title="删除选中行">❌行</button>
      </div>

      <div class="divider"></div>

      <div class="btn-group">
        <button class="menu-btn" @click="editor.chain().focus().mergeCells().run()" title="合并单元格">🔀</button>
        <button class="menu-btn" @click="editor.chain().focus().splitCell().run()" title="拆分单元格">💔</button>
        <button class="menu-btn danger-btn" @click="editor.chain().focus().deleteTable().run()" title="删除整个表格">🗑️表格</button>
      </div>
    </div>

    <div class="toolbar-row text-toolbar">
      <div class="section-group">
        <div class="btn-group">
          <button class="menu-btn" :class="{ 'is-active': editor.isActive('bold') }" @click="editor.chain().focus().toggleBold().run()" title="加粗"><strong>B</strong></button>
          <button class="menu-btn italic" :class="{ 'is-active': editor.isActive('italic') }" @click="editor.chain().focus().toggleItalic().run()" title="斜体"><em>i</em></button>
          <button class="menu-btn underline" :class="{ 'is-active': editor.isActive('underline') }" @click="editor.chain().focus().toggleUnderline().run()" title="下划线"><u>U</u></button>
          <button class="menu-btn strike" :class="{ 'is-active': editor.isActive('strike') }" @click="editor.chain().focus().toggleStrike().run()" title="删除线"><s>S</s></button>
        </div>
      </div>

      <div class="divider"></div>

      <div class="section-group">
        <div class="btn-group font-size-group">
          <button class="menu-btn fs-btn" :class="{ 'is-active': editor.isActive('textStyle', { fontSize: '12px' }) }" @click="editor.chain().focus().setFontSize('12px').run()"><span style="font-size: 12px">A</span></button>
          <button class="menu-btn fs-btn" @click="editor.chain().focus().unsetFontSize().run()"><span style="font-size: 14px">A</span></button>
          <button class="menu-btn fs-btn" :class="{ 'is-active': editor.isActive('textStyle', { fontSize: '20px' }) }" @click="editor.chain().focus().setFontSize('20px').run()"><span style="font-size: 18px">A</span></button>
        </div>
      </div>

      <div class="divider"></div>

      <div class="color-section-group">
        <div class="color-row">
          <span class="color-label">字</span>
          <div v-for="color in textColors" :key="color.val" class="color-dot" :style="{ background: color.val }" :class="{ 'is-active': editor.isActive('textStyle', { color: color.val }) }" @click="editor.chain().focus().setColor(color.val).run()"></div>
          <div class="color-dot remove" @click="editor.chain().focus().unsetColor().run()">✕</div>
        </div>
        <div class="color-row">
          <span class="color-label">底</span>
          <div v-for="color in bgColors" :key="color.val" class="color-dot" :style="{ background: color.val }" :class="{ 'is-active': editor.isActive('highlight', { color: color.val }) }" @click="editor.chain().focus().toggleHighlight({ color: color.val }).run()"></div>
          <div class="color-dot remove" @click="editor.chain().focus().unsetHighlight().run()">✕</div>
        </div>
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
</script>

<style lang="scss" scoped>
/* 外部包装器：控制整体布局 */
.bubble-menu-wrapper {
  background: #fff;
  border: 1px solid #e2e8f0;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  border-radius: 8px;
  overflow: hidden; /* 圆角 */
  display: flex;
  flex-direction: column; /* 🔥 关键：垂直排列两行工具栏 */
}

/* 每一行的公共样式 */
.toolbar-row {
  display: flex;
  align-items: center;
  padding: 6px 8px;
  gap: 6px;
}

/* 表格工具栏专属样式 */
.table-toolbar {
  background-color: #f8fafc; /* 浅灰色背景，区分表格操作 */
  border-bottom: 1px solid #e2e8f0;
}

/* 小标签 */
.label-tag {
  font-size: 10px;
  background: #e2e8f0;
  color: #64748b;
  padding: 2px 4px;
  border-radius: 3px;
  margin-right: 4px;
  font-weight: 600;
  white-space: nowrap;
}

/* 以下是复用你原来的按钮样式 */
.section-group { display: flex; align-items: center; gap: 4px; }
.btn-group { display: flex; gap: 2px; }

.menu-btn {
  background: transparent;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  padding: 4px 6px; /* 稍微调小一点内边距以适应双层 */
  font-size: 14px;
  color: #555;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 26px;
  height: 26px;
  white-space: nowrap;

  &:hover { background: #f1f5f9; color: #000; }
  &.is-active { background: #e0f2fe; color: #0284c7; }
  
  &.italic { font-family: serif; }
  &.underline { text-decoration: underline; }

  &.danger-btn { color: #e11d48; &:hover { background: #ffe4e6; } }
  &.danger-hover:hover { color: #e11d48; background: #ffe4e6; }
}

.fs-btn { min-width: 30px; }

.divider { width: 1px; height: 18px; background: #e2e8f0; margin: 0 2px; }

/* 颜色选择器部分 */
.color-section-group { display: flex; flex-direction: column; gap: 3px; }
.color-row { display: flex; align-items: center; gap: 3px; }
.color-label { font-size: 10px; color: #999; width: 12px; }
.color-dot {
  width: 14px; height: 14px; border-radius: 3px; cursor: pointer; border: 1px solid rgba(0,0,0,0.1);
  &:hover { transform: scale(1.1); border-color: #666; }
  &.is-active { border: 1px solid #000; box-shadow: 0 0 0 1px #fff inset; }
  &.remove { background: #fff; border: 1px solid #ccc; font-size: 8px; display: flex; align-items: center; justify-content: center; color: #666; }
}
</style>