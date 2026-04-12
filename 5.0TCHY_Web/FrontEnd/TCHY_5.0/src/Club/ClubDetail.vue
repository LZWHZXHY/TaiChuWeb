<template>
  <div class="club-detail-container" v-loading="loading">
    <header class="detail-header">
      <div class="header-left">
        <button class="back-btn" @click="router.back()">← 返回档案库</button>
      </div>
      <div class="header-right">
        <button class="edit-apply-btn" @click="openEditDialog">提交变更申请</button>
        <div class="status-tag">STATUS: {{ club?.verifyStatus === 1 ? '已过审' : '审核中' }}</div>
      </div>
    </header>

    <main v-if="club" class="detail-content" :class="{ 'is-disbanded': club.status === 2 }">
      <aside class="info-aside">
        <div class="visual-box">
          <img :src="club.logoUrl || 'https://via.placeholder.com/400'" class="club-logo" />
        </div>
        
        <div class="spec-list">
          <div class="spec-item">
            <span class="label">实体名称 / NAME</span>
            <span class="value">{{ club.name }}</span>
          </div>
          <div class="spec-item">
            <span class="label">运营状态 / STATUS</span>
            <span class="value">
              <el-tag :type="getStatusType(club.status)" effect="dark" class="status-badge">
                {{ getStatusText(club.status) }}
              </el-tag>
            </span>
          </div>
          <div class="spec-item">
            <span class="label">所属分类 / CATEGORY</span>
            <span class="value">{{ club.category?.categoryName || '未分类' }}</span>
          </div>
          <div class="spec-item">
            <span class="label">负责人 / LEADER</span>
            <span class="value">{{ club.leaderName }}</span>
          </div>
          <div class="spec-item">
            <span class="label">成员规模 / SIZE</span>
            <span class="value">{{ club.size }} 人</span>
          </div>
          <div class="spec-item">
            <span class="label">入社考核 / TEST</span>
            <span class="value">{{ club.isTestRequired ? `需考核 (难度:${club.testLevel})` : '免考核' }}</span>
          </div>
        </div>

        <button 
          class="join-btn" 
          @click="handleJoin" 
          :disabled="club.status === 2"
        >
          {{ club.status === 2 ? '该社团已解散' : '获取加入方式' }}
        </button>
      </aside>

      <section class="main-article">
        <div class="article-section">
          <h3 class="section-title">社团简介 / DESCRIPTION</h3>
          <p class="description-text">{{ club.description }}</p>
        </div>

        <div class="article-section">
          <h3 class="section-title">个性标签 / TAGS</h3>
          <div class="tag-cloud">
            <span v-for="tag in club.tagNames" :key="tag" class="detail-tag"># {{ tag }}</span>
            <span v-if="!club.tagNames?.length" class="no-data">暂无标签数据</span>
          </div>
        </div>

        <div class="article-section">
          <h3 class="section-title">系统日志 / SYSTEM_LOGS</h3>
          <div class="placeholder-activity">
            <div class="act-item">
              <span class="act-date">{{ formatDate(club.createdAt) }}</span>
              <span class="act-content">实体档案创建完成，初始状态设定为：{{ getStatusText(club.status) }}。</span>
            </div>
          </div>
        </div>
      </section>
    </main>

    <el-dialog 
      v-model="showEditDialog" 
      width="640px" 
      class="custom-dialog" 
      align-center 
      :show-close="false"
      destroy-on-close
    >
      <template #header><div class="d-head">档案变更提案 / UPDATE_PROPOSAL</div></template>
      
      <el-form :model="editForm" :rules="rules" ref="editFormRef" label-position="top">
        <el-row :gutter="20">
          <el-col :span="14">
            <el-form-item label="社团名称" prop="name">
              <el-input v-model="editForm.name" placeholder="名称..." />
            </el-form-item>
          </el-col>
          <el-col :span="10">
            <el-form-item label="所属大类" prop="categoryId">
              <el-select v-model="editForm.categoryId" placeholder="分类" style="width: 100%">
                <el-option v-for="c in categories" :key="c.id" :label="c.categoryName" :value="c.id" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="负责人姓名" prop="leaderName">
              <el-input v-model="editForm.leaderName" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="当前运营状态" prop="status">
              <el-select v-model="editForm.status" style="width: 100%">
                <el-option label="运营中" :value="0" />
                <el-option label="休整中" :value="1" />
                <el-option label="已解散" :value="2" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="外部加入方式/链接" prop="joinUrl">
              <el-input v-model="editForm.joinUrl" placeholder="QQ群、URL等" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="当前规模" prop="size">
              <el-input-number v-model="editForm.size" :min="1" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="Logo/头像 URL" prop="logoUrl">
          <el-input v-model="editForm.logoUrl" placeholder="https://..." />
        </el-form-item>

        <el-form-item label="详情描述" prop="description">
          <el-input v-model="editForm.description" type="textarea" :rows="4" />
        </el-form-item>

        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="是否需要入社考核">
              <el-switch v-model="editForm.isTestRequired" />
            </el-form-item>
          </el-col>
          <el-col :span="12" v-if="editForm.isTestRequired">
            <el-form-item label="考核难度等级">
              <el-rate v-model="editForm.testLevel" :max="5" style="margin-top: 10px;" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="个性标签 (回车添加)">
          <el-select v-model="editForm.tagNames" multiple filterable allow-create placeholder="新增或删除标签" />
        </el-form-item>

        <div class="form-hint">⚠ 注意：提交提案后需经档案员人工复核，过审前公开档案信息保持不变。</div>
      </el-form>

      <template #footer>
        <div class="d-btns">
          <button class="b-discard" @click="showEditDialog = false">放弃修改</button>
          <button class="b-submit" @click="submitEditApply" :disabled="submitting">
            {{ submitting ? '存档中...' : '提交变更提案' }}
          </button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import apiClient from '@/utils/api'

