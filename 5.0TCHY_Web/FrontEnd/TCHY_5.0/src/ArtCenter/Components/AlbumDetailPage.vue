<template>
  <div class="album-detail-container custom-scroll">
    <aside class="album-sidebar">
      <div class="sticky-box">
        <button class="back-btn" @click="$router.push('/gallery')">
          <span class="btn-icon">«</span> RETURN_GALLERY
        </button>

        <div class="album-info-panel">
          <div class="panel-tag">ARCHIVE_DETAILS</div>
          <h1 class="album-name">{{ albumInfo.Title || albumInfo.title }}</h1>
          
          <div class="data-row">
            <span class="label">CREATOR</span>
            <span class="value">{{ albumInfo.AuthorName || albumInfo.authorName }}</span>
          </div>
          <div class="data-row">
            <span class="label">SERIAL_NO</span>
            <span class="value">#{{ $route.params.id }}</span>
          </div>
          <div class="data-row">
            <span class="label">QUANTITY</span>
            <span class="value">{{ items.length }} UNITS</span>
          </div>

          <div class="description-box">
            <div class="desc-header">DESCRIPTION //</div>
            <p class="desc-text">{{ albumInfo.Description || albumInfo.description || '无补充档案描述。' }}</p>
          </div>

          <div class="status-indicator">
            <span class="dot blink"></span> SYSTEM_ACTIVE
          </div>
        </div>
      </div>
    </aside>

    <main class="album-content">
      <div v-if="loading" class="loader-wrap">
        <div class="glitch-text">SYNCHRONIZING_DATA...</div>
        <div class="progress-bar"><div class="bar"></div></div>
      </div>

      <div v-else class="waterfall-layout">
        <div 
          v-for="item in items" 
          :key="item.Id || item.id" 
          class="art-piece"
          @click="goToArt(item.TargetId || item.targetId)"
        >
          <div class="piece-frame">
            <div class="img-container">
              <img :src="item.ImageUrl || item.imageUrl" loading="lazy" />
              <div class="piece-overlay">
                <span class="action">EXTRACT_DATA</span>
              </div>
            </div>
            <div class="piece-footer">
              <span class="id-tag">REF_{{ item.TargetId || item.targetId }}</span>
              <span class="date-tag">ARCHIVE_UNIT</span>
            </div>
          </div>
        </div>
      </div>

      <div v-if="!loading && items.length === 0" class="empty-arch">
        [!] 当前档案库为空 // NO_RECORDS_FOUND
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import apiClient from '@/utils/api'

const route = useRoute()
const router = useRouter()
const items = ref([])
const albumInfo = ref({ title: '加载中...', authorName: '...', description: '' })
const loading = ref(true)

const fetchAlbumData = async () => {
  loading.value = true;
  const albumId = route.params.id;
  
  try {
    const res = await apiClient.get(`/Folder/detail/${albumId}`);
    
    if (res.data.success) {
      // 兼容后端返回的大小写
      albumInfo.value = res.data.data;
      items.value = res.data.data.Items || res.data.data.items || []; 
      console.log("档案数据提取成功:", items.value.length, "件作品");
    }
  } catch (e) {
    console.error("档案同步失败 // ACCESS_DENIED", e);
  } finally {
    loading.value = false;
  }
};

const goToArt = (id) => {
  router.push(`/gallery/${id}`)
}

onMounted(fetchAlbumData)
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

.album-detail-container {
  display: flex;
  background: #f4f1ea;
  min-height: 100vh;
  padding: 0;
  color: #111;
  font-family: 'JetBrains Mono', monospace;
}

/* --- 左侧边栏 --- */
.album-sidebar {
  width: 350px;
  border-right: 2px solid #000;
  padding: 40px;
  background: #ebe8e0;
  flex-shrink: 0;
}

.sticky-box { position: sticky; top: 40px; }

