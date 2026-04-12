<template>
  <div class="club-center">
    <aside class="side-nav">
      <div class="sticky-container">
        <div class="nav-brand">社团<span>存档</span></div>
        
        <div class="filter-section">
          <div class="filter-label">档案索引</div>
          <nav class="nav-links">
            <button 
              :class="{ 'active': activeCategory === 0 }"
              @click="activeCategory = 0"
            >
              全部存档
            </button>
            <button 
              v-for="cat in categories" 
              :key="cat.id"
              :class="{ 'active': activeCategory === cat.id }"
              @click="activeCategory = cat.id"
            >
              {{ cat.categoryName }}
            </button>
          </nav>
        </div>

        <div class="nav-footer">
          <div class="stat-box">
            <div class="n">{{ filteredClubs.length }}</div>
            <div class="l">已归档实体</div>
          </div>
          <button class="create-trigger" @click="openApplyDialog">申请创建 +</button>
        </div>
      </div>
    </aside>

    <main class="main-content">
      <header class="content-header">
        <div class="header-left">
          <h1 class="main-title">档案库<span>/</span></h1>
          <p class="main-subtitle">通过分类索引或搜索关键词查阅所有入驻社团</p>
        </div>
        <div class="header-right">
          <div class="search-box">
            <span class="search-icon">🔍</span>
            <input 
              v-model="searchText" 
              placeholder="搜索社团名称或关键字..." 
              class="minimal-search"
            />
          </div>
        </div>
      </header>

      <div class="club-list" v-loading="loading">
        <el-empty v-if="filteredClubs.length === 0" description="未找到匹配项" />

        <section 
          v-for="(club, index) in filteredClubs" 
          :key="club.id" 
          class="archive-row"
        >
          <div class="row-index">#{{ String(index + 1).padStart(2, '0') }}</div>
          
          <div class="row-info">
            <div class="info-top">
              <span class="info-cat">{{ club.category?.categoryName || '未分类' }}</span>
              <h2 class="info-name">{{ club.name }}</h2>
              <span class="info-leader">负责人：{{ club.leaderName }}</span>
            </div>
            <p class="info-desc">{{ club.description }}</p>
            <div class="info-tags">
              <span v-for="tag in club.tagNames" :key="tag">#{{ tag }}</span>
            </div>
          </div>

          <div class="row-data">
            <div class="data-group">
              <span class="v">{{ club.size }}</span>
              <span class="k">规模</span>
            </div>
            <button class="action-arrow" @click="goToDetail(club.id)">查看详情</button>
          </div>
        </section>
      </div>
    </main>

    <el-dialog 
      v-model="showApplyDialog" 
      width="480px" 
      class="custom-dialog" 
      align-center 
      :show-close="false"
      destroy-on-close
    >
      <template #header><div class="d-head">提交新社团提案</div></template>
      <el-form :model="applyForm" :rules="rules" ref="applyFormRef" label-position="top">
        <el-form-item label="社团名称" prop="name">
          <el-input v-model="applyForm.name" placeholder="输入社团的全称" />
        </el-form-item>
        
        <div class="form-row">
          <el-form-item label="所属大类" prop="categoryId" style="flex:1">
            <el-select v-model="applyForm.categoryId" placeholder="请选择分类">
              <el-option 
                v-for="c in categories" 
                :key="c.id" 
                :label="c.categoryName" 
                :value="c.id" 
              />
            </el-select>
          </el-form-item>
          <el-form-item label="负责人姓名" prop="leaderName" style="flex:1">
            <el-input v-model="applyForm.leaderName" placeholder="怎么称呼您？" />
          </el-form-item>
        </div>

        <el-form-item label="社团简介" prop="description">
          <el-input 
            v-model="applyForm.description" 
            type="textarea" 
            :rows="3" 
            placeholder="简要描述社团的宗旨和日常活动..."
          />
        </el-form-item>

        <el-form-item label="个性标签 (按回车添加)">
          <el-select
            v-model="applyForm.tagNames"
            multiple
            filterable
            allow-create
            default-first-option
            placeholder="输入标签后按回车"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="d-btns">
          <button class="b-discard" @click="showApplyDialog = false">放弃</button>
          <button class="b-submit" @click="submitApply" :disabled="submitting">
            {{ submitting ? '提交中...' : '提交申请' }}
          </button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router' 