const route = useRoute()
const router = useRouter()
const club = ref(null)
const loading = ref(false)
const categories = ref([])

// 变更申请表单
const showEditDialog = ref(false)
const submitting = ref(false)
const editFormRef = ref(null)
const editForm = ref({
  id: null,
  name: '',
  categoryId: null,
  leaderName: '',
  description: '',
  size: 1,
  joinUrl: '',
  logoUrl: '',
  isTestRequired: false,
  testLevel: 0,
  status: 0,
  tagNames: []
})

const rules = {
  name: [{ required: true, message: '名称不能为空', trigger: 'blur' }],
  categoryId: [{ required: true, message: '分类不能为空', trigger: 'change' }],
  leaderName: [{ required: true, message: '负责人不能为空', trigger: 'blur' }],
  description: [{ required: true, message: '简介不能为空', trigger: 'blur' }]
}

// 状态文本转换
const getStatusText = (s) => {
  const map = { 0: '运营中', 1: '休整中', 2: '已解散' }
  return map[s] || '未知状态'
}
const getStatusType = (s) => {
  const map = { 0: 'success', 1: 'warning', 2: 'info' }
  return map[s] || ''
}

const fetchCategories = async () => {
  try {
    const res = await apiClient.get('/Club/Categories')
    categories.value = res.data || res
  } catch (e) { console.error('分类加载失败') }
}

const fetchDetail = async () => {
  const id = route.params.id
  if (!id) return
  loading.value = true
  try {
    const res = await apiClient.get(`/Club/${id}`)
    club.value = res.data || res
  } catch (e) {
    ElMessage.error('无法调取该档案，请检查是否存在')
  } finally {
    loading.value = false
  }
}

const openEditDialog = async () => {
  if (!club.value) return
  if (categories.value.length === 0) await fetchCategories()
  
  // 深拷贝数据，防止弹窗修改直接映射到页面
  editForm.value = { 
    ...club.value,
    tagNames: [...(club.value.tagNames || [])]
  }
  showEditDialog.value = true
}

const submitEditApply = async () => {
  if (!editFormRef.value) return
  await editFormRef.value.validate(async (valid) => {
    if (valid) {
      submitting.value = true
      try {
        // 调用后端补全的 UpdateApply 接口
        await apiClient.post('/Club/UpdateApply', editForm.value) 
        ElMessage.success('档案变更提案已提交审核')
        showEditDialog.value = false
      } catch (e) {
        ElMessage.error('提交失败，请重试')
      } finally {
        submitting.value = false
      }
    }
  })
}

// 加入逻辑：根据 JoinUrl 判断
const handleJoin = async () => {
  try {
    const res = await apiClient.get(`/Club/JoinLink/${club.value.id}`)
    const joinUrl = res.data.joinUrl
    if (!joinUrl) {
      ElMessage.warning('该社团暂未提供加入方式')
      return
    }
    
    if (joinUrl.startsWith('http')) {
      window.open(joinUrl, '_blank')
    } else {
      ElMessageBox.alert(`请前往对应平台添加 ID / 群号：${joinUrl}`, '加入档案指引', {
        confirmButtonText: '收到',
        customClass: 'archive-alert'
      })
    }
  } catch (e) {
    ElMessage.error('无法获取加入方式')
  }
}

