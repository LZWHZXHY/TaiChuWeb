<!-- src/views/PageNotFound.vue -->
<template>
  <div class="not-found">
    <h1>404 - 页面未找到</h1>
    <p>抱歉，您访问的页面不存在。</p>
    <router-link to="/">返回首页</router-link>
  </div>
</template>

<script>
export default {
  name: 'PageNotFound'
}
</script>

<style scoped>
.not-found {
  text-align: center;
  padding: 50px;
  font-size: 1.2rem;
  color: var(--light);
  background-color: var(--dark);
}

.not-found a {
  color: var(--secondary);
  text-decoration: none;
  margin-top: 20px;
}

.not-found a:hover {
  text-decoration: underline;
}
</style>