<script setup>
import { ref, computed, watch } from 'vue'
import { useRouter } from 'vue-router' // 引入路由
import apiClient from '@/utils/api'

const router = useRouter() // 初始化路由实例

const activeTab = ref('blog')
const isLoading = ref(false)
const items = ref([])
const searchQuery = ref('')

const contentTabs = [
  { key: 'blog', label: '技术博客', endpoint: '/Blog/my-articles' },
  { key: 'post', label: '动态发布', endpoint: '/ThePost/my-posts' },
  { key: 'drawing', label: '绘画作品', endpoint: '/Drawing/my-drawings' }
]

const fetchManagementData = async () => {
  isLoading.value = true
  const tab = contentTabs.find(t => t.key === activeTab.value)
  try {
    const res = await apiClient.get(tab.endpoint)
    const rawData = res.data.data || res.data || []
    
    items.value = rawData.map(item => ({
      id: item.Id || item.id,
      title: item.Title || item.post_title || '未命名内容',
      status: item.Status ?? item.status,
      time: item.UpdateTime || item.UpdateTime || item.uploadAt,
      views: item.ViewCount || item.view_count || 0,
      comments: item.CommentCount || item.comment_count || 0,
      cover: item.CoverImage || item.ImageUrl || item.coverImage
    }))
  } catch (err) {
    console.error("内容同步失败", err)
  } finally {
    isLoading.value = false
  }
}

watch(activeTab, () => fetchManagementData(), { immediate: true })

// --- 跳转逻辑 ---
const handleViewDetail = (item) => {
  // 根据当前选中的 Tab 匹配路由表中的 Name
  const routeMap = {
    blog: 'BlogDetail',
    post: 'PostDetail',
    drawing: 'ArtWorkDetail'
  }
  
  const targetName = routeMap[activeTab.value]
  
  if (targetName) {
    router.push({
      name: targetName,
      params: { id: item.id }
    })
  }
}

// --- 操作逻辑 ---
const handleToggleHide = async (item) => {
  try {
    let newStatus = item.status === 0 ? 1 : 0; 
    const endpoint = activeTab.value === 'blog' 
      ? `/Blog/articles/${item.id}` 
      : `/ThePost/status/${item.id}`;
    
    await apiClient.put(endpoint, { status: newStatus });
    item.status = newStatus;
  } catch (err) {
    console.error("切换失败", err);
  }
}

const handleDelete = async (item) => {
  if (!confirm(`确定要永久删除《${item.title}》吗？此操作无法撤销。`)) return;
  try {
    let endpoint = '';
    switch(activeTab.value) {
      case 'blog': endpoint = `/Blog/articles/${item.id}`; break;
      case 'post': endpoint = `/ThePost/${item.id}`; break;
      case 'drawing': endpoint = `/Drawing/${item.id}`; break;
    }
    await apiClient.delete(endpoint);
    items.value = items.value.filter(i => i.id !== item.id);
  } catch (err) {
    alert("删除失败");
  }
}

const formatDate = (dateStr) => {
  if (!dateStr) return '未知时间'
  const date = new Date(dateStr)
  return `${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, '0')}-${date.getDate().toString().padStart(2, '0')}`
}

const getStatusInfo = (status) => {
  if (activeTab.value === 'blog') {
    return status === 1 ? { text: '展示中', class: 'published', isHidden: false } : { text: '已隐藏', class: 'draft', isHidden: true }
  }
  return status === 0 ? { text: '展示中', class: 'published', isHidden: false } : { text: '已隐藏', class: 'draft', isHidden: true }
}

const filteredItems = computed(() => {
  if (!searchQuery.value) return items.value;
  return items.value.filter(i => i.title.toLowerCase().includes(searchQuery.value.toLowerCase()));
})
</script>

