<template>
  <div class="archive-terminal">
    <div class="paper-texture"></div>
    <div class="drafting-grid"></div>

    <div class="archive-container">
      <header class="archive-header">
        <div class="header-main">
          <div class="brand-box">
            <span class="dept">BUREAU_ARCHIVE // 档案局</span>
            <h1 class="main-title">联合区域</h1>
            <div class="sub-info">
              <span class="status-indicator"><i class="dot"></i> 系统同步中</span>
              <span class="divider">|</span>
              <span class="ver">V 2.2.8</span>
            </div>
          </div>
        </div>

        <div class="header-controls">
          <div class="search-field">
            <input v-model="q" type="text" class="ink-search" placeholder="检索案卷名、指挥官或编号..." @input="onSearchInput" />
            <div class="focus-line"></div>
          </div>
          
          <div class="tab-selector">
            <button :class="{ active: tab === 'all' }" @click="switchTab('all')">ALL_全部</button>
            <button :class="{ active: tab === 'ongoing' }" @click="switchTab('ongoing')">ACTIVE_进行中</button>
            <button :class="{ active: tab === 'finished' }" @click="switchTab('finished')">ARCHIVED_已结束</button>
          </div>
        </div>
      </header>

      <main class="archive-scroll custom-scroll" ref="scrollContainer" @scroll="handleScroll">
        <div v-if="loading && items.length === 0" class="loading-state">
          <div class="spinner"></div> 正在解密历史档案...
        </div>

        <div v-else class="archive-grid">
          <article 
            v-for="it in items" 
            :key="it.id" 
            class="archive-card" 
            :class="{ 'is-expired': checkExpired(it.enddate) }"
            @click="viewDetail(it)"
          >
            <div class="card-sidebar">
              <span class="index-no">{{ padZero(it.id) }}</span>
              <div class="type-tag">{{ typeLabelEn(it.type) }}</div>
            </div>

            <div class="card-content">
              <div v-if="checkExpired(it.enddate)" class="closed-stamp">ARCHIVED_结案</div>

              <header class="card-head">
                <h3 class="card-title">{{ it.name }}</h3>
                <div class="stamp-wrapper">
                   <div class="stamp-ink" :class="it.verify === 1 ? 'approved' : 'pending'">
                     {{ it.verify === 1 ? 'FILE_OK' : 'UNDER_REVIEW' }}
                   </div>
                </div>
              </header>

              <div class="card-tags-area">
                <UniversalTags :tags="it.tags" />
              </div>

              <div class="card-meta">
                <div class="m-item"><span class="k">HOST:</span><span class="v">{{ it.host }}</span></div>
                <div class="m-item"><span class="k">LIMIT:</span><span class="v">{{ shortDate(it.enddate) || '永久' }}</span></div>
              </div>

              <p class="card-desc">{{ it.desc }}</p>

              <footer class="card-footer">
                <span class="access-btn">{{ checkExpired(it.enddate) ? '查阅存档内容' : 'ACCESS_ARCHIVE' }} →</span>
                <span class="file-ref">REF.THCY_{{ it.id }}</span>
              </footer>
            </div>
          </article>
        </div>

        <div v-if="!hasMore && items.length > 0" class="end-seal">
          <span class="line"></span>
          <span class="text">数据同步完毕 // EOF</span>
          <span class="line"></span>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/utils/api'
import UniversalTags from '@/GeneralComponents/UniversalTags.vue'

const router = useRouter()

// 核心状态
const items = ref([])
const loading = ref(false)
const hasMore = ref(true)
const page = ref(1)
const pageSize = ref(12)
const q = ref('')
const tab = ref('all')

const checkExpired = (endDate) => {
  if (!endDate) return false
  return new Date(endDate).getTime() < Date.now()
}

