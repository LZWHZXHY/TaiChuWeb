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
import { ref, onMounted, computed, nextTick, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import apiClient from '@/utils/api' 
import { useAuthStore } from '@/utils/auth'

// 引入子组件（确保路径正确）
import GlobalHeader from '@/WikiComponents/WikiHeader.vue'
import SidebarLeft from '@/WikiComponents/WikiSidebarLeft.vue'
import ArticleViewer from '@/WikiComponents/WikiViewer.vue'
import SidebarRight from '@/WikiComponents/WikiSidebarRight.vue'
import WikiEditor from '@/WikiComponents/WikiEditor.vue'
import WikiReviewCenter from '@/WikiComponents/WikiReviewCenter.vue'
import CategoryModal from '@/WikiComponents/CategoryModal.vue'
import WikiTrashBin from '@/WikiComponents/WikiTrashBin.vue'

const authStore = useAuthStore()
const route = useRoute()
const router = useRouter()

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

/**
 * 🛡️ 权限逻辑增强：增加对大小写和空值的兼容
 */
const isAdmin = computed(() => {
  // 1. 获取用户对象
  const user = authStore.user?.data || authStore.user || {};
  
  // 2. 检查所有可能的角色字段，且必须【有长度】才是有效的
  // 这样如果 RoleCodes 是 []，它会跳过去看小写的 roleCodes
  const roles = (user.RoleCodes?.length ? user.RoleCodes : null) || 
                (user.roleCodes?.length ? user.roleCodes : null) || 
                (user.roles?.length ? user.roles : null) || 
                [];

  const result = roles.includes('WikiAdmin') || roles.includes('SuperAdmin');

  // 保持调试打印，方便观察
  console.log("🛡️ 权限校验详情:", {
    "原始对象": user,
    "最终采用的角色列表": roles,
    "判定结果": result
  });

  return result;
});

const isSuperAdmin = computed(() => {
  const user = authStore.user?.data || authStore.user || {};
  const roles = (user.RoleCodes?.length ? user.RoleCodes : null) || 
                (user.roleCodes?.length ? user.roleCodes : null) || 
                [];
  return roles.includes('SuperAdmin');
});

// --- 生命周期 ---
onMounted(async () => {
  // 先加载树
  await loadCategoryTree(); 
  
  const targetId = route.params.id || '1'; 
  if (!route.params.id) {
    router.replace(`/wiki/${targetId}`);
  } else {
    await fetchArticle(targetId); 
  }

  // 挂载时如果已经是管理员，主动拉取一次审核数量
  if (isAdmin.value) fetchPendingReviews(true);
})

/**
 * 🚀 极致的数据清洗：既解决重复 Key，又兼容原有字段名
 */
const loadCategoryTree = async () => {
  try {
    const res = await apiClient.get('/Wiki/category-tree');
    const rawData = res.data ?? res;

    const sanitizeNodes = (nodes, depth = 0) => {
      return nodes.map((node, index) => {
        // 生成唯一 Key
        const uniqueKey = `${node.type || 'node'}-${node.id || '0'}-${depth}-${index}`;
        
        const newNode = {
          ...node,
          // 💡 关键：同时保留大小写字段，防止子组件引用失败
          id: node.id ?? node.Id,
          Id: node.id ?? node.Id,
          title: node.title ?? node.Title,
          Title: node.title ?? node.Title,
          type: node.type ?? node.Type,
          Type: node.type ?? node.Type,
          uKey: uniqueKey
        };

        const children = node.children || node.Children;
        if (children && children.length > 0) {
          newNode.children = sanitizeNodes(children, depth + 1);
          newNode.Children = newNode.children;
        } else {
          newNode.children = [];
          newNode.Children = [];
        }
        
        return newNode;
      });
    };

    directoryTree.value = sanitizeNodes(rawData);
  } catch (e) {
    console.error("加载目录树失败:", e);
  }
}

const fetchArticle = async (id) => {
  viewStatus.value = 'loading';
  currentToc.value = [];
  try {
    const res = await apiClient.get(`/Wiki/article/${id}`);
    const data = res.data;
    
    // 标准化 Blocks
    const blocks = (data.blocks || data.Blocks || []).map((b, index) => ({
      id: b.Id || b.id || `b-${index}`,
      content: String(b.Content || b.content || ""), 
      contributors: b.Contributors || b.contributors || []
    }));

    currentArticle.value = { ...data, id: data.id || data.Id, blocks: blocks };
    
    await nextTick();
    viewStatus.value = 'viewing';
    
    await nextTick();
    const fullMarkdown = blocks.map(b => b.content).join('\n\n');
    generateToc(fullMarkdown);
  } catch (error) {
    console.error("加载文章失败", error);
    viewStatus.value = 'empty';
  }
};

const fetchPendingReviews = async (silent = false) => {
  try {
    const uid = authStore.userID || authStore.userId;
    const res = await apiClient.get(`/Wiki/pending-reviews?adminId=${uid}`);
    const rawData = res.data || res;
    pendingReviews.value = Array.isArray(rawData) ? rawData.map(r => ({
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
    })) : [];
  } catch (e) {
    console.error('拉取审核失败', e);
  }
};

// --- 其他事件处理逻辑（保持不变） ---

const handleSelectArticle = (id) => { router.push(`/wiki/${id}`); };

const handleWikiLinkJump = (title) => {
  const findIdByTitle = (nodes, target) => {
    if (!nodes) return null;
    for (const node of nodes) {
      if ((node.Type === 'article' || node.type === 'article') && (node.Title === target || node.title === target)) return node.Id || node.id;
      const children = node.Children || node.children;
      if (children) {
        const found = findIdByTitle(children, target);
        if (found) return found;
      }
    }
    return null;
  };
  const targetId = findIdByTitle(directoryTree.value, title);
  if (targetId) router.push(`/wiki/${targetId}`);
  else alert(`词条《${title}》尚未创建。`);
};

const openReviewCenter = () => { 
  viewStatus.value = 'reviewing'; 
  fetchPendingReviews(); 
}

const exitReviewCenter = () => {
  fetchArticle(currentArticle.value?.id || '1');
}

const startEdit = () => { viewStatus.value = 'editing'; }
const cancelEdit = () => { viewStatus.value = 'viewing'; }

const handleSave = async (editData) => {
  const currentUser = authStore.user?.data || authStore.user || {};
  const actualUserId = currentUser.Id || currentUser.id || authStore.userId || authStore.userID;
  
  if (!actualUserId) {
    alert("无法获取用户信息，请重试");
    return;
  }

  try {
    const payload = {
      ArticleId: Number(currentArticle.value.id) || 0,
      CategoryId: Number(editData.categoryId || currentArticle.value.categoryId || 1),
      UserId: Number(actualUserId),
      Title: editData.title,
      ContentMarkdown: editData.content,
      EditSummary: currentArticle.value.id === 0 ? "创建新词条" : "内容修改"
    };
    await apiClient.post('/Wiki/submit-edit', payload);
    alert("提交成功，请等待审核！");
    if (isAdmin.value) await fetchPendingReviews(true); 
    fetchArticle(currentArticle.value.id || '1');
  } catch (e) { alert("保存失败"); }
}

const handleReviewAction = async (revisionId, isApproved) => {
  try {
    const uid = authStore.userID || authStore.userId;
    await apiClient.post('/Wiki/review-edit', { AdminId: uid, RevisionId: revisionId, IsApproved: isApproved });
    await fetchPendingReviews();
    if (isApproved) await loadCategoryTree();
  } catch (e) { alert("操作失败"); }
}

const handleDeleteArticle = async (id) => {
  if (!confirm("确定要删除此词条吗？")) return;
  try {
    const uid = authStore.userID || authStore.userId;
    await apiClient.post('/Wiki/delete-article', { UserId: uid, ArticleId: id });
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
  const uid = authStore.userID || authStore.userId;
  const payload = {
    UserId: uid,
    CategoryId: d.id || d.Id || d.CategoryId || 0,
    Name: d.name || d.Name,
    ParentId: d.parentId || d.ParentId || 0
  };
  try {
    await apiClient.post(url, payload);
    alert("操作成功！");
    showCategoryModal.value = false;
    await loadCategoryTree();
  } catch (e) { alert("操作失败"); }
};

const openCreateCategoryModal = () => { isEditCategoryMode.value = false; currentEditCategoryData.value = {}; showCategoryModal.value = true; }
const openEditCategoryModal = (n) => { 
  isEditCategoryMode.value = true; 
  currentEditCategoryData.value = { 
    id: n.Id || n.id, 
    name: n.Title || n.title, 
    parentId: n.ParentId || n.parentId || 0 
  }; 
  showCategoryModal.value = true; 
}
const handleDeleteCategory = async (id) => { 
  try { 
    const uid = authStore.userID || authStore.userId;
    await apiClient.post('/Wiki/delete-category', { UserId: uid, CategoryId: id }); 
    showCategoryModal.value = false; 
    loadCategoryTree(); 
  } catch (e) {} 
}

const handleScroll = (id) => { 
  activeHeadingId.value = id; 
  const el = document.getElementById(id);
  if (el) el.scrollIntoView({ behavior: 'smooth' });
}

const handleSearch = () => { console.log("搜索中...") }

watch(() => route.params.id, (newId) => {
  if (newId && (viewStatus.value === 'viewing' || viewStatus.value === 'loading' || viewStatus.value === 'empty')) {
    fetchArticle(newId);
  }
});
</script>

<style scoped>
/* 样式保持不变... */
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