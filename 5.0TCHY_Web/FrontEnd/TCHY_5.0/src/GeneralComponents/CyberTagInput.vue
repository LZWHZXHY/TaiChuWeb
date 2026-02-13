<template>
  <div class="cyber-tag-input-container" ref="containerRef">
    <div class="input-wrapper" :class="{ focused: isFocused }">
      
      <span v-for="(tag, index) in localTags" :key="index" class="selected-tag">
        <span class="hash">#</span>{{ tag }}
        <span class="remove-btn" @click="removeTag(index)">×</span>
      </span>

      <input 
        v-model="inputValue" 
        type="text" 
        class="real-input"
        :placeholder="localTags.length === 0 ? '添加标签 (输入并回车)...' : ''"
        @keydown.enter.prevent="addTag(inputValue)"
        @keydown.delete="handleBackspace"
        @input="handleInput"
        @focus="isFocused = true"
        :disabled="localTags.length >= maxTags"
      />
    </div>

    <div v-if="suggestions.length > 0 && isFocused" class="suggestion-dropdown">
      <div 
        v-for="s in suggestions" 
        :key="s" 
        class="suggestion-item"
        @click="addTag(s)"
      >
        <span class="s-hash">#</span> {{ s }}
      </div>
    </div>

    <div class="limit-indicator">{{ localTags.length }}/{{ maxTags }}</div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted, onUnmounted } from 'vue';
import apiClient from '@/utils/api'; // 确保引入 axios 实例

const props = defineProps({
  modelValue: { type: String, default: '' }, // 接收 "tag1,tag2"
  maxTags: { type: Number, default: 5 }
});

const emit = defineEmits(['update:modelValue']);

// 内部状态
const localTags = ref([]);
const inputValue = ref('');
const isFocused = ref(false);
const suggestions = ref([]);
const containerRef = ref(null);
let debounceTimer = null;

// 初始化同步
watch(() => props.modelValue, (newVal) => {
  if (!newVal) {
    localTags.value = [];
  } else {
    localTags.value = newVal.split(',').filter(t => t.trim());
  }
}, { immediate: true });

// 监听输入，调用后端搜索接口
const handleInput = () => {
  const kw = inputValue.value.trim();
  if (!kw) {
    suggestions.value = [];
    return;
  }

  // 防抖处理
  if (debounceTimer) clearTimeout(debounceTimer);
  debounceTimer = setTimeout(async () => {
    try {
      // 🔥 调用刚才写的后端接口
      const res = await apiClient.get(`/Tag/search?keyword=${kw}`);
      if (res.data.success) {
        // 过滤掉已经选中的标签
        suggestions.value = res.data.data.filter(t => !localTags.value.includes(t));
      }
    } catch (e) {
      console.error(e);
    }
  }, 300);
};

// 添加标签
const addTag = (tagName) => {
  const val = tagName.trim();
  if (val && !localTags.value.includes(val) && localTags.value.length < props.maxTags) {
    localTags.value.push(val);
    emitUpdate();
  }
  // 清理状态
  inputValue.value = '';
  suggestions.value = [];
};

// 删除标签
const removeTag = (index) => {
  localTags.value.splice(index, 1);
  emitUpdate();
};

// 退格删除
const handleBackspace = () => {
  if (inputValue.value === '' && localTags.value.length > 0) {
    localTags.value.pop();
    emitUpdate();
  }
};

// 向上传递数据
const emitUpdate = () => {
  emit('update:modelValue', localTags.value.join(','));
};

// 点击外部关闭下拉菜单
const handleClickOutside = (e) => {
  if (containerRef.value && !containerRef.value.contains(e.target)) {
    isFocused.value = false;
    suggestions.value = [];
  }
};

onMounted(() => document.addEventListener('click', handleClickOutside));
onUnmounted(() => document.removeEventListener('click', handleClickOutside));
</script>

<style scoped>
.cyber-tag-input-container { position: relative; width: 100%; }

.input-wrapper {
  border: 2px solid #ccc;
  background: #fff;
  min-height: 46px;
  padding: 6px;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 6px;
  transition: 0.3s;
  border-bottom: 4px solid #111; /* 赛博硬边框 */
}

.input-wrapper.focused {
  border-color: #111;
  box-shadow: 4px 4px 0 rgba(217, 35, 35, 0.1);
}

/* 已选标签 */
.selected-tag {
  background: #111;
  color: #fff;
  font-family: 'JetBrains Mono';
  font-size: 0.8rem;
  padding: 4px 10px;
  display: flex;
  align-items: center;
  gap: 6px;
  animation: popIn 0.2s;
}
.selected-tag .hash { color: #D92323; }
.remove-btn { cursor: pointer; color: #888; font-weight: bold; }
.remove-btn:hover { color: #fff; }

/* 真实输入框 */
.real-input {
  border: none;
  outline: none;
  flex: 1;
  min-width: 120px;
  font-family: 'JetBrains Mono';
  font-size: 0.9rem;
  background: transparent;
  color: #111;
}

/* 下拉菜单 */
.suggestion-dropdown {
  position: absolute;
  top: 100%; left: 0; right: 0;
  background: #fff;
  border: 2px solid #111;
  border-top: none;
  z-index: 100;
  max-height: 200px;
  overflow-y: auto;
  box-shadow: 4px 4px 0 rgba(0,0,0,0.2);
}

.suggestion-item {
  padding: 10px 15px;
  cursor: pointer;
  font-family: 'JetBrains Mono';
  font-size: 0.9rem;
  border-bottom: 1px dashed #eee;
  transition: 0.2s;
}
.suggestion-item:hover {
  background: #D92323;
  color: #fff;
}
.suggestion-item .s-hash { color: #D92323; margin-right: 5px; }
.suggestion-item:hover .s-hash { color: #fff; }

.limit-indicator {
  position: absolute;
  right: 10px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 0.7rem;
  color: #999;
  font-family: 'JetBrains Mono';
  pointer-events: none; /* 防止遮挡点击 */
}

@keyframes popIn { from { transform: scale(0.9); opacity: 0; } to { transform: scale(1); opacity: 1; } }
</style>