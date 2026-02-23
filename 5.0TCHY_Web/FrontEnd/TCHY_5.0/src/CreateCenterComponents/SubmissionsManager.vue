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
          
          <button class="action-btn" @click="editItem(item)">
            EDIT
          </button>

          <button class="action-btn danger" @click="deleteItem(item)">DELETE</button>
        </div>
      </div>
    </div>

    <div class="cyber-modal-overlay" v-if="isEditing">
      <div class="cyber-modal">
        <div class="modal-header">
          <h3>SYSTEM_OVERRIDE // 修改 {{ editForm.type.toUpperCase() }} 档案</h3>
          <button class="close-btn" @click="isEditing = false">✖</button>
        </div>
        
        <div class="modal-body" v-if="editLoading">
          <div class="loading-txt glitch-text">FETCHING_CORE_DATA...</div>
        </div>

        <form @submit.prevent="submitEdit" class="cyber-form modal-form" v-else>
          
          <div class="form-group" v-if="editForm.previewImages && editForm.previewImages.length > 0">
            <label>影像存档 // ARCHIVED VISUALS (不可在此修改)</label>
            <div class="modal-image-preview-matrix">
              <div v-for="(imgUrl, idx) in editForm.previewImages" :key="idx" class="preview-node">
                <img :src="imgUrl" alt="archive" />
              </div>
            </div>
          </div>

          <div class="form-group">
            <label>数据代号 // TITLE</label>
            <input type="text" v-model="editForm.title" required />
          </div>

          <div class="form-group" v-if="editForm.type === 'post'">
            <label>核心数据 // CONTENT</label>
            <textarea v-model="editForm.content" rows="5"></textarea>
          </div>

          <template v-if="editForm.type === 'art'">
            <div class="form-group">
              <label>创作者 // AUTHOR</label>
              <input type="text" v-model="editForm.author" />
            </div>
            <div class="form-group">
              <label>补充描述 // DESCRIPTION</label>
              <textarea v-model="editForm.desc" rows="4"></textarea>
            </div>
          </template>

          <div class="form-group">
            <label>特征标签 // TAGS</label>
            <CyberTagInput v-model="editForm.tags" :max-tags="5" />
          </div>

          <div class="form-actions">
            <button type="button" class="action-btn" @click="isEditing = false">CANCEL</button>
            <button type="submit" class="cyber-submit-btn">OVERWRITE // 覆写矩阵</button>
          </div>
        </form>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import apiClient from '@/utils/api'
import CyberTagInput from '@/GeneralComponents/CyberTagInput.vue'
const loading = ref(false)
const activeTab = ref('all')
const submissions = ref([])

// 🚀 编辑器状态，新增了 previewImages 数组
const isEditing = ref(false)
const editLoading = ref(false)
const editForm = reactive({ 
  id: null, type: '', title: '', content: '', desc: '', author: '', tags: '', 
  previewImages: [] 
})

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
    title: item.Title || item.title || item.post_title || "无标题档案",
    type: type,
    status: item.Status ?? item.status ?? 0,
    updatedAt: item.UpdateTime || item.uploadAt || item.createTime,
    views: item.ViewCount || item.views || 0,
    comments: item.CommentCount || 0,
    excerpt: item.content ? item.content.substring(0, 50) + '...' : "暂无摘要", 
    cover: item.CoverImage || item.imageUrl || item.url || (item.images && item.images.length > 0 ? item.images[0] : null)
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
    let rawData = Array.isArray(res.data) ? res.data : (res.data?.data || []);
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
const formatDate = (d) => d ? String(d).substring(0, 10) : 'N/A'

// =====================================
// 🚀 核心逻辑：编辑功能
// =====================================

