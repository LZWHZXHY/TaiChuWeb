<template>
  <div class="cyber-intro-terminal">
    <div class="scan-lines"></div>

    <header class="hero-matrix">
      <div class="hero-decoration">
        <span>// EST. 2025.05.20</span>
        <span class="pulse-dot">●</span>
        <span>SYSTEM_STATUS: ONLINE</span>
      </div>
      
      <div class="hero-content">
        <h1 class="glitch-title" data-text="TAICHU_UNIVERSE">
          太初寰宇
        </h1>
        <div class="hero-sub-box">
          <span class="bracket">[</span>
          <span class="typewriter-text">A TRUSTED COMMUNITY NETWORK</span>
          <span class="bracket">]</span>
        </div>
        <p class="hero-desc">
          > 正在初始化连接... <br>
          > 欢迎接入创意与灵感的共鸣网络。
        </p>
      </div>
      
      <div class="bg-giant-text">GENESIS</div>
    </header>

    <section class="section-block">
      <div class="section-label">
        <span class="icon">■</span> ARCHIVE_01 // INTRO
      </div>
      
      <div class="cyber-card intro-card">
        <div class="card-strip">DATA_STREAM_RECEIVED</div>
        <div class="card-body">
          <p class="highlight-paragraph">
            <span class="marker">>></span> <strong>太初寰宇</strong> 成立于 
            <span class="code-highlight">2025.05.20</span> 
            (系统日志确认为巧合)。这是一个去中心化的创意奇点。
          </p>
          <div class="divider-dashed"></div>
          <p>
            致力于构建 <span class="tag">#包容</span> <span class="tag">#友善</span> 的数字生态。
            无论你是代码游侠、视觉艺术家，还是知识猎人，这里都有你的数据接口。
            我们定期执行 <span class="accent-text">线上活动</span>、<span class="accent-text">创作挑战</span> 和 <span class="accent-text">知识转储</span>。
          </p>
          <p class="terminal-text">
            > echo "每一个创意都值得被渲染，每一次交流都是一次握手协议。"
          </p>
        </div>
      </div>
    </section>

    <section class="section-block">
      <div class="section-label">
        <span class="icon">■</span> PROTOCOL_02 // GET_STARTED
      </div>
      
      <div class="guide-grid">
        <div class="guide-step">
          <div class="step-header">
            <span class="step-idx">01</span>
            <span class="step-line"></span>
          </div>
          <div class="step-body">
            <h4>ACCOUNT_REG</h4>
            <p>注册接入凭证。解锁基础权限，建立你的数字人格。</p>
          </div>
        </div>

        <div class="guide-connector">></div>

        <div class="guide-step">
          <div class="step-header">
            <span class="step-idx">02</span>
            <span class="step-line"></span>
          </div>
          <div class="step-body">
            <h4>PROFILE_SYNC</h4>
            <p>上传头像数据，设置个性签名签名。定义你的唯一标识符。</p>
          </div>
        </div>

        <div class="guide-connector">></div>

        <div class="guide-step">
          <div class="step-header">
            <span class="step-idx">03</span>
            <span class="step-line"></span>
          </div>
          <div class="step-body">
            <h4>EXPLORE_NET</h4>
            <p>浏览节点内容。发送你的第一个信号波（作品/评论）。</p>
          </div>
        </div>
      </div>
    </section>

    <section class="section-block">
      <div class="section-label">
        <span class="icon">■</span> COMMS_LINK // CHANNELS
      </div>

      <div class="group-wrapper">
        <div class="comms-card" @click="joinQQGroup('784251645')">
          <div class="comms-status">
            <span class="live-dot"></span> SIGNAL_STRONG
          </div>
          <div class="comms-main">
            <div class="icon-box">T</div>
            <div class="info-box">
              <h3>太虚坛 // MAIN_HUB</h3>
              <div class="code-box">ID: 784251645</div>
              <p>最大外部无门槛交流节点。</p>
            </div>
          </div>
          <button class="connect-btn">CONNECT [ + ]</button>
        </div>

        <div class="comms-card feedback-mode" @click="joinQQGroup('718834306')">
          <div class="comms-status warning">
            <span class="live-dot yellow"></span> FEEDBACK_LINE
          </div>
          <div class="comms-main">
            <div class="icon-box">F</div>
            <div class="info-box">
              <h3>八卦乾听司 // LOGS</h3>
              <div class="code-box">ID: 718834306</div>
              <p>系统反馈与建议收集频段。</p>
            </div>
          </div>
          <button class="connect-btn">TRANSMIT [ + ]</button>
        </div>
      </div>
    </section>

    <section class="section-block">
      <div class="section-label">
        <span class="icon">■</span> FAQ_DATABASE // QUERY
      </div>
      
      <div class="qa-terminal">
        <div class="terminal-header-bar">
          <span>root@taichu:~/faq# view_all</span>
          <div class="win-controls">□ ■ x</div>
        </div>
        
        <div class="qa-entry">
          <div class="qa-q">
            <span class="prompt">$</span> query --target="level_up"
          </div>
          <div class="qa-a">
            > [RESULT]: 每日签到(Daily_Check)、发布内容(Post_Data)、参与活动(Events)均可获取EXP。
          </div>
        </div>

        <div class="qa-entry">
          <div class="qa-q">
            <span class="prompt">$</span> query --target="privileges"
          </div>
          <div class="qa-a">
            > [RESULT]: 高等级解锁：每日发布限额提升(Upload_Limit)、专属标识(Badge)、隐藏区域访问权(Access_Key)。
          </div>
        </div>

        <div class="qa-entry">
          <div class="qa-q">
            <span class="prompt">$</span> contact --admin
          </div>
          <div class="qa-a">
            > [RESULT]: 使用站内反馈模块 或 接入QQ通讯频道。
          </div>
        </div>
        
        <div class="terminal-cursor">_</div>
      </div>
    </section>

    <div class="footer-barcode">
      ||█||| █ ||█ ||| █ || TAICHU_UNIVERSE_VER_5.0
    </div>
  </div>
