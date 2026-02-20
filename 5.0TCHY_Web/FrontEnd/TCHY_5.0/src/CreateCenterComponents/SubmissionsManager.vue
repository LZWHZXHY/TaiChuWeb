<template>
  <section class="view-section">
    <div class="section-header">
      <h2>SUBMISSION_ARCHIVE // 稿件库管理</h2>
      <div class="filter-bar">
        <button 
          v-for="tab in tabs" 
          :key="tab.value"
          class="filter-tag"
          :class="{ active: activeTab === tab.value }"
          @click="activeTab = tab.value"
        >
          {{ tab.label }}
        </button>
      </div>
    </div>

    <div class="mini-stats">
      <div class="ms-box">TOTAL: {{ filteredList.length }}</div>
      <div class="ms-box warning">PENDING: {{ filteredList.filter(i => i.status === 0).length }}</div>
      <div class="ms-box success">PUBLISHED: {{ filteredList.filter(i => i.status === 1).length }}</div>
    </div>

    <div v-if="loading" class="loading-txt">SYNCING_RESOURCES...</div>

    <div v-else class="submission-grid">
     

      <div v-for="item in filteredList" :key="`${item.type}-${item.id}`" class="sub-card">
        <div class="card-status-bar" :class="statusClass(item.status)"></div>
        <div class="card-type-tag">{{ item.type.toUpperCase() }}</div>
        
        <div v-if="item.cover" class="card-cover">
          <img :src="item.cover" :alt="item.title" loading="lazy" />
        </div>

        <div class="card-body">
          <h3 class="card-title">{{ item.title }}</h3>
          <p class="card-excerpt">{{ item.excerpt }}</p>
          <div class="card-footer">
            <span class="date">MOD_DATE: {{ formatDate(item.updatedAt) }}</span>
            <span class="views">👁 {{ item.views || 0 }} // 💬 {{ item.comments }}</span>
          </div>
        </div>

        <div class="card-actions">
          <button class="action-btn toggle" @click="toggleStatus(item)">
            {{ item.status === 0 ? 'HIDE' : 'SHOW' }}
          </button>
          
          <button v-if="item.type === 'blog'" class="action-btn" @click="editItem(item)">
            EDIT
          </button>

          <button class="action-btn danger" @click="deleteItem(item)">DELETE</button>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import apiClient from '@/utils/api'

const loading = ref(false)
const activeTab = ref('all')
const submissions = ref([])

const tabs = [
  { label: '全部 // ALL', value: 'all' },
  { label: '绘画 // ART', value: 'art' },
  { label: '帖子 // POST', value: 'post' },
  { label: '博客 // BLOG', value: 'blog' }
]

const filteredList = computed(() => {
  if (activeTab.value === 'all') return submissions.value
  return submissions.value.filter(item => item.type === activeTab.value)
})

const normalizeData = (list, type) => {
  if (!Array.isArray(list)) return []; 

  return list.map(item => ({
    id: item.Id || item.id, 
    title: item.Title || "无标题档案",
    type: type,
    status: item.Status ?? 0,
    updatedAt: item.UpdateTime,
    views: item.ViewCount || 0,
    comments: item.CommentCount || 0,
    excerpt: "暂无摘要", 
    cover: item.CoverImage || null
  }))
}

const fetchAll = async () => {
  try {
    const [postRes, artRes, blogRes] = await Promise.allSettled([
      apiClient.get('ThePost/my-posts'),
      apiClient.get('Drawing/my-drawings'),
      apiClient.get('Blog/my-articles')
    ]);

    const posts = postRes.status === 'fulfilled' ? (postRes.value.data?.data || []) : [];
    const arts = artRes.status === 'fulfilled' ? (artRes.value.data?.data || []) : [];
    const blogs = blogRes.status === 'fulfilled' ? (blogRes.value.data || []) : [];

    submissions.value = [
      ...normalizeData(posts, 'post'),
      ...normalizeData(arts, 'art'),
      ...normalizeData(blogs, 'blog')
    ].sort((a, b) => new Date(b.updatedAt) - new Date(a.updatedAt));

  } catch (e) {
    console.error("全局数据矩阵同步失败", e);
  }
}

const fetchSubmissions = async () => {
  loading.value = true;
  submissions.value = [];
  try {
    if (activeTab.value === 'all') {
      await fetchAll();
      return;
    }

    let endpoint = '';
    switch (activeTab.value) {
      case 'post': endpoint = 'ThePost/my-posts'; break;
      case 'art': endpoint = 'Drawing/my-drawings'; break;
      case 'blog': endpoint = 'Blog/my-articles'; break;
    }

    const res = await apiClient.get(endpoint);
    
    let rawData = [];
    if (Array.isArray(res.data)) {
      rawData = res.data; 
    } else if (res.data && Array.isArray(res.data.data)) {
      rawData = res.data.data; 
    }

    submissions.value = normalizeData(rawData, activeTab.value);
    
  } catch (e) {
    console.error("同步分类数据失败", e);
  } finally {
    loading.value = false;
  }
};

watch(activeTab, () => {
  fetchSubmissions();
});

const statusClass = (s) => ({ 0: 's-pending', 1: 's-active', 2: 's-rejected' }[s] || 's-pending')
const formatDate = (d) => d ? d.substring(0, 10) : 'N/A'

// =====================================
// 交互逻辑
// =====================================

const initiateCreation = () => {
  alert('// TODO: 唤醒创造矩阵 (跳转至发布页)')
}