import { ElMessage } from 'element-plus'
import apiClient from '@/utils/api'

// --- 路由与状态 ---
const router = useRouter()
const clubs = ref([])
const categories = ref([])
const loading = ref(false)
const showApplyDialog = ref(false)
const submitting = ref(false)
const applyFormRef = ref(null)
const searchText = ref('')
const activeCategory = ref(0)

// --- 表单初始数据 ---
const applyForm = ref({
  categoryId: null,
  name: '',
  leaderName: '',
  description: '',
  size: 1,
  tagNames: []
})

// --- 校验规则 ---
const rules = {
  name: [{ required: true, message: '必须填写社团名称', trigger: 'blur' }],
  categoryId: [{ required: true, message: '请选择所属分类', trigger: 'change' }],
  leaderName: [{ required: true, message: '请填写负责人', trigger: 'blur' }],
  description: [{ required: true, message: '请提供简短的社团介绍', trigger: 'blur' }]
}

// --- 搜索与过滤逻辑 ---
const filteredClubs = computed(() => {
  return clubs.value.filter(club => {
    const name = club.name || ''
    const desc = club.description || ''
    const matchSearch = name.toLowerCase().includes(searchText.value.toLowerCase()) ||
                        desc.toLowerCase().includes(searchText.value.toLowerCase())
    const matchCat = activeCategory.value === 0 || club.categoryId === activeCategory.value
    return matchSearch && matchCat
  })
})

// --- 数据交互 ---
// --- 数据交互 ---
const fetchCategories = async () => {
  try {
    // 修正点：路径从 '/Category' 改为 '/Club/Categories'
    const res = await apiClient.get('/Club/Categories')
    categories.value = res.data || res
  } catch (e) {
    console.error('分类获取失败:', e)
  }
}

const fetchClubs = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('/Club')
    clubs.value = res.data || res
  } catch (e) {
    console.error('列表获取失败:', e)
  } finally {
    loading.value = false
  }
}

// ✨ 跳转详情
const goToDetail = (id) => {
  if (!id) return
  router.push({ name: 'ClubDetail', params: { id: id } }) // 或者 router.push(`/club/${id}`)
}

const openApplyDialog = () => {
  // 重置表单数据
  applyForm.value = {
    categoryId: null,
    name: '',
    leaderName: '',
    description: '',
    size: 1,
    tagNames: []
  }
  showApplyDialog.value = true
}

const submitApply = async () => {
  if (!applyFormRef.value) return
  
  await applyFormRef.value.validate(async (valid) => {
    if (valid) {
      submitting.value = true
      try {
        // 后端接口对应 [HttpPost("Apply")]
        await apiClient.post('/Club/Apply', applyForm.value)
        ElMessage.success('提议已提交，请等待管理员审核')
        showApplyDialog.value = false
        fetchClubs() // 刷新列表
      } catch (e) {
        console.error('提交失败:', e)
        ElMessage.error('提交失败，请检查网络')
      } finally {
        submitting.value = false
      }
    }
  })
}

onMounted(() => {
  fetchCategories()
  fetchClubs()
})
</script>

<style scoped>
/* 保持 MD 档案排版风格，确保只有一个滚动条 */
.club-center {
  display: flex;
  background: #fdfdfd;
  min-height: 100vh;
  padding: 0 40px;
  font-family: 'PingFang SC', sans-serif;
  color: #1a1a1a;
}

.side-nav {
  width: 240px;
  padding-top: 60px;
  position: relative;
}

.sticky-container {
  position: sticky;
  top: 60px;
  max-height: calc(100vh - 100px);
  display: flex;
  flex-direction: column;
}

