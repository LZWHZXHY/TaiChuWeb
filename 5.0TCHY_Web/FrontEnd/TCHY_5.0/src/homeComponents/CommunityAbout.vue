<template>
  <div class="about-industrial">
    
    <div class="manifesto-terminal">
      <div class="term-header">
        <span class="dot red"></span>
        <span class="dot yellow"></span>
        <span class="dot green"></span>
        <span class="title">mission_statement.sh</span>
      </div>
      <div class="term-body">
        <p class="comment"># INITIALIZING TAICHU_UNIVERSE PROTOCOL...</p>
        <p class="comment"># LOADING CORE VALUES...</p>
        <div class="code-block">
          <span class="var">const</span> <span class="func">createSingularity</span> = () => {<br>
          &nbsp;&nbsp;<span class="key">return</span> {<br>
          &nbsp;&nbsp;&nbsp;&nbsp;vision: <span class="str">"构建去中心化的创意奇点"</span>,<br>
          &nbsp;&nbsp;&nbsp;&nbsp;values: [<span class="str">"包容"</span>, <span class="str">"极客"</span>, <span class="str">"共创"</span>],<br>
          &nbsp;&nbsp;&nbsp;&nbsp;status: <span class="str">"ETERNAL_BETA"</span><br>
          &nbsp;&nbsp;}<br>
          }
        </div>
        <div class="typing-cursor">
          > 我们不仅仅是一个社区，我们是连接每一个孤独节点的神经元网络。<span class="blink">_</span>
        </div>
      </div>
    </div>

    <div class="metrics-grid">
      <div class="metric-card">
        <div class="label">OPERATIVES // 成员</div>
        <div class="value">12,508</div>
        <div class="bar"><div class="fill" style="width: 75%"></div></div>
      </div>
      <div class="metric-card">
        <div class="label">UPTIME // 运行天数</div>
        <div class="value">365 <span class="unit">DAYS</span></div>
        <div class="bar"><div class="fill" style="width: 40%"></div></div>
      </div>
      <div class="metric-card">
        <div class="label">DATA_BLOCKS // 帖子</div>
        <div class="value">84,201</div>
        <div class="bar"><div class="fill" style="width: 90%"></div></div>
      </div>
    </div>

    <div class="team-section">
      <div class="section-label">
        <span class="icon">■</span> ACTIVE_OPERATORS // 开发团队
      </div>
      
      <div class="operator-grid">
        <div v-for="member in teamMembers" :key="member.id" class="operator-card">
          <div class="card-bg-text">{{ member.role }}</div>
          <div class="op-header">
            <div class="op-avatar">
              <img :src="member.avatar" alt="AVATAR" @error="handleImgError">
              <div class="scan-overlay"></div>
            </div>
            <div class="op-status">
              <span class="status-light online"></span> ONLINE
            </div>
          </div>
          <div class="op-info">
            <div class="op-name">{{ member.name }}</div>
            <div class="op-role">[{{ member.role }}]</div>
            <div class="op-desc">{{ member.desc }}</div>
          </div>
          <div class="op-footer">
            <span>ID: {{ member.id }}</span>
            <span>ACC_LVL: {{ member.level }}</span>
          </div>
        </div>
      </div>
    </div>

    <div class="roadmap-section">
      <div class="section-label">
        <span class="icon">■</span> EXECUTION_LOG // 发展历程
      </div>
      <div class="console-log">
        <div v-for="(log, index) in historyLogs" :key="index" class="log-entry">
          <span class="log-time">[{{ log.time }}]</span>
          <span class="log-type" :class="log.type">{{ log.type }}</span>
          <span class="log-msg">{{ log.msg }}</span>
        </div>
        <div class="log-entry blink">
          <span class="log-time">[ NOW ]</span>
          <span class="log-type INFO">WAITING</span>
          <span class="log-msg">等待新的指令输入...</span>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref } from 'vue'

