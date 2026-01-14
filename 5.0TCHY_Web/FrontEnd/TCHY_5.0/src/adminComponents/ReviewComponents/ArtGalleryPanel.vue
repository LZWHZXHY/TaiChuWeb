<template>
  <div class="art-gallery-panel-root">
    <div class="panel-metrics-bar">
      <div class="metric-group">
        <span class="label">PENDING_NODES:</span>
        <span class="val blink">{{ stats.pending }}</span>
      </div>
      <div class="metric-group">
        <span class="label">TOTAL_ARCHIVE:</span>
        <span class="val">{{ stats.total }}</span>
      </div>
      <div class="tech-deco">SYSTEM_MONITOR_ACTIVE // 4_COLUMN_GRID</div>
    </div>

    <main class="gallery-viewport">
      <div class="gallery-content-wrapper">
        
        <div v-if="loading && totalLoaded === 0" class="loading-overlay">
          <div class="tech-spinner"></div>
          <span>[ INITIALIZING_NEURAL_LINK... ]</span>
        </div>

        <div class="gallery-waterfall">
          <div 
            v-for="(col, colIndex) in waterfallColumns" 
            :key="colIndex" 
            class="waterfall-column"
          >
            <div 
              v-for="work in col" 
              :key="work.id" 
              class="cyber-art-card"
              :class="{ 'is-pending': work.status === 2 }"
            >
              <div class="card-frame-industrial">
                <div class="img-viewport" @click="openDetail(work)">
                  <img :src="upgradeUrlToHttps(work.imageUrl)" @error="handleImgError" loading="lazy" />
                  <div class="scan-line"></div>
                  <div class="status-tag">{{ getStatusText(work.status) }}</div>
                </div>
                
                <div class="card-info-block">
                  <div class="work-title">{{ work.title }}</div>
                  <div class="work-author">> AUTH: {{ work.authorName }}</div>
                  
                  <div class="terminal-actions">
                    <button class="t-btn approve" @click="handleStatus(work, 0)">APPROVE</button>
                    <button class="t-btn reject" @click="handleStatus(work, 1)">HIDE</button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div ref="loadMoreTrigger" class="load-more-trigger">
          <div v-if="loading && totalLoaded > 0" class="sync-text">
            <div class="tech-spinner-sm"></div> >> SYNCING_DATA_STREAM...
          </div>
          <div v-else-if="!hasMore && totalLoaded > 0" class="end-tag">
            // END_OF_LOG_STREAM //
          </div>
        </div>
      </div>
    </main>

    <el-dialog v-model="detailVisible" width="1000px" class="cyber-detail-dialog" :show-close="false">
      <template #header>
        <div class="detail-header-tech">
          <span class="deco">■</span>
          <span class="title">VISUAL_ANALYSIS_NODE: {{ selectedWork?.id }}</span>
          <button class="close-rect-btn" @click="detailVisible = false">CLOSE [X]</button>
        </div>
      </template>
      <div v-if="selectedWork" class="detail-body-industrial custom-scroll">
        <div class="detail-layout-split">
          <div class="img-preview-box">
             <img :src="upgradeUrlToHttps(selectedWork.imageUrl)" />
          </div>
          <div class="data-specs">
            <div class="spec-row"><label>IDENTIFIER</label><div class="val">{{ selectedWork.title }}</div></div>
            <div class="spec-row"><label>AUTHOR_SIGN</label><div class="val">@{{ selectedWork.authorName }}</div></div>
            <div class="spec-row"><label>LOG_DESC</label><div class="val text">{{ selectedWork.desc || 'NO_DATA' }}</div></div>
            <div class="spec-row"><label>TIMESTAMP</label><div class="val">{{ formatDate(selectedWork.uploadAt) }}</div></div>
          </div>
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted, computed } from 'vue';
import apiClient from '@/utils/api';
import { ElMessage } from 'element-plus';

const props = defineProps(['search']);
const emit = defineEmits(['update-count']);

// --- 状态控制 ---
const waterfallColumns = ref([[], [], [], []]);
const loading = ref(false);
const currentPage = ref(1);
const stats = reactive({ pending: 0, total: 0 });
const detailVisible = ref(false);
const selectedWork = ref(null);
const loadMoreTrigger = ref(null); // 侦察兵 Ref
let observer = null;

const totalLoaded = computed(() => waterfallColumns.value.reduce((acc, col) => acc + col.length, 0));
const hasMore = computed(() => totalLoaded.value < stats.total);

const upgradeUrlToHttps = (url) => {
  if (!url) return '';
  return url.replace('http://', 'https://');
};

