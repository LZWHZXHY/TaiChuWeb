<template>
  <div class="forbidden">
    <h2>权限不足</h2>
    <p>此页面需要更高的等级或管理员账户权限，请以后再尝试</p>
    <p>门不能从这一侧打开.jpg</p>
    <router-link to="/">返回首页</router-link>
  </div>
</template>

<script setup></script>

<style scoped>
.forbidden {
  max-width: 460px;
  margin: 60px auto;
  background: #fff;
  padding: 28px;
  border: 1px solid #e5e7eb;
  border-radius: 14px;
  box-shadow: 0 4px 18px rgba(0,0,0,.06);
  text-align: center;
}
h2 { margin: 0 0 12px; font-size: 22px; font-weight: 800; color: #111827; }
p { margin: 0 0 18px; color: #555; }
a {
  display: inline-block;
  padding: 10px 18px;
  background: #2563eb;
  color: #fff;
  font-weight: 600;
  border-radius: 8px;
  text-decoration: none;
}
a:hover { background: #1d4ed8; }
</style>