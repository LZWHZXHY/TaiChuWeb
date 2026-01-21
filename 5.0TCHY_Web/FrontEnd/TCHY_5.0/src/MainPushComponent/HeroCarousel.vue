<template>
  <div class="hero-wrapper">
    
    <div class="hero-stage">
      <div class="text-content">
        <transition name="slide-up" mode="out-in">
          <div :key="currentIndex" class="info-group">
            <div class="meta-tag">
              <span class="tag-box"># {{ slides[currentIndex].tag }}</span>
              <span class="tag-line"></span>
            </div>
            
            <h1 class="main-title">
              {{ slides[currentIndex].title }}
              <span class="accent-dot">.</span>
            </h1>
            
            <p class="description">
              {{ slides[currentIndex].desc }}
            </p>
            
            <div class="action-area">
              <button class="cyber-btn primary">立即查看 <span class="arrow">→</span></button>
              <button class="cyber-btn secondary">详情</button>
            </div>
          </div>
        </transition>
      </div>

      <div class="visual-content" ref="visualContainer">
        <div class="glow-effect"></div>
      </div>
    </div>

    <div class="controls-bar">
      <div class="progress-track">
        <div class="progress-fill" :style="{ width: progress + '%' }"></div>
      </div>

      <div class="thumb-list">
        <div 
          v-for="(slide, index) in slides" 
          :key="index"
          class="thumb-item"
          :class="{ active: index === currentIndex }"
          @click="manualSwitch(index)"
        >
          <div class="thumb-cover">
            <img :src="slide.image" />
          </div>
          <div class="thumb-info">
            <span class="thumb-idx">0{{ index + 1 }}</span>
            <span class="thumb-text">{{ slide.shortTitle }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, nextTick } from 'vue';
import * as THREE from 'three';
import gsap from 'gsap'; // 确保安装: npm install gsap

// --- 数据源 ---
const slides = [
  {
    tag: 'FEATURED_EVENT',
    title: 'NEON GENESIS',
    shortTitle: '新世纪福音',
    desc: '探索赛博朋克美学与现代WebGL技术的完美融合。参与年度开发者大会，见证数字孪生城市的诞生。',
    image: 'https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?ixlib=rb-4.0.3&w=1200&q=80'
  },
  {
    tag: 'LATEST_GAME',
    title: 'VOID WALKER',
    shortTitle: '虚空行者',
    desc: '独立游戏杰作《虚空行者》试玩版发布。在破碎的维度中寻找回家的路，体验极致的粒子特效战斗。',
    image: 'https://images.unsplash.com/photo-1542751371-adc38448a05e?ixlib=rb-4.0.3&w=1200&q=80'
  },
  {
    tag: 'ART_EXHIBITION',
    title: 'DIGITAL SOUL',
    shortTitle: '数字灵魂',
    desc: 'AI 艺术生成大赛金奖作品展。当算法拥有了“灵魂”，它描绘出的梦境会是什么样子？',
    image: 'https://image2url.com/r2/default/images/1768800240160-f79ab1ee-64bc-43da-9a51-7977301da6de.blob'
  }
];

const currentIndex = ref(0);
const progress = ref(0);
const visualContainer = ref(null);
const INTERVAL = 10000; // 8秒切换

let timer = null;
let progressTimer = null;

// --- Three.js 核心变量 ---
let scene, camera, renderer, material, mesh;
let textures = [];
let isAnimating = false;

// --- 1. Vertex Shader (顶点着色器 - 保持标准) ---
const vertexShader = `
  varying vec2 vUv;
  void main() {
    vUv = uv;
    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
  }
`;

// --- 2. Fragment Shader (片段着色器 - 粒子破碎核心) ---
const fragmentShader = `
  uniform float uProgress;
  uniform float uIntensity; // 控制炸裂范围
  uniform sampler2D uTexture1;
  uniform sampler2D uTexture2;
  varying vec2 vUv;

  // 伪随机函数
  float random(vec2 st) {
      return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
  }

  void main() {
    vec2 uv = vUv;
    
    // 1. 扩散力度曲线：0 -> 1 -> 0
    // 在动画中间(0.5)时，dispersal 为 1.0，粒子散得最开
    float dispersal = sin(uProgress * 3.14159);
    
    // 2. 生成随机噪声
    // 乘以 50.0 是为了增加随机的高频程度（产生颗粒感）
    float noise = random(vec2(uv.x * 50.0, uv.y * 50.0 + uProgress));
    
    // 3. 计算位移
    // 基于噪声，让像素向随机方向偏移
    vec2 offset = vec2(
        (noise - 0.5) * uIntensity * dispersal,
        (random(uv + noise) - 0.5) * uIntensity * dispersal
    );

    // 4. 采样颜色 (加上偏移量)
    vec4 t1 = texture2D(uTexture1, uv + offset);
    vec4 t2 = texture2D(uTexture2, uv + offset);

    // 5. 混合
    gl_FragColor = mix(t1, t2, uProgress);
  }
`;

