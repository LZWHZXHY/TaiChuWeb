<template>
  <div>
    <h3>仓库面板</h3>
    <div>用户名：{{ user.name }}</div>
    <div v-if="isMe">你正在查看自己的仓库</div>
    <div v-else>你正在查看别人的仓库</div>
    <!-- 具体内容后续扩展 -->
  </div>
</template>

<script setup>
defineProps({
  user: { type: Object, required: true },
  isMe: { type: Boolean, default: false }
})
</script>