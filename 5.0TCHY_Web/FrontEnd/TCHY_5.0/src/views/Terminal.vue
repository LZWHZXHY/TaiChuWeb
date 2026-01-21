<template>
  <div class="crt-container" @click="focusInput">
    <div class="scanlines"></div>
    <div class="screen-flicker"></div>
    
    <div class="terminal-content" ref="terminalContainer">
      
      <div class="banner">
        <h1 class="neon-title">太初寰宇</h1>
        <div class="sub-title">TAI CHU UNIVERSE SYSTEM [KERNEL ACCESS]</div>
        <div class="system-border">----------------------------------------</div>
        <div class="system-info">
          <span>Connection: <span class="highlight">SECURE</span></span>
          <span>Route: <span class="highlight">{{ currentRoutePath }}</span></span>
        </div>
      </div>

      <div class="output-area">
        <div v-for="(line, index) in lines" :key="index" class="line">
          
          <template v-if="line.type === 'command'">
            <div class="user-line">
              <span class="prompt">operator@taichu:{{ currentPath }}$</span>
              <span class="cmd-text">{{ line.text }}</span>
            </div>
          </template>
          
          <template v-else>
            <div v-if="line.isTyping" class="response typing-effect">
              {{ line.text }}<span class="cursor-block">█</span>
            </div>
            <div v-else class="response fade-in" :class="line.styleClass" v-html="line.text"></div>
          </template>
        </div>
      </div>

      <div class="input-line" v-show="!isSystemBusy">
        <span class="prompt">operator@taichu:{{ currentPath }}$</span>
        <input 
          ref="cmdInput"
          v-model="inputValue"
          type="text" 
          class="cmd-input"
          autofocus
          spellcheck="false"
          autocomplete="off"
          @keydown.enter="handleEnter"
          @keydown.up.prevent="navigateHistory('up')"
          @keydown.down.prevent="navigateHistory('down')"
        >
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, nextTick, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router'; // 引入路由

// --- 1. 深度绑定：路由与系统状态 ---
const router = useRouter(); // 获取路由实例
const route = useRoute();   // 获取当前路由信息

// 模拟文件路径 (对应真实路由)
const currentPath = ref('~');
const currentRoutePath = computed(() => route ? route.path : '/');

// --- 2. 状态定义 ---
const inputValue = ref('');
const lines = ref([]); 
const cmdInput = ref(null);
const isSystemBusy = ref(false); // 关键：系统忙碌时不接受输入
const commandHistory = ref([]);
const historyIndex = ref(-1);

// --- 3. 打字机核心逻辑 ---
const delay = (ms) => new Promise(resolve => setTimeout(resolve, ms));

/**
 * 核心输出函数
 * @param {string} content - 要输出的内容
 * @param {boolean} isHtml - 是否是 HTML 代码 (HTML 不打字，直接显示)
 * @param {string} style - 额外的 CSS 类
 */
const print = async (content, isHtml = false, style = '') => {
  // 如果是 HTML 或者用户不需要打字效果，直接推入
  if (isHtml) {
    lines.value.push({ type: 'response', text: content, isTyping: false, styleClass: style });
    await scrollToBottom();
    await delay(300); // 稍微停顿，给人一种"数据加载完毕"的感觉
    return;
  }

  // 如果是纯文本，开始打字特效
  const lineIndex = lines.value.push({ 
    type: 'response', 
    text: '', 
    isTyping: true, 
    styleClass: style 
  }) - 1;

  const chars = content.split('');
  for (const char of chars) {
    lines.value[lineIndex].text += char;
    // 随机打字速度，模拟真人或机器处理
    await delay(Math.random() * 30 + 10); 
    await scrollToBottom();
  }

  // 打字结束，移除光标
  lines.value[lineIndex].isTyping = false;
};

// --- 4. 指令集 (深度绑定版) ---
const commands = {
  // 基础指令
  help: async () => {
    // 复杂排版直接用 HTML 输出，不打字
    await print(`
      <div class="help-grid">
        <div class="title">:: SYSTEM PROTOCOLS ::</div>
        <div class="row"><span class="cmd">goto [path]</span> : 空间跳跃 (路由跳转)</div>
        <div class="row"><span class="cmd">back</span>        : 返回上一级位面</div>
        <div class="row"><span class="cmd">theme [mode]</span>: 更改视觉矩阵 (dark/light)</div>
        <div class="row"><span class="cmd">scan</span>        : 扫描当前页面结构</div>
        <div class="row"><span class="cmd">clear</span>       : 清屏</div>
      </div>
    `, true);
  },

  // --- 深度绑定功能 1: 路由控制 ---
  goto: async (args) => {
    const target = args[0];
    if (!target) {
      await print("Error: Destination coordinates missing.", false, "error");
      return;
    }
    
    await print(`Initiating warp drive to sector [${target}]...`);
    await delay(500);
    
    try {
      // 尝试使用 Vue Router 跳转
      // 假设你有 /about, /blog, /project 这些路由
      await router.push(target); 
      currentPath.value = target; // 更新模拟路径
      await print(`Arrival confirmed: ${target}`, false, "success");
    } catch (e) {
      await print(`Warp failure: Route '${target}' invalid.`, false, "error");
    }
  },

  back: async () => {
    await print("Retreating to previous coordinates...");
    router.back();
  },

  // --- 深度绑定功能 2: DOM/UI 控制 ---
  theme: async (args) => {
    const mode = args[0];
    if (mode === 'light') {
      await print("Warning: Light mode may damage retinas. Applying...", false, "warn");
      document.body.style.backgroundColor = '#f0f0f0';
      document.body.style.filter = 'invert(1)'; // 简单粗暴的反色演示
    } else {
      await print("Restoring Dark Matter mode...", false, "success");
      document.body.style.backgroundColor = '';
      document.body.style.filter = '';
    }
  },

  // --- 模拟功能 ---
  scan: async () => {
    await print("Analyzing page structure...");
    await delay(400);
    // 真的去获取当前页面的标题
    const pageTitle = document.title;
    await print(`Target Identified: [ ${pageTitle} ]`, false, "highlight");
    await print("System Integrity: Stable.");
  },

  clear: () => {
    lines.value = [];
  }
};

