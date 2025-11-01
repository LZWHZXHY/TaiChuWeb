<template>
  <div class="BigContainer">
    <div class="topBar">
      <div class="logo"></div>
      <div class="topBarOptions">
        <label
          v-for="(option, idx) in topBarOptions"
          :key="option.name"
          :class="{ active: idx === selectedTopBarIdx }"
          @click="selectTopBar(idx)"
        >
          {{ option.name }}
        </label>
      </div>
    </div>
    <div class="midContainer">
      <div class="leftBar">
        <template v-if="selectedTopBarIdx === 0">
          <div
            v-for="(continent, idx) in leftBarOptions"
            :key="continent.id"
            class="leftItem"
          >
            <div
              :class="{ leftActive: selectedLeftBarId === continent.id }"
              @click="toggleContinent(continent, idx)"
            >
              <span>{{ continent.name }}</span>
              <span v-if="continent.children">
                {{ continent.expanded ? '▼' : '►' }}
              </span>
            </div>
            <!-- 二级：国家/地区 -->
            <div v-if="continent.children && continent.expanded" style="padding-left: 16px;">
              <div
                v-for="child in continent.children"
                :key="child.id"
                :class="{ leftActive: selectedLeftBarId === child.id }"
                class="leftItem"
                @click.stop="selectLeftBar(child)"
              >
                {{ child.name }} <small style="opacity:.7;">({{ child.type === 'country' ? '国家' : '地区' }})</small>
              </div>
            </div>
          </div>
        </template>
        <template v-else-if="selectedTopBarIdx === 2">
          <div
            v-for="(item, idx) in leftBarOptions"
            :key="item.id"
            class="leftItem"
          >
            <div
              :class="{ leftActive: selectedLeftBarId === item.id }"
              @click="toggleWorldRule(item, idx)"
            >
              <span>{{ item.name }}</span>
              <span v-if="item.children">
                {{ item.expanded ? '▼' : '►' }}
              </span>
            </div>
            <div v-if="item.children && item.expanded" style="padding-left: 16px;">
              <div
                v-for="child in item.children"
                :key="child.id"
                :class="{ leftActive: selectedLeftBarId === child.id }"
                class="leftItem"
                @click.stop="selectLeftBar(child)"
              >
                {{ child.name }}
              </div>
            </div>
          </div>
        </template>
        <!-- 其他顶部栏的左侧内容可以用类似逻辑扩展 -->
      </div>
      <div class="viweWindow">
        <template v-if="currentViewContent">
          <!-- 针对“间隙粒子”显示 Partical.vue -->
          <component
            v-if="currentViewContent.name === '间隙粒子'"
            :is="Partical"
            :detail="currentViewContent"
          />
          <!-- 针对世界规则主项显示 WorldRules.vue -->
          <component
            v-else-if="currentViewContent.type === 'worldRules' && (!currentViewContent.parent_id || currentViewContent.parent_id === 0)"
            :is="WorldRules"
            :detail="currentViewContent"
          />
          <!-- 其他情况 -->
          <template v-else>
            <h2>{{ currentViewContent.name }}</h2>
            <p>{{ currentViewContent.summary }}</p>
            <img v-if="currentViewContent.image_url" :src="currentViewContent.image_url" style="max-width:300px;margin-top:10px;" />
            <div v-if="currentViewContent.extra" style="margin-top:10px;">
              <pre>{{ currentViewContent.extra }}</pre>
            </div>
          </template>
        </template>
        <template v-else>
          <p>请选择左侧选项以查看内容</p>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import axios from "axios";
//import WorldRules from './components/WorldRules.vue';
//import Partical from './components/Particle.vue'; // 记得新建并完善这个组件

const baseApiUrl = import.meta.env.DEV
  ? "https://localhost:44321"
  : "https://bianyuzhou.com";

// 顶部栏选项
const topBarOptions = [
  { name: "地区场景" },
  { name: "角色生物" },
  { name: "世界观基础" },
  { name: "力量体系相关" },
  { name: "时间线" },
  { name: "物品道具" }
];

const selectedTopBarIdx = ref(0);
const leftBarOptions = ref([]);
const selectedLeftBarId = ref(null);
const currentViewContent = ref(null);

