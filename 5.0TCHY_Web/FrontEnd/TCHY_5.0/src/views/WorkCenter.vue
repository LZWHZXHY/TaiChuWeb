<template>
  <div class="tc-dashboard">
    <header class="sys-info-bar">
      <span class="pulse">●</span>
      <span>TAICHU_ART_HALL // VISUAL_ARCHIVE // {{ currentTime }}</span>
    </header>

    <div class="main-bridge">
      
      <aside class="left-column">
        <div class="section-label">CHANNELS // 频道</div>
        <div class="nav-menu">
          <div 
            class="nav-item" 
            :class="{ active: currentChannel === 'gallery' }"
            @click="currentChannel = 'gallery'"
          >
            <i class="fas fa-palette"></i> 寰宇画廊 (Gallery)
            <span class="count-badge">Live</span>
          </div>

          <div 
            class="nav-item" 
            :class="{ active: currentChannel === 'joint' }"
            @click="currentChannel = 'joint'"
          >
            <i class="fas fa-handshake"></i> 柴圈联合 (Joint)
            <span class="count-badge">New</span>
          </div>

          <div 
            class="nav-item" 
            :class="{ active: currentChannel === 'certification' }"
            @click="currentChannel = 'certification'"
          >
            <i class="fas fa-certificate"></i> 创作认证 (Cert)
            <span class="count-badge">System</span>
          </div>

          <div 
            class="nav-item" 
            :class="{ active: currentChannel === 'battlefield' }" 
            @click="currentChannel = 'battlefield'"
          >
            <i class="fas fa-khanda"></i> 太初约战场 (Battle)
            <span class="count-badge">Alpha</span>
          </div>

          <div 
            class="nav-item" 
            :class="{ active: currentChannel === 'society' }" 
            @click="currentChannel = 'society'"
          >
            <i class="fas fa-users"></i> 柴圈社团 (Society)
            <span class="count-badge">Beta</span>
          </div>
        </div>

        <div class="section-label" style="margin-top: 30px;">MY_ARTWORK</div>
        
        <div class="user-stat-box">
          <div class="stat" @click="currentChannel = 'gallery'" title="跳转至寰宇画廊">
            <span class="num">{{artAmount}}</span>
            <span class="txt">画廊作品</span>
          </div>
          <div class="stat" @click="currentChannel = 'joint'" title="跳转至柴圈联合">
            <span class="num">{{JointAmount}}</span>
            <span class="txt">已举行联合</span>
          </div>
          <div class="stat" @click="currentChannel = 'battlefield'" title="跳转至太初约战场">
            <span class="num">{{OCAmount}}</span>
            <span class="txt">已收录人设</span>
          </div>
          <div class="stat" @click="currentChannel = 'society'" title="跳转至柴圈社团">
            <span class="num">{{SocietyAmount}}</span>
            <span class="txt">已收录群聊</span>
          </div>
        </div>
      </aside>

      <main class="mid-column full-width">
        
        <ArtGallery 
          v-if="currentChannel === 'gallery'" 
          @refresh-stats="refreshGlobalStats"
        />

        <JointBoard v-else-if="currentChannel === 'joint'" />
        <Battlefield v-else-if="currentChannel === 'battlefield'" />
        <SocietyPanel v-else-if="currentChannel === 'society'" />
        <CertificationPanel v-else-if="currentChannel === 'certification'" />

      </main>

      </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'; 
import apiClient from '@/utils/api'; 

// 引入子组件
import ArtGallery from '@/ArtCenter/ArtGallery.vue'; 
import JointBoard from '@/ArtCenter/UnionPanel.vue' 
import SocietyPanel from '@/ArtCenter/SocietyPanel.vue' 
import Battlefield from '@/ArtCenter/Battlefield.vue' 
import CertificationPanel from '@/ArtCenter/CertificationPanel.vue' 

const currentTime = ref(new Date().toLocaleTimeString());
let clockTimer = null;

const currentChannel = ref('gallery');

// 统计数据 (保留全局统计)
const artAmount = ref(0);
const OCAmount = ref(0);
const JointAmount = ref(0);
const SocietyAmount = ref(0);

