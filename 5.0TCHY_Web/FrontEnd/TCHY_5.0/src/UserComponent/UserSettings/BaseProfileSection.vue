<template>
  <section class="form-section">
    <div class="section-header">
      <span class="sec-num">02</span>
      <span class="sec-title">基础档案 // DATA</span>
    </div>
    <div class="form-group">
      <label>BIO // 简介</label>
      <textarea v-model="localBio" class="cyber-textarea" rows="3"></textarea>
    </div>
    <div class="form-grid">
      <div class="form-group">
        <label>LOC // 坐标</label>
        <input type="text" v-model="localLocation" class="cyber-input" />
      </div>
      <div class="form-group">
        <label>ROLE // 职业</label>
        <input type="text" v-model="localRole" class="cyber-input" />
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref, watch } from 'vue'

// Props
const props = defineProps({
  bio: {
    type: String,
    required: true
  },
  location: {
    type: String,
    required: true
  },
  role: {
    type: String,
    required: true
  }
})

// Emits
const emit = defineEmits(['update:bio', 'update:location', 'update:role'])

// 本地数据（双向绑定）
const localBio = ref(props.bio)
const localLocation = ref(props.location)
const localRole = ref(props.role)

// 监听变化并emit
watch(localBio, (val) => emit('update:bio', val))
watch(localLocation, (val) => emit('update:location', val))
watch(localRole, (val) => emit('update:role', val))
</script>

<style scoped>
/* 基础档案模块样式 */
.form-section { margin-bottom: 50px; }
.section-header { display: flex; align-items: baseline; border-bottom: 2px solid #111; padding-bottom: 10px; margin-bottom: 20px; }
.sec-num { font-size: 2.5rem; font-weight: bold; color: #ccc; margin-right: 15px; line-height: 0.8; font-family: 'Noto Sans SC', sans-serif; }
.sec-title { font-size: 1.1rem; font-weight: bold; font-family: 'JetBrains Mono', monospace; }
.form-group { margin-bottom: 15px; display: flex; flex-direction: column; padding-top: 10px; }
.form-group label { font-size: 0.7rem; color: #666; margin-bottom: 5px; font-weight: bold; font-family: 'JetBrains Mono', monospace; }
.cyber-input, .cyber-textarea { border: 1px solid #999; background: #fff; padding: 10px; font-family: 'Noto Sans SC', sans-serif; font-size: 0.9rem; outline: none; transition: 0.2s; width: 100%; display: block; }
.cyber-input:focus, .cyber-textarea:focus { border-color: #D92323; box-shadow: 2px 2px 0 rgba(0,0,0,0.1); }

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 15px;
}
</style>