<template>
  <Transition name="modal-fade">
    <div v-if="visible" class="modal-overlay" @click.self="onCancel">
      <div class="modal-card">
        <div class="modal-icon">⚠️</div>
        <h3 class="modal-title">{{ title }}</h3>
        <p class="modal-desc">{{ message }}</p>
        
        <div class="modal-actions">
          <button class="btn cancel" @click="onCancel">取消</button>
          <button class="btn confirm" @click="onConfirm">确定删除</button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
defineProps({
  visible: Boolean,
  title: { type: String, default: '确认操作' },
  message: { type: String, default: '此操作无法撤销。' }
})

const emit = defineEmits(['update:visible', 'confirm', 'cancel'])

const onCancel = () => {
  emit('update:visible', false)
  emit('cancel')
}

const onConfirm = () => {
  emit('update:visible', false)
  emit('confirm')
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.4); /* 半透明遮罩 */
  backdrop-filter: blur(4px); /* 🔥 背景模糊，高级感的来源 */
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
}

.modal-card {
  background: #fff;
  width: 320px;
  padding: 24px;
  border-radius: 12px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
  text-align: center;
  transform: translateY(0);
  border: 1px solid rgba(0,0,0,0.05);
}

.modal-icon {
  font-size: 32px;
  margin-bottom: 12px;
}

.modal-title {
  font-size: 18px;
  font-weight: 600;
  color: #333;
  margin: 0 0 8px 0;
}

.modal-desc {
  font-size: 14px;
  color: #666;
  margin: 0 0 24px 0;
  line-height: 1.5;
}

.modal-actions {
  display: flex;
  gap: 12px;
  justify-content: center;
}

.btn {
  flex: 1;
  padding: 10px 0;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn.cancel {
  background: #f3f3f3;
  color: #555;
}
.btn.cancel:hover {
  background: #e5e5e5;
  color: #333;
}

.btn.confirm {
  background: #e03e3e; /* 红色警告色 */
  color: #fff;
}
.btn.confirm:hover {
  background: #c92a2a;
}

/* 动画效果 */
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.2s ease;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

.modal-fade-enter-active .modal-card,
.modal-fade-leave-active .modal-card {
  transition: transform 0.2s cubic-bezier(0.16, 1, 0.3, 1);
}

.modal-fade-enter-from .modal-card,
.modal-fade-leave-to .modal-card {
  transform: scale(0.95) translateY(10px);
}
</style>