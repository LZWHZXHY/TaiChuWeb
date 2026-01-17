<template>
  <div class="guestbook-input-area">
    <div class="gb-input-header">
      <span class="blink">PULSE_INPUT //</span> 请输入加密留言
    </div>
    <div class="gb-form">
      <textarea
        v-model="localMsg"
        placeholder="输入内容..."
        rows="3"
        class="gb-textarea"
      ></textarea>
      <div class="gb-actions">
        <span class="deco-code">CODE: 0x88</span>
        <button class="sign-btn" @click="handleSubmit">
          [ 签署并归档 / SIGN ]
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

// 接收父组件传递的输入值
const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  }
})

// 触发父组件的事件（更新值、提交）
const emit = defineEmits(['update:modelValue', 'submit'])

// 双向绑定本地值
const localMsg = computed({
  get() {
    return props.modelValue
  },
  set(val) {
    emit('update:modelValue', val)
  }
})

// 提交按钮点击逻辑
const handleSubmit = () => {
  if (!localMsg.value.trim()) return
  emit('submit')
}
</script>

<style scoped>
/* 迁移原ProfileMain中留言输入区的样式 */
.guestbook-input-area {
  border: 2px solid var(--black);
  background: #eee;
  padding: 15px;
}
.gb-input-header {
  font-family: var(--mono);
  font-weight: bold;
  margin-bottom: 10px;
  font-size: 0.8rem;
  color: #555;
}
.gb-form {
  display: flex;
  flex-direction: column;
  gap: 10px;
}
.gb-textarea {
  width: 100%;
  border: 1px solid var(--black);
  padding: 10px;
  font-family: var(--sans);
  background: var(--white);
  resize: vertical;
  min-height: 80px;
  box-sizing: border-box;
}
.gb-textarea:focus {
  outline: 2px solid var(--red);
}
.gb-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.deco-code {
  font-family: var(--mono);
  font-size: 0.7rem;
  color: #888;
}
.sign-btn {
  background: var(--black);
  color: var(--white);
  border: none;
  padding: 8px 20px;
  font-family: var(--mono);
  font-weight: bold;
  cursor: pointer;
  transition: 0.2s;
}
.sign-btn:hover {
  background: var(--red);
}

/* 闪烁动画 */
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
.blink { animation: blink 1s infinite; }
</style>