const fetchList = async (isRefresh = true) => {
  if (loading.value) return
  if (isRefresh) { page.value = 1; items.value = [] }
  loading.value = true
  
  try {
    const params = { 
      q: q.value || undefined, 
      page: page.value, 
      pageSize: pageSize.value 
    }
    const resp = await apiClient.get('Activity/list', { params })
    
    if (resp.data?.data) {
      const now = Date.now()
      const rawData = resp.data.data

      let filtered = rawData.filter(it => {
        const isExpired = it.enddate ? new Date(it.enddate).getTime() < now : false
        if (tab.value === 'all') return true
        if (tab.value === 'ongoing') return !isExpired
        return isExpired
      })

      items.value = isRefresh ? filtered : [...items.value, ...filtered]
      hasMore.value = items.value.length < resp.data.total
    }
  } catch (err) { 
    console.error('档案调取失败:', err) 
  } finally { 
    loading.value = false 
  }
}

const viewDetail = (it) => router.push(`/activity/${it.id}`)
const padZero = (n) => n < 10 ? `0${n}` : n
const typeLabelEn = (t) => ({ 1: 'COLLAB', 2: 'RELAY', 3: 'MATCH', 4: 'SYNC' }[t] || 'FILE')
const shortDate = (d) => d ? d.substring(0, 10).replace(/-/g, '.') : ''
const onSearchInput = () => fetchList(true)
const switchTab = (t) => { tab.value = t; fetchList(true) }

const handleScroll = (e) => {
  if (!hasMore.value || loading.value) return
  if (e.target.scrollTop + e.target.clientHeight >= e.target.scrollHeight - 50) {
    page.value++; fetchList(false)
  }
}

onMounted(() => fetchList(true))
</script>

<style scoped>
/* =========================================
   📄 核心布局与氛围 CSS (完整还原版)
   ========================================= */

.archive-terminal {
  --ink: #121212; --ink-light: #666; --paper: #ffffff; --paper-old: #f9f7f0; --divider: #e0e0e0; --approved: #2e7d32; --danger: #b71c1c;
  width: 100%; height: 100vh; background-color: #f0f0f0; color: var(--ink); font-family: "PingFang SC", sans-serif; display: flex; justify-content: center; overflow: hidden;
  position: relative;
}

