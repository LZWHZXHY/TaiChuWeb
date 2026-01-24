<template>
  <div class="about-industrial">
    <div class="scanline-overlay"></div>

    <div class="manifesto-terminal">
      <div class="term-header">
        <div class="dots">
          <span class="dot red"></span>
          <span class="dot yellow"></span>
          <span class="dot green"></span>
        </div>
        <span class="title">KERNEL_INIT.js // 核心协议</span>
        <span class="status-tag">READ_ONLY</span>
      </div>
      
      <div class="term-body custom-scroll">
        <div class="code-block">
          <div class="code-line">
            <span class="line-num">01</span>
            <span class="kwd">import</span> { <span class="var">Creativity</span>, <span class="var">Passion</span> } <span class="kwd">from</span> <span class="str">'@Universe/Core'</span>;
          </div>
          <div class="code-line">
            <span class="line-num">02</span>
          </div>
          <div class="code-line">
            <span class="line-num">03</span>
            <span class="cmt">// 定义太初协议接口</span>
          </div>
          <div class="code-line">
            <span class="line-num">04</span>
            <span class="kwd">class</span> <span class="cls">TaichuNetwork</span> <span class="kwd">extends</span> <span class="cls">Dream</span> {
          </div>
          <div class="code-line">
            <span class="line-num">05</span>
            &nbsp;&nbsp;<span class="kwd">constructor</span>() {
          </div>
          <div class="code-line">
            <span class="line-num">06</span>
            &nbsp;&nbsp;&nbsp;&nbsp;<span class="kwd">super</span>();
          </div>
          <div class="code-line">
            <span class="line-num">07</span>
            &nbsp;&nbsp;&nbsp;&nbsp;<span class="kwd">this</span>.<span class="prop">mission</span> = <span class="str">"REDEFINE_VIRTUAL_REALITY"</span>;
          </div>
          <div class="code-line">
            <span class="line-num">08</span>
            &nbsp;&nbsp;&nbsp;&nbsp;<span class="kwd">this</span>.<span class="prop">status</span> = <span class="const">Infinity</span>;
          </div>
          <div class="code-line">
            <span class="line-num">09</span>
            &nbsp;&nbsp;}
          </div>
          <div class="code-line">
            <span class="line-num">10</span>
          </div>
          <div class="code-line">
            <span class="line-num">11</span>
            &nbsp;&nbsp;<span class="kwd">async</span> <span class="func">forgeFuture</span>() {
          </div>
          <div class="code-line">
            <span class="line-num">12</span>
            &nbsp;&nbsp;&nbsp;&nbsp;<span class="kwd">await</span> <span class="func">connect</span>(<span class="str">"EVERY_SOUL"</span>);
          </div>
          <div class="code-line">
            <span class="line-num">13</span>
            &nbsp;&nbsp;&nbsp;&nbsp;<span class="kwd">return</span> <span class="str">"NEW_ERA_LOADING..."</span>;
          </div>
          <div class="code-line">
            <span class="line-num">14</span>
            &nbsp;&nbsp;}
          </div>
          <div class="code-line">
            <span class="line-num">15</span>
            }
          </div>
        </div>

        <div class="typing-container">
          <span class="prompt">root@taichu:~$</span>
          <span class="typing-text">{{ displayedText }}</span>
          <span class="cursor">_</span>
        </div>
      </div>
    </div>

    <div class="metrics-grid">
      <div class="metric-card highlight">
        <div class="label">UPTIME // 运行天数</div>
        <div class="value">{{ uptimeDays }} <span class="unit">DAYS</span></div>
        <div class="bar"><div class="fill" :style="{ width: '100%' }"></div></div>
      </div>
      <div class="metric-card">
        <div class="label">距离正式营业进度</div>
        <div class="value">1 <span class="unit">%</span></div>
        <div class="bar"><div class="fill" style="width: 1%"></div></div>
      </div>
      <div class="metric-card">
        <div class="label">BUILD_VER // 当前版本</div>
        <div class="value">5.6.1 <span class="unit">BETA</span></div>
        <div class="bar"><div class="fill" style="width: 10%"></div></div>
      </div>
    </div>


    <div class="roadmap-section">
      <div class="section-label">
        <span class="icon">■</span> SYSTEM_LOGS // 历史进程
      </div>
      <div class="console-log">
        <div class="log-scroll-area custom-scroll">
          <div v-for="(log, index) in historyLogs" :key="index" class="log-entry">
            <span class="log-time">[{{ log.time }}]</span>
            <span class="log-type" :class="log.type">{{ log.type }}</span>
            <span class="log-msg">>> {{ log.msg }}</span>
          </div>
          <div class="log-entry current">
            <span class="log-time">[NOW]</span>
            <span class="log-type INFO">LISTENING</span>
            <span class="log-msg">>> 等待新的历史进展......</span>
          </div>
        </div>
      </div>
    </div>


    <div class="section-group">
      <div class="section-label">
        <span class="icon red-blink">■</span> COMMAND_CENTER // 创始人
      </div>
      <div class="founder-wrap">
        <div class="operator-card founder-card">
          <div class="card-bg-text">FOUNDER</div>
          <div class="op-header">
            <div class="op-avatar-wrapper large">
              <div class="op-avatar">
                <img :src="founder.avatar" alt="AVATAR" @error="handleImgError">
                <div class="scan-overlay"></div>
              </div>
              <div class="avatar-frame" :style="{ borderColor: '#D92323' }"></div>
            </div>
            <div class="op-status">
              <span class="status-light online"></span> SYSTEM_ADMIN
            </div>
          </div>
          <div class="op-info">
            <div class="op-name">{{ founder.name }}</div>
            <div class="op-role" :style="{ color: '#D92323' }">[{{ founder.role }}]</div>
            <div class="op-desc">{{ founder.desc }}</div>
          </div>
          <div class="op-footer">
            <span>ID: {{ founder.id }}</span>
            <span>LVL: {{ founder.level }}</span>
            <span class="op-auth" :style="{ color: '#D92323' }">ROOT_ACCESS</span>
          </div>
        </div>
      </div>
    </div>

    <div class="section-group">
      <div class="section-label">
        <span class="icon">■</span> CORE_OPERATIVES // 核心协助者
      </div>
      <div class="operator-grid">
        <div v-for="member in coreOperatives" :key="member.id" class="operator-card">
          <div class="card-bg-text">{{ member.id }}</div>
          <div class="op-header">
            <div class="op-avatar-wrapper">
              <div class="op-avatar">
                <img 
                  :src="member.avatar" 
                  alt="AVATAR" 
                  :class="{ 'is-offline': !member.online }"
                  @error="handleImgError"
                >
                <div class="scan-overlay"></div>
              </div>
              <div class="avatar-frame" :style="{ borderColor: member.color }"></div>
            </div>
            <div class="op-status">
              <span class="status-light" :class="{ online: member.online }"></span> 
              {{ member.online ? 'ONLINE' : 'OFFLINE' }}
            </div>
          </div>
          <div class="op-info">
            <div class="op-name">{{ member.name }}</div>
            <div class="op-role" :style="{ color: member.color }">[{{ member.role }}]</div>
            <div class="op-desc">{{ member.desc }}</div>
          </div>
          <div class="op-footer">
            <span>ID: {{ member.id }}</span>
            <span>LVL: {{ member.level }}</span>
            <span class="op-auth" :style="{ color: member.color }">AUTHORIZED</span>
          </div>
        </div>
      </div>
    </div>

    <div class="section-group">
      <div class="section-label">
        <span class="icon">■</span> AUXILIARY_NODES // 协力节点
      </div>
      <div class="aux-grid">
        <div v-for="node in auxiliaryNodes" :key="node.name" class="aux-node">
          <div class="aux-avatar-container">
            <div class="aux-avatar">
              <img 
                :src="node.avatar" 
                :class="{ 'is-offline': !node.online }"
                @error="handleImgError"
              >
            </div>
            <div class="aux-status-dot" :class="{ 'is-online': node.online }"></div>
          </div>
          <div class="aux-body">
            <div class="aux-name">
              {{ node.name }}
              <span class="aux-status-tag" :class="{ 'is-online': node.online }">
                {{ node.online ? 'LINKED' : 'UNLINKED' }}
              </span>
            </div>
            <div class="aux-content">{{ node.content }}</div>
          </div>
        </div>
      </div>
    </div>

    

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