.back-btn {
  background: #000;
  color: #fff;
  border: none;
  padding: 10px 15px;
  font-family: 'Anton';
  font-size: 1rem;
  cursor: pointer;
  margin-bottom: 40px;
  transition: 0.2s;
  width: 100%;
  text-align: left;
}
.back-btn:hover { background: #D92323; }

.album-info-panel { border: 2px solid #000; padding: 20px; background: #fff; position: relative; }
.panel-tag {
  position: absolute;
  top: -12px;
  left: 15px;
  background: #D92323;
  color: #fff;
  font-size: 0.7rem;
  padding: 2px 8px;
  font-weight: bold;
}

.album-name { font-family: 'Anton'; font-size: 2.2rem; line-height: 1.1; margin: 15px 0; text-transform: uppercase; }

.data-row { display: flex; justify-content: space-between; border-bottom: 1px dashed #ccc; padding: 8px 0; font-size: 0.8rem; }
.data-row .label { color: #666; }
.data-row .value { font-weight: bold; }

.description-box { margin-top: 25px; }
.desc-header { font-weight: bold; font-size: 0.75rem; border-left: 3px solid #000; padding-left: 8px; margin-bottom: 10px; }
.desc-text { font-size: 0.85rem; line-height: 1.6; color: #444; }

.status-indicator { margin-top: 30px; font-size: 0.7rem; font-weight: bold; display: flex; align-items: center; gap: 8px; }
.blink { width: 8px; height: 8px; background: #27c93f; border-radius: 50%; animation: blinker 1s infinite; }

/* --- 右侧内容区 --- */
.album-content { flex: 1; padding: 60px; overflow-x: hidden; }

.waterfall-layout {
  display: grid;
  /* ✅ 修复点：强制限制列宽，确保图片不会过大 */
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 25px;
  align-items: start;
}

.art-piece { cursor: pointer; }

/* 卡片外框 */
.piece-frame {
  border: 1px solid #000;
  background: #fff;
  padding: 8px;
  transition: 0.3s cubic-bezier(0.23, 1, 0.32, 1);
}

.art-piece:hover .piece-frame {
  transform: translateY(-5px);
  box-shadow: 10px 10px 0 rgba(0,0,0,0.1);
  border-color: #D92323;
}

/* ✅ 修复点：强制图片容器限制并裁剪图片 */
.img-container { 
  position: relative; 
  overflow: hidden; 
  background: #000; 
  aspect-ratio: 1 / 1; /* 你可以根据需要调整比例，或者删除这行保持原图比例 */
}

.img-container img { 
  width: 100%; 
  height: 100%;
  display: block; 
  object-fit: cover; /* 👈 关键：图片填充并裁剪，不拉伸 */
  filter: grayscale(0.2); 
  transition: 0.5s; 
}

.art-piece:hover img { filter: grayscale(0); transform: scale(1.1); }

.piece-overlay {
  position: absolute; inset: 0;
  background: rgba(217, 35, 35, 0.7);
  display: flex; align-items: center; justify-content: center;
  opacity: 0; transition: 0.3s;
}
.art-piece:hover .piece-overlay { opacity: 1; }
.action { color: #fff; font-family: 'Anton'; font-size: 1rem; border: 1.5px solid #fff; padding: 4px 12px; }

.piece-footer {
  margin-top: 10px;
  display: flex;
  justify-content: space-between;
  font-size: 0.6rem;
  color: #888;
  font-weight: bold;
}

/* --- 加载状态 --- */
.loader-wrap { text-align: center; margin-top: 100px; }
.glitch-text { font-family: 'Anton'; font-size: 1.5rem; letter-spacing: 2px; }
.progress-bar { width: 140px; height: 3px; background: #ddd; margin: 20px auto; position: relative; overflow: hidden; }
.bar { width: 40%; height: 100%; background: #D92323; position: absolute; animation: move 1.5s infinite; }

.empty-arch { text-align: center; padding: 100px; color: #999; font-weight: bold; }

@keyframes move { 0% { left: -40%; } 100% { left: 100%; } }
@keyframes blinker { 50% { opacity: 0; } }

@media (max-width: 1000px) {
  .album-detail-container { flex-direction: column; }
  .album-sidebar { width: 100%; border-right: none; border-bottom: 2px solid #000; padding: 30px; }
  .album-content { padding: 30px; }
}
</style>