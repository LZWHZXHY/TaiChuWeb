<template>
  <div class="art-industrial-system">
    <div class="grid-bg moving-grid"></div>

    <div class="art-container">
      <header class="sys-header">
        <div class="header-left">
          <div class="logo-box"><span class="icon">■</span> >> 太初艺术数据中心</div>
          <div class="sys-status"><span class="pulse-dot"></span> 在线视觉档案...</div>
        </div>
        <div class="header-right">
          <div class="time-display">{{ currentTime }}</div>
          <div class="sys-id">TERM_ID: 0X-ART-505</div>
        </div>
      </header>

      <div class="art-body">
        <aside class="art-sidebar custom-scroll">
          <div class="sidebar-header">//频道选择</div>
          <nav class="channel-nav">

            <div class="cyber-channel-btn" :class="{ active: currentChannel === 'showcase' }" @click="switchChannel('showcase')">
              <div class="btn-deco"></div><div class="btn-content"><span class="ch-code">CH_00</span><span class="ch-name">精品！// SHOWCASE</span></div><div class="status-light"></div>
            </div>

            <div class="cyber-channel-btn" :class="{ active: currentChannel === 'gallery' }" @click="switchChannel('gallery')">
              <div class="btn-deco"></div><div class="btn-content"><span class="ch-code">CH_01</span><span class="ch-name">寰宇画廊 // GALLERY</span></div><div class="status-light"></div>
            </div>
            <div class="cyber-channel-btn" :class="{ active: currentChannel === 'joint' }" @click="switchChannel('joint')">
              <div class="btn-deco"></div><div class="btn-content"><span class="ch-code">CH_02</span><span class="ch-name">联合企划 // JOINT</span></div><div class="status-light"></div>
            </div>
            <div class="cyber-channel-btn" :class="{ active: currentChannel === 'certification' }" @click="switchChannel('certification')">
              <div class="btn-deco"></div><div class="btn-content"><span class="ch-code">CH_03</span><span class="ch-name">创作认证 // CERT</span></div><div class="status-light"></div>
            </div>
            <div class="cyber-channel-btn" :class="{ active: currentChannel === 'battlefield' }" @click="switchChannel('battlefield')">
              <div class="btn-deco"></div><div class="btn-content"><span class="ch-code">CH_04</span><span class="ch-name">太初约战 // BATTLE</span></div><div class="status-light"></div>
            </div>
            <div class="cyber-channel-btn" :class="{ active: currentChannel === 'society' }" @click="switchChannel('society')">
              <div class="btn-deco"></div><div class="btn-content"><span class="ch-code">CH_05</span><span class="ch-name">柴圈社团 // SOCIETY</span></div><div class="status-light"></div>
            </div>
            <div class="cyber-channel-btn" :class="{ active: currentChannel === 'world' }" @click="switchChannel('world')">
              <div class="btn-deco"></div><div class="btn-content"><span class="ch-code">CH_06</span><span class="ch-name">世界观收录 // WORLD</span></div><div class="status-light"></div>
            </div>
             <div class="cyber-channel-btn" :class="{ active: currentChannel === 'ocList' }" @click="switchChannel('ocList')">
              <div class="btn-deco"></div><div class="btn-content"><span class="ch-code">CH_07</span><span class="ch-name">OC数据库 // OC_DB</span></div><div class="status-light"></div>
            </div>
          </nav>

          <div class="monitor-panel">
            <div class="panel-label"><span class="icon">▼</span> 数据统计</div>
            <div class="stat-grid">
              <div class="stat-cell" @click="switchChannel('showcase')">
                <div class="stat-label">精品收录</div><div class="stat-val">{{ Showcase }}</div><div class="stat-bar"><div class="fill" style="width: 20%"></div></div>
              </div>
              <div class="stat-cell" @click="switchChannel('gallery')">
                <div class="stat-label">艺术作品</div><div class="stat-val">{{ artAmount }}</div><div class="stat-bar"><div class="fill" style="width: 60%"></div></div>
              </div>
              <div class="stat-cell" @click="switchChannel('joint')">
                <div class="stat-label">联合企划</div><div class="stat-val">{{ JointAmount }}</div><div class="stat-bar"><div class="fill" style="width: 30%"></div></div>
              </div>
              <div class="stat-cell" @click="switchChannel('battlefield')">
                <div class="stat-label">OC数据</div><div class="stat-val">{{ OCAmount }}</div><div class="stat-bar"><div class="fill" style="width: 80%"></div></div>
              </div>
              <div class="stat-cell" @click="switchChannel('society')">
                <div class="stat-label">社团</div><div class="stat-val">{{ SocietyAmount }}</div><div class="stat-bar"><div class="fill" style="width: 40%"></div></div>
              </div>
              <div class="stat-cell" @click="switchChannel('world')">
                <div class="stat-label">世界观</div><div class="stat-val">{{ WorldAmount }}</div><div class="stat-bar"><div class="fill" style="width: 50%"></div></div>
              </div>
            </div>
          </div>
          <div class="sidebar-footer">SYSTEM_READY...<br>WAITING_FOR_INPUT</div>
        </aside>

        <main class="art-viewport">
          <div class="view-frame">
            <div class="corner-tl"></div><div class="corner-tr"></div><div class="corner-bl"></div><div class="corner-br"></div>
            
            <div class="component-renderer">
              <Transition name="glitch-fade" mode="out-in">
                <ArtworkShowcase v-if="currentChannel === 'showcase'" />
                <ArtGallery v-else-if="currentChannel === 'gallery'" @refresh-stats="refreshGlobalStats" />
                <JointBoard v-else-if="currentChannel === 'joint'" />
                <Battlefield v-else-if="currentChannel === 'battlefield'" />
                <SocietyPanel v-else-if="currentChannel === 'society'" />
                <CertificationPanel v-else-if="currentChannel === 'certification'" />
                <WorldIpList v-else-if="currentChannel === 'world'" />
                <OcList v-else-if="currentChannel === 'ocList'" />
              </Transition>
            </div>
          </div>
        </main>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'; 
