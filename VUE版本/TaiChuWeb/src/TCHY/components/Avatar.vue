<template>
  <img
    :src="srcToUse"
    :alt="alt"
    :class="avatarClass"
    @error="onError"
  />
</template>

<script setup>
import { ref, watch } from 'vue';
const props = defineProps({
  src: String,         // 头像url
  alt: {
    type: String,
    default: 'avatar'
  },
  size: {
    type: Number,
    default: 32
  },
  class: {
    type: String,
    default: ''
  },
  fallback: {
    type: String,
    default: '/static/img/default-avatar.png'
  }
});
const srcToUse = ref(props.src || props.fallback);

function onError() {
  if (srcToUse.value !== props.fallback) {
    srcToUse.value = props.fallback;
  }
}

watch(
  () => props.src,
  (val) => {
    srcToUse.value = val || props.fallback;
  }
);

const avatarClass = `${props.class} post-author-avatar`;
</script>

<style scoped>
.post-author-avatar {
  border-radius: 50%;
  object-fit: cover;
  margin-right: 8px;
  border: 1px solid #eee;
  background: #fafafa;
  /* 默认32x32，也允许父组件覆盖 */
  width: 32px;
  height: 32px;
}
</style>