// --- 5. 逻辑处理 ---
const handleEnter = async () => {
  const raw = inputValue.value.trim();
  if (!raw) return;

  // 锁定输入框
  isSystemBusy.value = true;
  
  // 上屏用户输入
  lines.value.push({ type: 'command', text: raw });
  commandHistory.value.push(raw);
  historyIndex.value = commandHistory.value.length;
  inputValue.value = '';
  await scrollToBottom();

  // 解析指令 "goto /about" -> cmd="goto", args=["/about"]
  const [cmd, ...args] = raw.split(' ');
  
  if (commands[cmd]) {
    await commands[cmd](args);
  } else {
    await print(`Protocol '${cmd}' unknown. Input 'help' for manual.`, false, "error");
  }

  // 解锁输入框
  isSystemBusy.value = false;
  
  // 确保输入框重新聚焦
  nextTick(() => {
    cmdInput.value?.focus();
  });
};

const scrollToBottom = async () => {
  await nextTick();
  const container = document.querySelector('.terminal-content');
  if (container) container.scrollTop = container.scrollHeight;
};

const focusInput = () => {
  if (!isSystemBusy.value) {
    cmdInput.value?.focus();
  }
};

const navigateHistory = (direction) => {
  if (commandHistory.value.length === 0) return;
  if (direction === 'up' && historyIndex.value > 0) {
    historyIndex.value--;
    inputValue.value = commandHistory.value[historyIndex.value];
  } else if (direction === 'down') {
    if (historyIndex.value < commandHistory.value.length - 1) {
      historyIndex.value++;
      inputValue.value = commandHistory.value[historyIndex.value];
    } else {
      historyIndex.value = commandHistory.value.length;
      inputValue.value = '';
    }
  }
};

onMounted(async () => {
  // 初始启动序列
  isSystemBusy.value = true;
  await print("Initializing Kernel...");
  await delay(500);
  await print("Mounting Virtual File System...");
  await delay(500);
  await print("Welcome, Operator. System Online.", false, "success");
  isSystemBusy.value = false;
  focusInput();
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Fira+Code:wght@400;700&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Noto+Serif+SC:wght@700&display=swap');

.crt-container {
  width: 100%;
  height: 100vh;
  background-color: #050a14;
  color: #00f3ff;
  font-family: 'Fira Code', monospace;
  overflow: hidden;
  position: relative;
  text-shadow: 0 0 4px rgba(0, 243, 255, 0.6);
}

.terminal-content {
  height: 100%;
  padding: 40px;
  overflow-y: auto;
  box-sizing: border-box;
  z-index: 2;
  position: relative;
}
.terminal-content::-webkit-scrollbar { width: 0; }

/* Banner */
.neon-title {
  font-family: 'Noto Serif SC', serif;
  font-size: 3rem;
  color: #fff;
  text-shadow: 0 0 10px #00f3ff, 0 0 20px #00f3ff;
  margin: 0;
}
.sub-title { letter-spacing: 0.2em; opacity: 0.8; font-size: 0.9em; margin-top: 5px; }
.system-info { font-size: 0.8em; color: #007a80; margin-top: 5px; }
.system-border { opacity: 0.3; }

/* Prompt & Input */
.prompt { color: #ffb000; margin-right: 10px; font-weight: bold; }
.cmd-input { background: transparent; border: none; color: #fff; flex: 1; outline: none; font-family: inherit; font-size: 1rem; caret-color: #ffb000; }
.user-line { margin-top: 10px; margin-bottom: 5px; }
.cmd-text { color: #fff; }

/* Outputs */
.response { line-height: 1.5; margin-bottom: 2px; }
.cursor-block { display: inline-block; width: 10px; height: 1em; background: #00f3ff; animation: blink 1s infinite; vertical-align: middle; margin-left: 2px; }
.error { color: #ff4444; }
.warn { color: #ffbb00; }
.success { color: #00ff41; }
.highlight { color: #ffb000; }

/* HTML Help Grid */
:deep(.help-grid) { border-left: 2px solid #004e53; padding-left: 15px; margin: 10px 0; }
:deep(.help-grid .title) { color: #fff; margin-bottom: 5px; opacity: 0.8; }
:deep(.help-grid .cmd) { color: #ffb000; font-weight: bold; min-width: 120px; display: inline-block; }

/* Animations */
.fade-in { animation: fadeIn 0.3s ease-in; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(5px); } to { opacity: 1; transform: translateY(0); } }
@keyframes blink { 50% { opacity: 0; } }

/* CRT Effects */
.scanlines {
  position: absolute; top: 0; left: 0; width: 100%; height: 100%; pointer-events: none; z-index: 10;
  background: linear-gradient(to bottom, rgba(255,255,255,0), rgba(255,255,255,0) 50%, rgba(0,0,0,0.2) 50%, rgba(0,0,0,0.2));
  background-size: 100% 4px;
}
.screen-flicker {
  position: absolute; top: 0; left: 0; width: 100%; height: 100%; pointer-events: none; z-index: 9;
  background: rgba(0, 243, 255, 0.02); animation: flicker 0.1s infinite;
}
</style>