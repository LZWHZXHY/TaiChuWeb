<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import apiClient from '@/utils/api';

// 组件导入
import HeroCarousel from '@/MainPushComponent/HeroCarousel.vue';
import CyberTicker from '@/MainPushComponent/CyberTicker.vue';  
import PostCard from '@/MainPushComponent/cards/PostCard.vue';
import BlogCard from '@/MainPushComponent/cards/BlogCard.vue';
import ArtCard from '@/MainPushComponent/cards/ArtCard.vue';
import ActivityCard, { type ActivityItem } from '@/MainPushComponent/cards/ActivityCard.vue';
import OnlineNodesWidget from '@/GeneralComponents/OnlineNodesWidget.vue';

const router = useRouter();

// ==========================================
// --- 签到系统核心逻辑 (已适配后端驼峰) ---
// ==========================================
const isCheckedIn = ref(false); 
const checkInStreak = ref(0); 
const checkedDays = ref<number[]>([]); 
const retroCardsCount = ref(0); 

const today = computed(() => {
  const now = new Date();
  const days = ['SUNDAY', 'MONDAY', 'TUESDAY', 'WEDNESDAY', 'THURSDAY', 'FRIDAY', 'SATURDAY'];
  return {
    day: now.getDate().toString().padStart(2, '0'),
    rawDay: now.getDate(),
    month: now.toLocaleString('en-US', { month: 'short' }).toUpperCase(),
    year: now.getFullYear().toString(),
    weekday: days[now.getDay()]
  };
});

const calendarDays = computed(() => {
  const now = new Date();
  const year = now.getFullYear();
  const month = now.getMonth();
  const todayDate = now.getDate(); 
  
  const firstDay = new Date(year, month, 1).getDay();
  const totalDays = new Date(year, month + 1, 0).getDate();

  const days = [];
  for (let i = 0; i < firstDay; i++) {
    days.push({ day: null, isToday: false, isChecked: false });
  }
  for (let i = 1; i <= totalDays; i++) {
    const isToday = i === todayDate;
    const isChecked = checkedDays.value.includes(i);
    days.push({ day: i, isToday, isChecked });
  }
  return days;
});

const fetchCheckInStatus = async () => {
  try {
    const now = new Date();
    const { data: res } = await apiClient.get('/UserCheckIn/status', {
      params: { year: now.getFullYear(), month: now.getMonth() + 1 }
    });

    if (res && res.code === 200) {
      // 适配后端：isCheckedIn, checkedDays, streak, retroCardsCount
      isCheckedIn.value = res.data.isCheckedIn;
      checkedDays.value = res.data.checkedDays; 
      checkInStreak.value = res.data.streak;
      retroCardsCount.value = res.data.retroCardsCount;
    }
  } catch (error) {
    console.error('获取签到状态失败:', error);
  }
};

const handleCheckIn = async () => {
  if (isCheckedIn.value) return;
  try {
    const { data: res } = await apiClient.post('/UserCheckIn/checkin');
    if (res && res.code === 200) {
      alert(`${res.message}\n获得经验: ${res.rewardedExp}, 金币: ${res.rewardedCoins}`);
      await fetchCheckInStatus();
    } else {
      alert(res.message || '签到失败');
    }
  } catch (error: any) {
    alert(error.response?.data?.message || '服务器异常，签到失败');
  }
};

