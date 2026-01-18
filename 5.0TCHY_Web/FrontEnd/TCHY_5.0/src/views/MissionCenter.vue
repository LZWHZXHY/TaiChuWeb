<template>
  <div class="guild-hall-container">
    <div class="grid-bg moving-grid"></div>

    <header class="guild-header">
      <div class="header-content">
        <div class="title-group">
          <h1 class="glitch-title">
            <span class="text-row">GUILD_HALL</span>
            <span class="text-row outline">委托中心</span>
          </h1>
        </div>
        
        <div class="user-badge">
          <div class="avatar-frame">
            <img :src="myUser.avatar" />
          </div>
          <div class="info-col">
            <div class="name-row">
              <span class="name">{{ myUser.name }}</span>
              <span class="title-tag">[{{ userStatus.title }}]</span>
            </div>
            
            <div class="status-row">
              <span class="rank">LV.{{ userStatus.level }}</span>
              <span class="gold">💰 {{ userStatus.points }} G</span>
            </div>

            <div class="exp-bar-box">
              <div 
                class="exp-fill" 
                :style="{ width: userStatus.expPercent + '%' }"
              ></div>
            </div>
            <span class="exp-num">{{ userStatus.currentExp }} / {{ userStatus.nextLevelExp }} XP</span>
          </div>
        </div>
      </div>
      
      <div class="alert-strip">
        <div class="strip-content">
          ⚠️ [URGENT] NEW SSS-RANK BOUNTY POSTED: "THE_VOID_INCIDENT" // HIGH LETHALITY // AUTHORIZED PERSONNEL ONLY //
        </div>
      </div>
    </header>

    <div class="mission-grid custom-scroll">
      
      <div 
        v-for="task in missions" 
        :key="task.id" 
        class="mission-card"
        :class="[`rank-${task.rank.toLowerCase()}`, getStatusClass(task)]"
      >
        <div class="bg-watermark">{{ task.rank }}</div>

        <div class="card-top">
          <div class="rank-badge">{{ task.rank }}</div>
          <div class="reward-box">
            <span class="label">BOUNTY:</span>
            <span class="val">{{ task.reward.toLocaleString() }} G</span>
          </div>
        </div>

        <div class="card-body">
          <div class="id-row">
            <span class="id-num">#REQ_{{ task.id }}</span>
            <span class="type-tag">{{ task.type }}</span>
          </div>
          <h3 class="title">{{ task.title }}</h3>
          <p class="desc">{{ task.desc }}</p>
          
          <div class="client-row">
            <span class="client-label">CLIENT:</span>
            <span class="client-name">[ {{ task.publisher }} ]</span>
          </div>
        </div>

        <div v-if="task.status === 1 && !isMyTask(task)" class="sealed-overlay">
          <div class="seal-stamp">
            <div class="seal-inner">CONTRACT<br>SEALED</div>
          </div>
          <div class="hunter-info">
            <span>ACCEPTED BY:</span>
            <div class="hunter-row">
              <img :src="task.assignee.avatar" />
              <span>{{ task.assignee.name }}</span>
            </div>
          </div>
        </div>

        <div class="card-footer">
          <button 
            v-if="task.status === 0" 
            class="action-btn accept-btn"
            @click="handleAccept(task)"
          >
            SIGN CONTRACT [ 接取 ]
          </button>

          <button 
            v-else-if="isMyTask(task) && task.status === 1" 
            class="action-btn submit-btn"
            @click="openSubmitModal(task)"
          >
            REPORT MISSION [ 提交 ]
          </button>

          <div v-else-if="isMyTask(task) && task.status === 2" class="status-bar auditing">
            /// AWAITING APPROVAL ///
          </div>

          <div v-else-if="task.status === 3" class="status-bar completed">
            /// MISSION ACCOMPLISHED ///
          </div>

          <button 
            v-if="isMyPublishedTask(task) && task.status === 2"
            class="action-btn admin-btn"
            @click="handleApprove(task)"
          >
            [ADMIN] APPROVE // 批准
          </button>
        </div>
      </div>

    </div>

    <Teleport to="body">
      <div v-if="showModal" class="modal-overlay">
        <div class="guild-modal">
          <div class="modal-header">
            <span>MISSION REPORT // ID: {{ currentTask?.id }}</span>
            <button class="close-btn" @click="showModal = false">×</button>
          </div>
          <div class="modal-body">
            <p class="instruction">请提交任务完成证明 (Github / 截图 / 日志):</p>
            <textarea 
              v-model="submitContent" 
              class="cyber-textarea custom-scroll" 
              placeholder="INPUT_DATA..."
            ></textarea>
            <div class="modal-actions">
              <button class="cancel-btn" @click="showModal = false">DISCARD</button>
              <button class="confirm-btn" @click="confirmSubmit">UPLOAD DATA</button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import apiClient from '@/utils/api';
