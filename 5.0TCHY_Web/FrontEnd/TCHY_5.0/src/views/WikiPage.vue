<template>
  <div class="pro-wiki-app">
    <GlobalHeader 
      @create="handleCreateArticle" 
      @open-search="handleSearch" 
    />

    <div class="wiki-layout">
      <div class="sidebar-wrapper">
        <div class="admin-entry" v-if="isAdmin">
          <button class="btn-review-entry" @click="openReviewCenter">
            🛡️ 审核中心
            <span v-if="pendingReviews.length > 0" class="badge">{{ pendingReviews.length }}</span>
          </button>

          <button class="btn-trash-entry" @click="showTrashBin = true">
            🗑️ 词条回收站
          </button>
        </div>
        
        <div class="category-action-entry" v-if="isAdmin">
          <button class="btn-create-category" @click="openCreateCategoryModal">
            📁 新建分类目录
          </button>
        </div>

        <SidebarLeft 
          :treeData="directoryTree" 
          :activeId="String(currentArticle?.id || '')"
          @select-article="handleSelectArticle" 
          @edit-category="openEditCategoryModal"   
        />
      </div>

      <main class="main-content-area">
        <div v-if="viewStatus === 'loading'" class="loading-state">
          <div class="spinner"></div>
          <p>正在同步寰宇星际数据...</p>
        </div>

        <WikiReviewCenter 
          v-else-if="viewStatus === 'reviewing'"
          :reviews="pendingReviews"
          @exit="exitReviewCenter"
          @action="handleReviewAction"
        />

        <WikiEditor 
          v-else-if="viewStatus === 'editing'"
          :article="currentArticle"
          :treeData="directoryTree"    
          @save="handleSave"
          @cancel="cancelEdit"
        />

        <ArticleViewer 
          v-else-if="viewStatus === 'viewing' && currentArticle"
          :key="'view-' + currentArticle.id"
          :article="currentArticle" 
          :isAdmin="isAdmin" 
          @wiki-link-click="handleWikiLinkJump"
          @edit="startEdit" 
          @delete-article="handleDeleteArticle" 
        />

        <div v-else class="loading-state">
          <p>请从左侧选择一个词条开始探索宇宙...</p>
        </div>
      </main>

      <SidebarRight 
        v-if="viewStatus === 'viewing' && currentToc.length > 0"
        :tocList="currentToc" 
        :activeId="activeHeadingId"
        @scroll-to="handleScroll" 
      />
    </div>

    <WikiTrashBin 
      :visible="showTrashBin" 
      :isSuperAdmin="isSuperAdmin"
      @close="showTrashBin = false"
      @refresh-tree="loadCategoryTree"
    />

    <CategoryModal 
      :visible="showCategoryModal" 
      :treeData="directoryTree" 
      :isEdit="isEditCategoryMode"
      :editData="currentEditCategoryData" 
      @close="showCategoryModal = false"
      @submit="handleSubmitCategory"
      @delete="handleDeleteCategory" 
    />
  </div>
</template>

<script setup>
import { ref, onMounted, computed, nextTick, watch } from 'vue' // 补上 watch
import { useRoute, useRouter } from 'vue-router' // 🚀 引入路由
// ... 其他引入保持不变
import apiClient from '@/utils/api' 
import { useAuthStore } from '@/utils/auth'

// 引入子组件
import GlobalHeader from '@/WikiComponents/WikiHeader.vue'
import SidebarLeft from '@/WikiComponents/WikiSidebarLeft.vue'
import ArticleViewer from '@/WikiComponents/WikiViewer.vue'
import SidebarRight from '@/WikiComponents/WikiSidebarRight.vue'
import WikiEditor from '@/WikiComponents/WikiEditor.vue'
import WikiReviewCenter from '@/WikiComponents/WikiReviewCenter.vue'
import CategoryModal from '@/WikiComponents/CategoryModal.vue'
import WikiTrashBin from '@/WikiComponents/WikiTrashBin.vue'

const authStore = useAuthStore()
const route = useRoute()   // 用于获取当前 URL 上的信息
const router = useRouter() // 用于触发页面跳转


