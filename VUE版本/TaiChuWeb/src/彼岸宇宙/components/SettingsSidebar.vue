<template>
  <div class="sidebar">
    <!-- 一级菜单 -->
    <div class="parent" @click="toggleMenu('WorldBasicSetting')">世界观基础</div>
    <div class="submenu" :class="{ show: openMenus.WorldBasicSetting }">
    
      <div class="parent sub" @click.stop="toggleSubMenu('WorldView')">世界观设定
        <span class="arrow">{{ openSubMenus.WorldView ? '▼' : '▶' }}</span>
      </div>
      <div class="submenu sub" :class="{ show: openSubMenus.WorldView }">
        <div class="parent sub" @click.stop="toggleThirdMenu('ThirdMenu1')">世界规则
        <span class="arrow">{{ openThirdMenus.ThirdMenu1 ? '▼' : '▶' }}</span>
        </div>

        <div class="submenu third" :class="{ show: openThirdMenus.ThirdMenu1 }">
  <div class="parent sub" @click.stop="toggleFourthMenu('FourthMenu1')">
    详细条款
    <span class="arrow">{{ openFourthMenus.FourthMenu1 ? '▼' : '▶' }}</span>
  </div>
  <div class="submenu fourth" :class="{ show: openFourthMenus.FourthMenu1 }">
    <a href="#">条款A</a>
    <a href="#">条款B</a>
  </div>
</div>

        <a href="#" class="parent sub">零点星设定</a>


      </div>
      



      <div class="parent sub" @click.stop="toggleSubMenu('组织')">组织
        <span class="arrow">{{ openSubMenus.组织 ? '▼' : '▶' }}</span>
      </div>
      <div class="submenu sub" :class="{ show: openSubMenus.组织 }">
        <div class="parent sub" @click.stop="toggleThirdMenu('守序善良')">守序善良
        <span class="arrow">{{ openThirdMenus.守序善良 ? '▼' : '▶' }}</span>
        </div>

        <div class="submenu third" :class="{ show: openThirdMenus.守序善良 }"><a href="#">十二生肖</a></div>

        <div class="parent sub" @click.stop="toggleThirdMenu('中立善良')">中立善良
        <span class="arrow">{{ openThirdMenus.中立善良 ? '▼' : '▶' }}</span>
        </div>

        <div class="submenu third" :class="{ show: openThirdMenus.中立善良 }"><a href="#">塔罗牌密教</a></div>

        <div class="submenu third" :class="{ show: openThirdMenus.中立善良 }"><a href="#">各种公会</a></div>
      </div>




      <div class="parent sub" @click.stop="toggleSubMenu('职业')">职业
        <span class="arrow">{{ openSubMenus.职业 ? '▼' : '▶' }}</span>
      </div>



      <div class="parent sub" @click.stop="toggleSubMenu('技术分支')">技术分支
        <span class="arrow">{{ openSubMenus.技术分支 ? '▼' : '▶' }}</span>
      </div>



      <div class="submenu sub" :class="{ show: openSubMenus.技术分支 }">
        <div class="parent sub" @click.stop="toggleThirdMenu('工业技术')">工业技术
        <span class="arrow">{{ openThirdMenus.工业技术 ? '▼' : '▶' }}</span>
        </div>
      </div>

      

    </div>

    <!-- 一级菜单2 -->
    <div
      class="parent"
      @click="toggleMenu('PowerSystem')"
    >力量体系相关</div>
    <div class="submenu" :class="{ show: openMenus.PowerSystem }">
      <a href="#">间隙粒子</a>
    </div>
  </div>
</template>

<script setup>
import { reactive } from 'vue'

// 每一级菜单用对象记录多个菜单的开关状态
const openMenus = reactive({
  WorldBasicSetting: false,
  PowerSystem: false,
})

const openSubMenus = reactive({
  WorldView: false,
})

const openThirdMenus = reactive({
  ThirdMenu1: false,
})

const openFourthMenus = reactive({
  FourthMenu1: false,
})

// 切换一级菜单
function toggleMenu(name) {
  openMenus[name] = !openMenus[name]
}
// 切换二级菜单
function toggleSubMenu(name) {
  openSubMenus[name] = !openSubMenus[name]
}
// 切换三级菜单
function toggleThirdMenu(name) {
  openThirdMenus[name] = !openThirdMenus[name]
}
function toggleFourthMenu(name) {
  openFourthMenus[name] = !openFourthMenus[name]
}

</script>

<style>
.submenu.fourth {
  padding-left: 40px;
  display: none;
}
.submenu.fourth.show {
  display: block;
}
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
.parent.sub {
  padding-left: 30px;
  font-size: 0.98em;
  position: relative;
}
.submenu.sub {
  background: #42516a;
}
.submenu.third {
  background: #5a6a8a;
}
.submenu.third a {
  padding-left: 60px;
}
.arrow {
  float: right;
  font-size: 0.8em;
  margin-left: 10px;
}
</style>