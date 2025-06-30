<template>
  <div class="admin-page">
    <h1>后台管理</h1>
    <label>
      <input type="checkbox" v-model="showLogin" />
      显示“登录”按钮
    </label>
    <button @click="save">保存配置</button>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const showLogin = ref(true)

onMounted(async () => {
  const res = await fetch('/api/config')
  const config = await res.json()
  showLogin.value = config.showLogin
})

const save = async () => {
  await fetch('/api/config', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ showLogin: showLogin.value })
  })
  alert('保存成功')
}
</script>
