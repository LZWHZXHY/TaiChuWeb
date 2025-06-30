<template>
  <div class="container">
    <!-- 导航 -->
    <nav class="topBar">
      <div class="topBarLogo"></div>

      <div class="topBarTitle">
        <p>太初寰宇</p>
      </div>

      <div class="optionsContainer">
       <RouterLink to="/about" class = "topBarOption"><p>关于我们</p></RouterLink>
       <RouterLink to="/contact" class = "topBarOption"><p>联系我们</p></RouterLink>
       <RouterLink to="/HelloWorld" class = "topBarOption"><p>测试</p></RouterLink>
       <a href="/test.html" class="topBarOption">测试2</a>
      </div>
    </nav>

    <!-- 路由视图 -->
    <main class="main-content">
      <router-view></router-view>
    </main>
  </div>
</template>

<style scoped>
@import url('/static/css/general.css');
@import url('/static/css/index.css');
/* 确保有基本样式 */

/* 添加路由链接样式 */



</style>

<script setup>

import { RouterLink } from 'vue-router';

</script>