const uptimeDays = computed(() => {
  const startDate = new Date('2025-05-20');
  const today = new Date();
  const diffTime = Math.abs(today - startDate);
  return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
});

const handleImgError = (e) => {
  e.target.src = 'https://api.dicebear.com/7.x/identicon/svg?seed=fallback'; 
}

// --- 打字机效果逻辑 ---
const fullText = "以不变的初心前进 // FORGE_THE_FUTURE_WITH_ORIGINAL_HEART";
const displayedText = ref("");

const startTyping = () => {
  let i = 0;
  const speed = 50; // 打字速度
  
  const typeWriter = () => {
    if (i < fullText.length) {
      displayedText.value += fullText.charAt(i);
      i++;
      setTimeout(typeWriter, speed);
    }
  };
  typeWriter();
};

onMounted(() => {
  startTyping();
});

// --- 数据部分 (保持不变) ---
const founder = ref({
  id: 'DIR_001', 
  name: '腾蛇', 
  role: 'CHIEF_DIRECTOR', 
  level: 'S', 
  desc: '太初寰宇创始人。网站全栈开发、策划及底层协议制定，统筹社区发展方向。游戏全流程开发，3D建模，材质制作',
  avatar: 'https://image2url.com/r2/default/images/1768615897703-a73ba8ec-6f15-4960-abe8-4f9252614b9a.png' 
})