import { useAuthStore } from '@/utils/auth';

const authStore = useAuthStore();
const missions = ref([]);
const showModal = ref(false);
const currentTask = ref(null);
const submitContent = ref('');
const loading = ref(false);

// 🔥 用户资产状态
const userStatus = ref({
  level: 1,
  currentExp: 0,
  nextLevelExp: 100,
  gold: 0,
  reputation: 100,
  title: 'Loading...',
  expPercent: 0
});

// 当前登录用户信息
const myUser = computed(() => {
  return {
    id: authStore.user?.id,
    name: authStore.user?.username || 'GUEST',
    avatar: fixAvatar(authStore.user?.avatar)
  };
});

// --- API 1: 获取用户资产 ---
// --- API 1: 获取用户资产 ---
const fetchUserStatus = async () => {
  try {
    // 确保你的后端 Controller 路由是 api/Profile/me 还是 api/UserStatus/me
    // 根据你上一步发的后端代码，应该是 '/Profile/me'
    const res = await apiClient.get('/Profile/me'); 
    
    if(res.data.success) {
      const serverData = res.data.data;
      
      // 🔥 核心修复：手动映射大小写 🔥
      userStatus.value = {
        level: serverData.Level,            // 后端 Level -> 前端 level
        currentExp: serverData.CurrentExp,  // 后端 CurrentExp -> 前端 currentExp
        nextLevelExp: serverData.NextLevelExp,
        points: serverData.Points,
        reputation: serverData.Reputation,
        title: serverData.Title,
        expPercent: serverData.ExpPercent
      };
    }
  } catch(e) {
    console.error("获取资产失败", e);
  }
};

// --- API 2: 获取任务列表 ---
const fetchMissions = async () => {
  loading.value = true;
  try {
    const res = await apiClient.get('/Mission');
    if(res.data.success) {
      // 后端 PascalCase -> 前端 camelCase 映射
      missions.value = res.data.data.map(task => ({
        id: task.Id,
        title: task.Title,
        desc: task.Description,
        rank: task.Rank || 'D',
        type: task.Type || 'NORMAL',
        reward: task.Reward || 0,
        status: task.Status,
        publisher: task.Publisher, // 这是一个字符串(名字)
        
        assignee: task.Assignee ? {
          id: task.Assignee.Id,
          name: task.Assignee.Name,
          avatar: fixAvatar(task.Assignee.Avatar)
        } : null
      }));
    }
  } catch(e) {
    console.error("获取任务失败", e);
  } finally {
    loading.value = false;
  }
};

// --- 辅助逻辑 ---
const fixAvatar = (url) => {
  if (!url) return '/土豆.jpg';
  if (url.startsWith('http')) return url;
  const BASE_URL = 'https://bianyuzhou.com'; 
  return `${BASE_URL}/uploads/${url.startsWith('/') ? url.substring(1) : url}`;
};

const isMyTask = (task) => {
  return task.assignee && task.assignee.id === myUser.value.id;
};

// 判断是否是我发布的任务 (用于测试审核)
const isMyPublishedTask = (task) => {
  // 注意：真实场景应该用 ID 对比 (task.publisherId === myUser.value.id)
  // 这里暂时用名字对比作为测试
  return task.publisher === myUser.value.name;
};

const getStatusClass = (task) => {
  if (task.status === 1 && !isMyTask(task)) return 'card-sealed'; 
  return '';
};

// --- 交互动作 ---

