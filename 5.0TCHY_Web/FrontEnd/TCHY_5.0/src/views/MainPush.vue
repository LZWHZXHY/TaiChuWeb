<template>
  <div class="page-wrapper">
    
    <div ref="containerRef" class="MainBackground"></div>

    <div class="ContentLayer">
      <div class="scroll-content">
        
        <HeroCarousel />

        <ArtMasonry />

        <BlogShowcase />

        <CyberTicker />

        <VideoStream />

        <CyberTicker />

        

        <footer class="cyber-footer">
          <div class="footer-line"></div>
          <p>支持太初寰宇！</p>
          <p class="copyright">© 2026 DESIGNED BY 太初寰宇</p>
        </footer>

      </div>
    </div>


    
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import * as THREE from 'three';

// --- 引入所有子组件 ---
import HeroCarousel from '@/MainPushComponent/HeroCarousel.vue';
import BlogShowcase from '@/MainPushComponent/BlogShowcase.vue';
import CyberTicker from '@/MainPushComponent/CyberTicker.vue';  
import VideoStream from '@/MainPushComponent/VideoStream.vue';  
import ArtMasonry from '@/MainPushComponent/ArtMasonry.vue';   

// --- Three.js 背景逻辑 (保持不变) ---
const containerRef = ref(null);
let scene, camera, renderer, particlesMesh;
let animationId = null;

const initThreeJS = () => {
  scene = new THREE.Scene();
  // 雾效：让远处的粒子隐没在黑暗中，增加深邃感
  scene.fog = new THREE.FogExp2(0x050505, 0.05);

  camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
  camera.position.z = 3;

  renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true });
  renderer.setSize(window.innerWidth, window.innerHeight);
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));

  if (containerRef.value) {
    containerRef.value.innerHTML = '';
    containerRef.value.appendChild(renderer.domElement);
  }

  const particlesGeometry = new THREE.BufferGeometry();
  const particlesCount = 2000;
  const posArray = new Float32Array(particlesCount * 3);

  for (let i = 0; i < particlesCount * 3; i++) {
    posArray[i] = (Math.random() - 0.5) * 15;
  }

  particlesGeometry.setAttribute('position', new THREE.BufferAttribute(posArray, 3));

  const material = new THREE.PointsMaterial({
    size: 0.015,
    color: 0x00F0FF, // 赛博青
    transparent: true,
    opacity: 0.8,
  });

  particlesMesh = new THREE.Points(particlesGeometry, material);
  scene.add(particlesMesh);

  let mouseX = 0;
  let mouseY = 0;

  const onMouseMove = (event) => {
    mouseX = event.clientX / window.innerWidth - 0.5;
    mouseY = event.clientY / window.innerHeight - 0.5;
  };
  window.addEventListener('mousemove', onMouseMove);

  const clock = new THREE.Clock();

  const animate = () => {
    animationId = requestAnimationFrame(animate);
    const elapsedTime = clock.getElapsedTime();
    
    // 粒子自转 + 鼠标视差
    particlesMesh.rotation.y = elapsedTime * 0.05;
    particlesMesh.rotation.x += (mouseY * 0.5 - particlesMesh.rotation.x) * 0.05;
    particlesMesh.rotation.y += (mouseX * 0.5 - particlesMesh.rotation.y * 0.05) * 0.05;
    
    renderer.render(scene, camera);
  };

  animate();

  const onResize = () => {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
  };
  window.addEventListener('resize', onResize);

  return () => {
    window.removeEventListener('mousemove', onMouseMove);
    window.removeEventListener('resize', onResize);
    if(particlesMesh) {
        scene.remove(particlesMesh);
        particlesGeometry.dispose();
        material.dispose();
    }
    renderer.dispose();
  };
};

let cleanupThreeJS = null;

onMounted(() => {
  cleanupThreeJS = initThreeJS();
});

onUnmounted(() => {
  if (cleanupThreeJS) cleanupThreeJS();
  if (animationId) cancelAnimationFrame(animationId);
});
</script>

<style scoped>
.page-wrapper {
  position: relative;
  width: 100%;
  min-height: 100vh;
  background-color: #050505;
}

.MainBackground {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100vh;
  background-color: #050505;
  z-index: 0;
  overflow: hidden;
  pointer-events: none;
}

.ContentLayer {
  position: relative;
  z-index: 10;
  width: 100%;
  min-height: 100vh;
  overflow-y: auto; 
  overflow-x: hidden;
}

.scroll-content {
  padding-top: 80px; /* 避开顶部导航 */
  display: flex;
  flex-direction: column;
  gap: 20px; /* 组件间的基础间距，组件内部还有 margin */
}

.cyber-footer {
  text-align: center;
  padding: 60px 20px 40px;
  color: #444;
  font-family: 'JetBrains Mono', monospace;
  margin-top: 40px;
}

.footer-line {
  width: 100px;
  height: 2px;
  background: #333;
  margin: 0 auto 20px;
}

.copyright {
  font-size: 12px;
  margin-top: 10px;
  opacity: 0.5;
}
</style>

<style>
body {
  margin: 0;
  padding: 0;
  background-color: #050505;
  color: #fff;
}
</style>