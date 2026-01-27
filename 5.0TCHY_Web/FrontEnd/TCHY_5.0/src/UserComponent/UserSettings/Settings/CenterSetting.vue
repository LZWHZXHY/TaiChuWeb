<template>
  <div class="center-dashboard">
    
    <div v-if="loading" class="loading-state">
       <div class="spinner"></div>
       <span>数据同步中... // SYNCING</span>
    </div>

    <div v-else class="dashboard-card profile-card">
      <div class="card-content-left">
        <div class="identity-header">
          <div class="name-row">
            <h1 class="user-name">{{ userInfo.name }}</h1>
            <span class="gender-badge" v-if="userInfo.gender">
              {{ userInfo.gender }}
            </span>
          </div>
          <div class="job-title">
            <span class="highlight">
              {{ userInfo.signature || dataStatus.title || 'NEURAL_ARCHITECT' }}
            </span> 
          </div>
        </div>

        <div class="bio-section">
          <div class="tags-group" v-if="userInfo.tags.length > 0">
            <span v-for="(tag, index) in userInfo.tags" :key="index" class="tech-tag">
              #{{ tag }}
            </span>
          </div>
          <p class="bio-text">{{ userInfo.bio || '该用户很懒，什么都没写...' }}</p>
        </div>

        <div class="contact-footer">
          <div class="info-item">
            <span class="icon">📍</span> {{ fullLocation }}
          </div>
          <div class="divider">/</div>
          <div class="info-item">
            <span class="icon">✉</span> {{ userInfo.contact || '暂无联系方式' }}
          </div>
        </div>
      </div>

      <div class="card-content-right">
        <div class="avatar-container">
          <img :src="userInfo.avatar" class="big-avatar" draggable="false" />
          
          <div class="level-pill">
            <span class="lv-label">LV.</span>
            <span class="lv-val">{{ dataStatus.level }}</span>
          </div>
          
          <div class="ripple-circle c1"></div>
          <div class="ripple-circle c2"></div>
        </div>
      </div>
      
      <div class="bg-watermark">IDENTITY</div>
    </div>

    <div v-if="!loading" class="dashboard-card data-card">
      <div class="data-header">
        <span class="section-title">数据同步状态 // SYNCHRONIZATION</span>
        <div class="header-line"></div>
      </div>

      <div class="stats-grid">
        <div class="stat-box">
          <div class="stat-label">
            <span class="cn">声望</span>
            <span class="en">REPUTATION</span>
          </div>
          <div class="stat-value dark">{{ dataStatus.reputation }}</div>
        </div>

        <div class="stat-box">
          <div class="stat-label">
            <span class="cn">资产</span>
            <span class="en">ASSETS</span>
          </div>
          <div class="stat-value gold">{{ formatNumber(dataStatus.gold) }}</div>
        </div>

        <div class="exp-box">
          <div class="exp-info">
            <span class="exp-label">EXP_Link</span>
            <span class="exp-nums">{{ dataStatus.currentExp }} / {{ dataStatus.nextLevelExp }}</span>
          </div>
          <div class="progress-track">
            <div class="progress-bar" :style="{ width: expPercentage + '%' }">
              <div class="progress-glare"></div>
            </div>
          </div>
          <div class="exp-percent-text">{{ expPercentage }}% Synced</div>
        </div>
      </div>
      
      <div class="bg-watermark bottom">DATA_LAYER</div>
    </div>

  </div>
</template>

<script setup>
import { reactive, computed, onMounted, ref } from 'vue'
import apiClient from '@/utils/api' 

const loading = ref(true)

// --- 响应式数据 ---
const userInfo = reactive({
  name: '未命名用户',
  gender: '',
  region: '',
  address: '',
  contact: '',
  bio: '',
  signature: '', // 个性签名
  tags: [],
  avatar: 'https://img.bianyuzhou.com/uploads/默认头像/默认头像2.png' // 默认兜底
})

const dataStatus = reactive({
  title: '', // 系统头衔 (如: 初入太初)
  level: 1,
  reputation: 0,
  gold: 0,
  currentExp: 0,
  nextLevelExp: 100
})

// --- 计算属性 ---

// 拼接完整地址: "椰汁城 · 下水道"
const fullLocation = computed(() => {
  const r = userInfo.region || ''
  const a = userInfo.address || ''
  if (r && a) return `${r} · ${a}`
  return r || a || '未知区域'
})