const formatDate = (d) => {
  if (!d) return 'N/A'
  const date = new Date(d)
  return `${date.getFullYear()}.${String(date.getMonth()+1).padStart(2,'0')}.${String(date.getDate()).padStart(2,'0')}`
}

onMounted(fetchDetail)
</script>

<style scoped>
/* 延续 Neo-Brutalism 设计风格 */
.club-detail-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 40px 40px 100px 40px;
  background: #fdfdfd;
  min-height: 100vh;
  color: #1a1a1a;
  font-family: 'PingFang SC', sans-serif;
}

.detail-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 2px solid #1a1a1a;
  padding-bottom: 20px;
  margin-bottom: 50px;
}

.back-btn {
  background: none; border: 1px solid #1a1a1a;
  padding: 8px 16px; font-weight: 900; cursor: pointer;
  transition: all 0.2s;
}
.back-btn:hover { background: #1a1a1a; color: #fff; }

.edit-apply-btn {
  background: #fff; border: 1px solid #3b82f6; color: #3b82f6;
  padding: 8px 16px; font-weight: 900; cursor: pointer; margin-right: 20px;
}
.edit-apply-btn:hover { background: #3b82f6; color: #fff; }

.status-tag { font-size: 11px; font-weight: 900; color: #ccc; letter-spacing: 1px; }

.detail-content { display: flex; gap: 80px; align-items: flex-start; }

/* 解散状态样式 */
.is-disbanded { opacity: 0.7; filter: grayscale(0.8); }

.info-aside { width: 320px; flex-shrink: 0; position: sticky; top: 40px; }
.visual-box {
  width: 100%; aspect-ratio: 1; border: 2px solid #1a1a1a;
  box-shadow: 12px 12px 0 #1a1a1a; margin-bottom: 40px; background: #fff;
}
.club-logo { width: 100%; height: 100%; object-fit: cover; }

.spec-list { display: flex; flex-direction: column; gap: 24px; margin-bottom: 40px; }
.spec-item .label { font-size: 10px; font-weight: 900; color: #aaa; letter-spacing: 1px; display: block; margin-bottom: 4px;}
.spec-item .value { font-size: 16px; font-weight: 900; border-bottom: 1px solid #eee; padding-bottom: 8px; display: block; }

.status-badge { border-radius: 0 !important; border: 1px solid #1a1a1a !important; font-weight: 900; }

.join-btn {
  width: 100%; padding: 18px; background: #1a1a1a; color: #fff;
  border: none; font-weight: 900; cursor: pointer; font-size: 15px;
}
.join-btn:disabled { background: #eee; color: #aaa; cursor: not-allowed; }

.main-article { flex: 1; }
.article-section { margin-bottom: 60px; }
.section-title {
  font-size: 13px; font-weight: 900; background: #1a1a1a; color: #fff;
  padding: 4px 12px; margin-bottom: 30px; display: inline-block;
}
.description-text { font-size: 18px; line-height: 2; color: #444; white-space: pre-wrap; }

.tag-cloud { display: flex; flex-wrap: wrap; gap: 12px; }
.detail-tag { font-size: 13px; font-weight: bold; color: #888; border: 1px solid #eee; padding: 6px 12px; }

.act-item { display: flex; gap: 30px; padding: 20px 0; border-bottom: 1px solid #f5f5f5; }
.act-date { font-weight: 900; color: #1a1a1a; }
.act-content { color: #666; font-size: 15px; }

/* 弹窗设计重置 */
:deep(.custom-dialog) { border-radius: 0; border: 4px solid #1a1a1a; box-shadow: 20px 20px 0 rgba(0,0,0,0.1); padding: 0;}
.d-head { padding: 25px; font-weight: 900; font-size: 20px; border-bottom: 1px solid #eee; }
.form-hint { font-size: 12px; color: #ff4d4f; margin-top: 20px; font-weight: 900; }
.d-btns { display: flex; border-top: 2px solid #1a1a1a; }
.d-btns button { flex: 1; padding: 20px; font-weight: 900; border: none; cursor: pointer; font-family: inherit; }
.b-discard { background: #fff; color: #999; border-right: 1px solid #eee !important; }
.b-submit { background: #1a1a1a; color: #fff; }

:deep(.el-input__wrapper), :deep(.el-textarea__inner) { border-radius: 0 !important; box-shadow: none !important; border: 1px solid #dcdfe6; }

@media (max-width: 900px) {
  .detail-content { flex-direction: column; }
  .info-aside { width: 100%; position: static; }
}
</style>