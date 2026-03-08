<template>
  <div class="collections-matrix">
    <header class="matrix-header">
      <div class="title-block">
        <span class="icon blink">★</span>
        <h2>STAR_ARCHIVES // 星标档案库</h2>
      </div>
      <div class="header-stats">
        <span>ARCHIVE_COUNT: {{ folders.length }}</span>
      </div>
    </header>

    <div class="matrix-body">
      <aside class="folder-list custom-scroll">
        <div v-if="loadingFolders" class="loading-text">>> SCANNING_ARCHIVES...</div>
        
        <div v-else-if="folders.length === 0" class="empty-state">
          [ NO_ARCHIVES_FOUND ]
        </div>

        <div 
          v-else
          v-for="folder in folders" 
          :key="folder.id || folder.Id"
          class="folder-item"
          :class="{ active: selectedFolder?.id === (folder.id || folder.Id) }"
          @click="selectFolder(folder)"
        >
          <div class="f-icon">★</div>
          <div class="f-info">
            <div class="f-name">{{ folder.title || folder.Title || 'UNNAMED' }}</div>
            <div class="f-meta">{{ folder.itemCount || folder.ItemCount || 0 }} ITEMS</div>
          </div>
          <div class="active-indicator"></div>
        </div>
      </aside>

      <main class="asset-grid-container custom-scroll">
        <div v-if="!selectedFolder" class="awaiting-state">
          <div class="scan-line"></div>
          <p>[ AWAITING_ARCHIVE_SELECTION // 请在左侧选择档案袋 ]</p>
        </div>

        <div v-else class="active-archive-view">
          <div class="archive-info-bar">
            <h3>>> {{ selectedFolder.title || selectedFolder.Title }}</h3>
            <button class="cyber-btn-mini red">DELETE_ARCHIVE</button>
          </div>

          <div v-if="loadingItems" class="loading-text">>> EXTRACTING_ASSETS...</div>
          
          <div v-else-if="folderItems.length === 0" class="empty-state">
            [ ARCHIVE_IS_EMPTY // 档案袋为空 ]
          </div>

          <div v-else class="asset-grid">
            <div 
              v-for="item in folderItems" 
              :key="item.id || item.Id" 
              class="asset-card"
              @click="goToDetail(item.targetId || item.TargetId)"
            >
              <div class="card-img-box">
                <img :src="fixUrl(item.imageUrl || item.ImageUrl)" loading="lazy" />
                <div class="overlay">
                  <span class="view-btn">VIEW_NODE</span>
                </div>
              </div>
              <div class="card-meta">
                <div class="asset-id">NODE_#{{ item.targetId || item.TargetId }}</div>
                <div class="add-time">{{ formatDate(item.addedAt || item.AddedAt) }}</div>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import apiClient from '@/utils/api';

const router = useRouter();
const folders = ref([]);
const selectedFolder = ref(null);
const folderItems = ref([]);

const loadingFolders = ref(false);
const loadingItems = ref(false);

const BASE_URL = window.location.hostname === 'localhost' ? 'https://localhost:44359' : 'https://bianyuzhou.com';

const fixUrl = (url) => {
  if (!url) return '/土豆.jpg';
  if (url.startsWith('http') || url.startsWith('data:image')) return url;
  return `${BASE_URL}/uploads/${url.replace(/\\/g, '/')}`;
};

const formatDate = (dateString) => {
  if (!dateString) return 'UNKNOWN_TIME';
  const date = new Date(dateString);
  return `${date.getFullYear()}.${String(date.getMonth() + 1).padStart(2, '0')}.${String(date.getDate()).padStart(2, '0')}`;
};

// 1. 获取文件夹列表
const fetchFolders = async () => {
  loadingFolders.value = true;
  try {
    // category=0 (Drawing), type=1 (Collection)
    const res = await apiClient.get('/Folder/my-list?category=0&type=1');
    if (res.data.success) {
      folders.value = res.data.data;
    }
  } catch (e) {
    console.error("Failed to fetch archives", e);
  } finally {
    loadingFolders.value = false;
  }
};

// 2. 选中文件夹并加载内容
const selectFolder = async (folder) => {
  selectedFolder.value = folder;
  folderItems.value = [];
  loadingItems.value = true;
  
  const folderId = folder.id || folder.Id;
  
  try {
    // 注意：我们需要在后端写这个接口
    const res = await apiClient.get(`/Folder/${folderId}/items`);
    if (res.data.success) {
      folderItems.value = res.data.data;
    }
  } catch (e) {
    console.error("Failed to extract assets", e);
  } finally {
    loadingItems.value = false;
  }
};

// 3. 跳转到画廊详情页
const goToDetail = (targetId) => {
  if (!targetId) return;
  router.push(`/gallery/${targetId}`);
};

onMounted(() => {
  fetchFolders();
});
</script>

<style scoped>
.collections-matrix {
  display: flex;
  flex-direction: column;
  height: 100%;
  border: 2px solid var(--black, #111);
  background: #fff;
  box-shadow: 8px 8px 0 rgba(0,0,0,0.1);
}

.matrix-header {
  background: var(--black, #111);
  color: #fff;
  padding: 15px 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 4px solid #FFD700;
}

.title-block { display: flex; align-items: center; gap: 10px; }
.title-block .icon { color: #FFD700; font-size: 1.5rem; }
.title-block h2 { font-family: 'Anton', sans-serif; margin: 0; font-size: 1.5rem; letter-spacing: 1px; }
.header-stats { font-family: 'JetBrains Mono', monospace; font-size: 0.8rem; color: #aaa; }

.matrix-body {
  display: flex;
  flex: 1;
  overflow: hidden;
}

/* 左侧文件夹列表 */
.folder-list {
  width: 280px;
  background: #f4f4f4;
  border-right: 2px solid #ccc;
  overflow-y: auto;
  padding: 15px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.folder-item {
  display: flex;
  align-items: center;
  padding: 15px;
  background: #fff;
  border: 2px solid #ccc;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
}

.folder-item:hover { border-color: #111; transform: translateX(2px); }
.folder-item.active { border-color: #FFD700; background: #fffdf0; box-shadow: 4px 4px 0 rgba(0,0,0,0.1); }

.f-icon { font-size: 1.5rem; color: #999; margin-right: 15px; transition: 0.2s; }
.folder-item.active .f-icon { color: #FFD700; text-shadow: 0 0 5px rgba(255, 215, 0, 0.5); }

.f-info { flex: 1; overflow: hidden; }
.f-name { font-weight: bold; font-family: 'Anton', sans-serif; font-size: 1.1rem; color: #111; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.f-meta { font-size: 0.7rem; color: #888; margin-top: 4px; font-family: 'JetBrains Mono'; }

.active-indicator { position: absolute; right: 0; top: 0; bottom: 0; width: 6px; background: transparent; }
.folder-item.active .active-indicator { background: #FFD700; }

/* 右侧资产网格 */
.asset-grid-container {
  flex: 1;
  background: #e5e5e5;
  padding: 20px;
  overflow-y: auto;
  position: relative;
}

.awaiting-state, .empty-state {
  height: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  font-family: 'JetBrains Mono';
  font-weight: bold;
  color: #888;
}

.archive-info-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  border-bottom: 2px dashed #ccc;
  padding-bottom: 10px;
}
.archive-info-bar h3 { margin: 0; font-family: 'Anton'; font-size: 1.2rem; color: #111; }

.cyber-btn-mini {
  background: transparent; border: 1px solid #111; padding: 4px 10px; font-family: 'JetBrains Mono'; font-size: 0.7rem; font-weight: bold; cursor: pointer; transition: 0.2s;
}
.cyber-btn-mini.red:hover { background: #D92323; color: #fff; border-color: #D92323; }

.asset-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 15px;
}

.asset-card {
  background: #fff;
  border: 2px solid #111;
  padding: 5px;
  cursor: pointer;
  transition: 0.2s;
}
.asset-card:hover { transform: translateY(-3px); box-shadow: 6px 6px 0 rgba(0,0,0,0.1); border-color: #D92323; }

.card-img-box {
  width: 100%;
  aspect-ratio: 1;
  background: #000;
  position: relative;
  overflow: hidden;
  border-bottom: 2px solid #111;
}
.card-img-box img { width: 100%; height: 100%; object-fit: cover; transition: 0.3s; opacity: 0.9; }
.asset-card:hover .card-img-box img { opacity: 1; transform: scale(1.05); filter: contrast(1.2); }

.overlay { position: absolute; inset: 0; background: rgba(217, 35, 35, 0.6); display: flex; justify-content: center; align-items: center; opacity: 0; transition: 0.2s; }
.asset-card:hover .overlay { opacity: 1; }
.view-btn { color: #fff; border: 2px solid #fff; padding: 5px 10px; font-family: 'Anton'; font-size: 0.9rem; letter-spacing: 1px; background: rgba(0,0,0,0.5); }

.card-meta { padding: 8px 5px; display: flex; justify-content: space-between; align-items: center; font-family: 'JetBrains Mono'; font-size: 0.65rem; color: #111; font-weight: bold; }
.asset-id { color: #D92323; }
.add-time { color: #888; }

.blink { animation: blink 1s infinite; }
@keyframes blink { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }

.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #111; }
</style>