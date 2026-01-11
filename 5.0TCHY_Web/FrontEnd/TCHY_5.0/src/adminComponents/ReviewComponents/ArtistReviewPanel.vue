<template>
  <div class="artist-panel">
    <div v-if="loading" class="loading-state">加载中...</div>
    <div v-else-if="filteredList.length === 0" class="empty-state">暂无待审核申请</div>
    
    <div v-else class="card-grid">
      <div v-for="item in filteredList" :key="item.id" class="review-card">
        <div class="card-header">
          <span class="type-tag" :class="item.category.toLowerCase()">
            {{ item.category }}
          </span>
          <span class="time">{{ formatDate(item.submitTime) }}</span>
        </div>
        
        <div class="card-body">
          <div class="user-info">
            <span class="label">申请人ID:</span> #{{ item.userId }}
          </div>
          <div class="portfolio">
            <a :href="item.portfolioUrl" target="_blank" class="link-btn">
              🔗 查看作品集
            </a>
          </div>
          <div class="desc-box" v-if="item.description">
            <p>"{{ item.description }}"</p>
          </div>
        </div>

        <div class="card-actions">
          <button class="btn reject" @click="openAuditModal(item, false)">驳回</button>
          <button class="btn approve" @click="openAuditModal(item, true)">通过 & 评级</button>
        </div>
      </div>
    </div>

    <div v-if="showModal" class="modal-mask">
      <div class="modal-content">
        <h3>{{ isApproveMode ? '通过申请 - 评定等级' : '驳回申请' }}</h3>
        
        <div v-if="isApproveMode" class="level-selector">
          <p>请根据作品质量授予等级：</p>
          <div class="radios">
            <label v-for="i in 7" :key="i" :class="{ selected: grantLevel === i }">
              <input type="radio" :value="i" v-model="grantLevel"> Lv.{{ i }}
            </label>
          </div>
        </div>

        <div class="comment-area">
          <textarea 
            v-model="auditComment" 
            :placeholder="isApproveMode ? '可选：写给画师的评语...' : '必填：请输入驳回理由...'"
          ></textarea>
        </div>

        <div class="modal-actions">
          <button @click="showModal = false">取消</button>
          <button 
            class="confirm-btn" 
            :class="isApproveMode ? 'approve' : 'reject'"
            @click="submitAudit"
            :disabled="submitting"
          >
            {{ submitting ? '提交中...' : '确认提交' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/utils/api' 

const props = defineProps(['search'])
const emit = defineEmits(['update-count'])

const list = ref([])
const loading = ref(false)

// 模态框状态
const showModal = ref(false)
const currentItem = ref(null)
const isApproveMode = ref(false)
const grantLevel = ref(1)
const auditComment = ref('')
const submitting = ref(false)

// 1. 获取数据
const fetchData = async () => {
  loading.value = true
  try {
    const res = await apiClient.get('/ChaiArtist/pending')
    // 映射大小写
    list.value = (res.data || []).map(item => ({
      id: item.Id || item.id,
      userId: item.UserId || item.userId,
      category: item.Category,
      portfolioUrl: item.PortfolioUrl,
      description: item.Description,
      submitTime: item.SubmitTime
    }))
    emit('update-count', list.value.length)
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}

// 2. 搜索过滤
const filteredList = computed(() => {
  if (!props.search) return list.value
  const s = props.search.toLowerCase()
  return list.value.filter(item => 
    String(item.userId).includes(s) || 
    (item.description && item.description.toLowerCase().includes(s))
  )
})

// 3. 打开审核弹窗
const openAuditModal = (item, isApprove) => {
  currentItem.value = item
  isApproveMode.value = isApprove
  grantLevel.value = 1 // 重置默认等级
  auditComment.value = ''
  showModal.value = true
}

// 4. 提交审核
const submitAudit = async () => {
  if (!isApproveMode && !auditComment.value.trim()) {
    alert('驳回必须填写理由')
    return
  }

  submitting.value = true
  try {
    const payload = {
      applicationId: currentItem.value.id,
      isApproved: isApproveMode.value,
      grantLevel: isApproveMode.value ? grantLevel.value : 0, 
      comment: auditComment.value
    }
    
    await apiClient.post('/ChaiArtist/audit', payload)
    
    // 成功后移除该条目
    list.value = list.value.filter(i => i.id !== currentItem.value.id)
    emit('update-count', list.value.length)
    showModal.value = false
    
  } catch (err) {
    alert('操作失败: ' + (err.response?.data?.message || err.message))
  } finally {
    submitting.value = false
  }
}

const formatDate = (str) => {
  return new Date(str).toLocaleString()
}

// 暴露给父组件
defineExpose({
  onSearch: (val) => { },
  refresh: fetchData
})

onMounted(fetchData)
</script>

<style scoped>
/* 简单的卡片样式 */
.card-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
  padding: 10px;
}

.review-card {
  background: #fff;
  border: 1px solid #eee;
  border-radius: 8px;
  padding: 15px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
}

.card-header {
  display: flex; justify-content: space-between; margin-bottom: 15px;
}

.type-tag {
  padding: 2px 8px; border-radius: 4px; font-size: 12px; font-weight: bold;
}
.type-tag.static { background: #e3f2fd; color: #1976d2; }
.type-tag.animation { background: #ffebee; color: #c62828; }

.card-body { margin-bottom: 15px; font-size: 14px; }
.link-btn { color: #4F46E5; text-decoration: none; font-weight: 500; }
.desc-box { background: #f9fafb; padding: 8px; border-radius: 4px; margin-top: 8px; color: #666; font-size: 12px; }

.card-actions { display: flex; gap: 10px; }
.btn { flex: 1; padding: 8px; border: none; border-radius: 4px; cursor: pointer; font-weight: 500; }
.btn.approve { background: #10B981; color: white; }
.btn.reject { background: #EF4444; color: white; }

/* Modal Styles */
.modal-mask {
  position: fixed; inset: 0; background: rgba(0,0,0,0.5);
  display: flex; align-items: center; justify-content: center; z-index: 100;
}
.modal-content {
  background: white; padding: 25px; border-radius: 12px; width: 450px; /* 稍微加宽一点适应7个按钮 */
}

/* 调整等级选择器样式以支持多行或更紧凑的布局 */
.level-selector .radios {
  display: flex; 
  gap: 8px; 
  margin: 10px 0;
  flex-wrap: wrap; /* 允许换行 */
}
.radios label {
  padding: 6px 12px; 
  border: 1px solid #ddd; 
  border-radius: 4px; 
  cursor: pointer;
  font-size: 14px;
}
.radios label:hover {
  background-color: #f0fdf4;
  border-color: #10B981;
}
.radios label.selected {
  background: #10B981; color: white; border-color: #10B981;
}

.comment-area textarea {
  width: 100%; height: 80px; margin: 15px 0; padding: 8px; border: 1px solid #ddd; border-radius: 4px;
}
.modal-actions { display: flex; justify-content: flex-end; gap: 10px; }
.confirm-btn {
  padding: 8px 16px; border: none; border-radius: 4px; color: white; cursor: pointer;
}
.confirm-btn.approve { background-color: #10B981; }
.confirm-btn.reject { background-color: #EF4444; }
.confirm-btn:disabled { background-color: #ccc; cursor: not-allowed; }
</style>