// 计算经验百分比
const expPercentage = computed(() => {
  if (dataStatus.nextLevelExp === 0) return 100
  return Math.round((dataStatus.currentExp / dataStatus.nextLevelExp) * 100)
})

const formatNumber = (num) => {
  return num ? num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",") : '0'
}

// --- 数据获取逻辑 ---
const initData = async () => {
  try {
    loading.value = true
    
    // 并行请求两个接口
    const [detailRes, statusRes] = await Promise.all([
      apiClient.get('/profile/detail'),
      apiClient.get('/profile/me')
    ])

    // 1. 处理资产状态 (UserStatus)
    // 对应第二个 JSON
    if (statusRes.data && statusRes.data.success) {
      const s = statusRes.data.data
      dataStatus.level = s.Level
      dataStatus.reputation = s.Reputation
      dataStatus.gold = s.Points
      dataStatus.currentExp = s.CurrentExp
      dataStatus.nextLevelExp = s.NextLevelExp
      dataStatus.title = s.Title // "初入太初"
    }

    // 2. 处理个人资料 (聚合信息)
    // 对应第一个 JSON
    if (detailRes.data && detailRes.data.success) {
      const d = detailRes.data.data
      
      // 直接映射，无需 fallback
      userInfo.name = d.Name || '未命名用户'
      userInfo.avatar = d.Avatar || userInfo.avatar // 如果为空则用默认图
      userInfo.bio = d.Bio
      userInfo.gender = d.Gender
      userInfo.region = d.Region
      userInfo.address = d.Address
      userInfo.contact = d.ContactInfo
      userInfo.signature = d.PersonalSignature // "超级六边形全能之神"

      // 处理标签 (Interests) -> "编程,游戏" 转数组
      if (d.Interests && d.Interests.trim() !== "") {
        userInfo.tags = d.Interests.split(/[,，]/).filter(t => t.trim() !== '')
      } else {
        userInfo.tags = []
      }
    }

  } catch (error) {
    console.error('Failed to load profile data:', error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  initData()
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Roboto+Mono:wght@400;500;700&family=Noto+Sans+SC:wght@400;700;900&display=swap');

/* --- 容器布局 --- */
.center-dashboard {
  width: 100%; height: 100%;
  padding: 30px; box-sizing: border-box;
  display: flex; flex-direction: column; gap: 24px; 
  font-family: 'Noto Sans SC', sans-serif;
  overflow-y: auto; 
}

/* Loading */
.loading-state {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  height: 100%; color: #999; font-family: 'Roboto Mono'; gap: 15px;
}
.spinner {
  width: 30px; height: 30px; border: 3px solid #eee; border-top-color: #000;
  border-radius: 50%; animation: spin 1s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

.center-dashboard::-webkit-scrollbar { width: 4px; }
.center-dashboard::-webkit-scrollbar-thumb { background: #eee; border-radius: 2px; }

/* --- 卡片通用 --- */
.dashboard-card {
  width: 100%; background-color: #F4F1EA; border-radius: 24px;       
  position: relative; overflow: hidden;
  box-shadow: 0 4px 20px rgba(0,0,0,0.02); 
  transition: transform 0.2s ease;
}
.dashboard-card:hover { transform: translateY(-2px); }

/* 水印 */
.bg-watermark {
  position: absolute; font-family: 'Roboto Mono', monospace; font-weight: 900;
  font-size: 80px; color: rgba(0,0,0,0.03); pointer-events: none; z-index: 0;
}
.bg-watermark:not(.bottom) { top: -10px; right: 20px; }
.bg-watermark.bottom { bottom: -15px; left: 20px; }

/* ================== 身份档案 ================== */
.profile-card {
  flex: 1.2; min-height: 240px;
  display: flex; flex-direction: row; padding: 30px 40px; box-sizing: border-box;
}

.card-content-left {
  flex: 2; display: flex; flex-direction: column; justify-content: center; z-index: 1;
}

.identity-header { margin-bottom: 20px; }
.name-row { display: flex; align-items: center; gap: 12px; }
.user-name { margin: 0; font-size: 32px; font-weight: 900; color: #1a1a1a; letter-spacing: -1px; }

/* 性别徽章：自适应宽度 */
.gender-badge {
  font-size: 10px; font-weight: 700; padding: 2px 6px;
  background: #000; color: #fff; border-radius: 4px;
  font-family: 'Noto Sans SC', sans-serif;
  white-space: nowrap;
}

.job-title { margin-top: 4px; font-size: 12px; color: #666; font-family: 'Noto Sans SC'; }
.job-title .highlight { color: #d35400; font-weight: bold; }

.bio-section { margin-bottom: 24px; }
.tags-group { display: flex; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.tech-tag {
  font-size: 11px; color: #333; background: rgba(0,0,0,0.06);
  padding: 4px 10px; border-radius: 12px; font-weight: 600; cursor: default;
}
.tech-tag:hover { background: #000; color: #fff; }
.bio-text { font-size: 14px; color: #555; line-height: 1.6; max-width: 90%; margin: 0; }

.contact-footer { display: flex; align-items: center; gap: 12px; font-size: 13px; color: #888; font-family: 'Roboto Mono', monospace; }
.divider { opacity: 0.3; }

/* 右侧头像 */
.card-content-right {
  flex: 1; display: flex; align-items: center; justify-content: center; position: relative; z-index: 1;
}
.avatar-container { position: relative; width: 140px; height: 140px; }
.big-avatar {
  width: 100%; height: 100%; border-radius: 50%; object-fit: cover;
  border: 4px solid #fff; box-shadow: 0 10px 25px rgba(0,0,0,0.1);
  position: relative; z-index: 2; background: #ddd;
}
.level-pill {
  position: absolute; bottom: 0; right: 50%; transform: translateX(50%);
  background: #000; color: #fff; padding: 4px 12px; border-radius: 20px;
  border: 2px solid #fff; display: flex; gap: 2px; z-index: 3;
  box-shadow: 0 4px 10px rgba(0,0,0,0.2);
}
.lv-label { font-size: 10px; opacity: 0.7; font-family: 'Roboto Mono'; }
.lv-val { font-size: 14px; font-weight: bold; font-family: 'Roboto Mono'; color: #f1c40f; }

.ripple-circle {
  position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);
  border-radius: 50%; border: 1px dashed rgba(0,0,0,0.1); z-index: 0;
}
.c1 { width: 160px; height: 160px; animation: spin 20s linear infinite; }
.c2 { width: 210px; height: 210px; border-color: rgba(0,0,0,0.05); animation: spin 30s linear infinite reverse; }
@keyframes spin { from { transform: translate(-50%, -50%) rotate(0deg); } to { transform: translate(-50%, -50%) rotate(360deg); } }

/* ================== 数据中心 ================== */
.data-card { flex: 0 0 auto; padding: 24px 40px; display: flex; flex-direction: column; justify-content: center; }
.data-header { display: flex; align-items: center; gap: 10px; margin-bottom: 20px; }
.section-title { font-size: 12px; font-weight: 700; color: #999; letter-spacing: 1px; }
.header-line { flex: 1; height: 1px; background: rgba(0,0,0,0.05); }

.stats-grid { display: flex; align-items: center; gap: 40px; z-index: 1; }
.stat-box { display: flex; flex-direction: column; gap: 4px; }
.stat-label { display: flex; align-items: baseline; gap: 6px; font-size: 12px; color: #666; }
.en { font-family: 'Roboto Mono'; font-size: 10px; opacity: 0.5; }
.stat-value { font-family: 'Roboto Mono', monospace; font-size: 24px; font-weight: 700; line-height: 1; }
.dark { color: #2c3e50; }
.gold { color: #d35400; }

.exp-box { flex: 1; display: flex; flex-direction: column; gap: 6px; }
.exp-info { display: flex; justify-content: space-between; font-size: 11px; color: #555; font-family: 'Roboto Mono'; }
.progress-track { width: 100%; height: 10px; background: rgba(0,0,0,0.05); border-radius: 5px; overflow: hidden; position: relative; }
.progress-bar { height: 100%; background: #000; border-radius: 5px; position: relative; transition: width 0.5s ease; }
.progress-glare { position: absolute; top: 0; left: 0; bottom: 0; right: 0; background: linear-gradient(90deg, transparent, rgba(255,255,255,0.2), transparent); }
.exp-percent-text { text-align: right; font-size: 10px; color: #999; font-family: 'Roboto Mono'; }
</style>