const handleDayClick = async (item: any) => {
  if (!item.day || item.isChecked || item.day >= today.value.rawDay) return;
  if (retroCardsCount.value <= 0) {
    alert('补签卡余额不足，无法修复此时空节点！');
    return;
  }

  const confirmMsg = `是否消耗 1 张补签卡，修复 ${today.value.year}年${today.value.month}月${item.day}日 的签到记录？`;
  if (confirm(confirmMsg)) {
    try {
      const now = new Date();
      const targetDate = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(item.day).padStart(2, '0')}`;
      const { data: res } = await apiClient.post('/UserCheckIn/retroactive', { targetDate });

      if (res && res.code === 200) {
        alert(`${res.message}\n获得经验: ${res.rewardedExp}, 金币: ${res.rewardedCoins}`);
        await fetchCheckInStatus();
      } else {
        alert(res.message || '补签失败');
      }
    } catch (error: any) {
      alert(error.response?.data?.message || '服务器异常，补签失败');
    }
  }
};

// --- 类型定义 (已适配驼峰) ---
interface FeedItem {
  id: string | number;
  type: 'post' | 'blog' | 'art';
  author: string;
  authorId: string;
  timestamp: string;
  title: string;
  content: string;
  imgUrl?: string;
  stats?: { views: number; likes: number; comments: number };
}

const currentOnlineCount = ref(0);
const handleUpdateCount = (count: number) => {
  currentOnlineCount.value = count;
};

const rawFeedData = ref<FeedItem[]>([]);
const trendingActivities = ref<ActivityItem[]>([]); 
const activeFeedbacks = ref<any[]>([]); 
const sideActivities = ref<any[]>([]); 
const isLoading = ref(true); 

// --- 1. 获取门户内容 (适配后端小驼峰字段) ---
const fetchPortalData = async () => {
  try {
    isLoading.value = true;
    const { data: res } = await apiClient.get(`/Feed/latest`, {
      params: { page: 1, limit: 30 }
    });

    if (res && res.code === 200) {
      // 关键改动：这里的 item.Id 改为 item.id，以此类推
      rawFeedData.value = res.data.map((item: any) => ({
        id: item.id,
        type: item.type?.toLowerCase(),
        author: item.author,
        authorId: item.authorId,
        timestamp: item.timestamp,
        title: item.title,
        content: item.content,
        imgUrl: item.imgUrl,
        stats: item.stats || { views: 0, likes: 0, comments: 0 }
      }));
    }
  } catch (error) {
    console.error('SERVER_ERROR // 门户数据加载失败:', error);
  } finally {
    isLoading.value = false;
  }
};

// --- 2. 获取共创企划 (适配 startDate/endDate) ---
const fetchActivities = async () => {
  try {
    const { data: res } = await apiClient.get(`/Activity/list`, {
      params: { page: 1, pageSize: 20 } 
    });

    if (res && res.data) {
      const now = new Date().getTime();
      const activeProjects = res.data.filter((item: any) => {
        // 如果后端是驼峰，这里应该是 startDate 和 endDate
        const s = item.startDate || item.startdate;
        const e = item.endDate || item.enddate;
        if (!s || !e) return false;
        const start = new Date(s).getTime();
        const end = new Date(e).getTime();
        return now >= start && now <= end;
      });
      trendingActivities.value = activeProjects.slice(0, 6);
    }
  } catch (error) {
    console.error('SERVER_ERROR // 企划数据加载失败:', error);
  }
};

// --- 3. 获取反馈动态 ---
const fetchActiveFeedbacks = async () => {
  try {
    const { data: res } = await apiClient.get(`/FeedBack/active-widget`);
    if (res && (res.success || res.code === 200)) {
      // 确保后端返回的是 res.data (小驼峰)
      activeFeedbacks.value = res.data;
    }
  } catch (error) {
    console.error('SERVER_ERROR // 节点反馈列队加载失败:', error);
  }
};

// --- 4. 获取日历活动 ---
const fetchSideActivities = async () => {
  try {
    const now = new Date();
    const { data: res } = await apiClient.get(`/events`, {
      params: { year: now.getFullYear(), month: now.getMonth() + 1 }
    });

    const rawList = Array.isArray(res) ? res : (res?.data || []);

    sideActivities.value = rawList.slice(0, 4).map((item: any) => {
      // 统一适配小驼峰
      const eDate = item.date;
      const eTime = item.time;
      const eId = item.id;
      const eTitle = item.title;
      const eType = item.type;
      const eColor = item.color;

      const eventDateTime = new Date(`${eDate}T${eTime}`);
      let statusText = '即将开始';
      let isActive = false;
      
      if (eventDateTime.getTime() < now.getTime()) {
        statusText = '已结束';
      } else if (eventDateTime.getTime() - now.getTime() < 172800000) { 
        statusText = '进行中';
        isActive = true;
      }

      const dateParts = eDate.split('-');
      const formattedDate = `${dateParts[1]}.${dateParts[2]} ${eTime}`;

      return {
        id: eId,
        title: eTitle,
        status: statusText,
        type: eType?.toUpperCase() || 'EVENT', 
        color: eColor,
        date: formattedDate,
        isActive: isActive
      };
    });
  } catch (error) {
    console.error('SERVER_ERROR // 官方日历活动加载失败:', error);
  }
};

const latestIntel = computed(() => rawFeedData.value.filter(item => item.type === 'blog').slice(0, 6));
const trendingArts = computed(() => rawFeedData.value.filter(item => item.type === 'art').slice(0, 6));
const hotPosts = computed(() => rawFeedData.value.filter(item => item.type === 'post').slice(0, 5));

const formatDate = (ds: string) => {
  if (!ds) return '';
  const d = new Date(ds);
  return `${d.getFullYear()}.${(d.getMonth() + 1).toString().padStart(2, '0')}.${d.getDate().toString().padStart(2, '0')}`;
};

const navigateTo = (path: string) => {
  router.push(path);
};

onMounted(() => {
  fetchPortalData();
  fetchActivities();
  fetchActiveFeedbacks(); 
  fetchSideActivities(); 
  fetchCheckInStatus(); 
});
</script>

<template>
  <div class="doc-wrapper">
    <div v-if="isLoading" class="global-loader-line"></div>

    <main class="main-content">
      
      <section class="portal-hero-section">
        <div class="hero-main-container">
          <HeroCarousel class="carousel-instance" />
        </div>
        
        <aside class="hero-side-panel calendar-mode">
          <div class="calendar-card">
            <div class="calendar-rings">
              <span></span><span></span><span></span>
            </div>
            
            <div class="calendar-inner">
              <div class="cal-header">
                <span class="cal-year">{{ today.year }}</span>
                <span class="cal-month">{{ today.month }}</span>
              </div>
              
              <div class="cal-body monthly-grid">
                <div class="cal-weekdays">
                  <span v-for="d in ['S','M','T','W','T','F','S']" :key="d">{{ d }}</span>
                </div>
                
                <div class="cal-days">
                  <div 
                    v-for="(item, index) in calendarDays" 
                    :key="index"
                    class="day-cell"
                    :class="{ 
                      'empty': !item.day, 
                      'is-today': item.isToday, 
                      'is-checked': item.isChecked,
                      'can-retro': item.day && !item.isChecked && item.day < today.rawDay && retroCardsCount > 0
                    }"
                    @click="handleDayClick(item)"
                  >
                    <span v-if="item.day">{{ item.day }}</span>
                  </div>
                </div>
              </div>
              
              <div class="cal-footer">
                <div class="streak-info">
                  <span class="label">STREAK</span>
                  <span class="value">{{ checkInStreak }}</span>
                  
                  <span class="label" style="margin-left: 12px;">RETRO CARDS</span>
                  <span class="value" :style="{ color: retroCardsCount > 0 ? '#ff4757' : '#999' }">{{ retroCardsCount }}</span>
                </div>
                <button 
                  class="checkin-btn" 
                  :class="{ 'checked': isCheckedIn }"
                  @click="handleCheckIn"
                >
                  <span v-if="!isCheckedIn">INITIALIZE_SYNC // 签到</span>
                  <span v-else>SYNC_COMPLETED // 已同步</span>
                </button>
              </div>
            </div>
          </div>

          <div class="node-status-bar">
            <div class="status-item">
              <span class="label">NODE_STATUS:</span>
              <span class="value active">ENCRYPTED</span>
            </div>
            <div class="status-item">
              <span class="label">REWARD:</span>
              <span class="value">+20 Coins</span>
            </div>
          </div>

          <div class="ticker-constrain">
            <CyberTicker />
          </div>
        </aside>

      </section>

      <div class="portal-body-layout">
        
        <div class="hub-main-column">
          
          <section class="hub-block">
            <header class="block-header">
              <div class="header-left">
                <span class="block-icon">◈</span>
                <h2 class="block-title">LATEST_INTEL // 最新情报</h2>
              </div>
              <button class="view-more-btn" @click="navigateTo('/blogs')">查看更多博客 →</button>
            </header>
            
            <div class="intel-grid">
              <div v-if="isLoading" class="skeleton-card" v-for="i in 6" :key="'intel-sk-'+i"></div>
              <article v-else v-for="item in latestIntel" :key="item.id" class="intel-card-wrapper">
                <BlogCard :data="item" />
              </article>
            </div>
          </section>

          <section class="hub-block">
            <header class="block-header">
              <div class="header-left">
                <span class="block-icon">⨁</span>
                <h2 class="block-title">USER_PROJECTS // 热门共创企划</h2>
              </div>
              <button class="view-more-btn" @click="navigateTo('/WorkCenter?tab=joint')">探索全部企划 →</button>
            </header>
            
            <div class="project-grid">
              <div v-if="isLoading && trendingActivities.length === 0" class="skeleton-card" style="height: 140px;" v-for="i in 6" :key="'proj-sk-'+i"></div>
              <ActivityCard 
                v-else 
                v-for="project in trendingActivities" 
                :key="project.id" 
                :data="project" 
              />
            </div>
          </section>

          <section class="hub-block">
            <header class="block-header">
              <div class="header-left">
                <span class="block-icon">❖</span>
                <h2 class="block-title">VISUAL_ARCHIVES // 热门视觉档案</h2>
              </div>
              <button class="view-more-btn" @click="navigateTo('/WorkCenter?tab=gallery')">进入画廊 →</button>
            </header>
            
            <div class="art-grid">
              <div v-if="isLoading" class="skeleton-card" v-for="i in 6" :key="'art-sk-'+i"></div>
              <article v-else v-for="item in trendingArts" :key="item.id" class="art-card-wrapper">
                <ArtCard :data="item" />
                <div class="art-mini-stats">
                  <span>{{ item.stats?.views }} 阅</span>
                  <span>{{ item.stats?.likes }} 赞</span>
                </div>
              </article>
            </div>
          </section>

          <section class="hub-block">
            <header class="block-header">
              <div class="header-left">
                <span class="block-icon">⟡</span>
                <h2 class="block-title">HOT_DISCUSSIONS // 高维讨论节点</h2>
              </div>
              <button class="view-more-btn" @click="navigateTo('/posts')">进入讨论区 →</button>
            </header>
            
            <div class="post-list">
              <div v-if="isLoading" class="skeleton-list-item" v-for="i in 5" :key="'post-sk-'+i"></div>
              <article v-else v-for="item in hotPosts" :key="item.id" class="post-item-wrapper">
                <div class="post-meta">
                  <span class="author-handle">@{{ item.author }}</span>
                  <span class="timestamp">{{ formatDate(item.timestamp) }}</span>
                </div>
                <div class="post-body">
                  <PostCard :data="item" />
                </div>
                <div class="post-stats">
                  <span class="stat-pill">👁️ {{ item.stats?.views }}</span>
                  <span class="stat-pill">💬 {{ item.stats?.comments }}</span>
                  <span class="stat-pill">🔥 {{ item.stats?.likes }}</span>
                </div>
              </article>
            </div>
          </section>

        </div>

        <aside class="hub-side-column">
          
          <OnlineNodesWidget @update-count="handleUpdateCount" />

          <div class="sidebar-widget events-widget">
            <h5 class="widget-title">OFFICIAL_EVENTS // 官方活动</h5>
            
            <ul class="event-list" v-if="sideActivities.length > 0">
              <li v-for="event in sideActivities" :key="event.id" class="event-item">
                <div class="event-status" 
                     :class="{ 'active': event.isActive }"
                     :style="event.isActive ? { backgroundColor: event.color || '#000', color: '#fff' } : {}">
                  [{{ event.type }}] {{ event.status }}
                </div>
                <h6 class="event-title">{{ event.title }}</h6>
                <span class="event-date">{{ event.date }}</span>
              </li>
            </ul>
            
            <div v-else style="padding: 20px 0; text-align: center; color: #999; font-family: 'JetBrains Mono'; font-size: 12px;">
              > 当月暂无同步行程...
            </div>
            
            <button class="full-width-btn" @click="navigateTo('/events')" style="margin-top: 15px;">全部活动历程</button>
          </div>

          <div class="sidebar-widget">
            <h5 class="widget-title">TRENDING_TAGS // 热门索引</h5>
            <div class="tag-cloud">
              <span class="tag-item"># 虚拟实境渲染</span>
              <span class="tag-item"># 寰宇OS_更新</span>
              <span class="tag-item"># 赛博主义插画</span>
              <span class="tag-item"># 火柴人设定</span>
              <span class="tag-item"># 灵脉空间</span>
            </div>
          </div>

          <div class="sidebar-widget feedback-widget">
            <h5 class="widget-title">SYSTEM_FEEDBACK // 节点反馈列队</h5>
            
            <ul class="feedback-list" v-if="activeFeedbacks.length > 0">
              <li v-for="item in activeFeedbacks" :key="item.id" class="feedback-item" :class="`border-${item.type}`">
                <div class="feedback-meta">
                  <span class="feedback-id">{{ item.id }}</span>
                  <span class="feedback-status" :class="item.type">[{{ item.status }}]</span>
                </div>
                <p class="feedback-text">{{ item.text }}</p>
              </li>
            </ul>

            <div v-else style="padding: 20px 0; text-align: center; color: #999; font-family: 'JetBrains Mono'; font-size: 12px;">
              > 暂无正在处理的节点异常...
            </div>
          </div>

          <div class="sidebar-widget sticky-terminal">
            <div class="terminal-header">CONSOLE // 系统终端</div>
            <div class="terminal-content">
              <p class="term-line">> Fetching global nodes...</p>
              <p class="term-line">> {{ currentOnlineCount }} nodes online.</p>
              <p class="term-line">> Routing system: Active</p>
              <p class="term-line pulse-line">_</p>
            </div>
          </div>

        </aside>
      </div>
    </main>

    <footer class="doc-footer">
      <div class="footer-inner">
        <p class="footer-main">支持太初寰宇！</p>
        <p class="footer-sub">© 2026 TAI CHU HUAN YU. ALL RIGHTS RESERVED.</p>
      </div>
    </footer>
  </div>
</template>

<style scoped>
/* --- 全局与重置 --- */
.doc-wrapper {
  background-color: #f8f9fa;
  color: #1a1a1a;
  min-height: 100vh;
  width: 100%;
  overflow-x: hidden; 
  position: relative;
  font-family: 'Inter', -apple-system, sans-serif;
}

.main-content {
  max-width: 65%;
  margin: 0 auto;
  padding: 40px 20px;
  box-sizing: border-box; 
}

/* --- Section 1: Hero --- */
.portal-hero-section {
  display: grid;
  grid-template-columns: 1.6fr 1fr;
  gap: 24px;
  height: 500px;
  margin-bottom: 50px;
  min-width: 0; 
}

.hero-main-container { background: #fff; border: 1px solid #ddd; overflow: hidden; min-width: 0; height: 100%; }
.carousel-instance { width: 100% !important; height: 100% !important; }

.hero-side-panel { 
  display: flex; 
  flex-direction: column; 
  background: #fff; 
  border: 1px solid #ddd; 
  padding: 20px; 
  box-shadow: 6px 6px 0px #eee; 
  min-width: 0; 
  height: 100%; 
  box-sizing: border-box; 
  min-height: 500px;
}
.calendar-card {
  width: 100%;
  background: #f8f9fa;
  border: 1px solid #000;
  position: relative;
  margin-top: 5px;
}

/* 模拟日历铁环 */
.calendar-rings {
  position: absolute;
  top: -12px;
  left: 0;
  width: 100%;
  display: flex;
  justify-content: space-around;
  padding: 0 40px;
}
.calendar-rings span {
  width: 8px;
  height: 24px;
  background: #333;
  border-radius: 4px;
  border: 1px solid #000;
}

/* --- 月历系统核心布局 --- */
.calendar-inner {
  padding: 16px; 
  display: flex;
  flex-direction: column;
  gap: 16px; /* 内部元素死死保持 16px 的距离 */
  width: 100%;
  box-sizing: border-box;
}

.cal-header {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 2px solid #000;
  padding-bottom: 8px;
  font-family: 'JetBrains Mono', monospace;
  font-weight: 800;
  font-size: 14px;
}
.monthly-grid {
  width: 50%;
}

.cal-weekdays {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  font-family: 'JetBrains Mono', monospace;
  font-size: 10px;
  font-weight: 700;
  color: #999;
  margin-bottom: 6px;
  text-align: center;
}

.cal-days {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 4px; 
  justify-items: center; /* 确保格子在列里居中 */
}

.day-cell {
  width: 100%; 
  max-width: 28px; 
  height: 28px;    
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: 'JetBrains Mono', monospace;
  font-size: 11px;
  border: 1px solid transparent;
  transition: all 0.2s ease;
  position: relative;
  box-sizing: border-box; 
}

/* 常规日期 */
.day-cell:not(.empty) { border: 1px solid #eee; background: #fff; color: #666; }
/* 今天（未签到） */
.day-cell.is-today { border-color: #0047ff; color: #0047ff; font-weight: 800; background: #f0f4ff; }
/* 已签到 */
.day-cell.is-checked { background: #64ce1d; border-color: #000; color: #fff; }
/* 今天且已签到 */
.day-cell.is-today.is-checked { background: #088908; border-color: #0047ff; color: #fff; }

.day-cell.is-checked::after {
  content: ''; position: absolute; top: 0; left: 0; width: 4px; height: 4px; background: rgba(255,255,255,0.5);
}

/* 🔥 新增：如果当前用户有补签卡，过去的漏签格子显示为红色虚线，支持点击补签 */
.day-cell.can-retro {
  cursor: pointer;
  border: 1px dashed #ff4757;
  color: #ff4757;
  background: #fff0f0;
}
.day-cell.can-retro:hover {
  background: #ff4757;
  color: #fff;
  border: 1px solid #ff4757;
}

/* --- 底部签到按钮 --- */
.cal-footer {
  width: 100%;
  margin-top: 8px;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.streak-info {
  display: flex;
  justify-content: center;
  align-items: baseline;
  gap: 8px;
  margin-bottom: 12px;
  font-family: 'JetBrains Mono', monospace;
}

.checkin-btn {
  width: 100%;
  box-sizing: border-box;
  padding: 10px;
  background: #000;
  color: #fff;
  border: none;
  font-family: 'JetBrains Mono', monospace;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s ease;
  clip-path: polygon(0 0, 100% 0, 100% calc(100% - 12px), calc(100% - 12px) 100%, 0 100%);
}

.checkin-btn:hover:not(.checked) { background: #0047ff; transform: translateY(-2px); }
.checkin-btn.checked { background: #eee; color: #999; cursor: default; }

/* 状态栏 & Ticker */
.node-status-bar {
  display: flex; 
  justify-content: space-between; 
  margin-top: 20px; /* 固定向上的间距 */
  padding: 10px; 
  border: 1px dashed #ddd;
}
.status-item { display: flex; flex-direction: column; }
.status-item .label { font-size: 9px; color: #999; font-family: 'JetBrains Mono', monospace; }
.status-item .value { font-size: 11px; font-weight: 700; }
.status-item .value.active { color: #00b894; }
.ticker-constrain { 
  width: 100%; 
  overflow: hidden; 
  margin-top: auto; 
}
/* --- 主体下半部分布局 --- */
.portal-body-layout {
  display: grid; grid-template-columns: 1fr 340px; gap: 60px; min-width: 0;
}
.hub-main-column { min-width: 0; display: flex; flex-direction: column; gap: 60px; }
.hub-side-column { min-width: 0; display: flex; flex-direction: column; gap: 30px; }

/* 通用板块标题 (Block Header) */
.block-header {
  display: flex; justify-content: space-between; align-items: flex-end;
  border-bottom: 2px solid #000; padding-bottom: 12px; margin-bottom: 24px;
}
.header-left { display: flex; align-items: center; gap: 10px; }
.block-icon { font-size: 20px; color: #0047ff; font-weight: bold;}
.block-title { font-family: 'JetBrains Mono', sans-serif; font-size: 18px; font-weight: 800; margin: 0; }
.view-more-btn { background: none; border: none; font-family: 'JetBrains Mono', monospace; font-size: 12px; font-weight: 700; color: #666; cursor: pointer; padding: 0; transition: color 0.2s; }
.view-more-btn:hover { color: #0047ff; }

/* 各类网格 */
.intel-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 20px; }
.intel-card-wrapper { background: #fff; border: 1px solid #eee; padding: 0; transition: transform 0.3s; }
.intel-card-wrapper:hover { transform: translateY(-4px); box-shadow: 0 8px 24px rgba(0,0,0,0.06); }

.project-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; }

.art-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 14px; }
.art-card-wrapper { background: #ffffff; border: 1px solid #eee; transition: transform 0.3s; position: relative; }
.art-card-wrapper:hover { transform: translateY(-4px); border-color: #000; box-shadow: 4px 4px 0 #000; }
.art-mini-stats { position: absolute; bottom: 10px; right: 10px; background: rgba(0,0,0,0.8); color: #fff; padding: 4px 8px; font-family: 'JetBrains Mono', monospace; font-size: 10px; border-radius: 4px; display: flex; gap: 10px; pointer-events: none; }

.post-list { display: flex; flex-direction: column; gap: 16px; }
.post-item-wrapper { background: #fff; border: 1px solid #eee; border-left: 4px solid #ddd; padding: 20px; transition: all 0.2s; display: flex; flex-direction: column; gap: 12px; }
.post-item-wrapper:hover { border-left-color: #0047ff; background: #fafafa; }
.post-meta { display: flex; justify-content: space-between; font-family: 'JetBrains Mono', monospace; font-size: 12px; }
.author-handle { font-weight: 700; color: #1a1a1a; }
.timestamp { color: #999; }
.post-body { margin-top: -10px; }
.post-stats { display: flex; gap: 10px; font-family: 'JetBrains Mono', monospace; font-size: 11px; }
.stat-pill { background: #f0f0f0; padding: 4px 8px; border-radius: 4px; color: #555; }

/* 侧边栏 Widget */
.sidebar-widget { background: #fff; padding: 24px; border: 1px solid #eee; width: 100%; box-sizing: border-box; }
.widget-title { font-family: 'JetBrains Mono', monospace; font-size: 13px; border-left: 3px solid #000; padding-left: 10px; margin-bottom: 20px; }

.event-list { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 16px; }
.event-item { border-bottom: 1px dashed #eee; padding-bottom: 16px; }
.event-item:last-child { border-bottom: none; padding-bottom: 0; }
.event-status { display: inline-block; font-family: 'JetBrains Mono', monospace; font-size: 10px; padding: 3px 6px; background: #eee; color: #666; margin-bottom: 8px; border-radius: 2px; }
.event-status.active { font-weight: bold; }
.event-title { font-size: 14px; font-weight: 700; margin: 0 0 6px 0; line-height: 1.4; color: #1a1a1a; }
.event-date { font-family: 'JetBrains Mono', monospace; font-size: 11px; color: #999; }
.full-width-btn { width: 100%; background: #f8f9fa; border: 1px solid #ddd; padding: 10px; font-family: 'JetBrains Mono', monospace; font-size: 12px; cursor: pointer; transition: 0.2s; }
.full-width-btn:hover { background: #000; color: #fff; border-color: #000; }

.tag-cloud { display: flex; flex-wrap: wrap; gap: 8px; }
.tag-item { background: #f0f2f5; padding: 6px 12px; font-size: 12px; color: #444; cursor: pointer; transition: 0.2s; }
.tag-item:hover { background: #000; color: #fff; }

.sticky-terminal { background: #1a1a1a; color: #00ff41; padding: 20px; font-family: 'JetBrains Mono', monospace; font-size: 11px; }
.terminal-header { border-bottom: 1px solid #333; padding-bottom: 8px; margin-bottom: 12px; color: #888; }
.term-line { margin: 4px 0; }
.pulse-line { animation: blink 1s infinite; }

.feedback-widget { background: #fff; }
.feedback-list { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 12px; }
.feedback-item { background: #f8f9fa; padding: 12px; border-left: 3px solid #ccc; transition: all 0.2s ease; }
.feedback-item:hover { background: #f0f2f5; transform: translateX(4px); }
.feedback-item.border-processing { border-left-color: #0047ff; }
.feedback-item.border-investigating { border-left-color: #ff4757; }
.feedback-item.border-pending { border-left-color: #a4b0be; }

.feedback-meta { display: flex; justify-content: space-between; margin-bottom: 6px; font-family: 'JetBrains Mono', monospace; font-size: 11px; font-weight: 700; }
.feedback-id { color: #666; }
.feedback-status.processing { color: #0047ff; }
.feedback-status.investigating { color: #ff4757; }
.feedback-status.pending { color: #a4b0be; }
.feedback-text { font-size: 12px; color: #1a1a1a; margin: 0; line-height: 1.4; font-family: 'Inter', -apple-system, sans-serif; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }

/* 骨架屏与加载条 */
.global-loader-line { position: fixed; top: 0; left: 0; height: 2px; background: #000; width: 100%; z-index: 9999; }
.skeleton-card { background: #eee; animation: pulse 1.5s infinite; }
.intel-grid .skeleton-card, .art-grid .skeleton-card { height: 200px; }
.skeleton-list-item { height: 80px; background: #eee; margin-bottom: 16px; animation: pulse 1.5s infinite; }

/* Footer */
.doc-footer { border-top: 1px solid #ddd; padding: 60px 0; text-align: center; background: #fff; width: 100%; margin-top: 60px; }
.footer-main { font-weight: 700; margin-bottom: 8px; }
.footer-sub { font-size: 12px; color: #999; font-family: 'JetBrains Mono', monospace; }

@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
@keyframes pulse { 0% { opacity: 0.6; } 50% { opacity: 1; } 100% { opacity: 0.6; } }

/* 响应式 */
@media (max-width: 1100px) {
  .portal-hero-section { grid-template-columns: 1fr; height: auto; }
  .portal-body-layout { grid-template-columns: 1fr; }
  .intel-grid, .art-grid { grid-template-columns: 1fr; } 
  .project-grid { grid-template-columns: repeat(2, 1fr); }
}
@media (max-width: 768px) {
  .project-grid { grid-template-columns: 1fr; }
  .main-content{
    max-width: 90%;
  }
}






</style>