const handleImgError = (e) => {
  // 简单的纯色占位符或默认图
  e.target.src = 'https://via.placeholder.com/150/000000/FFFFFF/?text=USER'; 
}

// 模拟团队数据
const teamMembers = ref([
  { 
    id: 'OP_001', 
    name: 'Neo_Admin', 
    role: 'ROOT_ADMIN', 
    level: 5, 
    desc: '系统架构师，负责维护太初核心代码库。',
    avatar: 'https://api.dicebear.com/7.x/pixel-art/svg?seed=Neo' 
  },
  { 
    id: 'OP_002', 
    name: 'Trinity_Des', 
    role: 'UI_ARCHITECT', 
    level: 4, 
    desc: '视觉接口设计师，构建人机交互界面。',
    avatar: 'https://api.dicebear.com/7.x/pixel-art/svg?seed=Trinity' 
  },
  { 
    id: 'OP_003', 
    name: 'Morpheus_Dev', 
    role: 'BACKEND_OPS', 
    level: 4, 
    desc: '数据流管理员，确保服务器连接稳定。',
    avatar: 'https://api.dicebear.com/7.x/pixel-art/svg?seed=Morpheus' 
  },
   { 
    id: 'OP_004', 
    name: 'Cipher_Mod', 
    role: 'COMM_OFFICER', 
    level: 3, 
    desc: '社区协议执行官，维护社区秩序与规则。',
    avatar: 'https://api.dicebear.com/7.x/pixel-art/svg?seed=Cipher' 
  }
])

// 模拟历史日志
const historyLogs = ref([
  { time: '2025.05.20', type: 'INIT', msg: '太初寰宇系统内核启动 (Kernel Panic fixed)' },
  { time: '2025.06.01', type: 'INFO', msg: '首批 100 名种子用户接入神经网络' },
  { time: '2025.08.15', type: 'WARN', msg: '流量过载，服务器扩容至 2.0 架构' },
  { time: '2025.12.10', type: 'UPDATE', msg: '发布 "太虚坛" 模块，开启无限制交流协议' },
  { time: '2026.01.01', type: 'SUCCESS', msg: '注册用户突破 10,000 节点' }
])
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.about-industrial {
  --red: #D92323; 
  --black: #111111; 
  --white: #F4F1EA;
  --green: #239b56;
  --yellow: #f1c40f;
  --mono: 'JetBrains Mono', monospace; 
  --heading: 'Anton', sans-serif;
  
  width: 100%;
  font-family: var(--mono);
  color: var(--black);
  display: flex;
  flex-direction: column;
  gap: 40px;
}