// --- 🎯 核心状态机 ---
// 取值范围: 'loading', 'viewing', 'editing', 'reviewing', 'empty'
const viewStatus = ref('loading') 

const currentArticle = ref(null)
const directoryTree = ref([]) 
const currentToc = ref([])
const activeHeadingId = ref('')
const pendingReviews = ref([]) 

const showTrashBin = ref(false)
const showCategoryModal = ref(false)
const isEditCategoryMode = ref(false)
const currentEditCategoryData = ref({})

// 🛡️ 权限计算
const isAdmin = computed(() => {
  const user = authStore.user?.data || authStore.user;
  const roles = user?.RoleCodes || user?.roleCodes || []; 
  return roles.includes('WikiAdmin') || roles.includes('SuperAdmin');
});

const isSuperAdmin = computed(() => {
  const user = authStore.user?.data || authStore.user;
  const roles = user?.RoleCodes || user?.roleCodes || []; 
  return roles.includes('SuperAdmin');
});

// --- 生命周期 ---
onMounted(async () => {
  await loadCategoryTree(); 
  
  // 🚀 读取 URL 中的 ID，如果没有则默认加载 1 号文章
  const targetId = route.params.id || '1'; 
  
  // 如果是空 URL (/wiki)，我们把它重定向到带有 ID 的标准 URL (/wiki/1)，方便用户直接复制
  if (!route.params.id) {
    router.replace(`/wiki/${targetId}`);
  } else {
    await fetchArticle(targetId); 
  }

  if (isAdmin.value) fetchPendingReviews(true);
})

/**
 * 🚀 极致稳健的文章加载逻辑
 * 彻底解决 nextSibling 和 shapeFlag 报错
 */
const fetchArticle = async (id) => {
  // 1. 立即切换到加载态，强制卸载当前所有内容组件，清空 DOM
  viewStatus.value = 'loading';
  currentToc.value = [];

  try {
    const res = await apiClient.get(`/Wiki/article/${id}`);
    const data = res.data;

    // 2. 数据标准化映射
    const blocks = (data.blocks || []).map((b, index) => ({
      id: b.Id || b.id || `b-${index}`,
      content: String(b.Content || b.content || ""), 
      lastEditor: b.LastEditor || b.lastEditor || "系统",
      lastEditorId: b.LastEditorId || b.lastEditorId || 0, 
      updatedAt: b.UpdatedAt || b.updatedAt || "",
      
      // 🚀 终极修复：把后端传来的贡献者数组安全地接住！
      contributors: b.Contributors || b.contributors || []
    }));

    // 3. 准备数据对象
    currentArticle.value = {
      ...data,
      id: data.id,
      blocks: blocks
    };

    // 4. 第一步：先结束加载态，让阅读器挂载
    await nextTick();
    viewStatus.value = 'viewing';

    // 5. 第二步：在阅读器 DOM 稳定后再生成目录，避开渲染冲突
    await nextTick();
    const fullMarkdown = blocks.map(b => b.content).join('\n\n');
    generateToc(fullMarkdown);

  } catch (error) {
    console.error("加载文章失败:", error);
    viewStatus.value = 'empty';
  }
};



/**
 * 🚀 字段映射修复：解决“分类显示为空”问题
 */
const fetchPendingReviews = async (silent = false) => {
  try {
    const res = await apiClient.get(`/Wiki/pending-reviews?adminId=${authStore.userID}`);
    const rawData = res.data || res;
    
    pendingReviews.value = rawData.map(r => ({
      id: r.Id || r.id,
      articleTitle: r.ArticleTitle || r.articleTitle,
      title: r.Title || r.title,
      authorName: r.AuthorName || r.authorName,
      createdAt: r.CreatedAt || r.createdAt,
      editSummary: r.EditSummary || r.editSummary,
      oldContent: r.OldContent || r.oldContent || '',
      newContent: r.NewContent || r.newContent || '',
      oldCategoryName: r.OldCategoryName || r.oldCategoryName || '未分类',
      newCategoryName: r.NewCategoryName || r.newCategoryName || '未分类'
    }));
  } catch (e) {
    console.error('拉取审核失败', e);
  }
};