// 顶部栏切换
function selectTopBar(idx) {
  selectedTopBarIdx.value = idx;
  leftBarOptions.value = [];
  selectedLeftBarId.value = null;
  currentViewContent.value = null;
  if (idx === 0) {
    loadContinents();
  } else if (idx === 2) {
    loadWorldRules();
  }
}

// 加载大陆
async function loadContinents() {
  const res = await axios.get(`${baseApiUrl}/api/byd/setting`, { params: { type: "continent" } });
  leftBarOptions.value = res.data.map(item => ({ ...item, expanded: false, children: null }));
}

async function loadWorldRules() {
  // 只查 type
  const res = await axios.get(`${baseApiUrl}/api/byd/setting`, { params: { type: "worldRules" } });
  // 只展示 parent_id 为 null 或 0 的
  leftBarOptions.value = res.data
    .filter(item => !item.parent_id || item.parent_id === 0)
    .map(item => ({ ...item, expanded: false, children: null }));
}

async function toggleWorldRule(item, idx) {
  selectedLeftBarId.value = item.id;
  if (item.expanded) {
    item.expanded = false;
    return;
  }
  if (!item.children) {
    // 只根据 parent_id 查询，不传 type
    const res = await axios.get(`${baseApiUrl}/api/byd/setting`, { params: { parent_id: item.id } });
    item.children = res.data;
  }
  item.expanded = true;
  loadDetail(item.id);
}

// 展开/收起大陆，并异步加载其下的国家和地区
async function toggleContinent(continent, idx) {
  selectedLeftBarId.value = continent.id;
  if (continent.expanded) {
    continent.expanded = false;
    return;
  }
  
  if (!continent.children) {
    const res = await axios.get(`${baseApiUrl}/api/byd/setting`, { params: { parent_id: continent.id } });
    continent.children = res.data;
  }
  continent.expanded = true;
  loadDetail(continent.id);
}

// 选中二级（国家/地区）或普通列表项
function selectLeftBar(item) {
  selectedLeftBarId.value = item.id;
  loadDetail(item.id);
}

// 加载详情
async function loadDetail(id) {
  const res = await axios.get(`${baseApiUrl}/api/byd/setting/${id}`);
  currentViewContent.value = res.data;
}

// 初始化加载
onMounted(() => {
  selectTopBar(0);
});
</script>

<style scoped>
.BigContainer {
  background-color: aliceblue;
  width: 100%;
  height: 100vh;
  overflow: hidden;
}
.topBar {
  width: 100%;
  height: 5vh;
  background-color: antiquewhite;
  display: flex;
  align-items: center;
}
.logo {
  background-color: black;
  width: 3vw;
  height: 80%;
  margin-left: 20px;
  border-radius: 6px;
}
.topBarOptions {
  display: flex;
  align-items: center;
  margin-left: 40px;
  gap: 16px;
  height: 100%;
}
.topBarOptions label {
  cursor: pointer;
  padding: 4px 12px;
  border-radius: 6px;
  transition: background 0.2s;
  font-size: 1rem;
}
.topBarOptions .active {
  background: #cfcfcf;
  font-weight: bold;
}
.midContainer {
  background-color: rgb(15, 75, 55);
  width: 100%;
  height: 95vh;
  display: flex;
  flex-direction: row;
}
.leftBar {
  width: 200px;
  background-color: rgb(161, 112, 48);
  padding-top: 10px;
  display: flex;
  flex-direction: column;
  align-items: stretch;
  overflow-y: auto;
}
.leftItem {
  padding: 10px 0;
  cursor: pointer;
  text-align: left;
  transition: background 0.2s, color 0.2s;
  font-size: 1.1rem;
  border-bottom: 1px solid #d4b075;
  padding-left: 16px;
}

.leftActive {
  background: #ffe4b5;
  color: #a0522d;
  font-weight: bold;
}
.viweWindow {
  background-color: rgb(32, 34, 77);
  flex: 1;
  color: #fff;
  padding: 30px;
  overflow-y: auto;
}
.viweWindow h2 {
  margin-top: 0;
  font-size: 2rem;
}
</style>