// 接取任务
const handleAccept = async (task) => {
  if(!confirm(`[工会系统] 确认承接 ${task.rank} 级任务？\n高难度任务失败可能会降低信誉分。`)) return;
  
  try {
    const res = await apiClient.post(`/Mission/accept/${task.id}`);
    if(res.data.success) {
      alert("接取成功！请尽快完成委托。");
      fetchMissions(); // 刷新列表
    }
  } catch(e) {
    alert(e.response?.data || "抢单失败，可能已被他人接取。");
    fetchMissions(); 
  }
};

// 打开提交弹窗
const openSubmitModal = (task) => {
  currentTask.value = task;
  submitContent.value = '';
  showModal.value = true;
};

// 提交任务证明
const confirmSubmit = async () => {
  if (!submitContent.value.trim()) return alert("报告内容不能为空");
  
  try {
    const res = await apiClient.post(`/Mission/submit/${currentTask.value.id}`, {
      content: submitContent.value
    });
    
    if(res.data.success) {
      alert("报告已上传，等待公会审核。");
      showModal.value = false;
      fetchMissions(); 
    }
  } catch(e) {
    alert("提交失败: " + (e.response?.data || "未知错误"));
  }
};

// 🔥 [测试用] 审核通过
const handleApprove = async (task) => {
  if(!confirm("确认审核通过？这将直接给对方发钱！")) return;
  
  try {
    const res = await apiClient.post(`/Mission/approve/${task.id}`);
    if(res.data.success) {
      alert(res.data.message);
      // 两个都要刷新，才能看到钱涨了，状态也变了
      fetchMissions();
      fetchUserStatus();
    }
  } catch(e) {
    alert(e.response?.data || "审核失败");
  }
};

