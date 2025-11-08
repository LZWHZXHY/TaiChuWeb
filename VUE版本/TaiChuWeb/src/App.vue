<template>
  <div class="container">
    <!-- 导航栏 -->

    <div class = "rightBar">

      <ul>
        <RouterLink to="/Login" class="rightBarOption">
          <p>太初寰宇登录</p>
          <div class="underline"></div>
      </RouterLink>



      </ul>
      

    </div>



    <nav class="topBar">
      <div class="topBarLogo">
        <div class="logo-icon">
          <!--LOGO放在这里-->
        </div>
      </div>



      <div class="topBarTitle">
        <RouterLink to="/" class="topBarOption">
          <h1>太初寰宇</h1> <p>beta内测中ing......</p>
        </RouterLink>
      </div>

 
    </nav>

    <!-- 路由视图 -->
    <main class="main-content">
          <div class="content-wrapper">
        <router-view v-slot="{ Component }">
            <Transition name="fade-up" mode="out-in">
              <component :is="Component" />
            </Transition>
          </router-view>
      </div>

      <!--
      <h2 class="serve-people">因为老班的谴责，我得在这里放点不像是班里横幅的东西</h2>
      <h2 class="serve-people">照烧鸡排！我要狠狠的干你呀！！！！</h2>
      -->
      

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

<style>
  @import url('/static/css/general.css');
  @import url('/static/css/index.css');
  @import url('/static/css/mobileCSS/indexMoblie.css');

</style>