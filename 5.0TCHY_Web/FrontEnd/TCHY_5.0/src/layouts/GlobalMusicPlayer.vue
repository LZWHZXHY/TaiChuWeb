<template>
  <div class="cyber-audio-engine">
    <audio 
      ref="audioRef" 
      :src="musicStore.currentTrack.url"
      crossorigin="anonymous"
      @ended="handleEnded"
    ></audio>

    <div class="visualizer-container">
      <canvas ref="canvasRef" width="280" height="60"></canvas>
      <div class="track-info-overlay">
        <span class="indicator" :class="{ 'active': musicStore.isPlaying }">▶</span>
        <span class="track-name">{{ musicStore.currentTrack.title }}</span>
      </div>
    </div>

    <div class="tactical-controls">
      <button class="cyber-btn" @click="handleTogglePlay">
        {{ musicStore.isPlaying ? '[ PAUSE ]' : '[ PLAY ]' }}
      </button>
      
      <button 
        class="cyber-btn" 
        :class="{ 'sync-active': musicStore.isGlobalMode }"
        @click="musicStore.toggleMode"
      >
        {{ musicStore.isGlobalMode ? '< SYNC_ON >' : '< PVT_ONLY >' }}
      </button>

      <input 
        type="range" 
        min="0" max="1" step="0.01" 
        v-model="musicStore.volume" 
        @input="updateVolume"
        class="cyber-slider"
      >
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { useMusicStore } from '@/stores/music'

const musicStore = useMusicStore()
const audioRef = ref(null)
const canvasRef = ref(null)

// Web Audio API 变量
let audioCtx = null
let analyser = null
let source = null
let animationId = null

// 1. 初始化音频引擎 (Canvas 频谱图用)
const initAudioEngine = () => {
  if (audioCtx) return // 避免重复初始化

  // 浏览器规定 AudioContext 必须在用户交互后才能创建/恢复
  audioCtx = new (window.AudioContext || window.webkitAudioContext)()
  analyser = audioCtx.createAnalyser()
  analyser.fftSize = 128 // 采样数，决定频谱条的数量

  source = audioCtx.createMediaElementSource(audioRef.value)
  source.connect(analyser)
  analyser.connect(audioCtx.destination)

  drawVisualizer()
}

// 2. 绘制 Canvas 频谱
const drawVisualizer = () => {
  if (!canvasRef.value) return
  
  const canvas = canvasRef.value
  const ctx = canvas.getContext('2d')
  const bufferLength = analyser.frequencyBinCount
  const dataArray = new Uint8Array(bufferLength)

  const draw = () => {
    animationId = requestAnimationFrame(draw)
    analyser.getByteFrequencyData(dataArray)

    // 清空画布 (带一点拖影效果)
    ctx.fillStyle = 'rgba(244, 241, 234, 0.3)' 
    ctx.fillRect(0, 0, canvas.width, canvas.height)

    const barWidth = (canvas.width / bufferLength) * 2.5
    let x = 0

    for (let i = 0; i < bufferLength; i++) {
      const barHeight = (dataArray[i] / 255) * canvas.height
      
      // 赛博红蓝/金色渐变
      const r = barHeight + 50
      const g = 50
      const b = 250 - (i * 2)

      ctx.fillStyle = musicStore.isGlobalMode ? '#f1c40f' : `rgb(${r},${g},${b})`
      ctx.fillRect(x, canvas.height - barHeight, barWidth, barHeight)
      x += barWidth + 1
    }
  }
  draw()
}

// 3. 播放/暂停逻辑 (兼顾 Web Audio API 的唤醒和 Pinia 状态)
const handleTogglePlay = () => {
  if (!audioCtx) initAudioEngine()
  
  if (audioCtx.state === 'suspended') {
    audioCtx.resume()
  }

  // 将实际的播放控制权交给 Pinia 里的 togglePlay
  musicStore.togglePlay()
}

// 监听 Pinia 状态改变，控制 audio 标签 (作为个人模式下的后备补充)
watch(() => musicStore.isPlaying, (newVal) => {
  if (!audioRef.value) return
  if (newVal) {
    audioRef.value.play().catch(e => {
      console.warn('播放被浏览器拦截:', e)
      musicStore.isPlaying = false
    })
  } else {
    audioRef.value.pause()
  }
})

// 音量控制
const updateVolume = () => {
  if (audioRef.value) {
    audioRef.value.volume = musicStore.volume
  }
}


// GlobalMusicPlayer.vue 中的 handleEnded 修正
const handleEnded = () => {
  console.log(">> [AUDIO_SYS] 当前曲目物理结束");
  
  // ⚡ 核心修复：只有在个人模式(PVT)下才设为 false
  // 在全局模式(SYNC)下，我们要保持 isPlaying 为 true，直到下一首信号接管
  if (!musicStore.isGlobalMode) {
    musicStore.isPlaying = false;
  } else {
    console.log(">> [AUDIO_SYS] 正在等待电台指挥部下达切歌指令...");
  }
}

// ⚡ 核心新增：组件挂载时，把音频节点交给 Store，启动 SignalR 电台连接！
onMounted(() => {
  if (audioRef.value) {
    musicStore.initRadio(audioRef.value)
  }
})

// 卸载时清理资源
onUnmounted(() => {
  if (animationId) cancelAnimationFrame(animationId)
  if (audioCtx) audioCtx.close()
})
</script>

<style scoped>
.cyber-audio-engine {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.visualizer-container {
  position: relative;
  width: 100%;
  height: 60px;
  background: var(--ink-black, #111);
  border: 1px solid #333;
  overflow: hidden;
}

canvas {
  width: 100%;
  height: 100%;
  display: block;
}

.track-info-overlay {
  position: absolute;
  top: 4px;
  left: 6px;
  font-size: 10px;
  color: #fff;
  font-family: 'JetBrains Mono', monospace;
  text-shadow: 1px 1px 0 #000;
  pointer-events: none;
}

.indicator {
  color: #666;
  margin-right: 4px;
}
.indicator.active {
  color: #00FF00;
  animation: blink 1s infinite;
}

.tactical-controls {
  display: flex;
  align-items: center;
  gap: 8px;
}

.cyber-btn {
  background: transparent;
  border: 1px solid var(--ink-black, #111);
  color: var(--ink-black, #111);
  font-family: 'JetBrains Mono', monospace;
  font-size: 10px;
  font-weight: bold;
  padding: 4px 8px;
  cursor: pointer;
  transition: 0.2s;
}
.cyber-btn:hover {
  background: var(--ink-black, #111);
  color: #fff;
}
.cyber-btn.sync-active {
  background: #f1c40f;
  border-color: #f1c40f;
  color: #111;
}

.cyber-slider {
  flex-grow: 1;
  height: 2px;
  background: #ccc;
  appearance: none;
  outline: none;
}
.cyber-slider::-webkit-slider-thumb {
  appearance: none;
  width: 8px;
  height: 12px;
  background: var(--ink-black, #111);
  cursor: pointer;
}

@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0; } }
</style>