.nav-brand { font-size: 26px; font-weight: 900; margin-bottom: 40px; border-bottom: 4px solid #1a1a1a; padding-bottom: 8px; }
.nav-brand span { color: #3b82f6; }

.filter-label { font-size: 11px; font-weight: 900; color: #ccc; margin-bottom: 12px; letter-spacing: 2px; }

.nav-links { display: flex; flex-direction: column; gap: 4px; }
.nav-links button {
  background: none; border: none; text-align: left; padding: 10px 0;
  font-size: 14px; font-weight: 700; color: #888; cursor: pointer;
  transition: all 0.2s;
}
.nav-links button.active { color: #1a1a1a; text-decoration: underline; text-underline-offset: 8px; }

.nav-footer { margin-top: 40px; border-top: 1px solid #eee; padding-top: 20px; }
.stat-box { margin-bottom: 20px; }
.stat-box .n { font-size: 32px; font-weight: 900; }
.stat-box .l { font-size: 11px; color: #aaa; }

.create-trigger {
  width: 100%; padding: 14px; background: #1a1a1a; color: #fff;
  border: none; font-weight: 900; cursor: pointer;
}

.main-content {
  flex: 1;
  padding: 60px 0 100px 80px;
}

.content-header { 
  display: flex; justify-content: space-between; align-items: flex-end;
  margin-bottom: 80px; padding-bottom: 30px; border-bottom: 2px solid #1a1a1a;
}
.main-title { font-size: 48px; font-weight: 900; margin: 0; letter-spacing: -2px; }
.main-subtitle { font-size: 14px; color: #888; margin-top: 12px; }

.minimal-search {
  border: none; border-bottom: 1px solid #eee; padding: 8px 0;
  width: 240px; font-family: inherit; outline: none;
}
.minimal-search:focus { border-bottom-color: #1a1a1a; }

.club-list { display: flex; flex-direction: column; }

.archive-row {
  display: flex;
  align-items: flex-start;
  padding: 30px 0;
  border-bottom: 1px solid #1a1a1a;
  transition: all 0.2s;
}
.archive-row:hover { background: #f9f9f9; padding-left: 20px; }

.row-index { width: 60px; font-size: 18px; font-weight: 900; color: #eee; }

.row-info { flex: 1; padding-right: 40px; }
.info-top { display: flex; align-items: center; gap: 15px; margin-bottom: 10px; }
.info-cat { font-size: 10px; font-weight: 900; background: #1a1a1a; color: #fff; padding: 2px 6px; }
.info-name { font-size: 22px; font-weight: 900; margin: 0; }
.info-leader { font-size: 12px; color: #3b82f6; font-weight: 700; }

.info-desc { font-size: 15px; color: #555; line-height: 1.6; margin-bottom: 15px; max-width: 800px; }

.info-tags { display: flex; gap: 12px; }
.info-tags span { font-size: 11px; color: #aaa; }

.row-data { display: flex; align-items: center; gap: 40px; }
.data-group { text-align: right; }
.data-group .v { font-size: 24px; font-weight: 900; display: block; line-height: 1; }
.data-group .k { font-size: 9px; color: #bbb; font-weight: 900; }

.action-arrow {
  background: none; border: 1px solid #1a1a1a; padding: 8px 16px;
  font-family: inherit; font-weight: 900; font-size: 12px; cursor: pointer;
}
.action-arrow:hover { background: #1a1a1a; color: #fff; }

:deep(.custom-dialog) { border-radius: 0; border: 4px solid #1a1a1a; box-shadow: 20px 20px 0 rgba(0,0,0,0.1); }
.d-head { padding: 25px; font-weight: 900; font-size: 20px; border-bottom: 1px solid #eee; }
.form-row { display: flex; gap: 20px; }
.d-btns { display: flex; border-top: 2px solid #1a1a1a; }
.d-btns button { flex: 1; padding: 18px; font-weight: 900; border: none; cursor: pointer; font-family: inherit; }
.b-discard { background: #fff; color: #999; border-right: 1px solid #eee !important; }
.b-submit { background: #1a1a1a; color: #fff; }
</style>