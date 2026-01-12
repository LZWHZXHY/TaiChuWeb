<template>
  <div class="panel-card md-elevation-1 activity-panel">
    <div class="card-header rank-header">
      <div class="header-left">
        <span class="header-label">ACTIVITY_INDEX // 活跃指数</span>
        <span class="pulse-orange"></span>
      </div>
      <div class="my-rank" v-if="myRankInfo">
        <span class="label">ME:</span>
        <span class="val">NO.{{ myRankInfo.rank }}</span>
      </div>
    </div>

    <div class="rank-list custom-scroll">
      <div v-if="loading" class="loading-state">
        <i class="fas fa-circle-notch fa-spin"></i> CALCULATING...
      </div>

      <div v-else-if="list.length === 0" class="empty-state">
        暂无活跃数据
      </div>
      
      <div v-else v-for="(user, index) in list" :key="user.id" class="rank-item">
        <div class="rank-num">
          <i v-if="index === 0" class="fas fa-crown gold"></i>
          <i v-else-if="index === 1" class="fas fa-shield-alt silver"></i>
          <i v-else-if="index === 2" class="fas fa-shield-alt bronze"></i>
          <span v-else class="normal-num">{{ index + 1 }}</span>
        </div>

        <div class="rank-avatar">
          <img :src="fixAvatarUrl(user.avatar)" @error="handleImgError" />
          <div class="status-dot"></div>
        </div>

        <div class="rank-info">
          <div class="info-top">
            <span class="r-name">{{ user.username }}</span>
            <span class="r-level">Lv.{{ user.level || 1 }}</span>
          </div>
          <div class="r-bar-bg">
            <div class="r-bar-fill" :style="{ width: getBarWidth(user.score) + '%' }"></div>
          </div>
        </div>

        <div class="rank-score">
          <span class="score-val">{{ formatNumber(user.score) }}</span>
          <span class="score-unit">AP</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import apiClient from '@/utils/api';
import { useI18n } from 'vue-i18n';

const { t } = useI18n(); // 如果没有配置多语言，这里不会报错，模板里用了 || '默认值'

const loading = ref(true);
const list = ref([]);
const myRankInfo = ref(null);
let refreshTimer = null;

// 开发环境/生产环境 URL 处理
const isDev = window.location.hostname === 'localhost';
const BASE_URL = isDev ? 'https://localhost:44359' : 'https://bianyuzhou.com';

const fixAvatarUrl = (url) => {
  if (!url) return '/土豆.jpg';
  if (url.startsWith('http')) return url;
  let path = url.replace(/\\/g, '/');
  if (path.startsWith('/')) path = path.substring(1);
  return `${BASE_URL}/${path.startsWith('uploads/') ? path : 'uploads/' + path}`;
};

const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };

// 数字格式化 (例如 1200 -> 1.2k)
const formatNumber = (num) => {
  if (num >= 10000) return (num / 10000).toFixed(1) + 'w';
  if (num >= 1000) return (num / 1000).toFixed(1) + 'k';
  return num;
};

// 计算能量条宽度 (相对于第一名的比例)
const getBarWidth = (score) => {
  if (list.value.length === 0) return 0;
  const maxScore = list.value[0].score;
  return maxScore > 0 ? Math.min(100, (score / maxScore) * 100) : 0;
};

// 获取排行榜数据
const fetchRanking = async () => {
  try {
    // 真实 API 调用
    const res = await apiClient.get('/OnlineStats/activity-rank');
    if (res.data.success) {
      list.value = res.data.list;
      myRankInfo.value = res.data.myRank;
    }
  } catch (e) {
    console.error("活跃榜获取失败，加载模拟数据", e);
    // 模拟数据 (后端没写好前可以用这个看效果)
    list.value = [
      { id: 1, username: '陈天', avatar: null, level: 5, score: 8540 },
      { id: 2, username: 'Admin', avatar: null, level: 99, score: 6200 },
      { id: 3, username: 'Neo', avatar: null, level: 3, score: 3150 },
      { id: 4, username: 'Trinity', avatar: null, level: 4, score: 1200 },
      { id: 5, username: 'Guest_01', avatar: null, level: 1, score: 50 },
    ];
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  fetchRanking();
  // 每 30 秒刷新一次排行榜，让用户看到分数实时变化
  refreshTimer = setInterval(fetchRanking, 30000);
});