const editItem = async (item) => {
  if (item.type === 'blog') {
    alert(`// TODO: 深度博客将跳转至独立的富文本编辑页，ID: ${item.id}`)
    return;
  }

  isEditing.value = true;
  editLoading.value = true;
  editForm.id = item.id;
  editForm.type = item.type;
  // 清理上一张图的残留
  editForm.previewImages = [];

  try {
    // 拉取完整数据进行回显
    const endpoint = item.type === 'post' ? `/ThePost/${item.id}` : `/Drawing/${item.id}`;
    const res = await apiClient.get(endpoint);
    const data = res.data.data;

    // 🚀 填充表单，包含图片提取逻辑
    if (item.type === 'post') {
      editForm.title = data.post_title || data.title;
      editForm.content = data.content;
      editForm.tags = Array.isArray(data.tags) ? data.tags.join(',') : (data.tags || '');
      editForm.previewImages = data.images || []; 
    } else if (item.type === 'art') {
      editForm.title = data.title;
      editForm.desc = data.desc;
      editForm.author = data.userName || data.authorName;
      editForm.tags = data.tags || '';
      editForm.previewImages = data.url ? [data.url] : (data.imageUrl ? [data.imageUrl] : []);
    }
  } catch (e) {
    alert("档案拉取失败，数据可能已被清除。");
    isEditing.value = false;
  } finally {
    editLoading.value = false;
  }
}

// 提交编辑数据
const submitEdit = async () => {
  try {
    const endpoint = editForm.type === 'post' ? `/ThePost/${editForm.id}` : `/Drawing/${editForm.id}`;
    
    // 组装 PUT 请求负载
    const payload = {
      Title: editForm.title,
      Tags: editForm.tags
    };

    if (editForm.type === 'post') {
      payload.Content = editForm.content;
    } else {
      payload.Desc = editForm.desc;
      payload.AuthorName = editForm.author;
    }

    await apiClient.put(endpoint, payload);
    
    alert("档案覆写成功！");
    isEditing.value = false;
    
    fetchSubmissions();

  } catch (e) {
    alert(`覆写失败: ${e.response?.data?.message || '服务器拒绝连接'}`);
  }
}

