<template>
  <div class="background-wrapper">
    <div ref="canvasContainer" class="canvas-layer"></div>
    <div class="overlay vignette"></div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue';
import * as THREE from 'three';

// --- 组件接收的 Props (可选，让你在使用时能自定义颜色) ---
const props = defineProps({
  color: { type: Number, default: 0x00ff41 }, // 默认霓虹绿
  speed: { type: Number, default: 0.0005 },
  bgColor: { type: Number, default: 0x050505 }
});

const canvasContainer = ref(null);
let scene, camera, renderer, animationId;
let terrainMesh;

// 配置合并
const CONFIG = {
  waveHeight: 35,
  fogDensity: 0.0012,
  ...props // 使用 props 覆盖默认值
};

const initThree = () => {
  if (!canvasContainer.value) return;

  scene = new THREE.Scene();
  scene.fog = new THREE.FogExp2(CONFIG.bgColor, CONFIG.fogDensity);
  scene.background = new THREE.Color(CONFIG.bgColor);

  camera = new THREE.PerspectiveCamera(60, window.innerWidth / window.innerHeight, 1, 2000);
  camera.position.set(0, 400, 400); 
  camera.lookAt(0, 0, 0);

  renderer = new THREE.WebGLRenderer({ alpha: false, antialias: true });
  renderer.setSize(window.innerWidth, window.innerHeight);
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
  
  // 清理容器
  while(canvasContainer.value.firstChild) {
      canvasContainer.value.removeChild(canvasContainer.value.firstChild);
  }
  canvasContainer.value.appendChild(renderer.domElement);

  const geometry = new THREE.PlaneGeometry(2400, 2400, 60, 60);
  const material = new THREE.MeshBasicMaterial({ 
    color: CONFIG.color,
    wireframe: true,
    transparent: true,
    opacity: 0.3,
    side: THREE.DoubleSide
  });

  terrainMesh = new THREE.Mesh(geometry, material);
  terrainMesh.rotation.x = -Math.PI / 2;
  scene.add(terrainMesh);

  const posAttribute = geometry.attributes.position;
  geometry.userData.originalZ = [];
  for (let i = 0; i < posAttribute.count; i++) {
    geometry.userData.originalZ.push(posAttribute.getZ(i));
  }
};

const animate = (time) => {
  const t = time * CONFIG.speed;
  if (terrainMesh) {
    const position = terrainMesh.geometry.attributes.position;
    const originalZ = terrainMesh.geometry.userData.originalZ;
    for (let i = 0; i < position.count; i++) {
      const x = position.getX(i);
      const y = position.getY(i);
      const waveX = Math.sin(x * 0.003 + t * 0.5) * (CONFIG.waveHeight * 0.5);
      const waveY = Math.sin((y - time * 0.2) * 0.0025) * CONFIG.waveHeight;
      position.setZ(i, originalZ[i] + waveX + waveY);
    }
    position.needsUpdate = true;
  }
  renderer.render(scene, camera);
  animationId = requestAnimationFrame(animate);
};

const handleResize = () => {
  if (camera && renderer) {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
  }
};

onMounted(() => {
  initThree();
  animate(0);
  window.addEventListener('resize', handleResize);
});

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize);
  cancelAnimationFrame(animationId);
  if (renderer) renderer.dispose();
  if (scene) scene.clear();
});
</script>

<style scoped>
.background-wrapper {
  position: fixed; /* 固定在视窗，不随父级滚动 */
  top: 0; left: 0;
  width: 100vw; height: 100vh;
  z-index: 0; /* 层级最低 */
  background-color: #050505;
  pointer-events: none; /* 让点击事件穿透背景 */
}

.canvas-layer {
  width: 100%; height: 100%;
}

.vignette {
  background: radial-gradient(circle, transparent 50%, rgba(0,0,0,0.9) 100%);
  position: absolute;
  top: 0; left: 0; right: 0; bottom: 0;
  z-index: 1;
}
</style>