onMounted(() => {
  fetchMissions();
  if(authStore.isAuthenticated) {
    fetchUserStatus(); // 获取钱和等级
  }
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;800&display=swap');

/* --- 核心容器 --- */
.guild-hall-container {
  --bg: #F4F1EA;
  --black: #111111;
  --text-main: #111;
  --card-bg: #fff;
  
  /* 等级色谱 */
  --r-sss: #FF00FF; --r-ss: #FFD700; --r-s: #D92323; --r-a: #FF6600; 
  --r-b: #9D00FF; --r-c: #0099FF; --r-d: #00CC66;

  width: 100%; height: 100%;
  background-color: var(--bg);
  display: flex; flex-direction: column;
  font-family: 'JetBrains Mono', monospace;
  color: var(--text-main);
  position: relative; overflow: hidden;
}

/* 背景网格 */
.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(#ccc 1px, transparent 1px), linear-gradient(90deg, #ccc 1px, transparent 1px); 
  background-size: 50px 50px; opacity: 0.4; pointer-events: none; 
}
.moving-grid { animation: gridScroll 60s linear infinite; }

/* --- 1. 头部 --- */
.guild-header {
  flex-shrink: 0; background: var(--black); border-bottom: 4px solid var(--black);
}
.header-content {
  padding: 20px 40px; display: flex; justify-content: space-between; align-items: center;
  color: #fff;
}
.glitch-title { font-family: 'Anton'; font-size: 3rem; margin: 0; line-height: 0.9; text-transform: uppercase; }
.text-row.outline { -webkit-text-stroke: 1px #fff; color: transparent; display: block; font-size: 2rem; }

/* 用户信息栏样式更新 */
.user-badge { display: flex; align-items: center; gap: 15px; border: 2px solid #fff; padding: 10px 20px; background: #000; box-shadow: 4px 4px 0 rgba(255,255,255,0.2); min-width: 260px; }
.avatar-frame { width: 50px; height: 50px; border: 2px solid #fff; overflow: hidden; flex-shrink: 0; }
.avatar-frame img { width: 100%; height: 100%; object-fit: cover; }
.info-col { display: flex; flex-direction: column; width: 100%; }

.name-row { display: flex; align-items: center; gap: 8px; font-weight: bold; font-size: 0.9rem; }
.title-tag { font-size: 0.7rem; color: #FFD700; border: 1px solid #FFD700; padding: 0 4px; border-radius: 4px; }

.status-row { display: flex; justify-content: space-between; font-size: 0.8rem; margin-top: 4px; color: #ccc; }
.gold { color: #FFD700; text-shadow: 0 0 5px rgba(255, 215, 0, 0.3); }

/* 经验条 */
.exp-bar-box { width: 100%; height: 6px; background: #333; margin-top: 4px; border-radius: 3px; overflow: hidden; border: 1px solid #444; }
.exp-fill { height: 100%; background: #00CC66; box-shadow: 0 0 8px #00CC66; transition: width 0.5s ease; }
.exp-num { font-size: 0.6rem; color: #666; display: block; text-align: right; margin-top: 2px; }

.alert-strip { background: var(--black); border-top: 1px solid #333; color: var(--r-s); font-weight: bold; font-size: 0.8rem; padding: 4px 0; overflow: hidden; white-space: nowrap; }
.strip-content { display: inline-block; animation: marquee 30s linear infinite; }

/* --- 2. 任务网格 --- */
.mission-grid {
  flex: 1; padding: 40px; overflow-y: auto;
  display: grid; 
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); 
  gap: 30px; align-content: start;
}

/* 卡片样式 */
.mission-card {
  background: var(--card-bg); 
  border: 2px solid var(--black);
  position: relative; display: flex; flex-direction: column;
  height: 420px; transition: all 0.25s cubic-bezier(0.25, 0.8, 0.25, 1);
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0,0,0,0.12);
}
.mission-card:hover { transform: translateY(-8px); box-shadow: 0 14px 28px rgba(0,0,0,0.25); z-index: 5; }

.bg-watermark { position: absolute; top: 20px; right: 10px; font-family: 'Anton'; font-size: 9rem; opacity: 0.08; pointer-events: none; z-index: 0; line-height: 1; color: var(--black); }

/* 等级颜色 */
.mission-card.rank-sss { border-color: var(--r-sss); box-shadow: 0 0 15px rgba(255, 0, 255, 0.4); }
.mission-card.rank-sss .rank-badge { background: linear-gradient(135deg, var(--r-sss), #FFF); color: #000; }

.mission-card.rank-ss { border-color: var(--r-ss); }
.mission-card.rank-ss .rank-badge { background: var(--r-ss); color: #000; }

.mission-card.rank-s { border-color: var(--r-s); }
.mission-card.rank-s .rank-badge { background: var(--r-s); color: #fff; }

.mission-card.rank-a { border-color: var(--r-a); }
.mission-card.rank-a .rank-badge { background: var(--r-a); color: #000; }

.mission-card.rank-b { border-color: var(--r-b); }
.mission-card.rank-b .rank-badge { background: var(--r-b); color: #fff; }

.mission-card.rank-c { border-color: var(--r-c); }
.mission-card.rank-c .rank-badge { background: var(--r-c); color: #000; }

.mission-card.rank-d { border-color: var(--r-d); }
.mission-card.rank-d .rank-badge { background: var(--r-d); color: #000; }

.card-top { display: flex; justify-content: space-between; padding: 15px; border-bottom: 2px solid var(--black); background: rgba(0,0,0,0.02); z-index: 1; }
.rank-badge { padding: 4px 12px; font-weight: 900; font-family: 'Anton'; letter-spacing: 1px; font-size: 1.4rem; box-shadow: 2px 2px 0 rgba(0,0,0,0.2); }
.reward-box { display: flex; align-items: center; gap: 5px; background: var(--black); color: #fff; padding: 4px 8px; font-weight: bold; }
.reward-box .label { color: #ccc; font-size: 0.7rem; }

.card-body { flex: 1; padding: 20px; z-index: 1; display: flex; flex-direction: column; }
.id-row { font-size: 0.8rem; color: #666; margin-bottom: 10px; display: flex; justify-content: space-between; align-items: center; }
.type-tag { border: 1px solid #666; padding: 2px 6px; font-size: 0.65rem; border-radius: 4px; }
.title { font-family: 'Anton'; font-size: 1.6rem; margin: 0 0 15px 0; line-height: 1.1; text-transform: uppercase; color: var(--black); }
.desc { font-size: 0.9rem; color: #444; line-height: 1.5; flex: 1; }
.client-row { margin-top: 15px; font-size: 0.75rem; color: #555; border-top: 1px dashed #ccc; padding-top: 10px; font-weight: bold; }

/* 封条 */
.sealed-overlay { position: absolute; inset: 0; z-index: 10; background: rgba(255,255,255,0.85); backdrop-filter: blur(2px) grayscale(100%); display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 20px; }
.seal-stamp { border: 5px solid var(--r-s); padding: 10px; transform: rotate(-15deg); box-shadow: 0 0 0 2px #fff, 0 0 0 4px var(--r-s); }
.seal-inner { font-family: 'Anton'; font-size: 2.2rem; color: var(--r-s); text-align: center; line-height: 1; border: 2px solid var(--r-s); padding: 5px 20px; }
.hunter-info { display: flex; flex-direction: column; align-items: center; color: var(--black); font-size: 0.8rem; font-weight: bold; }
.hunter-row { display: flex; align-items: center; gap: 10px; margin-top: 5px; background: var(--black); color: #fff; padding: 5px 15px; }
.hunter-row img { width: 25px; height: 25px; object-fit: cover; border: 1px solid #fff; }

/* 底部操作 */
.card-footer { padding: 15px; border-top: 2px solid var(--black); z-index: 1; background: #fff; }

.action-btn { width: 100%; padding: 12px; font-weight: bold; cursor: pointer; border: 2px solid var(--black); font-family: 'JetBrains Mono'; text-transform: uppercase; box-shadow: 4px 4px 0 var(--black); background: #fff; color: var(--black); }
.action-btn:hover { transform: translate(-2px, -2px); box-shadow: 6px 6px 0 var(--black); }
.action-btn:active { transform: translate(2px, 2px); box-shadow: 2px 2px 0 var(--black); }

.admin-btn { background: #000; color: #fff; border-color: #fff; margin-top: 10px; }
.admin-btn:hover { background: #fff; color: #000; }

.accept-btn:hover { background: var(--black); color: #fff; }
.submit-btn { background: var(--r-ss); border-color: var(--black); }

.status-bar { width: 100%; padding: 12px; text-align: center; font-weight: bold; font-size: 0.9rem; border: 2px dashed #999; color: #666; }
.status-bar.auditing { color: var(--r-c); border-color: var(--r-c); background: rgba(0, 153, 255, 0.05); }
.status-bar.completed { color: var(--r-d); border-color: var(--r-d); background: rgba(0, 204, 102, 0.05); }

/* 弹窗 */
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.85); z-index: 999; display: flex; align-items: center; justify-content: center; backdrop-filter: blur(4px); }
.guild-modal { width: 600px; background: #fff; border: 4px solid var(--black); box-shadow: 15px 15px 0 rgba(0,0,0,0.5); display: flex; flex-direction: column; }
.modal-header { background: var(--black); color: #fff; padding: 15px 20px; font-weight: bold; display: flex; justify-content: space-between; }
.close-btn { background: none; border: 1px solid #fff; color: #fff; font-size: 1.2rem; cursor: pointer; padding: 0 8px; }
.close-btn:hover { background: var(--r-s); border-color: var(--r-s); }
.modal-body { padding: 30px; }
.instruction { color: #333; margin-bottom: 15px; font-weight: bold; }
.cyber-textarea { width: 100%; height: 180px; background: #fafafa; border: 2px solid var(--black); color: #000; padding: 15px; font-family: 'JetBrains Mono'; margin-bottom: 25px; outline: none; resize: none; font-size: 1rem; }
.modal-actions { display: flex; justify-content: flex-end; gap: 15px; }
.cancel-btn, .confirm-btn { padding: 12px 25px; border: 2px solid var(--black); cursor: pointer; font-weight: bold; box-shadow: 4px 4px 0 var(--black); }
.cancel-btn { background: #ccc; }
.confirm-btn { background: var(--r-ss); }

@keyframes marquee { 0% { transform: translateX(100%); } 100% { transform: translateX(-100%); } }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-40px); } }
@keyframes border-pulse { 0% { border-color: var(--r-sss); } 50% { border-color: #fff; } 100% { border-color: var(--r-sss); } }

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 8px; }
.custom-scroll::-webkit-scrollbar-track { background: #eee; border-left: 2px solid var(--black); }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); border: 1px solid #fff; }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: var(--r-s); }
</style>