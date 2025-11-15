<template>
  <button 
    :class="['app-button', `app-button--${variant}`]"
    @click="$emit('click', $event)"
  >
    <slot></slot>
  </button>
</template>

<script setup>
defineProps({
  variant: {
    type: String,
    default: 'primary',
    validator: (value) => ['primary', 'outline', 'text'].includes(value)
  }
})

defineEmits(['click'])
</script>

<style scoped>
.app-button {
  padding: 10px 24px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  font-family: inherit;
}

.app-button--primary {
  background: #3498db;
  color: white;
}

.app-button--primary:hover {
  background: #2980b9;
  transform: translateY(-1px);
}

.app-button--outline {
  background: transparent;
  color: #666;
  border: 1px solid #ddd;
}

.app-button--outline:hover {
  color: #3498db;
  border-color: #3498db;
}
</style>