const coreOperatives = ref([
  { 
    id: 'DEV_001', name: '峰峰子', role: '全链视觉设计', color: '#6b27c4', level: 'A', online: true,
    desc: '网站前端视觉师，交互设计与美术概念，品牌宣发、造型设计，摄影与后期制作',
    avatar: 'https://image2url.com/r2/default/images/1768440579620-518d987a-37b7-4f81-8406-5fa77e6d79c1.jpg' 
  },
  { 
    id: 'PR_001', name: 'SIMON', role: '项目研发,产品宣发', color: '#FF6B00', level: 'A', online: true,
    desc: '游戏概念美术设计，3D动画师，项目主策划，游戏制作人',
    avatar: 'https://image2url.com/r2/default/images/1768690065252-298402c8-483f-4993-bd9a-331c3cc87b73.jpg' 
  },
  { 
    id: 'OPS_001', name: '援受', role: '社区管理,自媒体运营', color: '#f1c40f', level: 'B', online: true,
    desc: '社区执行官。负责内容分发、用户反馈采集及社群秩序维护。负责自媒体矩阵运营、视频剪辑与商业对接',
    avatar: 'https://image2url.com/r2/default/images/1768690111264-18a501cb-2a7d-486b-98ae-206c91b49de6.jpg' 
  },
  { 
    id: 'Draw_001', name: '伊渡', role: '动画管理,静态立绘', color: '#00D1FF', level: 'A', online: true,
    desc: '动画项目管理，制作进度跟踪，背景制作',
    avatar: 'https://image2url.com/r2/default/images/1768690345558-4df4c727-6b55-47a4-801a-9a1ee065b83b.jpg' 
  },
  { 
    id: 'WorldIP_001', name: '口嘿嗨', role: '世界观设计', color: '#00D1FF', level: 'A', online: false,
    desc: '世界观设定，剧情编写',
    avatar: 'https://image2url.com/r2/default/images/1768691134725-b7a4f65f-7d4e-41d1-a308-bd1a1e17a70c.jpg' 
  },
  { 
    id: 'Animation_002', name: '云绍', role: '动画制作', color: '#00D1FF', level: 'A', online: true,
    desc: '动画制作',
    avatar: 'https://image2url.com/r2/default/images/1768690862512-a59e76a0-2c4e-4f37-a573-e02f2301f1bc.jpg' 
  },
  { 
    id: 'Animation_003', name: '橘橘兔', role: '动画制作,世界观', color: '#00D1FF', level: 'A', online: false,
    desc: '动画制作，世界观内容设计',
    avatar: 'https://image2url.com/r2/default/images/1768691184574-ce55168f-826f-4489-a2a9-e71dac6a343b.jpg' 
  },
  { 
    id: 'Draw_002', name: '寒鸦', role: '静态立绘', color: '#00D1FF', level: 'A', online: false,
    desc: '人物静态立绘，世界观设计',
    avatar: 'https://image2url.com/r2/default/images/1768691578204-4bf29127-9848-4b06-b686-5ec279bf9f74.jpg' 
  },
  { 
    id: 'Animation_004', name: 'BHZ32', role: '动画制作', color: '#00D1FF', level: 'A', online: true,
    desc: '动画制作，世界观设计',
    avatar: 'https://image2url.com/r2/default/images/1768691449272-c15f30b4-4d95-4d29-b7ea-27d0855b2f1c.jpg' 
  },
])