const fetchWorks = async (isRefresh = false) => {
  if (loading.value) return;
  if (!isRefresh && !hasMore.value && stats.total > 0) return;

  if (isRefresh) {
    currentPage.value = 1;
    waterfallColumns.value = [[], [], [], []];
  }

  loading.value = true;
  try {
    const res = await apiClient.get('/Drawing/admin/all', {
      params: { page: currentPage.value, pageSize: 20 }
    });

    if (res.data.success) {
      const rawList = res.data.list || [];
      stats.total = res.data.total || 0;

      // 标准化映射数据字段
      const normalizedItems = rawList.map(item => ({
        id: item.Id,
        title: item.title,
        imageUrl: item.ImageUrl,
        uploadAt: item.UploadAt,
        status: item.status,
        likes: item.likes,
        desc: item.desc,
        authorName: item.Author?.username || 'UNKNOWN'
      }));

      // 🔥 修正分配逻辑：使用起始长度作为固定偏移
      const currentCount = totalLoaded.value;
      normalizedItems.forEach((item, index) => {
        const colIndex = (currentCount + index) % 4;
        waterfallColumns.value[colIndex].push(item);
      });

      currentPage.value++;
      fetchStats(); 
    }
  } catch (e) {
    console.error("Fetch Error:", e);
  } finally {
    loading.value = false;
  }
};

const fetchStats = async () => {
  try {
    const res = await apiClient.get('/Drawing/admin/stats');
    if (res.data.success) {
      stats.pending = res.data.data.pending;
      emit('update-count', stats.pending);
    }
  } catch (e) {}
};

// --- 🔥 核心：初始化交叉观察者 ---
const initObserver = () => {
  observer = new IntersectionObserver((entries) => {
    if (entries[0].isIntersecting && hasMore.value && !loading.value) {
      console.log(">> DETECTED_BOTTOM: LOADING_NEXT_PAGE");
      fetchWorks(false);
    }
  }, { threshold: 0.1 });

  if (loadMoreTrigger.value) {
    observer.observe(loadMoreTrigger.value);
  }
};

const handleStatus = async (work, status) => {
  try {
    await apiClient.put(`/Drawing/admin/status/${work.id}`, { status });
    ElMessage.success("DATA_STABILIZED");
    work.status = status;
    fetchStats();
  } catch (e) {
    ElMessage.error("SYNC_INTERRUPTED");
  }
};

const handleImgError = (e) => e.target.src = '/土豆.jpg';
const openDetail = (work) => { selectedWork.value = work; detailVisible.value = true; };
const getStatusText = (s) => ['VERIFIED', 'HIDDEN', 'PENDING'][s] || 'UNKNOWN';
const formatDate = (d) => d ? new Date(d).toLocaleString() : 'N/A';

defineExpose({
  refresh: () => fetchWorks(true),
  onSearch: () => fetchWorks(true)
});

onMounted(async () => {
  await fetchWorks(true);
  initObserver();
});

onUnmounted(() => {
  if (observer) observer.disconnect();
});
</script>

<style scoped>
.art-gallery-panel-root {
  display: flex; flex-direction: column; height: 100%; background: var(--off-white);
}

.panel-metrics-bar {
  background: var(--black); color: #fff; padding: 8px 20px; display: flex; gap: 30px; 
  font-family: var(--mono); font-size: 0.75rem; border-bottom: 3px solid var(--red);
}

.gallery-viewport { flex: 1; position: relative; }

.gallery-waterfall {
  display: flex; gap: 15px; align-items: flex-start; width: 100%;
}

.waterfall-column {
  flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 15px;
}

.cyber-art-card { width: 100%; }

.card-frame-industrial { 
  background: #fff; border: 2px solid var(--black); 
  box-shadow: 4px 4px 0 var(--black); overflow: hidden; 
}
.is-pending { border-color: var(--red); box-shadow: 4px 4px 0 var(--red); }

.img-viewport { background: #000; position: relative; cursor: pointer; }
.img-viewport img { width: 100%; display: block; filter: grayscale(0.2); }
.img-viewport:hover img { filter: grayscale(0); }

.status-tag {
  position: absolute; top: 10px; left: 10px; background: var(--black); color: #fff;
  font-family: var(--mono); font-size: 0.6rem; padding: 2px 6px;
}

.card-info-block { padding: 10px; border-top: 2px solid var(--black); }
.work-title { font-family: var(--heading); font-size: 1rem; text-transform: uppercase; margin-bottom: 4px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.work-author { font-family: var(--mono); font-size: 0.65rem; color: #666; margin-bottom: 10px; }

.terminal-actions { display: flex; gap: 5px; }
.t-btn { 
  flex: 1; background: transparent; border: 1px solid var(--black); 
  font-family: var(--heading); font-size: 0.7rem; padding: 5px; cursor: pointer;
}
.t-btn.approve:hover { background: var(--black); color: #fff; }
.t-btn.reject:hover { background: var(--red); color: #fff; border-color: var(--red); }

/* 侦察兵样式 */
.load-more-trigger {
  padding: 40px; text-align: center; font-family: var(--mono); color: #888;
}

.tech-spinner-sm {
  display: inline-block; width: 12px; height: 12px; border: 2px solid var(--red);
  border-top-color: transparent; border-radius: 50%; animation: spin 1s linear infinite;
}

@keyframes spin { to { transform: rotate(360deg); } }
@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }
</style>