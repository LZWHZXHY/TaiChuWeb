<template>
  <div class="container">
    <!-- 导航栏 -->
    <nav class="topBar">
      <div class="topBarLogo">
        <div class="logo-icon">
          <i class="fas fa-infinity"></i>
        </div>
      </div>

      <div class="topBarTitle">
        <RouterLink to="/" class="topBarOption">
          <h1>太初寰宇</h1>
        </RouterLink>
      </div>

      <div class="optionsContainer">
        <RouterLink to="/about" class="topBarOption">
          <p>关于我们</p>
          <div class="underline"></div>
        </RouterLink>
        <RouterLink to="/Transfer" class="topBarOption">
          <p>中转站</p>
          <div class="underline"></div>
        </RouterLink>
        <RouterLink to="/HelloWorld" class="topBarOption">
          <p>测试</p>
          <div class="underline"></div>
        </RouterLink>
      </div>
    </nav>

    <!-- 路由视图 -->
    <main class="main-content">
      <div class="content-wrapper">
        <router-view></router-view>
      </div>
      <h1 class="serve-people">水可载舟亦可覆舟</h1>
    </main>

    <!-- 底部区域 -->
    <div class="bottomArea">
      <a href="https://beian.miit.gov.cn/" target="_blank">京ICP备2022020268号</a>
      
    </div>
  </div>
</template>

<script setup>
import { RouterLink } from 'vue-router';
</script>

<style scoped>
  @import url('/static/css/general.css');
  @import url('/static/css/index.css');

</style>