// --- 其他业务逻辑 ---

const loadCategoryTree = async () => {
  try {
    const res = await apiClient.get('/Wiki/category-tree');
    directoryTree.value = res.data ?? res;
  } catch (e) {}
}

// 🚀 改造左侧点击：不再直接调接口，而是改变 URL
const handleSelectArticle = (id) => {
  router.push(`/wiki/${id}`);
};

// 🚀 改造内部链接跳转：计算出 ID 后，改变 URL
const handleWikiLinkJump = (title) => {
  const findIdByTitle = (nodes, target) => {
    if (!nodes) return null;
    for (const node of nodes) {
      if (node.Type === 'article' && node.Title === target) return node.Id;
      if (node.Children) {
        const found = findIdByTitle(node.Children, target);
        if (found) return found;
      }
    }
    return null;
  };

  const targetId = findIdByTitle(directoryTree.value, title);
  if (targetId) {
    router.push(`/wiki/${targetId}`); // 👈 修改这里
  } else {
    alert(`词条《${title}》尚未创建。`);
  }
};

const openReviewCenter = () => { viewStatus.value = 'reviewing'; fetchPendingReviews(); }
const exitReviewCenter = () => fetchArticle(currentArticle.value?.id || '1');

const startEdit = () => { viewStatus.value = 'editing'; }
const cancelEdit = () => { viewStatus.value = 'viewing'; }

const handleSave = async (editData) => {
  // 1. 🛑 提取真正的用户对象 (参考你 isAdmin 里的写法)
  const currentUser = authStore.user?.data || authStore.user || {};
  
  // 2. 🛑 兼容各种可能的字段名大小写，找到真正的 ID
  const actualUserId = currentUser.Id || currentUser.id || authStore.userId || authStore.userID;

  // 3. 🛑 打印出来看看到底拿到了什么（如果控制台打印是 undefined，说明你的 authStore 结构不对）
  console.log("🛠️ 准备提交的 User ID:", actualUserId);

  // 4. 🛑 防呆拦截：如果没有拿到 ID，直接阻止请求，防止触发后端的 400 报错
  if (!actualUserId || isNaN(Number(actualUserId))) {
    alert("无法获取当前用户信息，请尝试重新登录！");
    return;
  }

  try {
    const payload = {
      ArticleId: Number(currentArticle.value.id) || 0,
      CategoryId: Number(editData.categoryId || currentArticle.value.categoryId || 1),
      UserId: Number(actualUserId), // 👈 此时这里 100% 是个合法的纯数字了
      Title: editData.title,
      ContentMarkdown: editData.content,
      EditSummary: currentArticle.value.id === 0 ? "创建新词条" : "内容修改"
    };

    console.log("🚀 发送给后端的完整数据:", payload);

    await apiClient.post('/Wiki/submit-edit', payload);
    
    alert("提交成功，请等待审核！");

    if (isAdmin.value) {
      await fetchPendingReviews(true); 
    }

    fetchArticle(currentArticle.value.id || '1');
  } catch (e) { 
    console.error("保存失败:", e);
    alert("保存失败，请打开控制台查看具体错误！"); 
  }
}

const handleReviewAction = async (revisionId, isApproved) => {
  try {
    await apiClient.post('/Wiki/review-edit', { AdminId: authStore.userID, RevisionId: revisionId, IsApproved: isApproved });
    await fetchPendingReviews();
    if (isApproved) await loadCategoryTree();
  } catch (e) { alert("操作失败"); }
}

const handleDeleteArticle = async (id) => {
  if (!confirm("确定要删除此词条吗？")) return;
  try {
    await apiClient.post('/Wiki/delete-article', { UserId: authStore.userID, ArticleId: id });
    await loadCategoryTree();
    fetchArticle('1');
  } catch (e) {}
}

const handleCreateArticle = () => {
  currentArticle.value = { id: 0, title: "", blocks: [] };
  viewStatus.value = 'editing';
}