// --- 初始化 Three.js ---
const initVisualEffect = () => {
  if (!visualContainer.value) return;
  const width = visualContainer.value.clientWidth;
  const height = visualContainer.value.clientHeight;

  scene = new THREE.Scene();
  // 使用正交相机确保图片铺满，无透视变形
  camera = new THREE.OrthographicCamera(width / -2, width / 2, height / 2, height / -2, 1, 1000);
  camera.position.z = 1;

  renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true });
  renderer.setSize(width, height);
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
  visualContainer.value.appendChild(renderer.domElement);

  // 预加载所有图片纹理
  const loader = new THREE.TextureLoader();
  let loadedCount = 0;

  slides.forEach((slide, index) => {
    loader.load(slide.image, (tex) => {
      // 优化纹理设置
      tex.minFilter = THREE.LinearFilter;
      tex.magFilter = THREE.LinearFilter;
      tex.wrapS = THREE.ClampToEdgeWrapping;
      tex.wrapT = THREE.ClampToEdgeWrapping;
      
      // 按顺序存入数组 (简单的预加载处理)
      textures[index] = tex;
      loadedCount++;

      // 当所有图片加载完毕，或者是第一张加载完，初始化Mesh
      if (loadedCount === slides.length) {
         if (!mesh) setupMesh(textures[0]);
      } else if (index === 0 && !mesh) {
         setupMesh(tex);
      }
    });
  });
};

const setupMesh = (initialTexture) => {
  const width = visualContainer.value.clientWidth;
  const height = visualContainer.value.clientHeight;
  const geometry = new THREE.PlaneGeometry(width, height);
  
  material = new THREE.ShaderMaterial({
    uniforms: {
      uProgress: { value: 0 },
      uIntensity: { value: 1.5 }, // 🔥 强度加大，让粒子炸得更远
      uTexture1: { value: initialTexture },
      uTexture2: { value: initialTexture }
    },
    vertexShader,
    fragmentShader,
    transparent: true
  });

  mesh = new THREE.Mesh(geometry, material);
  scene.add(mesh);
  
  animateWebGL();
};

const animateWebGL = () => {
  requestAnimationFrame(animateWebGL);
  renderer.render(scene, camera);
};

// --- 动画切换逻辑 ---
const runTransition = (nextIdx) => {
  if (isAnimating || !material || !textures[nextIdx]) return;
  isAnimating = true;

  // 设置目标纹理
  const nextTex = textures[nextIdx];
  material.uniforms.uTexture2.value = nextTex;
  
  // GSAP 动画：爆发感
  gsap.to(material.uniforms.uProgress, {
    value: 1,
    duration: 1.2, // 1.2秒完成切换
    ease: "power3.inOut", // 🔥 更有力量感的缓动曲线
    onComplete: () => {
      // 动画结束，重置状态
      material.uniforms.uTexture1.value = nextTex;
      material.uniforms.uProgress.value = 0;
      isAnimating = false;
      currentIndex.value = nextIdx;
    }
  });
};

// --- 轮播控制逻辑 ---
const nextSlide = () => {
  const nextIdx = (currentIndex.value + 1) % slides.length;
  runTransition(nextIdx);
  resetProgress();
};

const manualSwitch = (index) => {
  if (index === currentIndex.value || isAnimating) return;
  runTransition(index);
  resetProgress();
  clearInterval(timer);
  timer = setInterval(nextSlide, INTERVAL);
};

const resetProgress = () => {
  progress.value = 0;
  clearInterval(progressTimer);
  const step = 100 / (INTERVAL / 50); 
  progressTimer = setInterval(() => {
    progress.value += step;
    if (progress.value >= 100) progress.value = 100;
  }, 50);
};

// 窗口自适应
const onResize = () => {
  if (!visualContainer.value || !renderer || !camera) return;
  const width = visualContainer.value.clientWidth;
  const height = visualContainer.value.clientHeight;
  
  renderer.setSize(width, height);
  
  camera.left = width / -2;
  camera.right = width / 2;
  camera.top = height / 2;
  camera.bottom = height / -2;
  camera.updateProjectionMatrix();
  
  if (mesh) {
    mesh.geometry.dispose();
    mesh.geometry = new THREE.PlaneGeometry(width, height);
  }
};

onMounted(async () => {
  await nextTick();
  initVisualEffect();
  window.addEventListener('resize', onResize);
  
  resetProgress();
  timer = setInterval(nextSlide, INTERVAL);
});

