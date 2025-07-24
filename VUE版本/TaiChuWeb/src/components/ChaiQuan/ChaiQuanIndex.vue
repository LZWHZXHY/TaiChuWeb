<template>
  <div class="container">
    <!-- 导航 -->
    <nav class="topBar">
      <div class="topBarLogo"></div>

      <div class="topBarTitle">
        <RouterLink to="/" class = "topBarOption"><p>太初寰宇</p></RouterLink>
    
      </div>


      <div class="optionsContainer">
       <RouterLink to="/about" class = "topBarOption"><p>关于我们</p></RouterLink>
       <RouterLink to="/Transfer" class = "topBarOption"><p>中转站</p></RouterLink>
       <RouterLink to="/HelloWorld" class = "topBarOption"><p>测试</p></RouterLink>

       <!-- 导航 
       <a href="/test.html" class="topBarOption">测试2</a>
       <a href="/功能性页面/functionPage.html" class="topBarOption">功能性页面</a>
       -->
      </div>


    </nav>




    
    <!-- 路由视图 -->
    <main class="main-content">
      <router-view></router-view>
      <h1 class="serve-people">水可载舟亦可覆舟</h1>
    </main>


    <div class="bottomArea">
      <a href="https://beian.miit.gov.cn/" target="_blank">京ICP备2022020268号</a>
      
    </div>
  </div>



</template>

<style scoped>
@import url('/static/css/general.css');





</style>

<script setup>



</script>