// ✅ 修改后的编辑逻辑：增加了安全判定
const editItem = (item) => {
  if (item.type !== 'blog') return; // 双重保护：前端限制只能编辑博客
  console.log('即将进入博客编辑模式:', item)
  alert(`// TODO: 跳转至博客编辑器，文章ID: ${item.id}`)
}

const toggleStatus = async (item) => {
  const newStatus = item.status === 0 ? 1 : 0; 
  try {
    let url = '';
    if (item.type === 'post') url = `ThePost/status/${item.id}`;
    else if (item.type === 'art') url = `Drawing/status/${item.id}`;
    else if (item.type === 'blog') {
      alert("博客系统使用整体覆盖更新，请进入EDIT模式修改状态");
      return;
    }

    await apiClient.put(url, { status: newStatus });
    item.status = newStatus; 
  } catch (e) {
    alert("状态同步指令遭到拦截");
  }
}

const deleteItem = async (item) => {
  if (!confirm(`警告：确定要将该 ${item.type.toUpperCase()} 记录从矩阵中永久抹除吗？此操作不可逆。`)) return;
  
  try {
    let url = '';
    if (item.type === 'post') url = `ThePost/${item.id}`;
    else if (item.type === 'art') url = `Drawing/${item.id}`;
    else if (item.type === 'blog') url = `Blog/articles/${item.id}`;

    await apiClient.delete(url);
    submissions.value = submissions.value.filter(s => s.id !== item.id);
  } catch (e) {
    alert("数据抹除指令执行失败");
  }
}

onMounted(fetchSubmissions)
</script>

<style scoped>
.view-section { position: relative; z-index: 1; max-width: 1200px; margin: 0 auto; }

/* 头部筛选 */
.section-header { display: flex; justify-content: space-between; align-items: center; border-bottom: 3px solid #111; padding-bottom: 10px; margin-bottom: 20px; }
.section-header h2 { font-family: 'Anton'; font-size: 1.8rem; margin: 0; }
.filter-bar { display: flex; gap: 10px; }
.filter-tag { background: transparent; border: 1px solid #111; padding: 5px 15px; font-family: 'JetBrains Mono'; font-weight: bold; cursor: pointer; transition: 0.2s; }
.filter-tag:hover, .filter-tag.active { background: #111; color: #fff; }

/* 统计条 */
.mini-stats { display: flex; gap: 20px; margin-bottom: 30px; }
.ms-box { background: #fff; border-left: 4px solid #111; padding: 10px 20px; font-weight: bold; font-size: 0.8rem; box-shadow: 4px 4px 0 rgba(0,0,0,0.05); }
.ms-box.warning { border-color: #f1c40f; }
.ms-box.success { border-color: #2ecc71; }

/* 稿件网格 */
.submission-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 20px; }

.sub-card { 
  background: #fff; border: 2px solid #111; position: relative; 
  display: flex; flex-direction: column; min-height: 200px; transition: 0.2s;
  overflow: hidden; 
}
.sub-card:hover { transform: translate(-2px, -2px); box-shadow: 6px 6px 0 #D92323; }

/* 状态条 */
.card-status-bar { height: 6px; width: 100%; flex-shrink: 0; }
.s-pending { background: #f1c40f; }
.s-active { background: #2ecc71; }
.s-rejected { background: #D92323; }

.card-type-tag { position: absolute; top: 15px; right: 15px; font-size: 0.6rem; background: rgba(238,238,238,0.9); padding: 2px 6px; font-weight: bold; z-index: 2; backdrop-filter: blur(2px); }

/* 图片容器样式 */
.card-cover { 
  width: 100%; 
  height: 160px; 
  border-bottom: 2px solid #111; 
  background: #222; 
  overflow: hidden;
  flex-shrink: 0; 
}
.card-cover img { 
  width: 100%; 
  height: 100%; 
  object-fit: cover; 
  display: block;
  transition: transform 0.3s ease;
}
.sub-card:hover .card-cover img {
  transform: scale(1.05);
}

.card-body { padding: 20px; flex: 1; display: flex; flex-direction: column; }
.card-title { font-family: 'Anton'; font-size: 1.2rem; margin: 0 0 10px; color: #111; }
.card-excerpt { font-size: 0.8rem; color: #666; line-height: 1.4; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; margin-bottom: 10px; }

.card-footer { margin-top: auto; display: flex; justify-content: space-between; font-size: 0.65rem; color: #999; font-weight: bold; }

/* ✅ 修改后的按钮操作区：改用 Flexbox 自动平分宽度 */
.card-actions { display: flex; border-top: 2px solid #111; flex-shrink: 0; }
.action-btn { flex: 1; border: none; background: #fff; padding: 10px; font-family: 'Anton'; font-size: 0.8rem; cursor: pointer; transition: 0.2s; border-right: 1px solid #111; }
.action-btn:last-child { border-right: none; }
.action-btn:hover { background: #111; color: #fff; }
.action-btn.toggle { font-weight: bold; color: #555; }
.action-btn.toggle:hover { background: #f1c40f; color: #111; }
.action-btn.danger:hover { background: #D92323; color: #fff; }

/* 新增按钮卡片 */
.add-entry { border-style: dashed; color: #ccc; justify-content: center; align-items: center; cursor: pointer; background: transparent; }
.add-entry:hover { color: #D92323; border-color: #D92323; background: #fff; }
.add-entry .icon { font-size: 2.5rem; }
.add-entry .label { font-size: 0.8rem; font-weight: bold; }
</style>