<template>1
<div>用户名：{{ user.name }}</div>

</template>
<script setup>
defineProps({
  user: { type: Object, required: true },
  isMe: { type: Boolean, default: false }
})
</script>