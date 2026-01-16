<template>
  <section class="form-section">
    <div class="section-header">
      <span class="sec-num">04</span>
      <span class="sec-title">通讯 // COMMS</span>
    </div>
    <div class="form-group">
      <label>EMAIL</label>
      <input type="email" v-model="localEmail" class="cyber-input" />
    </div>
  </section>
</template>

<script setup>
import { ref, watch } from 'vue'

// Props
const props = defineProps({
  email: {
    type: String,
    required: true
  }
})

// Emits
const emit = defineEmits(['update:email'])

// 本地数据
const localEmail = ref(props.email)

// 监听变化
watch(localEmail, (val) => {
  emit('update:email', val)
})
</script>

<style scoped>
/* 通讯模块样式 */
.form-section { margin-bottom: 50px; }
.section-header { display: flex; align-items: baseline; border-bottom: 2px solid #111; padding-bottom: 10px; margin-bottom: 20px; }
.sec-num { font-size: 2.5rem; font-weight: bold; color: #ccc; margin-right: 15px; line-height: 0.8; font-family: 'Noto Sans SC', sans-serif; }
.sec-title { font-size: 1.1rem; font-weight: bold; font-family: 'JetBrains Mono', monospace; }
.form-group { margin-bottom: 15px; display: flex; flex-direction: column; padding-top: 10px; }
.form-group label { font-size: 0.7rem; color: #666; margin-bottom: 5px; font-weight: bold; font-family: 'JetBrains Mono', monospace; }
.cyber-input { border: 1px solid #999; background: #fff; padding: 10px; font-family: 'Noto Sans SC', sans-serif; font-size: 0.9rem; outline: none; transition: 0.2s; width: 100%; display: block; }
.cyber-input:focus { border-color: #D92323; box-shadow: 2px 2px 0 rgba(0,0,0,0.1); }
</style>