const auxiliaryNodes = ref([
  { name: '土豆', content: '绘院联合管理', avatar: 'https://image2url.com/r2/default/images/1768691881825-21a82807-61a7-48b8-adc9-bb9a9d5e2a21.jpg', online: true },
  { name: '索少', content: '绘院联合管理', avatar: 'https://image2url.com/r2/default/images/1768691862407-37acfd2f-83e7-472b-8dbe-c5c3511a34b5.jpg', online: true }
])

const historyLogs = ref([
  { time: '2025.05.20', type: 'INIT', msg: '太初寰宇官网4.0正式上线' },
  { time: '2025.12.07', type: 'UPDATE', msg: '太初寰宇官网更新至5.0正式版' },
  { time: '2026.01.23', type: 'SUCCESS', msg: '核心人数突破160+' }
])
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.about-industrial {
  --red: #D92323; --black: #0F0F0F; --white: #F4F1EA;
  --green: #00FF41; --gray: #2A2A2A; --blue: #569cd6; --purple: #c586c0; --orange: #ce9178;
  --mono: 'JetBrains Mono', monospace; --heading: 'Anton', sans-serif;
  
  width: 100%; padding: 40px; box-sizing: border-box;
  background: var(--white); font-family: var(--mono); color: var(--black);
  display: flex; flex-direction: column; gap: 60px;
}

.scanline-overlay {
  position: fixed; top: 0; left: 0; width: 100%; height: 100%;
  background: linear-gradient(to bottom, transparent 50%, rgba(0,0,0,0.02) 50%);
  background-size: 100% 4px; pointer-events: none; z-index: 100;
}

/* 1. 宣言终端 (IDE 风格优化) */
.manifesto-terminal {
  background: #1e1e1e; /* VS Code dark theme bg */
  border: 4px solid var(--black); 
  box-shadow: 12px 12px 0 rgba(0,0,0,0.2); 
  color: #d4d4d4;
  overflow: hidden;
  position: relative;
}

/* 终端发光效果 */
.manifesto-terminal::after {
  content: ''; position: absolute; inset: 0;
  box-shadow: inset 0 0 40px rgba(0,0,0,0.5);
  pointer-events: none;
}