const toggleStatus = async (item) => {
  const newStatus = item.status === 0 ? 1 : 0; 
  try {
    let url = '';
    if (item.type === 'post') url = `ThePost/status/${item.id}`;
    else if (item.type === 'art') url = `Drawing/status/${item.id}`;
    else if (item.type === 'blog') return alert("请进入EDIT模式修改状态");

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
.section-header { display: flex; justify-content: space-between; align-items: center; border-bottom: 3px solid #111; padding-bottom: 10px; margin-bottom: 20px; }
.section-header h2 { font-family: 'Anton'; font-size: 1.8rem; margin: 0; }
.filter-bar { display: flex; gap: 10px; }
.filter-tag { background: transparent; border: 1px solid #111; padding: 5px 15px; font-family: 'JetBrains Mono'; font-weight: bold; cursor: pointer; transition: 0.2s; }
.filter-tag:hover, .filter-tag.active { background: #111; color: #fff; }
.mini-stats { display: flex; gap: 20px; margin-bottom: 30px; }
.ms-box { background: #fff; border-left: 4px solid #111; padding: 10px 20px; font-weight: bold; font-size: 0.8rem; box-shadow: 4px 4px 0 rgba(0,0,0,0.05); }
.ms-box.warning { border-color: #f1c40f; }
.ms-box.success { border-color: #2ecc71; }
.submission-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 20px; }
.sub-card { background: #fff; border: 2px solid #111; position: relative; display: flex; flex-direction: column; min-height: 200px; transition: 0.2s; overflow: hidden; }
.sub-card:hover { transform: translate(-2px, -2px); box-shadow: 6px 6px 0 #D92323; }
.card-status-bar { height: 6px; width: 100%; flex-shrink: 0; }
.s-pending { background: #f1c40f; }
.s-active { background: #2ecc71; }
.s-rejected { background: #D92323; }
.card-type-tag { position: absolute; top: 15px; right: 15px; font-size: 0.6rem; background: rgba(238,238,238,0.9); padding: 2px 6px; font-weight: bold; z-index: 2; backdrop-filter: blur(2px); }
.card-cover { width: 100%; height: 160px; border-bottom: 2px solid #111; background: #222; overflow: hidden; flex-shrink: 0; }
.card-cover img { width: 100%; height: 100%; object-fit: cover; display: block; transition: transform 0.3s ease; }
.sub-card:hover .card-cover img { transform: scale(1.05); }
.card-body { padding: 20px; flex: 1; display: flex; flex-direction: column; }
.card-title { font-family: 'Anton'; font-size: 1.2rem; margin: 0 0 10px; color: #111; }
.card-excerpt { font-size: 0.8rem; color: #666; line-height: 1.4; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; margin-bottom: 10px; }
.card-footer { margin-top: auto; display: flex; justify-content: space-between; font-size: 0.65rem; color: #999; font-weight: bold; }
.card-actions { display: flex; border-top: 2px solid #111; flex-shrink: 0; }
.action-btn { flex: 1; border: none; background: #fff; padding: 10px; font-family: 'Anton'; font-size: 0.8rem; cursor: pointer; transition: 0.2s; border-right: 1px solid #111; }
.action-btn:last-child { border-right: none; }
.action-btn:hover { background: #111; color: #fff; }
.action-btn.toggle { font-weight: bold; color: #555; }
.action-btn.toggle:hover { background: #f1c40f; color: #111; }
.action-btn.danger:hover { background: #D92323; color: #fff; }

/* =====================================
   🚀 全息编辑面板 (Modal) 样式
===================================== */
.cyber-modal-overlay {
  position: fixed; top: 0; left: 0; width: 100vw; height: 100vh;
  background: rgba(0, 0, 0, 0.85); backdrop-filter: blur(5px);
  display: flex; justify-content: center; align-items: center; z-index: 9999;
}

.cyber-modal {
  background: #fff; border: 3px solid #111; width: 90%; max-width: 600px;
  box-shadow: 10px 10px 0 #D92323; position: relative;
  display: flex; flex-direction: column; max-height: 90vh;
}

.modal-header {
  background: #111; color: #fff; padding: 15px 20px;
  display: flex; justify-content: space-between; align-items: center;
}
.modal-header h3 { margin: 0; font-family: 'Anton'; font-size: 1.2rem; letter-spacing: 1px;}
.close-btn { background: transparent; border: none; color: #D92323; font-size: 1.2rem; cursor: pointer; transition: 0.2s;}
.close-btn:hover { color: #fff; transform: scale(1.1); }

.modal-body, .modal-form { padding: 25px; overflow-y: auto; }

.form-group { display: flex; flex-direction: column; gap: 8px; margin-bottom: 20px;}
.form-group label { font-weight: bold; font-size: 0.85rem; color: #333; }
.cyber-form input[type="text"], .cyber-form textarea {
  width: 100%; padding: 12px; border: 2px solid #ccc; background: #f9f9f9;
  font-family: 'JetBrains Mono', monospace; font-size: 0.9rem; transition: 0.2s;
  outline: none; resize: vertical; box-sizing: border-box;
}
.cyber-form input[type="text"]:focus, .cyber-form textarea:focus {
  border-color: #111; background: #fff; box-shadow: 4px 4px 0 rgba(217, 35, 35, 0.2);
}

/* 🚀 模态框里的只读图片矩阵 */
.modal-image-preview-matrix {
  display: flex; gap: 10px; overflow-x: auto; padding-bottom: 10px;
  border-left: 4px solid #D92323; padding-left: 10px; background: #f9f9f9;
}
.modal-image-preview-matrix .preview-node {
  width: 100px; height: 100px; flex-shrink: 0;
  border: 2px solid #ccc; background: #eee;
  clip-path: polygon(0 0, 100% 0, 100% calc(100% - 8px), calc(100% - 8px) 100%, 0 100%);
}
.modal-image-preview-matrix img {
  width: 100%; height: 100%; object-fit: cover;
}

.form-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 20px; }
.cyber-submit-btn {
  background: #111; color: #fff; border: none; padding: 12px 30px; 
  font-family: 'Anton'; font-size: 1rem; cursor: pointer; transition: 0.2s;
}
.cyber-submit-btn:hover { background: #D92323; box-shadow: 4px 4px 0 rgba(0,0,0,0.1); transform: translate(-2px, -2px); }

@keyframes glitch { 0% { opacity: 1; } 50% { opacity: 0.8; } 100% { opacity: 1; } }
.glitch-text { animation: glitch 0.5s infinite; font-family: 'Anton'; font-size: 1.5rem; text-align: center; color: #111;}
</style>