import { useRoute, useRouter } from 'vue-router'; 
import apiClient from '@/utils/api'; 
import ArtGallery from '@/ArtCenter/ArtGallery.vue'; 
import JointBoard from '@/ArtCenter/UnionPanel.vue';
import SocietyPanel from '@/ArtCenter/SocietyPanel.vue';
import Battlefield from '@/ArtCenter/Battlefield.vue';
import CertificationPanel from '@/ArtCenter/CertificationPanel.vue';
import WorldIpList from '@/ArtCenter/WorldIpList.vue';
import OcList from '@/ArtCenter/OcList.vue';
// 🔥 引入了精品组件
import ArtworkShowcase from '@/ArtCenter/ArtworkShowcase.vue';

const route = useRoute();
const router = useRouter();

const currentTime = ref(new Date().toLocaleTimeString());
let clockTimer = null;

// 初始化时优先读取 URL 参数
const currentChannel = ref(route.query.tab || 'showcase');

const Showcase = ref(0); // 精品数量如果需要后端获取，也可以在此处绑定
const artAmount = ref(0);
const OCAmount = ref(0);
const JointAmount = ref(0);
const SocietyAmount = ref(0);
const WorldAmount = ref(0);

const switchChannel = (channelName) => {
  currentChannel.value = channelName;
  router.replace({ query: { ...route.query, tab: channelName } });
};

watch(
  () => route.query.tab,
  (newTab) => {
    if (newTab && newTab !== currentChannel.value) {
      currentChannel.value = newTab;
    }
  }
);

const fetchTotalCount = async () => {
  try {
    const [galleryRes, jointRes, ocRes, societyRes, worldRes] = await Promise.all([
      apiClient.get('/Drawing/AllArtWork'),
      apiClient.get('/ChaiLianHe/verified-count'),
      apiClient.get('/Chai/stats/OCcount'),
      apiClient.get('/ChaiSheTuan/verified-count'),
      apiClient.get('/World/verified-count').catch(() => ({ status: 404, data: 0 }))
      // 如果精品有数量接口，你可以在这里加一个请求给 Showcase.value 赋值
    ]);
    if (galleryRes.status === 200) artAmount.value = galleryRes.data;
    if (jointRes.status === 200) JointAmount.value = jointRes.data;
    if (ocRes.status === 200) OCAmount.value = ocRes.data;
    if (societyRes.status === 200) SocietyAmount.value = societyRes.data;
    if (worldRes.status === 200) WorldAmount.value = worldRes.data;
  } catch (error) { console.error(error); }
};