/* 1. 宣言终端 */
.manifesto-terminal {
  background: #1e1e1e;
  border: 2px solid var(--black);
  box-shadow: 8px 8px 0 rgba(0,0,0,0.2);
  color: #ddd;
  overflow: hidden;
}
.term-header {
  background: #333;
  padding: 8px 15px;
  display: flex;
  align-items: center;
  gap: 8px;
  border-bottom: 1px solid #000;
}
.dot { width: 10px; height: 10px; border-radius: 50%; }
.red { background: #ff5f56; }
.yellow { background: #ffbd2e; }
.green { background: #27c93f; }
.title { margin-left: 10px; font-size: 0.8rem; opacity: 0.7; }

.term-body { padding: 20px; font-size: 0.9rem; line-height: 1.6; }
.comment { color: #6a9955; }
.code-block { margin: 15px 0; padding-left: 15px; border-left: 2px solid #555; }
.var { color: #569cd6; }
.func { color: #dcdcaa; }
.key { color: #c586c0; }
.str { color: #ce9178; }

.typing-cursor { color: var(--white); margin-top: 15px; font-weight: bold; }
.blink { animation: blink 1s infinite; }

/* 2. 核心指标 */
.metrics-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
}
.metric-card {
  border: 2px solid var(--black);
  background: var(--white);
  padding: 15px;
  box-shadow: 4px 4px 0 rgba(0,0,0,0.1);
  position: relative;
}
.metric-card .label { font-size: 0.75rem; font-weight: bold; color: #666; margin-bottom: 5px; }
.metric-card .value { font-family: var(--heading); font-size: 2.5rem; line-height: 1; color: var(--black); }
.metric-card .unit { font-size: 1rem; color: #888; font-family: var(--mono); }
.metric-card .bar { height: 4px; background: #ddd; margin-top: 10px; width: 100%; }
.metric-card .fill { height: 100%; background: var(--red); }

/* 3. 团队成员 (操作员) */
.section-label {
  font-family: var(--heading); font-size: 1.5rem; margin-bottom: 20px;
  display: flex; align-items: center; gap: 10px;
  border-bottom: 2px solid var(--black); padding-bottom: 5px;
}
.icon { color: var(--red); font-size: 0.8em; }

.operator-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 20px;
}
.operator-card {
  border: 2px solid var(--black);
  background: #fff;
  padding: 0;
  display: flex; flex-direction: column;
  position: relative;
  overflow: hidden;
  transition: transform 0.2s;
}
.operator-card:hover {
  transform: translateY(-5px);
  box-shadow: 6px 6px 0 var(--red);
}

.card-bg-text {
  position: absolute; top: -10px; right: -10px;
  font-family: var(--heading); font-size: 4rem;
  color: rgba(0,0,0,0.03); pointer-events: none; z-index: 0;
  white-space: nowrap;
}

.op-header {
  padding: 20px;
  border-bottom: 2px solid var(--black);
  background: #f9f9f9;
  display: flex; justify-content: space-between; align-items: flex-start;
  position: relative; z-index: 1;
}
.op-avatar {
  width: 80px; height: 80px;
  border: 2px solid var(--black);
  background: #000;
  position: relative;
  overflow: hidden;
}
.op-avatar img { width: 100%; height: 100%; object-fit: cover; filter: grayscale(100%); }
.scan-overlay {
  position: absolute; inset: 0;
  background: linear-gradient(to bottom, transparent 50%, rgba(0,255,0,0.1) 50%);
  background-size: 100% 4px;
  pointer-events: none;
}
.op-status {
  font-size: 0.7rem; font-weight: bold;
  border: 1px solid var(--black); padding: 2px 6px; background: #fff;
  display: flex; align-items: center; gap: 5px;
}
.status-light { width: 6px; height: 6px; border-radius: 50%; background: #ccc; }
.status-light.online { background: var(--green); box-shadow: 0 0 4px var(--green); }

.op-info { padding: 20px; flex: 1; position: relative; z-index: 1; }
.op-name { font-family: var(--heading); font-size: 1.5rem; line-height: 1; margin-bottom: 5px; }
.op-role { font-size: 0.8rem; font-weight: bold; color: var(--red); margin-bottom: 10px; }
.op-desc { font-size: 0.85rem; color: #555; line-height: 1.4; }

.op-footer {
  background: var(--black); color: var(--white);
  padding: 8px 15px;
  font-size: 0.7rem;
  display: flex; justify-content: space-between;
}

/* 4. 发展路线 (Console Log) */
.console-log {
  background: #f4f4f4;
  border: 2px solid var(--black);
  padding: 20px;
  font-size: 0.85rem;
  font-family: 'Consolas', monospace;
}
.log-entry { margin-bottom: 8px; display: flex; gap: 10px; }
.log-time { color: #888; }
.log-type { font-weight: bold; width: 60px; }
.log-type.INIT { color: #9C27B0; }
.log-type.INFO { color: #2196F3; }
.log-type.WARN { color: #FF9800; }
.log-type.UPDATE { color: #00BCD4; }
.log-type.SUCCESS { color: #4CAF50; }
.log-msg { color: #333; flex: 1; }

@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }

/* 响应式 */
@media (max-width: 768px) {
  .metrics-grid { grid-template-columns: 1fr; }
  .operator-grid { grid-template-columns: 1fr; }
  .manifesto-terminal { font-size: 0.8rem; }
}
</style>