onUnmounted(() => {
  if (refreshTimer) clearInterval(refreshTimer);
});
</script>

<style scoped>
.activity-panel {
  display: flex;
  flex-direction: column;
  height: 320px; /* 固定高度或者 min-height */
  background: #fff;
  overflow: hidden;
  margin-bottom: 20px;
}

.rank-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  border-bottom: 1px solid #f3f4f6;
  background: linear-gradient(to right, #fff, #fff);
}

.header-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.header-label {
  font-size: 12px;
  font-weight: 800;
  color: #4b5563;
  letter-spacing: 1px;
}

/* 呼吸灯效果 */
.pulse-orange {
  width: 6px;
  height: 6px;
  background-color: #f59e0b;
  border-radius: 50%;
  box-shadow: 0 0 0 0 rgba(245, 158, 11, 0.7);
  animation: pulse-orange 2s infinite;
}

.my-rank {
  font-size: 10px;
  background: #fef3c7;
  color: #d97706;
  padding: 2px 8px;
  border-radius: 12px;
  font-weight: 700;
}

.rank-list {
  flex: 1;
  overflow-y: auto;
  padding: 10px 16px;
}

.rank-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 0;
  border-bottom: 1px solid #f9fafb;
  transition: background-color 0.2s;
}
.rank-item:last-child { border-bottom: none; }
.rank-item:hover { background-color: #fcfcfc; }

/* 排名数字/图标 */
.rank-num {
  width: 24px;
  text-align: center;
  display: flex;
  justify-content: center;
}
.gold { color: #f59e0b; font-size: 16px; filter: drop-shadow(0 2px 4px rgba(245,158,11,0.3)); }
.silver { color: #94a3b8; font-size: 14px; }
.bronze { color: #b45309; font-size: 14px; }
.normal-num { font-size: 12px; font-weight: 700; color: #9ca3af; font-family: monospace; }

/* 头像 */
.rank-avatar {
  position: relative;
  width: 36px; height: 36px;
  flex-shrink: 0;
}
.rank-avatar img {
  width: 100%; height: 100%;
  border-radius: 50%;
  object-fit: cover;
  border: 2px solid #fff;
  box-shadow: 0 2px 5px rgba(0,0,0,0.08);
}

/* 名字和进度条 */
.rank-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 0; /* 防止文本溢出 */
}

.info-top {
  display: flex;
  align-items: center;
  gap: 6px;
}

.r-name {
  font-size: 13px;
  font-weight: 600;
  color: #1f2937;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.r-level {
  font-size: 9px;
  background: #f3f4f6;
  color: #6b7280;
  padding: 1px 4px;
  border-radius: 4px;
  font-weight: 700;
}

.r-bar-bg {
  height: 4px;
  background: #f3f4f6;
  border-radius: 2px;
  width: 100%;
  overflow: hidden;
}

.r-bar-fill {
  height: 100%;
  background: linear-gradient(90deg, #fcd34d 0%, #f59e0b 100%);
  border-radius: 2px;
  transition: width 0.5s ease;
}

/* 分数 */
.rank-score {
  text-align: right;
  min-width: 45px;
}

.score-val {
  display: block;
  font-size: 13px;
  font-weight: 800;
  color: #f59e0b;
  font-family: 'Inter', monospace;
  line-height: 1;
}

.score-unit {
  font-size: 8px;
  color: #9ca3af;
  font-weight: 600;
}

.loading-state, .empty-state {
  text-align: center;
  padding: 40px 0;
  color: #9ca3af;
  font-size: 12px;
}

@keyframes pulse-orange {
  0% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(245, 158, 11, 0.7); }
  70% { transform: scale(1); box-shadow: 0 0 0 6px rgba(245, 158, 11, 0); }
  100% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(245, 158, 11, 0); }
}

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #e5e7eb; border-radius: 4px; }
</style>