const generateToc = (markdown) => {
  if (!markdown) return currentToc.value = [];
  const headings = markdown.match(/^(#{1,6})\s+(.+)$/gm);
  currentToc.value = headings?.map((h, i) => ({
    id: `heading-${i}`,
    level: h.match(/^#+/)[0].length,
    text: h.replace(/^#+\s+/, '').trim()
  })) || [];
}

const handleSubmitCategory = async (d) => {
  const url = isEditCategoryMode.value ? '/Wiki/update-category' : '/Wiki/create-category';
  
  // 🚀 核心修复：兼容大写和小写，防止数据丢失
  const payload = {
    UserId: authStore.userID,
    CategoryId: d.id || d.Id || d.CategoryId || 0,
    Name: d.name || d.Name,
    ParentId: d.parentId || d.ParentId || 0
  };

  console.log("🚀 准备发送的分类数据:", payload); // 可以在控制台检查数据是否完整

  try {
    await apiClient.post(url, payload);
    alert(isEditCategoryMode.value ? "分类修改成功！" : "分类创建成功！");
    showCategoryModal.value = false;
    await loadCategoryTree();
  } catch (e) { 
    console.error("分类操作失败:", e);
    // 捕获并显示后端返回的具体错误信息（如“目录名称不能为空”）
    alert("操作失败：" + (e.response?.data || e.message || "未知错误")); 
  }
};

const openCreateCategoryModal = () => { isEditCategoryMode.value = false; currentEditCategoryData.value = {}; showCategoryModal.value = true; }
const openEditCategoryModal = (n) => { isEditCategoryMode.value = true; currentEditCategoryData.value = { id: n.Id, name: n.Title, parentId: n.ParentId || 0 }; showCategoryModal.value = true; }
const handleDeleteCategory = async (id) => { try { await apiClient.post('/Wiki/delete-category', { UserId: authStore.userID, CategoryId: id }); showCategoryModal.value = false; loadCategoryTree(); } catch (e) {} }

const handleScroll = (id) => { 
  activeHeadingId.value = id; 
  const el = document.getElementById(id);
  if (el) el.scrollIntoView({ behavior: 'smooth' });
}

const handleSearch = () => { console.log("搜索中...") }


watch(
  () => route.params.id,
  (newId) => {
    // 只有在当前视图是“查看文章”或“加载中”时，才触发获取文章（防止在编辑或审核时误刷）
    if (newId && (viewStatus.value === 'viewing' || viewStatus.value === 'loading' || viewStatus.value === 'empty')) {
      fetchArticle(newId);
    }
  }
);




</script>

<style scoped>
.pro-wiki-app { height: 100%; display: flex; flex-direction: column; overflow: hidden; background: #fff;}
.wiki-layout { flex: 1; display: flex; overflow: hidden; }
.sidebar-wrapper { width: 260px; flex-shrink: 0; display: flex; flex-direction: column; border-right: 1px solid #f0f0f0; background: #fafafa; }
.admin-entry { padding: 16px; border-bottom: 1px solid #e2e8f0; }
.btn-review-entry { width: 100%; display: flex; justify-content: center; align-items: center; gap: 8px; padding: 10px; background: #1e293b; color: white; border: none; border-radius: 8px; font-weight: 600; cursor: pointer; }
.btn-trash-entry { margin-top: 8px; background: #64748b; color: white; border:none; width:100%; padding:10px; border-radius:8px; font-weight:600; cursor:pointer; }
.category-action-entry { padding: 16px 16px 0 16px; }
.btn-create-category { width: 100%; display: flex; justify-content: center; align-items: center; gap: 8px; padding: 10px; background: #f8fafc; color: #475569; border: 1px dashed #cbd5e1; border-radius: 8px; font-weight: 600; cursor: pointer; }
.main-content-area { flex: 1; display: flex; flex-direction: column; overflow: hidden; background: #ffffff; }
.loading-state { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #64748b; }
.spinner { width: 40px; height: 40px; border: 4px solid #f1f5f9; border-top: 4px solid #2563eb; border-radius: 50%; animation: spin 1s linear infinite; margin-bottom: 16px; }
@keyframes spin { to { transform: rotate(360deg); } }
.badge { background: #ef4444; color: white; padding: 2px 6px; border-radius: 12px; font-size: 12px; margin-left: 4px; }
</style>