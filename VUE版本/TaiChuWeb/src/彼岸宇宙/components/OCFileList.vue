<template>
  <div class="sidebar">
    <div class="parent" @click="openMenu = openMenu === '前传OC' ? '' : '前传OC'">前传OC列表</div>
    <div class="submenu" :class="{ show: openMenu === '前传OC' }">
      <a href="#">陈天</a>
      <a href="#">云绍</a>
      <a href="#">白·奥菲利亚</a>
      <a href="#">芮</a>
    </div>


    <div class="parent" @click="openMenu = openMenu === '正传oc' ? '' : '正传oc'">正传OC列表</div>
    <div class="submenu" :class="{ show: openMenu === '正传oc' }">
      <a href="#">陈合一</a>
     
    </div>

    
  </div>
</template>

<script setup>
import { ref } from 'vue'
const openMenu = ref('')
</script>

<style>
/* 你的样式保持不变 */
.sidebar {
  
  
  background: #2c3e50;
  color: #fff;
  display: flex;
  flex-direction: column;
  padding-top: 20px;
}
.sidebar a, .sidebar .parent {
  color: #ecf0f1;
  text-decoration: none;
  padding: 15px 20px;
  transition: background 0.3s;
  cursor: pointer;
  display: block;
}
.sidebar a:hover, .sidebar .parent:hover {
  background: #34495e;
}
.submenu {
  display: none;
  flex-direction: column;
  background: #34495e;
}
.submenu a {
  padding-left: 40px;
  font-size: 0.95em;
}
.submenu.show {
  display: flex;
}
.content {
  flex: 1;
  padding: 40px;
  background: #f4f7fa;
}
</style>