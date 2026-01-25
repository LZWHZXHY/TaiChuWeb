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
              <button class="cyber-btn primary" @click="handleExplore">
                立即查看 <span class="arrow">→</span>
              </button>
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
import { useRouter } from 'vue-router'; // 引入路由
import * as THREE from 'three';
import gsap from 'gsap'; 

const router = useRouter(); // 初始化路由

// --- 数据源 (已添加 URL) ---
const slides = [
  {
    tag: '联动',
    title: '火柴人|柴圈柴设OC社区',
    shortTitle: '火柴人频道',
    desc: '国内QQ最大的火柴人频道',
    image: 'https://groupprocover.gtimg.cn/55970351642777069?imageView2/1/w/750/h/388&t=1767508176417',
    url: 'https://pd.qq.com/s/g719558f5?b=9', 
  },{
    tag: '宣传',
    title: '太初寰宇B站官方账号',
    shortTitle: '视频频道',
    desc: '目前的官网视频上传地',
    image: 'https://image2url.com/r2/default/images/1769276795298-0c078a07-dce2-4160-90e8-a787bcc339f4.png',
    url: 'https://space.bilibili.com/3546736970696727?spm_id_from=333.337.0.0', 
  }
];

const currentIndex = ref(0);
const progress = ref(0);
const visualContainer = ref(null);
const INTERVAL = 10000; 

let timer = null;
let progressTimer = null;

// --- 跳转逻辑 (新增) ---
const handleExplore = () => {
  const targetUrl = slides[currentIndex.value].url;
  
  if (!targetUrl) return;

  // 判断是外部链接还是内部路由
  if (targetUrl.startsWith('http')) {
    window.open(targetUrl, '_blank'); // 新窗口打开外部链接
  } else {
    router.push(targetUrl).catch(err => {
      console.warn('Router navigation failed:', err);
    }); // 站内跳转
  }
};

// --- Three.js 核心变量 ---
let scene, camera, renderer, material, mesh;
let textures = [];
let isAnimating = false;

// --- 1. Vertex Shader ---
const vertexShader = `
  varying vec2 vUv;
  void main() {
    vUv = uv;
    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
  }
`;

// --- 2. Fragment Shader ---
const fragmentShader = `
  uniform float uProgress;
  uniform float uIntensity;
  uniform sampler2D uTexture1;
  uniform sampler2D uTexture2;
  varying vec2 vUv;

  float random(vec2 st) {
      return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
  }

  void main() {
    vec2 uv = vUv;
    float dispersal = sin(uProgress * 3.14159);
    float noise = random(vec2(uv.x * 50.0, uv.y * 50.0 + uProgress));
    vec2 offset = vec2(
        (noise - 0.5) * uIntensity * dispersal,
        (random(uv + noise) - 0.5) * uIntensity * dispersal
    );
    vec4 t1 = texture2D(uTexture1, uv + offset);
    vec4 t2 = texture2D(uTexture2, uv + offset);
    gl_FragColor = mix(t1, t2, uProgress);
  }
`;

// --- 初始化 Three.js ---
const initVisualEffect = () => {
  if (!visualContainer.value) return;
  const width = visualContainer.value.clientWidth;
  const height = visualContainer.value.clientHeight;

  scene = new THREE.Scene();
  camera = new THREE.OrthographicCamera(width / -2, width / 2, height / 2, height / -2, 1, 1000);
  camera.position.z = 1;

  renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true });
  renderer.setSize(width, height);
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
  visualContainer.value.appendChild(renderer.domElement);

  const loader = new THREE.TextureLoader();
  let loadedCount = 0;

  slides.forEach((slide, index) => {
    loader.load(slide.image, (tex) => {
      tex.minFilter = THREE.LinearFilter;
      tex.magFilter = THREE.LinearFilter;
      tex.wrapS = THREE.ClampToEdgeWrapping;
      tex.wrapT = THREE.ClampToEdgeWrapping;
      
      textures[index] = tex;
      loadedCount++;

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
      uIntensity: { value: 1.5 },
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

  const nextTex = textures[nextIdx];
  material.uniforms.uTexture2.value = nextTex;
  
  gsap.to(material.uniforms.uProgress, {
    value: 1,
    duration: 1.2,
    ease: "power3.inOut",
    onComplete: () => {
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

.text-content {
  display: flex;
  flex-direction: column;
  justify-content: center;
  z-index: 2;
  pointer-events: auto; 
}

.visual-content {
  position: relative;
  width: 100%;
  height: 100%;
  border-radius: 12px;
  overflow: hidden;
  clip-path: polygon(10% 0, 100% 0, 100% 90%, 90% 100%, 0 100%, 0 10%);
}

.glow-effect {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  pointer-events: none;
  box-shadow: inset 0 0 50px rgba(0, 240, 255, 0.1);
  z-index: 2;
}

.meta-tag { display: flex; align-items: center; gap: 10px; margin-bottom: 20px; }
.tag-box { background: rgba(0, 240, 255, 0.1); color: #00F0FF; padding: 4px 10px; font-size: 12px; font-weight: bold; border: 1px solid rgba(0, 240, 255, 0.3); }
.tag-line { height: 1px; width: 50px; background: #00F0FF; }
.main-title { font-family: 'Anton', sans-serif; font-size: 5rem; line-height: 1; margin-bottom: 20px; text-transform: uppercase; letter-spacing: 2px; background: linear-gradient(to right, #fff, #aaa); -webkit-background-clip: text; -webkit-text-fill-color: transparent; }
.accent-dot { color: #00F0FF; -webkit-text-fill-color: #00F0FF; }
.description { font-size: 1rem; color: #bbb; line-height: 1.6; max-width: 90%; margin-bottom: 40px; border-left: 2px solid #333; padding-left: 20px; }
.action-area { display: flex; gap: 20px; }

.cyber-btn { padding: 12px 30px; font-family: 'JetBrains Mono', monospace; font-weight: bold; cursor: pointer; transition: 0.3s; border: none; text-transform: uppercase; }
.cyber-btn.primary { background: #fff; color: #000; clip-path: polygon(0 0, 100% 0, 100% 80%, 90% 100%, 0 100%); }
.cyber-btn.primary:hover { background: #00F0FF; box-shadow: 0 0 20px rgba(0, 240, 255, 0.4); }
.cyber-btn.secondary { background: transparent; color: #fff; border: 1px solid rgba(255,255,255,0.3); }
.cyber-btn.secondary:hover { border-color: #fff; background: rgba(255,255,255,0.05); }

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

@media (max-width: 900px) {
  .hero-wrapper { height: auto; margin: 20px; }
  .hero-stage { grid-template-columns: 1fr; gap: 30px; padding: 30px; }
  .main-title { font-size: 3rem; }
  .visual-content { height: 300px; }
}

.slide-up-enter-active, .slide-up-leave-active { transition: all 0.5s cubic-bezier(0.2, 1, 0.3, 1); }
.slide-up-enter-from { opacity: 0; transform: translateY(30px); }
.slide-up-leave-to { opacity: 0; transform: translateY(-30px); }
</style>