onUnmounted(() => {
  clearInterval(timer);
  clearInterval(progressTimer);
  window.removeEventListener('resize', onResize);
  if (renderer) renderer.dispose();
  if (material) material.dispose();
  if (scene) scene.clear();
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.hero-wrapper {
  position: relative;
  width: 100%;
  max-width: 1400px;
  margin: 40px auto;
  height: 600px;
  background: rgba(20, 20, 20, 0.4);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 24px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 30px 60px rgba(0,0,0,0.5);
  font-family: 'JetBrains Mono', sans-serif;
  color: #fff;
}

.hero-stage {
  flex: 1;
  display: grid;
  grid-template-columns: 45% 55%;
  padding: 50px;
  overflow: hidden;
}

/* 左侧文本 */
.text-content {
  display: flex;
  flex-direction: column;
  justify-content: center;
  z-index: 2;
  pointer-events: auto; 
}

/* 右侧视觉容器 */
.visual-content {
  position: relative;
  width: 100%;
  height: 100%;
  border-radius: 12px;
  overflow: hidden;
  clip-path: polygon(10% 0, 100% 0, 100% 90%, 90% 100%, 0 100%, 0 10%);
}

/* 装饰性光效 */
.glow-effect {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  pointer-events: none;
  box-shadow: inset 0 0 50px rgba(0, 240, 255, 0.1);
  z-index: 2;
}

/* 文本样式 */
.meta-tag { display: flex; align-items: center; gap: 10px; margin-bottom: 20px; }
.tag-box { background: rgba(0, 240, 255, 0.1); color: #00F0FF; padding: 4px 10px; font-size: 12px; font-weight: bold; border: 1px solid rgba(0, 240, 255, 0.3); }
.tag-line { height: 1px; width: 50px; background: #00F0FF; }
.main-title { font-family: 'Anton', sans-serif; font-size: 5rem; line-height: 1; margin-bottom: 20px; text-transform: uppercase; letter-spacing: 2px; background: linear-gradient(to right, #fff, #aaa); -webkit-background-clip: text; -webkit-text-fill-color: transparent; }
.accent-dot { color: #00F0FF; -webkit-text-fill-color: #00F0FF; }
.description { font-size: 1rem; color: #bbb; line-height: 1.6; max-width: 90%; margin-bottom: 40px; border-left: 2px solid #333; padding-left: 20px; }
.action-area { display: flex; gap: 20px; }

/* 按钮样式 */
.cyber-btn { padding: 12px 30px; font-family: 'JetBrains Mono', monospace; font-weight: bold; cursor: pointer; transition: 0.3s; border: none; text-transform: uppercase; }
.cyber-btn.primary { background: #fff; color: #000; clip-path: polygon(0 0, 100% 0, 100% 80%, 90% 100%, 0 100%); }
.cyber-btn.primary:hover { background: #00F0FF; box-shadow: 0 0 20px rgba(0, 240, 255, 0.4); }
.cyber-btn.secondary { background: transparent; color: #fff; border: 1px solid rgba(255,255,255,0.3); }
.cyber-btn.secondary:hover { border-color: #fff; background: rgba(255,255,255,0.05); }

/* 底部控制栏 */
.controls-bar { height: 100px; border-top: 1px solid rgba(255,255,255,0.1); background: rgba(0,0,0,0.2); display: flex; flex-direction: column; }
.progress-track { width: 100%; height: 2px; background: rgba(255,255,255,0.1); }
.progress-fill { height: 100%; background: #00F0FF; box-shadow: 0 0 10px #00F0FF; }
.thumb-list { flex: 1; display: flex; align-items: center; padding: 0 50px; gap: 20px; }
.thumb-item { display: flex; align-items: center; gap: 15px; cursor: pointer; opacity: 0.5; transition: 0.3s; padding: 10px; border-radius: 8px; }
.thumb-item:hover { opacity: 0.8; background: rgba(255,255,255,0.05); }
.thumb-item.active { opacity: 1; background: rgba(255,255,255,0.1); border: 1px solid rgba(0, 240, 255, 0.3); }
.thumb-cover { width: 60px; height: 40px; border-radius: 4px; overflow: hidden; }
.thumb-cover img { width: 100%; height: 100%; object-fit: cover; }
.thumb-info { display: flex; flex-direction: column; }
.thumb-idx { font-size: 10px; color: #00F0FF; }
.thumb-text { font-weight: bold; font-size: 14px; }

/* 响应式 */
@media (max-width: 900px) {
  .hero-wrapper { height: auto; margin: 20px; }
  .hero-stage { grid-template-columns: 1fr; gap: 30px; padding: 30px; }
  .main-title { font-size: 3rem; }
  .visual-content { height: 300px; }
}

/* Vue Transition */
.slide-up-enter-active, .slide-up-leave-active { transition: all 0.5s cubic-bezier(0.2, 1, 0.3, 1); }
.slide-up-enter-from { opacity: 0; transform: translateY(30px); }
.slide-up-leave-to { opacity: 0; transform: translateY(-30px); }
</style>