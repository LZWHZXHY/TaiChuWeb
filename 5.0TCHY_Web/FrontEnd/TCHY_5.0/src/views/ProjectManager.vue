<template>
  <div class="workspace-container">
    <header class="workspace-header">
      <div class="org-breadcrumb">
        <span class="root-label">工作区 // WORKSPACE</span>
        <span class="separator">/</span>
        <div class="org-selector-wrapper">
          <h2 class="org-name">{{ currentOrg?.name || '初始化终端...' }}</h2>
          <span class="org-tag" v-if="currentOrg">URL: /{{ currentOrg.slug }}</span>
        </div>
      </div>
      <div class="header-actions">
        <button class="action-btn secondary" @click="showMemberManage = true" v-if="currentOrg">
          <span class="icon">👥</span> 人员与权限配置
        </button>
        <button class="action-btn secondary" @click="fetchOrgs" :disabled="loading.orgs">
          <span class="icon">⟳</span> 刷新
        </button>
        <button 
          class="action-btn primary" 
          @click="showCreateProject = true"
          v-if="myRole === 'Owner' || myRole === 'Admin'"
        >
          <span class="icon">+</span> 新建项目
        </button>
        <button class="action-btn primary" @click="showCreateOrg = true">
          <span class="icon">#</span> 新建组织
        </button>
      </div>
    </header>

    <main class="workspace-content">
      <aside class="org-sidebar">
        <div class="sidebar-label">组织列表 // ORG_LIST</div>
        <div class="org-list" v-if="!loading.orgs">
          <div
            v-for="org in orgList"
            :key="org.id"
            class="org-item"
            :class="{ active: currentOrg?.id === org.id }"
            @click="selectOrg(org)"
            :title="`Slug: ${org.slug}`"
          >
            <div class="org-indicator"></div>
            <div class="org-icon">{{ org.name.charAt(0).toUpperCase() }}</div>
            <div class="org-details">
              <span class="org-title">{{ org.name }}</span>
              <span class="org-role" :class="org.role">{{ org.role }}</span>
            </div>
            <button 
              v-if="currentOrg?.id === org.id && (org.role === 'Owner' || org.role === 'Admin')" 
              class="sidebar-invite-btn" 
              @click.stop="openInviteModal"
              title="招募新干员进入组织"
            >
              +
            </button>
          </div>
        </div>
        <div v-else class="sidebar-loading">正在建立链路...</div>
      </aside>

      <section class="project-view">
        <div class="view-header">
          <div class="title-group">
            <span class="section-title">项目终端 // PROJECT_TERMINAL</span>
            <span class="project-count">{{ projectList.length }} 个活跃扇区</span>
          </div>
          <div class="search-box">
            <span class="search-icon">🔍</span>
            <input type="text" placeholder="FILTER_PROJECTS..." v-model="searchQuery" />
          </div>
        </div>

        <div class="project-grid" v-if="!loading.projects">
          <div
            v-for="proj in filteredProjects"
            :key="proj.id"
            class="project-card"
            @click="enterProject(proj.id)"
          >
            <div class="card-edge"></div>
            <div class="card-content">
              <div class="card-top">
                <span class="proj-type" :class="proj.type">{{ proj.type }}</span>
                <div class="status-chip">
                  <span class="dot">●</span> {{ proj.status }}
                </div>
              </div>
              <h3 class="proj-title">{{ proj.name }}</h3>
              <p class="proj-desc">{{ proj.description || '该节点暂无描述数据。' }}</p>

              <div class="proj-footer">
                <div class="progress-info">
                  <span class="progress-label">COMPLETION</span>
                  <span class="progress-value">{{ proj.progress }}%</span>
                </div>
                <div class="progress-bar-container">
                  <div class="progress-bar-fill" :style="{ width: proj.progress + '%' }"></div>
                </div>
              </div>
            </div>
          </div>

          <button 
            v-if="myRole === 'Owner' || myRole === 'Admin'"
            class="project-card add-card" 
            @click="showCreateProject = true"
          >
            <div class="add-inner">
              <span class="plus-icon">+</span>
              <span class="add-text">INITIALIZE_PROJECT</span>
              <span class="add-sub">部署新的项目节点</span>
            </div>
          </button>
          <div v-else class="permission-denied-card">
            <span>权限不足：需 Admin 权限创建项目</span>
          </div>
        </div>
        <div v-else class="loading">读取扇区数据中...</div>
      </section>
    </main>

    <OrgMemberModal 
      v-if="showMemberManage && currentOrg"
      :org-id="currentOrg.id"
      :my-user-id="myUserId"
      :my-role="myRole"
      @close="showMemberManage = false"
      @role-changed="handleMyRoleChanged"
    />

    <div v-if="showInviteMember" class="modal-overlay" @click.self="showInviteMember = false">
      <div class="modal-content cyber-modal" style="overflow: visible;">
        <h3>// 招募社区干员</h3>
        <form @submit.prevent="submitInvite">
          <div class="form-group" style="position: relative;">
            <label>搜索用户名 / ID</label>
            <input 
              type="text" 
              v-model="inviteSearchQuery" 
              @input="handleSearchInput"
              placeholder="输入用户名搜索..." 
              required 
              autocomplete="off"
            />
            <div v-if="userSearchResults.length > 0" class="search-dropdown">
              <div 
                v-for="user in userSearchResults" 
                :key="user.id" 
                class="search-item"
                @click="selectInviteUser(user)"
              >
                <div class="search-avatar">
                  <img v-if="user.avatar" :src="user.avatar" />
                  <span v-else>{{ user.username.charAt(0).toUpperCase() }}</span>
                </div>
                <div class="search-info">
                  <span class="s-name">{{ user.username }}</span>
                  <span class="s-level">Lv.{{ user.level || 0 }}</span>
                </div>
                <span class="plus-tag">+</span>
              </div>
            </div>
          </div>
          <div class="form-actions">
            <button type="button" class="modal-btn cancel" @click="showInviteMember = false">取消</button>
            <button type="submit" class="modal-btn submit" :disabled="inviteLoading || !selectedInviteUser">
              {{ inviteLoading ? '传输中...' : '发送邀请' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="showCreateOrg" class="modal-overlay" @click.self="showCreateOrg = false">
      <div class="modal-content cyber-modal">
        <h3>// 创建新组织</h3>
        <form @submit.prevent="createOrg">
          <div class="form-group">
            <label>组织名称</label>
            <input type="text" v-model="newOrg.name" required />
          </div>
          <div class="form-group">
            <label>标识符 (Slug)</label>
            <input type="text" v-model="newOrg.slug" required />
          </div>
          <div class="form-actions">
            <button type="button" class="modal-btn cancel" @click="showCreateOrg = false">取消</button>
            <button type="submit" class="modal-btn submit" :disabled="creating">确认部署</button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="showCreateProject" class="modal-overlay" @click.self="showCreateProject = false">
      <div class="modal-content cyber-modal">
        <h3>// 初始化新项目</h3>
        <form @submit.prevent="createProject">
          <div class="form-group">
            <label>项目名称</label>
            <input type="text" v-model="newProject.name" required />
          </div>
          <div class="form-group">
            <label>描述</label>
            <textarea v-model="newProject.description" rows="3"></textarea>
          </div>
          <div class="form-actions">
            <button type="button" class="modal-btn cancel" @click="showCreateProject = false">取消</button>
            <button type="submit" class="modal-btn submit" :disabled="creating">启动初始化</button>
          </div>
        </form>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'

// 🔥 引入成员管理组件
import OrgMemberModal from '@/ProjectComponents/OrgMemberModal.vue'

// ================= 类型定义 (统一为小写) =================
interface Organization {
  id: number; 
  name: string; 
  slug: string; 
  role?: string; 
}

interface Project {
  id: number; 
  name: string; 
  description?: string; 
  status: string; 
  organizationId: number; 
  type: string; 
  progress: number; 
}

// ================= 状态管理 =================
const router = useRouter()
const authStore = useAuthStore()
const myUserId = computed(() => authStore.user?.id || 0)

const orgList = ref<Organization[]>([])
const currentOrg = ref<Organization | null>(null)
const projectList = ref<Project[]>([])

const loading = reactive({ orgs: false, projects: false })
const showCreateOrg = ref(false)
const showCreateProject = ref(false)
const showInviteMember = ref(false)
const showMemberManage = ref(false) 
const creating = ref(false)
const searchQuery = ref('')

const newOrg = reactive({ name: '', slug: '' })
const newProject = reactive({ name: '', description: '' })

const inviteSearchQuery = ref('')
const userSearchResults = ref<any[]>([])
const inviteLoading = ref(false)
const selectedInviteUser = ref<any>(null)
let searchTimer: any = null

// ================= 计算属性 =================
const filteredProjects = computed(() => {
  if (!searchQuery.value) return projectList.value
  const q = searchQuery.value.toLowerCase()
  return projectList.value.filter(p => p.name.toLowerCase().includes(q))
})

const myRole = computed(() => {
  if (!currentOrg.value) return 'Guest'
  return currentOrg.value.role || 'Guest'
})

// ================= 核心方法 =================

/**
 * 获取组织列表
 */
const fetchOrgs = async () => {
  loading.orgs = true
  try {
    const response = await apiClient.get<any[]>('/organizations') 
    // 🔥 修复：完全使用小写访问后端属性
    orgList.value = response.data.map(item => ({
      id: item.id, 
      name: item.name, 
      slug: item.slug,
      role: item.role || 'Guest' 
    }))

    if (orgList.value.length > 0) {
      const target = currentOrg.value 
        ? orgList.value.find(o => o.id === currentOrg.value!.id) 
        : orgList.value[0]
      if (target) selectOrg(target)
    }
  } catch (error) { 
    console.error('获取组织失败:', error) 
  } finally { 
    loading.orgs = false 
  }
}

/**
 * 切换选中的组织
 */
const selectOrg = async (org: Organization) => {
  currentOrg.value = org
  await fetchProjects()
}

/**
 * 获取当前组织下的项目列表
 */
const fetchProjects = async () => {
  if (!currentOrg.value || !currentOrg.value.id) return
  loading.projects = true
  try {
    // 这里的 ID 现在一定是有效的数字，不再是 undefined
    const response = await apiClient.get<any[]>(`/organizations/${currentOrg.value.id}/projects`)
    projectList.value = response.data.map(p => ({
      id: p.id, 
      name: p.name, 
      description: p.description, 
      status: p.status || 'Active',
      organizationId: p.organizationId, 
      type: inferProjectType(p.name),
      progress: Math.floor(Math.random() * 80) + 10 
    }))
  } catch (error) { 
    console.error('获取项目失败:', error)
    projectList.value = [] 
  } finally { 
    loading.projects = false 
  }
}

/**
 * 创建新组织
 */
const createOrg = async () => {
  if (!newOrg.name || !newOrg.slug) return;
  creating.value = true;
  try {
    const response = await apiClient.post('/organizations', {
      name: newOrg.name, 
      slug: newOrg.slug
    });
    // 🔥 修复：读取响应时也使用小写
    const newOrgData = {
      id: response.data.id, 
      name: response.data.name, 
      slug: response.data.slug, 
      role: 'Owner'
    };
    orgList.value.push(newOrgData);
    selectOrg(newOrgData);
    showCreateOrg.value = false;
    newOrg.name = ''; newOrg.slug = '';
  } catch (error) {
    console.error('创建组织失败', error)
  } finally { 
    creating.value = false; 
  }
};

/**
 * 初始化新项目
 */
const createProject = async () => {
  if (!currentOrg.value || !newProject.name) return;
  creating.value = true;
  try {
    await apiClient.post('/projects', {
      name: newProject.name, 
      description: newProject.description, 
      organizationId: currentOrg.value.id 
    });
    await fetchProjects();
    showCreateProject.value = false;
    newProject.name = ''; newProject.description = '';
  } catch (error) {
    console.error('项目初始化失败', error)
  } finally { 
    creating.value = false; 
  }
};

/**
 * 成员搜索逻辑
 */
const handleSearchInput = () => {
  if (inviteSearchQuery.value.length < 2) { userSearchResults.value = []; return }
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(async () => {
    try {
      const res = await apiClient.get<any[]>(`/organizations/${currentOrg.value?.id}/search-users`, {
        params: { query: inviteSearchQuery.value }
      })
      // 🔥 修复：用户数据同样映射为小写
      userSearchResults.value = res.data.map(u => ({
        id: u.id, 
        username: u.username, 
        avatar: u.avatar, 
        level: u.level
      }))
    } catch (e) {
      console.error('用户搜索失败', e)
    }
  }, 300)
}

/**
 * 发送组织邀请
 */
const submitInvite = async () => {
  if (!currentOrg.value || !selectedInviteUser.value) return;
  inviteLoading.value = true;
  try {
    await apiClient.post(`/organizations/${currentOrg.value.id}/members`, {
      targetUsername: selectedInviteUser.value.username 
    });
    showInviteMember.value = false; 
    inviteSearchQuery.value = ''; 
    selectedInviteUser.value = null;
    alert('邀请发送成功');
  } catch (error: any) {
    alert(error.response?.data?.message || '邀请失败');
  } finally { 
    inviteLoading.value = false; 
  }
}

// ================= 辅助函数 =================
const handleMyRoleChanged = (newRole: string) => {
  if (currentOrg.value) currentOrg.value.role = newRole
}

const inferProjectType = (name: string) => 'general'

const selectInviteUser = (user: any) => {
  inviteSearchQuery.value = user.username
  selectedInviteUser.value = user
  userSearchResults.value = []
}

const openInviteModal = () => {
  if (!currentOrg.value) return
  inviteSearchQuery.value = ''
  userSearchResults.value = []
  selectedInviteUser.value = null
  showInviteMember.value = true
}

const enterProject = (id: number) => router.push(`/projects/${id}`)

onMounted(fetchOrgs)
</script>

<style scoped>
/* ================= 1. 全局核心布局 ================= */
.workspace-container {
  --primary-blue: #00A3FF;
  --bg-sand: #F4F1EA;
  --ink-black: #111111;
  --border-color: rgba(17, 17, 17, 0.1);
  --md-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  --md-shadow-hover: 0 8px 24px rgba(0, 0, 0, 0.12);

  
  min-height: 100vh;
  background-color: var(--bg-sand);
  font-family: 'JetBrains Mono', monospace;
  display: flex;
  flex-direction: column;
}

/* ================= 2. Header 样式 ================= */
.workspace-header {
  display: flex; justify-content: space-between; align-items: center;
  padding: 16px 40px; background: #fff; border-bottom: 2px solid var(--ink-black);
  box-shadow: var(--md-shadow);
}
.org-breadcrumb { display: flex; align-items: center; gap: 12px; }
.root-label { font-size: 12px; font-weight: 800; color: #888; }
.org-name { font-size: 24px; font-weight: 800; margin: 0; letter-spacing: -1px; }
.org-tag { font-size: 10px; background: var(--ink-black); color: #fff; padding: 2px 6px; }

.action-btn {
  border: 2px solid var(--ink-black); padding: 8px 16px; font-weight: 700; font-size: 12px;
  cursor: pointer; margin-left: 10px; transition: 0.2s; background: transparent;
  display: inline-flex; align-items: center; gap: 6px;
  clip-path: polygon(0 0, 100% 0, 100% 80%, 92% 100%, 0 100%);
}
.action-btn:hover { background: #eee; transform: translateY(-2px); }
.action-btn.primary { background: var(--ink-black); color: #fff; }
.action-btn.primary:hover { background: var(--primary-blue); border-color: var(--primary-blue); }
.action-btn:disabled { opacity: 0.5; cursor: not-allowed; }

/* ================= 3. 侧边栏样式 ================= */
.workspace-content { display: flex; flex: 1; overflow: hidden; }
.org-sidebar {
  width: 280px; background: rgba(255, 255, 255, 0.5);
  border-right: 1px solid var(--border-color); padding: 24px 0;
  display: flex; flex-direction: column;
}
.sidebar-label { font-size: 10px; font-weight: 800; padding: 0 24px 16px; color: #999; }
.org-list { flex: 1; overflow-y: auto; }

.org-item {
  display: flex; align-items: center; padding: 12px 24px; cursor: pointer;
  position: relative; transition: all 0.2s;
}
.org-item:hover { background: rgba(255, 255, 255, 0.8); }
.org-item.active { background: #fff; box-shadow: 0 2px 8px rgba(0,0,0,0.05); }
.org-indicator { position: absolute; left: 0; width: 4px; height: 0; background: var(--primary-blue); transition: 0.3s; }
.org-item.active .org-indicator { height: 100%; }

.org-icon {
  width: 40px; height: 40px; background: var(--ink-black); color: #fff;
  display: flex; align-items: center; justify-content: center;
  font-weight: 800; font-size: 18px; margin-right: 12px; flex-shrink: 0;
}
.org-details { display: flex; flex-direction: column; flex: 1; overflow: hidden; }
.org-title { font-weight: 700; font-size: 14px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.org-role { font-size: 10px; color: #777; }

.sidebar-invite-btn {
  width: 24px; height: 24px; border: 1px solid var(--ink-black); background: #fff;
  display: flex; align-items: center; justify-content: center; cursor: pointer;
  font-weight: 800; transition: 0.2s; margin-left: 8px; flex-shrink: 0;
}
.sidebar-invite-btn:hover { background: var(--primary-blue); color: #fff; border-color: var(--primary-blue); }

/* ================= 4. 项目看板样式 ================= */
.project-view { flex: 1; padding: 40px; overflow-y: auto; }
.view-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 32px; }
.section-title { font-size: 12px; font-weight: 800; display: block; margin-bottom: 4px; }
.project-count { font-size: 11px; color: var(--primary-blue); font-weight: 700; }

.search-box { border-bottom: 1px solid var(--ink-black); display: flex; align-items: center; padding: 4px 0; }
.search-box input { border: none; background: transparent; padding: 4px 8px; font-family: inherit; font-size: 14px; outline: none; min-width: 200px; }
.search-icon { opacity: 0.5; }

.project-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 24px; }
.project-card {
  background: #fff; border: 1px solid var(--border-color); position: relative;
  cursor: pointer; transition: 0.3s cubic-bezier(0.4, 0, 0.2, 1); display: flex; text-align: left;
}
.project-card:hover { transform: translateY(-4px); box-shadow: var(--md-shadow-hover); border-color: var(--ink-black); }
.card-edge { width: 6px; background: var(--ink-black); transition: 0.3s; }
.card-content { padding: 24px; flex: 1; }
.card-top { display: flex; justify-content: space-between; margin-bottom: 16px; }
.proj-type { font-size: 10px; font-weight: 800; padding: 2px 8px; border: 1px solid var(--ink-black); text-transform: uppercase; }
.status-chip { font-size: 10px; font-weight: 800; color: #4CAF50; }
.proj-title { margin: 0 0 8px 0; font-size: 18px; font-weight: 800; }
.proj-desc { font-size: 12px; color: #666; line-height: 1.5; margin-bottom: 20px; height: 36px; overflow: hidden; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; }

.progress-info { display: flex; justify-content: space-between; margin-bottom: 4px; font-size: 10px; font-weight: 700; color: #888; }
.progress-bar-container { height: 4px; background: #eee; width: 100%; position: relative; }
.progress-bar-fill { height: 100%; background: var(--primary-blue); transition: width 1s ease-out; }

.add-card { border: 2px dashed #ccc; background: transparent; justify-content: center; align-items: center; min-height: 180px; }
.add-inner { text-align: center; color: #888; }
.plus-icon { font-size: 32px; display: block; margin-bottom: 8px; }
.add-text { font-size: 14px; font-weight: 800; color: var(--ink-black); }
.add-sub { font-size: 11px; }
.add-card:hover { border-style: solid; border-color: var(--primary-blue); }
.add-card:hover .add-text { color: var(--primary-blue); }

/* ================= 5. 通用模态框样式 ================= */
.modal-overlay {
  position: fixed; top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(0, 0, 0, 0.6); backdrop-filter: blur(2px);
  display: flex; justify-content: center; align-items: center; z-index: 2000;
}
.modal-content.cyber-modal {
  background: #fff; padding: 32px; border: 2px solid var(--ink-black);
  width: 100%; max-width: 420px;
  box-shadow: 12px 12px 0 var(--primary-blue);
  position: relative;
}
.modal-content h3 { margin-top: 0; font-size: 18px; font-weight: 800; margin-bottom: 24px; border-left: 4px solid var(--ink-black); padding-left: 12px; }

.form-group { margin-bottom: 20px; }
.form-group label { display: block; font-size: 12px; font-weight: 700; margin-bottom: 6px; }
.form-group input, .form-group textarea {
  width: 100%; padding: 10px; border: 2px solid var(--ink-black);
  font-family: inherit; font-size: 14px; outline: none; background: #fff;
}
.form-group input:focus, .form-group textarea:focus { border-color: var(--primary-blue); }

.form-actions { display: flex; justify-content: flex-end; gap: 12px; margin-top: 32px; padding-top: 16px; border-top: 1px solid #eee; }
.modal-btn {
  padding: 10px 20px; font-weight: 700; cursor: pointer;
  border: 2px solid var(--ink-black); background: transparent; font-size: 12px;
}
.modal-btn.submit { background: var(--ink-black); color: #fff; }
.modal-btn.submit:hover:not(:disabled) { background: var(--primary-blue); border-color: var(--primary-blue); }
.modal-btn:disabled { opacity: 0.5; cursor: not-allowed; }

/* ================= 6. 搜索下拉列表 ================= */
.search-dropdown {
  position: absolute; top: 100%; left: 0; width: 100%;
  background: #fff; border: 2px solid var(--ink-black); border-top: none;
  max-height: 200px; overflow-y: auto; z-index: 10;
}
.search-item {
  display: flex; align-items: center; padding: 10px 12px; cursor: pointer;
  border-bottom: 1px solid #eee;
}
.search-item:hover { background: var(--primary-blue); color: #fff; }
.search-avatar {
  width: 32px; height: 32px; background: #111; color: #fff;
  display: flex; align-items: center; justify-content: center;
  font-weight: 800; margin-right: 12px; border: 1px solid #ccc; flex-shrink: 0;
}
.search-avatar img { width: 100%; height: 100%; object-fit: cover; }
.search-info { flex: 1; display: flex; flex-direction: column; }
.s-name { font-weight: 700; font-size: 13px; }
.s-level { font-size: 10px; opacity: 0.8; }
</style>