<template>
  <div class="resource-board">
    <!-- 用户信息区 -->
    <el-card class="user-card">
      <div class="user-info">
        <div class="avatar" />
        <div>
          <div class="username">你好，{{ userStore.user?.username || "未登录" }}</div>
          <div>
            等级：Lv.{{ userStore.user?.level || 0 }}　
            积分：{{ userStore.user?.points || 0 }}
          </div>
        </div>
      </div>
    </el-card>

    <!-- 分类导航 -->
    <el-tabs v-model="currentCategory" type="card" class="category-tabs">
      <el-tab-pane
        v-for="cat in categories"
        :key="cat.id"
        :label="cat.name"
        :name="cat.id"
      />
    </el-tabs>

    <!-- 资源列表区 -->
    <el-row :gutter="20" class="resource-list">
      <el-col :span="8" v-for="resource in resources" :key="resource.id">
        <el-card class="resource-card">
          <div class="card-header">
            <span class="resource-title">{{ resource.title }}</span>
            <el-tag size="small" type="info">{{ getCategoryName(resource.categoryId) }}</el-tag>
          </div>
          <div class="resource-desc">{{ resource.desc }}</div>
          <div class="resource-meta">
            <el-tag size="small" v-if="resource.level > 0" type="warning">Lv.{{ resource.level }}以上</el-tag>
            <el-tag size="small" v-if="resource.points > 0" type="success">消耗积分: {{ resource.points }}</el-tag>
          </div>
          <div class="resource-actions">
            <el-button
              type="primary"
              size="small"
              :disabled="!canDownload(resource)"
              @click="tryDownload(resource)"
            >
              <template v-if="canDownload(resource)">下载</template>
              <template v-else-if="userStore.user?.level < resource.level">等级不足</template>
              <template v-else-if="userStore.user?.points < resource.points">积分不足</template>
              <template v-else>不可下载</template>
            </el-button>
            <el-button
              type="default"
              size="small"
              @click="showDetail(resource)"
            >详情</el-button>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 下载弹窗 -->
    <el-dialog v-model="showLink" title="下载资源" width="400px" :close-on-click-modal="false">
      <div v-if="currentDownload">
        <div class="download-link">
          <el-link :href="currentDownload.url" target="_blank" type="primary">点击前往下载地址</el-link>
        </div>
        <div v-if="currentDownload.code" class="extract-code">
          提取码：<b>{{ currentDownload.code }}</b>
        </div>
        <el-alert
          v-if="currentDownload.points > 0"
          type="success"
          show-icon
          :closable="false"
          title="已消耗 {{ currentDownload.points }} 积分"
        />
      </div>
      <template #footer>
        <el-button @click="showLink = false">关闭</el-button>
      </template>
    </el-dialog>

    <!-- 详情弹窗 -->
    <el-dialog v-model="showDetailDialog" title="资源详情" width="500px">
      <div v-if="detailResource">
        <h3>{{ detailResource.title }}</h3>
        <div style="margin-bottom:12px;">分类：{{ getCategoryName(detailResource.categoryId) }}</div>
        <div>{{ detailResource.desc }}</div>
        <div style="margin-top:18px;">
          <el-tag size="small" type="warning" v-if="detailResource.level > 0">Lv.{{ detailResource.level }}以上</el-tag>
          <el-tag size="small" type="success" v-if="detailResource.points > 0">消耗积分: {{ detailResource.points }}</el-tag>
        </div>
      </div>
      <template #footer>
        <el-button @click="showDetailDialog = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/store/user' // 路径请根据你的项目实际情况调整

const userStore = useUserStore()

// 分类（后续可由API获取）
const categories = ref([
  { id: 0, name: "全部" },
  { id: 1, name: "软件" },
  { id: 2, name: "书籍" },
  { id: 3, name: "漫画" },
])

const currentCategory = ref(0)

// 资源数据（后续由API分页获取）
const allResources = ref([
  {
    id: 101,
    categoryId: 1,
    title: "PS2021绿色版",
    desc: "免安装，全功能PhotoShop软件。",
    url: "https://pan.baidu.com/s/xxxxxx",
    code: "abcd",
    level: 2,
    points: 15
  },
  {
    id: 102,
    categoryId: 2,
    title: "Vue3全栈开发电子书",
    desc: "系统学习Vue3前端开发的电子书。",
    url: "https://pan.baidu.com/s/yyyyyy",
    code: "efgh",
    level: 1,
    points: 0
  },
  {
    id: 103,
    categoryId: 3,
    title: "海贼王漫画全集",
    desc: "高清完整版，持续更新。",
    url: "https://pan.baidu.com/s/zzzzzz",
    code: "ijkl",
    level: 3,
    points: 30
  },
  {
    id: 104,
    categoryId: 1,
    title: "国产输入法",
    desc: "适合各类系统的高效输入法。",
    url: "https://pan.baidu.com/s/wwwwww",
    code: "",
    level: 1,
    points: 0
  }
])

const resources = computed(() => {
  if (currentCategory.value === 0) return allResources.value
  return allResources.value.filter(r => r.categoryId === currentCategory.value)
})

function getCategoryName(cid) {
  const cat = categories.value.find(c => c.id === cid)
  return cat ? cat.name : ""
}

function canDownload(res) {
  const user = userStore.user
  return user && user.level >= res.level && user.points >= res.points
}

const showLink = ref(false)
const currentDownload = ref(null)
function tryDownload(resource) {
  const user = userStore.user
  if (!canDownload(resource)) {
    if (!user || user.level < resource.level) {
      ElMessage.warning(`需要等级 Lv.${resource.level} 才能下载`)
    } else if (user.points < resource.points) {
      ElMessage.warning("积分不足，无法下载")
    }
    return
  }
  // 积分消耗（实际应调用后端！这里只是前端模拟）
  if (resource.points > 0) {
    userStore.user.points -= resource.points
    userStore.updateUser(userStore.user) // 更新Pinia和localStorage
  }
  currentDownload.value = resource
  showLink.value = true
}

const showDetailDialog = ref(false)
const detailResource = ref(null)
function showDetail(resource) {
  detailResource.value = resource
  showDetailDialog.value = true
}
</script>

<style scoped>
.resource-board {
  max-width: 1200px;
  margin: 32px auto;
  padding: 24px 0 32px 0;
}
.user-card {
  margin-bottom: 24px;
  background: #f5faff;
}
.user-info {
  display: flex;
  align-items: center;
  gap: 18px;
}
.avatar {
  width: 54px;
  height: 54px;
  border-radius: 50%;
  background: #d9ecff;
  margin-right: 8px;
}
.username {
  font-size: 20px;
  font-weight: bold;
  color: #1976d2;
  margin-bottom: 4px;
}
.category-tabs {
  margin-bottom: 24px;
}
.resource-list {
  margin-top: 16px;
}
.resource-card {
  margin-bottom: 22px;
  min-height: 220px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}
.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 18px;
  font-weight: bold;
  margin-bottom: 7px;
}
.resource-title {
  color: #1976d2;
}
.resource-desc {
  color: #555;
  margin-bottom: 8px;
  font-size: 15px;
}
.resource-meta {
  margin-bottom: 10px;
  display: flex;
  gap: 12px;
}
.resource-actions {
  display: flex;
  gap: 12px;
}
.download-link {
  margin-bottom: 12px;
}
.extract-code {
  margin-top: 6px;
  font-family: monospace;
  color: #ff9800;
  background: #f8f3e5;
  padding: 2px 10px;
  border-radius: 4px;
  font-size: 17px;
  letter-spacing: 2px;
}
</style>