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
            <span class="name">{{ myUser.name }}</span>
            <span class="rank">HUNTER_RANK: <span class="highlight">S</span></span>
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
import { ref } from 'vue';

// --- 模拟当前用户 ---
const myUser = {
  id: 101,
  name: 'ShadowWalker',
  avatar: '/土豆.jpg'
};

// --- 模拟任务数据 (7个等级) ---
const missions = ref([
  {
    id: 9001,
    title: '讨伐：核心数据库溢出',
    desc: '系统核心出现严重内存泄漏，导致服务器频繁重启。找出原因并修复。',
    rank: 'SSS', // 1. 神话
    type: 'FATAL_ERROR',
    reward: 50000,
    publisher: 'SYSTEM_CORE',
    status: 0,
    assignee: null
  },
  {
    id: 9002,
    title: '护送：新版架构图',
    desc: '将 V5.0 架构图安全传输至前端资源库，确保无损压缩。',
    rank: 'SS', // 2. 传说
    type: 'ARCHITECT',
    reward: 20000,
    publisher: 'CTO_OFFICE',
    status: 0,
    assignee: null
  },
  {
    id: 9003,
    title: '猎杀：高并发死锁进程',
    desc: '一个死锁进程正在阻塞交易系统，强制结束它并优化锁逻辑。',
    rank: 'S', // 3. 史诗
    type: 'BACKEND',
    reward: 8000,
    publisher: 'DBA_MASTER',
    status: 1, // 别人接了
    assignee: { id: 999, name: 'CodeSlayer', avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=Felix' }
  },
  {
    id: 9004,
    title: '重构：评论区组件',
    desc: '将旧版评论组件重构为递归树组件，支持无限层级。',
    rank: 'A', // 4. 精英
    type: 'FRONTEND',
    reward: 5000,
    publisher: 'UI_LEADER',
    status: 1, // 我接了
    assignee: { id: 101, name: 'ShadowWalker', avatar: '/土豆.jpg' }
  },
  {
    id: 9005,
    title: '绘制：活动宣传 Banner',
    desc: '设计本周“黑客松”活动的宣传海报，风格要求赛博朋克。',
    rank: 'B', // 5. 资深
    type: 'DESIGN',
    reward: 2000,
    publisher: 'OPERATION',
    status: 0,
    assignee: null
  },
  {
    id: 9006,
    title: '编写：API 接口文档',
    desc: '为 MissionController 的接口补充 Swagger 注释。',
    rank: 'C', // 6. 普通
    type: 'DOCS',
    reward: 800,
    publisher: 'QA_TEAM',
    status: 0,
    assignee: null
  },
  {
    id: 9007,
    title: '清理：过期日志文件',
    desc: '服务器磁盘报警，清理 30 天前的 Log 文件。',
    rank: 'D', // 7. 新手
    type: 'MAINTENANCE',
    reward: 200,
    publisher: 'JANITOR_BOT',
    status: 2, // 审核中
    assignee: { id: 101, name: 'ShadowWalker', avatar: '/土豆.jpg' }
  }
]);

// --- 状态控制 ---
const showModal = ref(false);
const currentTask = ref(null);
const submitContent = ref('');

// --- 辅助逻辑 ---
const isMyTask = (task) => {
  return task.assignee && task.assignee.id === myUser.id;
};

const getStatusClass = (task) => {
  if (task.status === 1 && !isMyTask(task)) return 'card-sealed'; 
  return '';
};

// --- 交互动作 ---
const handleAccept = (task) => {
  if(!confirm(`[工会系统] 确认承接 ${task.rank} 级任务？\n高难度任务失败可能会降低信誉分。`)) return;
  
  // 模拟接单
  task.status = 1;
  task.assignee = myUser;
};

const openSubmitModal = (task) => {
  currentTask.value = task;
  submitContent.value = '';
  showModal.value = true;
};

const confirmSubmit = () => {
  if (!submitContent.value) return alert("报告内容不能为空");
  const target = missions.value.find(m => m.id === currentTask.value.id);
  if (target) target.status = 2; // 变成审核中
  showModal.value = false;
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;800&display=swap');

/* --- 核心容器 --- */
.guild-hall-container {
  /* 基础色盘 (参考 ComCenter) */
  --bg: #F4F1EA;
  --black: #111111;
  --text-main: #111;
  --card-bg: #fff;
  
  /* --- 🔥 七大等级色谱 (高对比度) 🔥 --- */
  --r-sss: #FF00FF; /* 神话：幻彩紫/洋红 */
  --r-ss:  #FFD700; /* 传说：黄金 */
  --r-s:   #D92323; /* 史诗：猩红 (ComCenter红) */
  --r-a:   #FF6600; /* 精英：橙色 */
  --r-b:   #9D00FF; /* 资深：深紫 */
  --r-c:   #0099FF; /* 普通：科技蓝 */
  --r-d:   #00CC66; /* 新手：基础绿 */

  width: 100%; height: 100%;
  background-color: var(--bg);
  display: flex; flex-direction: column;
  font-family: 'JetBrains Mono', monospace;
  color: var(--text-main);
  position: relative; overflow: hidden;
}

/* 背景网格 (ComCenter 同款) */
.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(#ccc 1px, transparent 1px), linear-gradient(90deg, #ccc 1px, transparent 1px); 
  background-size: 50px 50px; opacity: 0.4; pointer-events: none; 
}
.moving-grid { animation: gridScroll 60s linear infinite; }

/* --- 1. 头部 (ComCenter 风格) --- */
.guild-header {
  flex-shrink: 0; background: var(--black); border-bottom: 4px solid var(--black);
}
.header-content {
  padding: 20px 40px; display: flex; justify-content: space-between; align-items: center;
  color: #fff;
}
.glitch-title { font-family: 'Anton'; font-size: 3rem; margin: 0; line-height: 0.9; text-transform: uppercase; }
.text-row.outline { -webkit-text-stroke: 1px #fff; color: transparent; display: block; font-size: 2rem; }

.user-badge { display: flex; align-items: center; gap: 15px; border: 2px solid #fff; padding: 5px 15px; background: #000; box-shadow: 4px 4px 0 rgba(255,255,255,0.2); }
.avatar-frame { width: 40px; height: 40px; border: 2px solid #fff; overflow: hidden; }
.avatar-frame img { width: 100%; height: 100%; object-fit: cover; }
.info-col { display: flex; flex-direction: column; }
.info-col .name { font-weight: bold; color: #fff; }
.info-col .rank { font-size: 0.7rem; color: #ccc; }
.highlight { color: var(--r-ss); font-weight: bold; }

.alert-strip { background: var(--black); border-top: 1px solid #333; color: var(--r-s); font-weight: bold; font-size: 0.8rem; padding: 4px 0; overflow: hidden; white-space: nowrap; }
.strip-content { display: inline-block; animation: marquee 30s linear infinite; }

/* --- 2. 任务卡片网格 --- */
.mission-grid {
  flex: 1; padding: 40px; overflow-y: auto;
  display: grid; 
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); 
  gap: 30px; align-content: start;
}

/* --- 卡片通用样式 (MD 风格升级) --- */
.mission-card {
  background: var(--card-bg); 
  border: 2px solid var(--black);
  position: relative; display: flex; flex-direction: column;
  height: 420px; transition: all 0.25s cubic-bezier(0.25, 0.8, 0.25, 1);
  overflow: hidden;
  /* MD Elevation 1 */
  box-shadow: 0 1px 3px rgba(0,0,0,0.12), 0 1px 2px rgba(0,0,0,0.24);
}

/* MD Hover Lift */
.mission-card:hover { 
  transform: translateY(-8px); 
  /* MD Elevation 4 */
  box-shadow: 0 14px 28px rgba(0,0,0,0.25), 0 10px 10px rgba(0,0,0,0.22);
  z-index: 5; 
}

/* 水印 */
.bg-watermark {
  position: absolute; top: 20px; right: 10px;
  font-family: 'Anton'; font-size: 9rem;
  opacity: 0.08; pointer-events: none; z-index: 0;
  line-height: 1; color: var(--black);
}

/* --- 🔥 等级样式变体 (边框 & 徽章) 🔥 --- */
/* SSS */
.mission-card.rank-sss { border-color: var(--r-sss); box-shadow: 0 0 15px rgba(255, 0, 255, 0.4); animation: border-pulse 2s infinite; }
.mission-card.rank-sss .rank-badge { background: linear-gradient(135deg, var(--r-sss), #FFF); color: #000; }
.mission-card.rank-sss .bg-watermark { color: var(--r-sss); opacity: 0.15; }

/* SS */
.mission-card.rank-ss { border-color: var(--r-ss); box-shadow: 0 5px 15px rgba(255, 215, 0, 0.3); }
.mission-card.rank-ss .rank-badge { background: var(--r-ss); color: #000; }

/* S */
.mission-card.rank-s { border-color: var(--r-s); }
.mission-card.rank-s .rank-badge { background: var(--r-s); color: #fff; }

/* A */
.mission-card.rank-a { border-color: var(--r-a); }
.mission-card.rank-a .rank-badge { background: var(--r-a); color: #000; }

/* B */
.mission-card.rank-b { border-color: var(--r-b); }
.mission-card.rank-b .rank-badge { background: var(--r-b); color: #fff; }

/* C */
.mission-card.rank-c { border-color: var(--r-c); }
.mission-card.rank-c .rank-badge { background: var(--r-c); color: #000; }

/* D */
.mission-card.rank-d { border-color: var(--r-d); }
.mission-card.rank-d .rank-badge { background: var(--r-d); color: #000; }

/* 卡片内容布局 */
.card-top { 
  display: flex; justify-content: space-between; align-items: center;
  padding: 15px; border-bottom: 2px solid var(--black); 
  background: rgba(0,0,0,0.02); z-index: 1; 
}
.rank-badge { 
  padding: 4px 12px; font-weight: 900; font-family: 'Anton'; letter-spacing: 1px; font-size: 1.4rem; 
  box-shadow: 2px 2px 0 rgba(0,0,0,0.2); 
}
.reward-box { display: flex; align-items: center; gap: 5px; background: var(--black); color: #fff; padding: 4px 8px; font-weight: bold; }
.reward-box .label { color: #ccc; font-size: 0.7rem; }

.card-body { flex: 1; padding: 20px; z-index: 1; display: flex; flex-direction: column; }
.id-row { font-size: 0.8rem; color: #666; margin-bottom: 10px; display: flex; justify-content: space-between; align-items: center; }
.type-tag { border: 1px solid #666; padding: 2px 6px; font-size: 0.65rem; border-radius: 4px; }

.title { font-family: 'Anton'; font-size: 1.6rem; margin: 0 0 15px 0; line-height: 1.1; text-transform: uppercase; color: var(--black); }
.desc { font-size: 0.9rem; color: #444; line-height: 1.5; flex: 1; }

.client-row { margin-top: 15px; font-size: 0.75rem; color: #555; border-top: 1px dashed #ccc; padding-top: 10px; font-weight: bold; }

/* 🔒 封条层 (公会风格) */
.sealed-overlay {
  position: absolute; inset: 0; z-index: 10;
  background: rgba(255,255,255,0.85);
  backdrop-filter: blur(2px) grayscale(100%);
  display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 20px;
}
.seal-stamp {
  border: 5px solid var(--r-s); padding: 10px;
  transform: rotate(-15deg);
  box-shadow: 0 0 0 2px #fff, 0 0 0 4px var(--r-s);
}
.seal-inner { 
  font-family: 'Anton'; font-size: 2.2rem; color: var(--r-s); text-align: center; line-height: 1; 
  border: 2px solid var(--r-s); padding: 5px 20px; letter-spacing: 2px;
}

.hunter-info { display: flex; flex-direction: column; align-items: center; color: var(--black); font-size: 0.8rem; font-weight: bold; }
.hunter-row { display: flex; align-items: center; gap: 10px; margin-top: 5px; background: var(--black); color: #fff; padding: 5px 15px; }
.hunter-row img { width: 25px; height: 25px; object-fit: cover; border: 1px solid #fff; }

/* 底部操作 */
.card-footer { padding: 15px; border-top: 2px solid var(--black); z-index: 1; background: #fff; }

.action-btn { 
  width: 100%; padding: 12px; font-weight: bold; cursor: pointer; border: 2px solid var(--black); 
  font-family: 'JetBrains Mono'; transition: 0.2s; text-transform: uppercase; box-shadow: 4px 4px 0 var(--black);
  background: #fff; color: var(--black);
}
.action-btn:hover { transform: translate(-2px, -2px); box-shadow: 6px 6px 0 var(--black); }
.action-btn:active { transform: translate(2px, 2px); box-shadow: 2px 2px 0 var(--black); }

/* 接受按钮：根据等级变色 (可选，这里用统一风格保持整洁，或者用绿色) */
.accept-btn:hover { background: var(--black); color: #fff; }

.submit-btn { background: var(--r-ss); border-color: var(--black); }

.status-bar { width: 100%; padding: 12px; text-align: center; font-weight: bold; font-size: 0.9rem; border: 2px dashed #999; color: #666; }
.status-bar.auditing { color: var(--r-c); border-color: var(--r-c); background: rgba(0, 153, 255, 0.05); }
.status-bar.completed { color: var(--r-d); border-color: var(--r-d); background: rgba(0, 204, 102, 0.05); }

/* --- 弹窗 --- */
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.85); z-index: 999; display: flex; align-items: center; justify-content: center; backdrop-filter: blur(4px); }
.guild-modal { width: 600px; background: #fff; border: 4px solid var(--black); box-shadow: 15px 15px 0 rgba(0,0,0,0.5); display: flex; flex-direction: column; }
.modal-header { background: var(--black); color: #fff; padding: 15px 20px; font-weight: bold; display: flex; justify-content: space-between; align-items: center; font-family: 'JetBrains Mono'; }
.close-btn { background: none; border: 1px solid #fff; color: #fff; font-weight: bold; font-size: 1.2rem; cursor: pointer; padding: 0 8px; transition: 0.2s; }
.close-btn:hover { background: var(--r-s); border-color: var(--r-s); }

.modal-body { padding: 30px; }
.instruction { color: #333; margin-bottom: 15px; font-weight: bold; }
.cyber-textarea { width: 100%; height: 180px; background: #fafafa; border: 2px solid var(--black); color: #000; padding: 15px; font-family: 'JetBrains Mono'; margin-bottom: 25px; outline: none; resize: none; font-size: 1rem; }
.cyber-textarea:focus { box-shadow: inset 4px 4px 0 rgba(0,0,0,0.1); }

.modal-actions { display: flex; justify-content: flex-end; gap: 15px; }
.cancel-btn { padding: 12px 25px; background: #ccc; color: #000; border: 2px solid var(--black); cursor: pointer; font-weight: bold; box-shadow: 4px 4px 0 var(--black); }
.confirm-btn { padding: 12px 25px; background: var(--r-ss); color: #000; border: 2px solid var(--black); cursor: pointer; font-weight: bold; box-shadow: 4px 4px 0 var(--black); }
.confirm-btn:hover, .cancel-btn:hover { transform: translate(-2px, -2px); box-shadow: 6px 6px 0 var(--black); }

@keyframes marquee { 0% { transform: translateX(100%); } 100% { transform: translateX(-100%); } }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-40px); } }
@keyframes border-pulse { 0% { border-color: var(--r-sss); } 50% { border-color: #fff; } 100% { border-color: var(--r-sss); } }

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 8px; }
.custom-scroll::-webkit-scrollbar-track { background: #eee; border-left: 2px solid var(--black); }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--black); border: 1px solid #fff; }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: var(--r-s); }
</style>