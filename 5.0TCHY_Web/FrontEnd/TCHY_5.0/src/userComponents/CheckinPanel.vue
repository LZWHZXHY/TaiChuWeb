<template>
  <div>
    <h3>签到面板</h3>
    <div>当前用户：{{ user.name }}</div>
    <div v-if="isMe">这是你的签到功能</div>
    <div v-else>这是别人的签到信息</div>
  </div>
</template>

<script setup>
defineProps({
  user: { type: Object, required: true },
  isMe: { type: Boolean, default: false }
})
</script>