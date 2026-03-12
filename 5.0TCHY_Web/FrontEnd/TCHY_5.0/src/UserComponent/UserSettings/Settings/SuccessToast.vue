<template>
  <div 
    v-if="isVisible" 
    class="success-toast-overlay"
    @click="hideToast"
  >
    <div 
      class="success-toast-container"
      @click.stop
    >
      <div class="toast-icon">✓</div>
      <div class="toast-content">
        <h3 class="toast-title">{{ title }}</h3>
        <p class="toast-message">{{ message }}</p>
      </div>
      <button 
        class="toast-close" 
        @click="hideToast"
      >×</button>
    </div>
  </div>
</template>

<script setup>
import { ref, defineProps, defineEmits } from 'vue';

const props = defineProps({
  isVisible: {
    type: Boolean,
    required: true
  },
  title: {
    type: String,
    default: '操作成功 // SUCCESS'
  },
  message: {
    type: String,
    default: '设置已保存 // SETTINGS_SAVED'
  }
});

const emit = defineEmits(['update:isVisible']);

const hideToast = () => {
  emit('update:isVisible', false);
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Noto+Sans+SC:wght@400;700;900&display=swap');

.success-toast-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  animation: fadeIn 0.3s ease-out;
}

.success-toast-container {
  background: #F4F1EA;
  border-radius: 24px;
  padding: 24px;
  min-width: 300px;
  max-width: 400px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2);
  display: flex;
  align-items: center;
  gap: 16px;
  position: relative;
  transform: translateY(0);
  animation: slideUp 0.3s ease-out;
}

.toast-icon {
  width: 48px;
  height: 48px;
  background: #2e7d32;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  font-weight: bold;
  flex-shrink: 0;
}

.toast-content {
  flex: 1;
}

.toast-title {
  margin: 0 0 8px 0;
  font-size: 16px;
  font-weight: 700;
  color: #333;
  font-family: 'Noto Sans SC', sans-serif;
}

.toast-message {
  margin: 0;
  font-size: 14px;
  color: #666;
  font-family: 'Noto Sans SC', sans-serif;
  line-height: 1.4;
}

.toast-close {
  position: absolute;
  top: 12px;
  right: 12px;
  background: none;
  border: none;
  font-size: 20px;
  color: #999;
  cursor: pointer;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: all 0.2s;
}

.toast-close:hover {
  background: #f0f0f0;
  color: #333;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

@keyframes slideUp {
  from {
    transform: translateY(20px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}
</style>