// 获取各项总数
const fetchTotalCount = async () => {
  try {
    const [galleryRes, jointRes, ocRes, societyRes] = await Promise.all([
      apiClient.get('/Drawing/AllArtWork'),
      apiClient.get('/ChaiLianHe/verified-count'),
      apiClient.get('/Chai/stats/OCcount'),
      apiClient.get('/ChaiSheTuan/verified-count')
    ]);
    if (galleryRes.status === 200) artAmount.value = galleryRes.data;
    if (jointRes.status === 200) JointAmount.value = jointRes.data;
    if (ocRes.status === 200) OCAmount.value = ocRes.data;
    if (societyRes.status === 200) SocietyAmount.value = societyRes.data;
  } catch (error) { console.error("获取统计数据失败", error); }
};

// 刷新函数 (现在只需要刷新总数统计，不需要刷新排行榜了，因为排行榜是 gallery 内部的事)
const refreshGlobalStats = () => {
  fetchTotalCount();
}

onMounted(() => {
  fetchTotalCount();
  // fetchLeaderboard(); // 已删除
  clockTimer = setInterval(() => { currentTime.value = new Date().toLocaleTimeString(); }, 1000);
});

onUnmounted(() => clearInterval(clockTimer));
</script>

<style scoped>
/* 布局样式大大简化 */

.tc-dashboard {
  --bg-main: #f8fafc;
  --accent-purple: #8b5cf6;
  --accent-red: #ef4444;
  --accent-dark: #0f172a;
  --text-primary: #1e293b;
  --text-sub: #64748b;
  --border: #e2e8f0;
  width: 100%; height: 100%; background-color: var(--bg-main);
  display: flex; flex-direction: column; overflow: hidden; box-sizing: border-box;
}
* { box-sizing: border-box; }

.sys-info-bar { flex-shrink: 0; height: 36px; background: #fff; border-bottom: 1px solid var(--border); display: flex; align-items: center; padding: 0 25px; font-size: 11px; font-weight: bold; color: var(--text-sub); }
.main-bridge { flex: 1; display: flex; padding: 20px; gap: 20px; min-height: 0; width: 100%; max-width: 1920px; margin: 0 auto; box-sizing: border-box;}

/* 左侧栏保持不变 */
.left-column { width: 18%; display: flex; flex-direction: column; gap: 20px; }
.nav-menu { display: flex; flex-direction: column; gap: 5px; }
.nav-item { padding: 12px 15px; background: #fff; border-radius: 8px; border: 1px solid var(--border); cursor: pointer; display: flex; align-items: center; justify-content: space-between; font-size: 14px; color: var(--text-primary); transition: 0.2s; }
.nav-item:hover, .nav-item.active { background: var(--accent-dark); color: #fff; border-color: var(--accent-dark); }
.count-badge { font-size: 10px; background: #f1f5f9; color: var(--text-sub); padding: 2px 6px; border-radius: 4px; }
.nav-item.active .count-badge { background: rgba(255,255,255,0.2); color: #fff; }

.user-stat-box { display: flex; justify-content: space-around; background: #fff; padding: 15px; border-radius: 8px; border: 1px solid var(--border); }
.stat { display: flex; flex-direction: column; align-items: center; padding: 5px; transition: all 0.2s; border-radius: 6px; cursor: pointer; }
.stat:hover { background-color: #f1f5f9; transform: translateY(-2px); }
.stat:active { transform: translateY(0); }
.stat .num { font-weight: 800; font-size: 16px; color: var(--accent-purple); }
.stat .txt { font-size: 10px; color: var(--text-sub); }

/* 中间区域 - 现在直接占满剩余宽度 */
.mid-column { 
  flex: 1; /* 自动占据除左侧栏外的所有空间 */
  display: flex; 
  flex-direction: column; 
  gap: 20px; 
  min-height: 0; 
  overflow: hidden; 
}

/* 删除了所有 .right-column, .tags-panel, .artist-row 等样式 */
.section-label { font-size: 10px; font-weight: 800; color: #94a3b8; letter-spacing: 1.5px; margin-bottom: 10px; }

/* 响应式优化 */
@media screen and (max-width: 1920px) {
  .tc-dashboard {
    font-size: 13px;
    zoom: 1; 
    padding: 15px;
    gap: 15px;
  }
  .left-column { width: 220px; flex-shrink: 0; } 
  /* 中间区域自适应即可 */
}
</style>