const refreshGlobalStats = () => { fetchTotalCount(); }

onMounted(() => { 
  fetchTotalCount(); 
  clockTimer = setInterval(() => { currentTime.value = new Date().toLocaleTimeString(); }, 1000); 
});

onUnmounted(() => clearInterval(clockTimer));
</script>

<style scoped>
/* 原有的样式保持不变，没有改动 */
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.art-industrial-system {
  --red: #D92323; --black: #111111; --white: #F4F1EA; --gray: #E0DDD5;
  --mono: 'JetBrains Mono', monospace; --heading: 'Anton', sans-serif;
  width: 100%; height: 100%; background-color: var(--gray); color: var(--black); font-family: var(--mono); position: relative; overflow: hidden; padding: 20px; box-sizing: border-box;
}

.grid-bg { position: absolute; inset: 0; background-image: linear-gradient(rgba(17, 17, 17, 0.1) 1px, transparent 1px), linear-gradient(90deg, rgba(17, 17, 17, 0.1) 1px, transparent 1px); background-size: 40px 40px; z-index: 0; pointer-events: none; }
.moving-grid { animation: gridScroll 60s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-40px); } }

.art-container { width: 90%;  height: 92vh; background: var(--white); border: 4px solid var(--black); box-shadow: 20px 20px 0 rgba(0,0,0,0.2); z-index: 1; display: flex; flex-direction: column; margin: 0 auto; }
.sys-header { height: 60px; background: var(--black); color: var(--white); display: flex; justify-content: space-between; align-items: center; padding: 0 20px; border-bottom: 2px solid var(--red); flex-shrink: 0; }
.header-left { display: flex; align-items: center; gap: 20px; }
.logo-box { font-family: var(--heading); font-size: 1.5rem; display: flex; align-items: center; gap: 10px; }
.logo-box .icon { color: var(--red); }
.sys-status { font-size: 0.8rem; display: flex; align-items: center; gap: 8px; color: #aaa; }
.pulse-dot { width: 8px; height: 8px; background: #00ff00; border-radius: 50%; box-shadow: 0 0 5px #00ff00; animation: pulse 1s infinite; }
.header-right { text-align: right; font-size: 0.8rem; }
.time-display { font-weight: bold; font-size: 1.1rem; }
.sys-id { color: #666; font-size: 0.7rem; }

.art-body { 
  flex: 1; 
  display: flex; 
  overflow: hidden; 
  min-height: 0;    
}

.art-sidebar { 
  width: 260px; 
  background: #f0f0f0; 
  border-right: 4px solid var(--black); 
  display: flex; 
  flex-direction: column; 
  padding: 20px 15px; 
  flex-shrink: 0; 
  user-select: none;
  overflow: hidden; 
}
  
.sidebar-header { font-size: 0.7rem; color: #888; border-bottom: 2px dashed #ccc; margin-bottom: 15px; padding-bottom: 5px; }

.channel-nav { 
  display: flex; 
  flex-direction: column; 
  gap: 10px; 
  flex: 0 1 auto;
  margin-bottom: 15px;       
  overflow-y: auto; 
  min-height: 0;    
  padding-right: 5px; 
}

.channel-nav::-webkit-scrollbar { display: none; }

.cyber-channel-btn { background: #fff; border: 2px solid var(--black); padding: 12px; cursor: pointer; display: flex; align-items: center; justify-content: space-between; position: relative; transition: all 0.2s; box-shadow: 4px 4px 0 rgba(0,0,0,0.1); flex-shrink: 0; }
.cyber-channel-btn:hover { transform: translateX(5px); box-shadow: 6px 6px 0 var(--red); z-index: 2; }
.cyber-channel-btn.active { background: var(--black); color: var(--white); border-color: var(--black); transform: translateX(10px); box-shadow: 6px 6px 0 var(--red); }
.btn-content { display: flex; flex-direction: column; }
.ch-code { font-size: 0.6rem; opacity: 0.6; font-weight: bold; }
.ch-name { font-size: 0.9rem; font-weight: bold; }
.status-light { width: 6px; height: 6px; border: 1px solid #999; background: #ccc; }
.cyber-channel-btn.active .status-light { background: var(--red); border-color: var(--red); box-shadow: 0 0 5px var(--red); }

.monitor-panel { 
  background: var(--black); 
  color: var(--white); 
  padding: 15px; 
  border: 2px solid var(--black); 
  box-shadow: 4px 4px 0 rgba(0,0,0,0.2); 
  margin-top: 15px; 
  flex-shrink: 0; 
}

.panel-label { font-size: 0.8rem; border-bottom: 1px dashed #555; padding-bottom: 5px; margin-bottom: 15px; color: var(--red); font-weight: bold; }
.stat-grid { display: flex; flex-direction: column; gap: 15px; }
.stat-cell { cursor: pointer; transition: 0.2s; }
.stat-cell:hover .stat-label { color: var(--red); }
.stat-label { font-size: 0.7rem; color: #888; margin-bottom: 2px; }
.stat-val { font-family: var(--heading); font-size: 1.5rem; line-height: 1; }
.stat-bar { width: 100%; height: 4px; background: #333; margin-top: 4px; }
.fill { height: 100%; background: var(--white); }
.sidebar-footer { margin-top: 15px; font-size: 0.7rem; color: #999; text-align: center; line-height: 1.5; opacity: 0.5; flex-shrink: 0; }

.art-viewport {
  flex: 1;
  background: #fff;
  padding: 30px; 
  position: relative;
  overflow: hidden; 
  display: flex;
  flex-direction: column;
}

.view-frame {
  flex: 1; 
  width: 100%;
  position: relative;
  display: flex;
  flex-direction: column;
  overflow: hidden; 
  padding-bottom: 0; 
}

.component-renderer {
  flex: 1;
  width: 100%;
  height: 100%; 
  display: flex;
  flex-direction: column;
}

.corner-tl, .corner-tr, .corner-bl, .corner-br { position: absolute; width: 20px; height: 20px; border: 4px solid var(--black); pointer-events: none; z-index: 10; }
.corner-tl { top: -10px; left: -10px; border-right: none; border-bottom: none; }
.corner-tr { top: -10px; right: -10px; border-left: none; border-bottom: none; }
.corner-bl { bottom: -10px; left: -10px; border-right: none; border-top: none; }
.corner-br { bottom: -10px; right: -10px; border-left: none; border-top: none; }

@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.5; } 100% { opacity: 1; } }
.glitch-fade-enter-active, .glitch-fade-leave-active { transition: opacity 0.3s ease, transform 0.3s ease; }
.glitch-fade-enter-from { opacity: 0; transform: translateY(10px); }
.glitch-fade-leave-to { opacity: 0; transform: translateY(-10px); }

@media (max-height: 1080px) {
  .art-industrial-system { padding: 10px; }
  .art-container { height: 98vh; border-width: 3px; }
  .sys-header { height: 50px; }
  .art-sidebar { width: 230px; padding: 10px; }
  .channel-nav { gap: 6px; }
  .cyber-channel-btn { padding: 8px 10px; }
  .ch-name { font-size: 0.8rem; }
  .monitor-panel { padding: 10px; }
  .stat-grid { gap: 6px; } 
  .stat-val { font-size: 1.2rem; }
  .stat-label { font-size: 0.6rem; }
  .sidebar-footer { display: none; } 
  .art-viewport { padding: 15px; }
  .corner-tl { top: -5px; left: -5px; width: 15px; height: 15px; border-width: 3px; }
  .corner-tr { top: -5px; right: -5px; width: 15px; height: 15px; border-width: 3px; }
  .corner-bl { bottom: -5px; left: -5px; width: 15px; height: 15px; border-width: 3px; }
  .corner-br { bottom: -5px; right: -5px; width: 15px; height: 15px; border-width: 3px; }
}

@media (max-width: 1024px) {
  .art-container { height: auto; min-height: 95vh; }
  .art-body { flex-direction: column; }
  .art-sidebar { width: 100%; height: auto; border-right: none; border-bottom: 4px solid var(--black); flex-direction: row; flex-wrap: wrap; gap: 20px; align-items: flex-start; }
  .channel-nav { flex-direction: row; flex-wrap: wrap; margin-bottom: 0; flex: 1; overflow: visible; }
  .cyber-channel-btn { flex: 1; min-width: 140px; }
  .monitor-panel { width: 100%; max-width: 300px; margin-top: 0; }
  .sidebar-footer { display: none; }
  .art-viewport { padding: 15px; }
  .corner-tl, .corner-tr, .corner-bl, .corner-br { display: none; }
}
</style>