.term-header {
  background: #2d2d2d; padding: 12px 20px; border-bottom: 2px solid #000;
  display: flex; align-items: center; justify-content: space-between;
}
.dots { display: flex; gap: 8px; }
.dot { width: 12px; height: 12px; border-radius: 50%; }
.red { background: #ff5f56; } .yellow { background: #ffbd2e; } .green { background: #27c93f; }
.status-tag { font-size: 0.6rem; background: var(--gray); border: 1px solid #555; color: #aaa; padding: 2px 8px; border-radius: 4px; }
.title { font-size: 0.8rem; color: #999; letter-spacing: 1px; }

.term-body { padding: 30px; font-size: 14px; line-height: 1.6; }

/* 代码块高亮样式 */
.code-block { margin-bottom: 30px; }
.code-line { display: flex; align-items: flex-start; }
.line-num { 
  color: #555; margin-right: 20px; user-select: none; width: 20px; text-align: right; 
  font-size: 0.8rem; opacity: 0.7;
}

/* 语法高亮颜色 */
.kwd { color: var(--purple); font-weight: bold; } /* 关键字 */
.var { color: #9cdcfe; } /* 变量 */
.str { color: var(--orange); } /* 字符串 */
.cls { color: #4ec9b0; } /* 类名 */
.func { color: #dcdcaa; } /* 函数 */
.cmt { color: #6a9955; font-style: italic; } /* 注释 */
.prop { color: #9cdcfe; } /* 属性 */
.const { color: #4fc1ff; } /* 常量 */

/* 打字机样式 */
.typing-container { 
  border-top: 1px solid #333; padding-top: 15px; 
  color: #ccc; font-weight: bold; display: flex; align-items: center; flex-wrap: wrap;
}
.prompt { color: var(--green); margin-right: 10px; }
.typing-text { color: var(--white); }
.cursor { animation: blink 0.8s infinite; background: var(--white); width: 8px; height: 18px; display: inline-block; margin-left: 5px; vertical-align: middle; }

/* 2. 指标 (保持) */
.metrics-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; }
.metric-card { border: 3px solid var(--black); background: white; padding: 20px; }
.metric-card.highlight { background: var(--black); color: var(--white); }
.metric-card .label { font-size: 0.7rem; font-weight: bold; margin-bottom: 5px; }
.metric-card .value { font-family: var(--heading); font-size: 2.5rem; line-height: 1; }
.metric-card .unit { font-size: 0.8rem; opacity: 0.6; }
.metric-card .bar { height: 6px; background: #eee; margin-top: 15px; overflow: hidden; }
.metric-card.highlight .bar { background: #333; }
.metric-card .fill { height: 100%; background: var(--red); transition: width 1.5s ease; }

/* 3. 成员通用卡片 */
.section-label {
  font-family: var(--heading); font-size: 1.8rem; margin-bottom: 25px;
  border-bottom: 3px solid var(--black); padding-bottom: 10px; display: flex; align-items: center; gap: 10px;
}
.operator-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); gap: 30px; }
.operator-card { border: 3px solid var(--black); background: white; position: relative; overflow: hidden; transition: 0.2s; }
.operator-card:hover { transform: translate(-4px, -4px); box-shadow: 8px 8px 0 var(--black); }

/* 头像通用样式 */
.op-avatar { width: 80px; height: 80px; border: 2px solid var(--black); background: #000; z-index: 2; position: relative; overflow: hidden; }
.op-avatar img { width: 100%; height: 100%; object-fit: cover; transition: filter 0.3s ease; }
.is-offline { filter: grayscale(100%); opacity: 0.7; }

/* 创始人特化 */
.founder-card { max-width: 500px; margin: 0 auto; border-width: 4px; box-shadow: 12px 12px 0 var(--black); }
.op-avatar-wrapper.large .op-avatar { width: 120px; height: 120px; }
.op-avatar-wrapper.large .avatar-frame { width: 120px; height: 120px; }

.card-bg-text { position: absolute; top: -10px; right: -10px; font-family: var(--heading); font-size: 4rem; color: rgba(0,0,0,0.03); pointer-events: none; }
.op-header { padding: 20px; border-bottom: 2px solid var(--black); display: flex; justify-content: space-between; background: #fdfdfd; }
.op-avatar-wrapper { position: relative; }
.avatar-frame { position: absolute; top: 4px; left: 4px; width: 80px; height: 80px; border: 2px solid; z-index: 1; }

.op-status { font-size: 0.65rem; font-weight: bold; display: flex; align-items: center; gap: 5px; }
.status-light { width: 8px; height: 8px; border-radius: 50%; background: #ccc; }
.status-light.online { background: var(--green); box-shadow: 0 0 5px var(--green); animation: blink 2s infinite; }

.op-info { padding: 20px; min-height: 120px; }
.op-name { font-family: var(--heading); font-size: 1.8rem; margin-bottom: 4px; }
.op-role { font-size: 0.75rem; font-weight: bold; margin-bottom: 10px; }
.op-desc { font-size: 0.85rem; color: #555; line-height: 1.4; }
.op-footer { background: var(--black); color: #888; padding: 8px 15px; font-size: 0.6rem; display: flex; justify-content: space-between; }

/* 4. 协力节点 */
.aux-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 15px; }
.aux-node {
  display: flex; gap: 15px; padding: 12px; border: 2px solid var(--black); background: white;
  transition: 0.2s; align-items: center; position: relative;
}
.aux-node:hover { background: var(--black); color: white; transform: skewX(-2deg); }

.aux-avatar-container { position: relative; flex-shrink: 0; }
.aux-avatar { width: 50px; height: 50px; border: 1px solid var(--black); overflow: hidden; background: #000; }
.aux-avatar img { width: 100%; height: 100%; object-fit: cover; transition: filter 0.3s ease; }

.aux-status-dot {
  position: absolute; bottom: -2px; right: -2px; width: 10px; height: 10px; 
  background: #ccc; border: 2px solid var(--white); z-index: 10;
}
.aux-status-dot.is-online {
  background: var(--green); box-shadow: 0 0 5px var(--green);
  animation: blink 1.5s infinite;
}
.aux-node:hover .aux-status-dot { border-color: var(--black); }

.aux-body { display: flex; flex-direction: column; overflow: hidden; }
.aux-name { 
  font-family: var(--heading); font-size: 1.2rem; line-height: 1; margin-bottom: 4px; 
  display: flex; align-items: center; gap: 8px;
}
.aux-status-tag { 
  font-family: var(--mono); font-size: 0.5rem; padding: 1px 4px; 
  border: 1px solid #999; color: #999; font-weight: normal;
}
.aux-status-tag.is-online { border-color: var(--green); color: var(--green); }
.aux-node:hover .aux-status-tag:not(.is-online) { color: #555; border-color: #555; }

.aux-content { font-size: 0.75rem; opacity: 0.8; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

/* 5. 日志 */
.console-log { background: #1a1a1a; border: 3px solid var(--black); padding: 5px; }
.log-scroll-area { max-height: 250px; overflow-y: auto; padding: 15px; }
.log-entry { margin-bottom: 8px; font-size: 0.8rem; color: #ccc; display: flex; gap: 10px; border-bottom: 1px solid #222; }
.log-time { color: #555; font-weight: bold; }
.log-type { font-weight: bold; min-width: 60px; }
.log-type.INIT { color: var(--blue); }
.log-type.UPDATE { color: var(--purple); }
.log-type.SUCCESS { color: var(--green); }
.log-type.INFO { color: #aaa; }
.log-msg { color: #ddd; }
.current { animation: blink 2s infinite; }

/* 滚动条 */
.custom-scroll::-webkit-scrollbar { width: 8px; }
.custom-scroll::-webkit-scrollbar-track { background: #1e1e1e; }
.custom-scroll::-webkit-scrollbar-thumb { background: #444; border-radius: 4px; }
.custom-scroll::-webkit-scrollbar-thumb:hover { background: #666; }

@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
.red-blink { animation: blink 1.5s infinite; color: var(--red) !important; }

@media (max-width: 768px) {
  .metrics-grid { grid-template-columns: 1fr; }
  .aux-grid { grid-template-columns: 1fr; }
  .manifesto-terminal { font-size: 12px; }
}
</style>