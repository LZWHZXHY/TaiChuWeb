<template>
  <div class="app-container">
    <LeftSidebar />
    <MainContent />
    <RightSidebar />
  </div>
</template>

<script>
import LeftSidebar from '@/BlogComponents/LeftSidebar.vue'
import MainContent from '@/BlogComponents/MainContent.vue'
import RightSidebar from '@/BlogComponents/RightSiderbar.vue'

export default {
  name: 'App',
  components: {
    LeftSidebar,
    MainContent,
    RightSidebar
  }
}
</script>

<style>
:root {
  --primary-color: #4a6fa5;
  --secondary-color: #166088;
  --accent-color: #4fc3f7;
  --light-color: #f8f9fa;
  --dark-color: #343a40;
  --gray-color: #6c757d;
  --light-gray: #e9ecef;
  --border-radius: 8px;
  --box-shadow: 0 5px 15px rgba(0, 0, 0, 0.05);
  --transition: all 0.3s ease;
}

* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  font-family: 'Segoe UI', 'Microsoft YaHei', sans-serif;
}

body {
  background-color: #f5f7fa;
  color: #333;
  line-height: 1.6;
}

.app-container {
  display: flex;
  min-height: 100vh;
}
</style>