/* 纸张质感与网格 */
.paper-texture { position: absolute; inset: 0; pointer-events: none; opacity: 0.05; background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.8'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E"); }
.drafting-grid { position: absolute; inset: 0; pointer-events: none; opacity: 0.1; background-image: linear-gradient(var(--divider) 1px, transparent 1px), linear-gradient(90deg, var(--divider) 1px, transparent 1px); background-size: 30px 30px; }

.archive-container { width: 100%; max-width: 1400px; background: var(--paper); margin: 20px; box-shadow: 0 10px 40px rgba(0,0,0,0.1); display: flex; flex-direction: column; z-index: 2; border: 1px solid var(--divider); }

/* 页眉详情 */
.archive-header { padding: 40px 60px 25px; border-bottom: 5px solid var(--ink); }
.header-main { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 30px; }
.brand-box { flex: 1; }
.main-title { font-size: 3.2rem; font-weight: 900; letter-spacing: -2px; margin: 10px 0; line-height: 1; }
.dept { font-size: 0.75rem; font-weight: bold; color: var(--ink-light); letter-spacing: 3px; font-family: monospace; }
.sub-info { font-size: 0.8rem; font-weight: bold; color: #999; display: flex; align-items: center; gap: 10px; }
.dot { width: 6px; height: 6px; background: var(--approved); border-radius: 50%; display: inline-block; }
.divider { color: #ddd; }

/* 搜索与选项卡 */
.header-controls { display: grid; grid-template-columns: 1fr 480px; gap: 60px; align-items: flex-end; }
.search-field { position: relative; }
.ink-search { width: 100%; border: none; background: transparent; font-size: 1.4rem; padding: 8px 0; outline: none; border-bottom: 2px solid var(--divider); transition: border-color 0.3s; }
.ink-search:focus { border-color: var(--ink); }

.tab-selector { display: flex; border: 2px solid var(--ink); border-radius: 4px; overflow: hidden; background: var(--ink); gap: 2px; }
.tab-selector button { flex: 1; border: none; background: #fff; padding: 10px 15px; font-size: 0.7rem; font-weight: 900; cursor: pointer; transition: 0.2s; }
.tab-selector button.active { background: var(--ink); color: #fff; }

/* 网格与滚动 */
.archive-scroll { flex: 1; overflow-y: auto; padding: 40px 60px; }
.archive-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 35px; }

/* 档案卡片完整定义 */
.archive-card { display: flex; background: #fff; border: 1px solid var(--divider); transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); cursor: pointer; height: 280px; position: relative; overflow: hidden; }
.archive-card:hover { transform: translateY(-5px); border-color: var(--ink); box-shadow: 0 10px 20px rgba(0,0,0,0.05); }

/* 卡片侧边 */
.card-sidebar { width: 50px; background: var(--ink); color: #fff; display: flex; flex-direction: column; align-items: center; padding: 20px 0; flex-shrink: 0; }
.index-no { font-weight: 900; font-family: monospace; font-size: 0.9rem; }
.type-tag { writing-mode: vertical-rl; transform: rotate(180deg); margin-top: auto; font-size: 0.6rem; font-weight: bold; letter-spacing: 3px; opacity: 0.6; }

/* 卡片内容 */
.card-content { flex: 1; padding: 25px; display: flex; flex-direction: column; position: relative; }
.card-head { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 8px; }
.card-title { font-size: 1.4rem; font-weight: 900; margin: 0; line-height: 1.1; flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

/* 印章效果 */
.stamp-ink { border: 3px double #ccc; color: #ccc; padding: 4px 10px; font-size: 0.7rem; font-weight: 900; transform: rotate(-10deg); opacity: 0.5; flex-shrink: 0; }
.stamp-ink.approved { border-color: var(--approved); color: var(--approved); opacity: 1; }

/* ✅ 标签嵌入样式微调 */
.card-tags-area { margin-bottom: 8px; }
.card-tags-area :deep(.universal-tags-wrapper) { margin-top: 0; padding: 0; border-top: none; gap: 6px; }
.card-tags-area :deep(.tags-icon) { display: none; }
.card-tags-area :deep(.cyber-tag-chip) { font-size: 0.65rem; padding: 2px 8px; border-color: #eee; }

/* 元数据与描述 */
.card-meta { display: grid; grid-template-columns: 1fr 1fr; border-top: 1px solid #eee; padding-top: 12px; margin-bottom: 12px; }
.m-item .k { font-size: 0.6rem; color: #999; font-weight: bold; display: block; }
.m-item .v { font-size: 0.85rem; font-weight: bold; }

.card-desc { font-size: 0.85rem; color: var(--ink-light); line-height: 1.6; height: 1.6em; overflow: hidden; display: -webkit-box; -webkit-line-clamp: 1; -webkit-box-orient: vertical; }

.card-footer { display: flex; justify-content: space-between; border-top: 1px dashed #eee; padding-top: 12px; margin-top: auto; }
.access-btn { font-size: 0.75rem; font-weight: 900; color: var(--ink); text-decoration: underline; }
.file-ref { font-family: monospace; font-size: 0.65rem; color: #bbb; }

/* 特殊状态水印 */
.is-expired { background: var(--paper-old); filter: grayscale(0.8); opacity: 0.8; border-style: dashed; }
.closed-stamp { position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%) rotate(-15deg); border: 4px solid var(--danger); color: var(--danger); padding: 5px 15px; font-size: 1.8rem; font-weight: 900; opacity: 0.3; pointer-events: none; letter-spacing: 5px; text-transform: uppercase; z-index: 5; }

/* 底部 EOF */
.end-seal { display: flex; align-items: center; justify-content: center; gap: 20px; padding: 60px 0; opacity: 0.4; }
.end-seal .line { height: 1px; flex: 1; background: var(--ink); }
.end-seal .text { font-size: 0.75rem; font-weight: 900; letter-spacing: 2px; }

/* 加载与滚动条 */
.loading-state { padding: 100px; text-align: center; color: var(--ink-light); font-weight: bold; letter-spacing: 2px; }
.spinner { width: 40px; height: 40px; border: 4px solid var(--divider); border-top-color: var(--ink); border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 20px; }
@keyframes spin { to { transform: rotate(360deg); } }

.custom-scroll::-webkit-scrollbar { width: 6px; }
.custom-scroll::-webkit-scrollbar-thumb { background: var(--ink); }
</style>