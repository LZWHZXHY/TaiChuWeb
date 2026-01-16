<template>
  <section class="form-section">
    <div class="section-header">
      <span class="sec-num">03</span>
      <span class="sec-title">标签 // TAGS</span>
    </div>
    <div class="tag-manager">
      <div class="current-tags">
        <span v-for="(tag, index) in tags" :key="index" class="tag-chip">
          {{ tag }} <span class="remove-x" @click="removeTag(index)">×</span>
        </span>
      </div>
      <input type="text" v-model="localNewTagInput" @keyup.enter="addTag" class="cyber-input" placeholder="Add Tag + Enter" />
    </div>
  </section>
</template>

<script setup>
import { ref, watch } from 'vue'

// Props
const props = defineProps({
  tags: {
    type: Array,
    required: true
  },
  newTagInput: {
    type: String,
    required: true
  }
})

// Emits
const emit = defineEmits(['update:tags', 'update:newTagInput', 'add-tag', 'remove-tag'])

// 本地数据
const localNewTagInput = ref(props.newTagInput)

// 监听newTagInput变化
watch(localNewTagInput, (val) => {
  emit('update:newTagInput', val)
})

// 添加标签
const addTag = () => {
  const val = localNewTagInput.value.trim()
  if (val && !props.tags.includes(val)) {
    const newTags = [...props.tags, val]
    emit('update:tags', newTags)
    localNewTagInput.value = ''
  }
}

// 移除标签
const removeTag = (index) => {
  const newTags = [...props.tags]
  newTags.splice(index, 1)
  emit('update:tags', newTags)
}
</script>

<style scoped>
/* 标签模块样式 */
.form-section { margin-bottom: 50px; }
.section-header { display: flex; align-items: baseline; border-bottom: 2px solid #111; padding-bottom: 10px; margin-bottom: 20px; }
.sec-num { font-size: 2.5rem; font-weight: bold; color: #ccc; margin-right: 15px; line-height: 0.8; font-family: 'Noto Sans SC', sans-serif; }
.sec-title { font-size: 1.1rem; font-weight: bold; font-family: 'JetBrains Mono', monospace; }
.cyber-input { border: 1px solid #999; background: #fff; padding: 10px; font-family: 'Noto Sans SC', sans-serif; font-size: 0.9rem; outline: none; transition: 0.2s; width: 100%; display: block; }
.cyber-input:focus { border-color: #D92323; box-shadow: 2px 2px 0 rgba(0,0,0,0.1); }

.tag-manager {
  padding-top: 10px;
}
.current-tags {
  margin-bottom: 10px;
}
.tag-chip { 
  background: #111; 
  color: #fff; 
  display: inline-flex; 
  align-items: center; 
  gap: 5px; 
  padding: 2px 8px; 
  font-size: 0.8rem; 
  margin: 0 5px 5px 0; 
}
.remove-x { 
  color: #D92323; 
  cursor: pointer; 
}
</style>