<template>
  <div class="content-manage-panel">
    <div class="panel-header">
      <div class="title-row">
        <h2>内容管理中枢</h2>
        <div class="search-bar">
          <input v-model="searchQuery" placeholder="输入关键字筛选记录..." />
        </div>
      </div>
      
      <div class="tab-nav">
        <button 
          v-for="tab in contentTabs" 
          :key="tab.key"
          :class="['tab-btn', { active: activeTab === tab.key }]"
          @click="activeTab = tab.key"
        >
          {{ tab.label }}
        </button>
      </div>
    </div>

    <div class="content-body" v-loading="isLoading">
      <div v-if="filteredItems.length === 0" class="empty-placeholder">
        当前分区尚无符合条件的数据
      </div>

      <div 
        v-for="item in filteredItems" 
        :key="item.id" 
        class="manage-card"
        :class="{ 'is-hidden-item': getStatusInfo(item.status).isHidden }"
      >
        <div class="card-preview clickable" @click="handleViewDetail(item)">
          <img v-if="item.cover" :src="item.cover" loading="lazy" />
          <div v-else class="empty-cover">TEXT</div>
          <div v-if="getStatusInfo(item.status).isHidden" class="hide-overlay">已隐藏</div>
        </div>

        <div class="card-detail">
          <div class="detail-top">
            <span :class="['status-dot', getStatusInfo(item.status).class]">
              {{ getStatusInfo(item.status).text }}
            </span>
            <span class="id-tag">#{{ item.id }}</span>
          </div>
          
          <h3 class="item-title clickable" @click="handleViewDetail(item)">
            {{ item.title }}
          </h3>
          
          <div class="item-meta">
            <span>📅 {{ formatDate(item.time) }}</span>
            <span>👁️ {{ item.views }}</span>
            <span>💬 {{ item.comments }}</span>
          </div>
        </div>

        <div class="card-ops">
          <button 
            class="op-btn hide-toggle" 
            @click="handleToggleHide(item)"
          >
            {{ getStatusInfo(item.status).isHidden ? '恢复显示' : '隐藏内容' }}
          </button>
          
          <button class="op-btn delete" @click="handleDelete(item)">删除</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.content-manage-panel { padding: 40px; background: #FFFDF8; min-height: 100%; }

/* 新增可点击交互样式 */
.clickable {
  cursor: pointer;
  transition: all 0.2s ease;
}

.item-title.clickable:hover {
  color: #e67e22; /* 悬浮变为主题橙色 */
  text-decoration: underline;
}

.card-preview.clickable:hover img {
  transform: scale(1.05); /* 略微放大预览图 */
  filter: brightness(1.1);
}

/* 之前的基础样式 */
.manage-card.is-hidden-item { opacity: 0.6; filter: grayscale(0.5); background: #fcfcfc; }
.card-preview { position: relative; width: 100px; height: 70px; border-radius: 10px; overflow: hidden; background: #f9f9f9; margin-right: 20px; flex-shrink: 0; }
.card-preview img { width: 100%; height: 100%; object-fit: cover; transition: transform 0.3s; }
.hide-overlay { position: absolute; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.4); color: white; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: bold; }
.op-btn.hide-toggle { background: #f0f0f0; color: #444; }
.op-btn.hide-toggle:hover { background: #e0e0e0; }
.panel-header { margin-bottom: 30px; }
.title-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 25px; }
.title-row h2 { font-size: 24px; font-weight: 800; color: #1a1a1a; margin: 0; }
.search-bar input { background: #f5f5f5; border: none; padding: 10px 20px; border-radius: 20px; font-size: 14px; width: 240px; transition: all 0.3s; }
.tab-nav { display: flex; gap: 10px; border-bottom: 1px solid #eee; padding-bottom: 2px; }
.tab-btn { background: none; border: none; padding: 10px 25px; cursor: pointer; font-size: 15px; font-weight: 600; color: #999; position: relative; }
.tab-btn.active { color: #000; }
.tab-btn.active::after { content: ''; position: absolute; bottom: -2px; left: 0; width: 100%; height: 3px; background: #000; }
.manage-card { display: flex; align-items: center; padding: 15px; background: #fff; border-radius: 16px; margin-bottom: 15px; border: 1px solid #f0f0f0; transition: all 0.3s; }
.manage-card:hover { transform: translateY(-3px); box-shadow: 0 10px 30px rgba(0,0,0,0.05); }
.card-detail { flex: 1; min-width: 0; }
.detail-top { display: flex; align-items: center; gap: 10px; margin-bottom: 6px; }
.status-dot { font-size: 10px; padding: 2px 8px; border-radius: 4px; font-weight: bold; }
.status-dot.published { background: #e8f5e9; color: #2e7d32; }
.status-dot.draft { background: #eee; color: #666; }
.item-title { font-size: 16px; font-weight: 700; margin: 0 0 8px 0; color: #1a1a1a; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.item-meta { display: flex; gap: 20px; font-size: 12px; color: #999; }
.card-ops { display: flex; gap: 10px; margin-left: 20px; }
.op-btn { padding: 8px 18px; border-radius: 10px; border: none; font-size: 13px; font-weight: 700; cursor: pointer; transition: 0.2s; }
.op-btn.delete { background: #fff1f0; color: #ff4d4f; }
.op-btn:hover { opacity: 0.8; }
.empty-placeholder { text-align: center; padding: 100px 0; color: #bbb; font-weight: 600; }
</style>