<template>
  <section class="form-section">
    <div class="section-header">
      <span class="sec-num">05</span>
      <span class="sec-title">链路 // LINKS</span>
    </div>
    <div class="links-editor">
      <div v-for="(link, index) in externalLinks" :key="index" class="link-row">
        <input type="text" v-model="link.name" class="cyber-input sm" placeholder="Name" @change="updateLink(index, link)" />
        <input type="text" v-model="link.url" class="cyber-input lg" placeholder="URL" @change="updateLink(index, link)" />
        <button class="del-btn" @click="removeLink(index)">×</button>
      </div>
      <button class="add-link-btn" @click="addLink">+ ADD NODE</button>
    </div>
  </section>
</template>

<script setup>
import { defineProps, defineEmits } from 'vue'

// Props
const props = defineProps({
  externalLinks: {
    type: Array,
    required: true
  }
})

// Emits
const emit = defineEmits(['update:externalLinks'])

// 更新单个链接
const updateLink = (index, updatedLink) => {
  const newLinks = [...props.externalLinks]
  newLinks[index] = updatedLink
  emit('update:externalLinks', newLinks)
}

// 添加链接
const addLink = () => {
  const newLinks = [...props.externalLinks, { name: '', url: '' }]
  emit('update:externalLinks', newLinks)
}

// 移除链接
const removeLink = (index) => {
  const newLinks = [...props.externalLinks]
  newLinks.splice(index, 1)
  emit('update:externalLinks', newLinks)
}
</script>

<style scoped>
/* 链路模块样式 */
.form-section { margin-bottom: 50px; }
.section-header { display: flex; align-items: baseline; border-bottom: 2px solid #111; padding-bottom: 10px; margin-bottom: 20px; }
.sec-num { font-size: 2.5rem; font-weight: bold; color: #ccc; margin-right: 15px; line-height: 0.8; font-family: 'Noto Sans SC', sans-serif; }
.sec-title { font-size: 1.1rem; font-weight: bold; font-family: 'JetBrains Mono', monospace; }
.cyber-input { border: 1px solid #999; background: #fff; padding: 10px; font-family: 'Noto Sans SC', sans-serif; font-size: 0.9rem; outline: none; transition: 0.2s; display: block; }
.cyber-input:focus { border-color: #D92323; box-shadow: 2px 2px 0 rgba(0,0,0,0.1); }

.links-editor {
  padding-top: 10px;
}
.link-row { 
  display: flex; 
  gap: 5px; 
  margin-bottom: 5px; 
}
.cyber-input.sm { width: 30%; }
.cyber-input.lg { flex: 1; }
.del-btn { 
  background: #ccc; 
  border: none; 
  cursor: pointer; 
  width: 30px; 
}
.del-btn:hover { 
  background: #D92323; 
  color: white; 
}
.add-link-btn { 
  width: 100%; 
  border: 1px dashed #999; 
  background: transparent; 
  padding: 8px; 
  cursor: pointer; 
  font-size: 0.8rem; 
}
.add-link-btn:hover { 
  border-color: #111; 
  background: #eee; 
}
</style>