</template>

<script setup>
const joinQQGroup = (groupNumber) => {
  let qqGroupUrl;
  if (groupNumber === '784251645') {
    qqGroupUrl = 'https://qm.qq.com/q/AlnCgd4EXA';
  } else if (groupNumber === '718834306') {
    qqGroupUrl = 'https://qm.qq.com/q/2t9O9DQlZ0';
  } else {
    qqGroupUrl = `https://qm.qq.com/q/${groupNumber}`;
  }
  window.open(qqGroupUrl, '_blank');
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 核心变量 --- */
.cyber-intro-terminal {
  --red: #D92323;
  --black: #111111;
  --white: #F4F1EA;
  --gray: #333333;
  --light-gray: #e0e0e0;
  --mono: 'JetBrains Mono', monospace;
  --heading: 'Anton', sans-serif;
  
  font-family: var(--mono);
  color: var(--black);
  max-width: 1200px;
  margin: 0 auto;
  position: relative;
  overflow: hidden;
  padding-bottom: 50px;
}

/* 扫描线背景 */
.scan-lines {
  position: fixed; inset: 0; pointer-events: none; z-index: 0;
  background: linear-gradient(rgba(18, 16, 16, 0) 50%, rgba(0, 0, 0, 0.05) 50%);
  background-size: 100% 4px;
}

/* --- 1. Hero Section --- */
.hero-matrix {
  position: relative;
  background: var(--black);
  color: var(--white);
  padding: 60px 40px;
  margin-bottom: 60px;
  border-bottom: 4px solid var(--red);
  overflow: hidden;
}

.hero-decoration {
  font-size: 0.8rem;
  color: var(--red);
  margin-bottom: 20px;
  display: flex;
  gap: 10px;
  align-items: center;
}
.pulse-dot { animation: pulse 1s infinite; }

.hero-content { position: relative; z-index: 2; }

.glitch-title {
  font-family: var(--heading);
  font-size: 5rem;
  margin: 0;
  line-height: 1;
  text-transform: uppercase;
  position: relative;
  letter-spacing: 2px;
}
/* Glitch 效果简写 */
.glitch-title::before {
  content: attr(data-text);
  position: absolute; left: -2px; text-shadow: 1px 0 blue; top: 0;
  color: white; background: var(--black); overflow: hidden; clip: rect(0, 900px, 0, 0); 
  animation: glitch-anim-2 3s infinite linear alternate-reverse;
}

.hero-sub-box {
  font-size: 1.5rem;
  margin: 10px 0 20px 0;
  font-weight: 700;
}
.bracket { color: var(--red); }

.hero-desc {
  color: #ccc;
  line-height: 1.6;
  border-left: 2px solid var(--red);
  padding-left: 15px;
}

.bg-giant-text {
  position: absolute;
  right: -20px;
  bottom: -40px;
  font-size: 10rem;
  font-family: var(--heading);
  color: rgba(255,255,255,0.03);
  pointer-events: none;
  z-index: 1;
}

/* --- 通用 Section --- */
.section-block {
  margin-bottom: 60px;
  position: relative;
  z-index: 2;
}
.section-label {
  font-family: var(--heading);
  font-size: 1.5rem;
  margin-bottom: 20px;
  display: flex;
  align-items: center;
  gap: 10px;
  border-bottom: 2px solid var(--black);
  padding-bottom: 5px;
}
.icon { color: var(--red); font-size: 0.8em; }

/* --- 2. About Card --- */
.cyber-card {
  border: 2px solid var(--black);
  background: #fff;
  box-shadow: 8px 8px 0 rgba(0,0,0,0.15);
}
.intro-card .card-strip {
  background: var(--black);
  color: var(--white);
  padding: 5px 15px;
  font-size: 0.7rem;
  text-align: right;
}
.intro-card .card-body { padding: 30px; }
.highlight-paragraph { font-size: 1.1rem; line-height: 1.8; }
.marker { color: var(--red); font-weight: bold; }
.code-highlight { background: #eee; padding: 2px 6px; border: 1px solid #ccc; font-weight: bold; }
.divider-dashed { border-bottom: 1px dashed #ccc; margin: 20px 0; }
.tag { color: var(--red); margin-right: 5px; }
.accent-text { border-bottom: 2px solid var(--red); font-weight: bold; }
.terminal-text { 
  background: #f4f4f4; border-left: 4px solid var(--black); 
  padding: 15px; margin-top: 20px; color: #555; font-style: italic; 
}

/* --- 3. Guide Grid --- */
.guide-grid {
  display: flex;
  justify-content: space-between;
  align-items: stretch;
  gap: 20px;
}
.guide-step {
  flex: 1;
  border: 2px solid var(--black);
  padding: 20px;
  background: #fff;
  transition: transform 0.2s;
}
.guide-step:hover {
  transform: translateY(-5px);
  box-shadow: 5px 5px 0 var(--red);
  border-color: var(--red);
}
.step-header {
  display: flex; align-items: center; gap: 10px; margin-bottom: 15px;
}
.step-idx {
  font-family: var(--heading); font-size: 2rem; color: var(--black);
}
.step-line { flex: 1; height: 2px; background: var(--black); }
.step-body h4 { margin: 0 0 10px 0; font-weight: bold; }
.step-body p { font-size: 0.9rem; color: #666; margin: 0; }

.guide-connector {
  display: flex; align-items: center; justify-content: center;
  font-size: 2rem; font-weight: bold; color: #ccc;
}

/* --- 4. Comms Channels --- */
.group-wrapper {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 30px;
}
.comms-card {
  border: 2px solid var(--black);
  background: #fff;
  display: flex;
  flex-direction: column;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
  overflow: hidden;
}
.comms-card:hover {
  background: var(--black);
  color: var(--white);
  transform: translate(-4px, -4px);
  box-shadow: 8px 8px 0 var(--red);
}
/* 悬停时改变内部元素颜色 */
.comms-card:hover .comms-status { border-bottom-color: #555; }
.comms-card:hover .icon-box { background: var(--white); color: var(--black); }
.comms-card:hover .code-box { background: #333; color: #fff; border-color: #555; }
.comms-card:hover .connect-btn { background: var(--red); color: var(--white); }

.comms-status {
  padding: 8px 15px; border-bottom: 2px solid var(--black);
  font-size: 0.7rem; display: flex; align-items: center; gap: 8px;
}
.live-dot { width: 8px; height: 8px; background: #00ff00; border-radius: 50%; box-shadow: 0 0 5px #00ff00; }
.live-dot.yellow { background: #ffcc00; box-shadow: 0 0 5px #ffcc00; }

.comms-main { padding: 20px; display: flex; gap: 15px; flex: 1; }
.icon-box {
  width: 50px; height: 50px; background: var(--black); color: var(--white);
  display: flex; justify-content: center; align-items: center;
  font-family: var(--heading); font-size: 1.5rem;
}
.info-box h3 { margin: 0 0 5px 0; font-size: 1.1rem; }
.code-box {
  display: inline-block; background: #eee; padding: 2px 5px; 
  font-size: 0.8rem; border: 1px solid #ccc; margin-bottom: 8px;
}
.info-box p { margin: 0; font-size: 0.85rem; opacity: 0.8; }

.connect-btn {
  border: none; border-top: 2px solid var(--black);
  background: transparent; padding: 10px;
  font-family: var(--mono); font-weight: bold; cursor: pointer;
  text-align: right;
}

/* --- 5. QA Terminal --- */
.qa-terminal {
  background: #1e1e1e;
  color: #ddd;
  padding: 0;
  border: 2px solid var(--black);
  box-shadow: 8px 8px 0 rgba(0,0,0,0.2);
  font-family: 'Consolas', monospace;
}
.terminal-header-bar {
  background: #333; color: #fff; padding: 5px 10px;
  font-size: 0.8rem; display: flex; justify-content: space-between;
}
.qa-entry {
  padding: 15px 20px;
  border-bottom: 1px solid #333;
}
.qa-q { color: #fff; margin-bottom: 5px; }
.prompt { color: var(--red); margin-right: 10px; }
.qa-a { color: #00ff00; font-size: 0.9rem; margin-left: 20px; opacity: 0.9; }
.terminal-cursor {
  padding: 10px 20px; color: #00ff00; animation: blink 1s infinite;
}

/* Footer */
.footer-barcode {
  text-align: center; font-size: 1.2rem; opacity: 0.5; margin-top: 40px;
  letter-spacing: 5px; font-weight: 900;
}

/* --- 动画 --- */
@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0; } 100% { opacity: 1; } }
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
@keyframes glitch-anim-2 {
  0% { clip: rect(20px, 9999px, 10px, 0); }
  100% { clip: rect(50px, 9999px, 80px, 0); }
}

/* --- 响应式 --- */
@media (max-width: 768px) {
  .hero-matrix { padding: 30px 20px; }
  .glitch-title { font-size: 3rem; }
  .guide-grid { flex-direction: column; gap: 0; }
  .guide-connector { transform: rotate(90deg); padding: 10px; }
  .bg-giant-text { display: none; }
}
</style>