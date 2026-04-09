<template>
  <div class="excel-manager-modern">
    <div v-if="!currentExcelId" class="excel-list-view">
      <header class="view-header">
        <div class="title-group">
          <h1>云端数据中心</h1>
          <p>管理并协作处理项目的表格数据</p>
        </div>
        <button class="btn-md-primary" @click="createNewExcel">
          <span class="plus-icon">+</span> 新建表格
        </button>
      </header>
      
      <div class="excel-grid">
        <div v-for="sheet in excelList" :key="sheet.id" class="md-card" @click="openExcel(sheet)">
          <div class="card-icon">
            <span class="icon-bg"></span>
            <span class="symbol">📊</span>
          </div>
          <div class="card-body">
            <h3 class="name">{{ sheet.name }}</h3>
            <div class="meta">
              <span class="time">{{ formatDate(sheet.updatedAt) }} 更新</span>
              <span class="dot">·</span>
              <span class="status" :class="{ 'busy': sheet.occupiedBy }">
                {{ sheet.occupiedBy ? '有人正在编辑' : '空闲' }}
              </span>
            </div>
          </div>
          <div class="card-action">
            <span class="arrow">→</span>
          </div>
        </div>

        <div v-if="excelList.length === 0" class="empty-state">
          <div class="empty-icon">📂</div>
          <p>当前项目暂无表格，开始创建一个吧</p>
        </div>
      </div>
    </div>

    <div v-else class="excel-editor-modern">
      <nav class="editor-nav">
        <div class="nav-left">
          <button class="btn-icon-back" @click="closeExcel" title="返回列表">
            ←
          </button>
          <div class="divider"></div>
          <div class="sheet-info">
            <h3>{{ currentSheetName }}</h3>
            <div class="sync-indicator" :class="{ 'warning': isLockedByOthers }">
              <span class="status-dot"></span>
              {{ isLockedByOthers ? '只读模式' : saveStatus }}
            </div>
          </div>
        </div>
        <div class="nav-right">
          <button 
            class="btn-md-save" 
            @click="saveExcelData" 
            :disabled="isSaving || isLockedByOthers"
          >
            {{ isSaving ? '正在保存...' : '保存更改' }}
          </button>
        </div>
      </nav>
      
      <div class="spreadsheet-container">
        <div v-if="isLockedByOthers" class="readonly-banner">
          <span class="banner-icon">👁️</span>
          <p>
            <strong>只读模式</strong> 
            干员 {{ lockMessage.split('[')[1]?.split(']')[0] || '他人' }} 正在编辑，你目前仅拥有查看权限。
          </p>
        </div>

        <div id="x-spreadsheet-demo" ref="sheetRef"></div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, nextTick } from 'vue'
import Spreadsheet from 'x-data-spreadsheet'
import 'x-data-spreadsheet/dist/xspreadsheet.css'
import apiClient from '@/utils/api'

const props = defineProps<{ projectId: string | number }>()

const excelList = ref<any[]>([])
const currentExcelId = ref<number | null>(null)
const currentSheetName = ref('')
const sheetRef = ref<HTMLElement | null>(null)
let spreadsheetInstance: any = null

const isSaving = ref(false)
const saveStatus = ref('已与云端同步')
const isLockedByOthers = ref(false)
const lockMessage = ref('')
let lockTimer: any = null

const fetchList = async () => {
  try {
    const res = await apiClient.get(`/projects/${props.projectId}/excels`)
    const myUserId = parseInt(localStorage.getItem('userId') || '0') 
    
    excelList.value = res.data.map((item: any) => {
      const occupiedId = item.OccupiedBy || item.occupiedBy
      return {
        ...item,
        id: item.Id || item.id,
        name: item.Name || item.name,
        updatedAt: item.UpdatedAt || item.updatedAt,
        occupiedBy: (occupiedId && occupiedId !== myUserId) ? occupiedId : null 
      }
    })
  } catch (e) { 
    console.error("获取列表失败", e) 
  }
}

const createNewExcel = async () => {
  const name = prompt('请输入表格名称:')
  if (!name) return
  try {
    await apiClient.post(`/projects/${props.projectId}/excels`, { name })
    fetchList()
  } catch (e) { alert('创建失败') }
}

const openExcel = async (sheet: any) => {
  currentExcelId.value = sheet.id
  currentSheetName.value = sheet.name
  await nextTick()
  initSpreadsheet()
  await loadExcelDetail()
  await tryAcquireLock()
}

const initSpreadsheet = () => {
  if (!sheetRef.value) return
  spreadsheetInstance = new Spreadsheet(sheetRef.value, {
    showToolbar: true,
    view: {
      height: () => document.documentElement.clientHeight - 180,
      width: () => document.documentElement.clientWidth - 40
    }
  })
  spreadsheetInstance.change(() => {
    if (!isLockedByOthers.value) saveStatus.value = '编辑中...'
  })
}

const loadExcelDetail = async () => {
  try {
    const res = await apiClient.get(`/excels/${currentExcelId.value}`)
    const contentData = res.data.Content || res.data.content;
    
    if (contentData && contentData !== "[]") {
      const parsedData = JSON.parse(contentData);
      spreadsheetInstance.loadData(parsedData);
      saveStatus.value = '已对齐最新数据';
    }
  } catch (e) { 
    console.error("加载详情失败", e);
  }
}

const tryAcquireLock = async () => {
  try {
    await apiClient.post(`/excels/${currentExcelId.value}/lock`)
    isLockedByOthers.value = false 
    lockMessage.value = "" 
    if (spreadsheetInstance) spreadsheetInstance.setMode('edit')
    startHeartbeat()
  } catch (error: any) {
    if (error.response?.status === 423) {
      isLockedByOthers.value = true
      lockMessage.value = error.response.data.message || "他人正在编辑"
      if (spreadsheetInstance) spreadsheetInstance.setMode('read')
    }
  }
}

const startHeartbeat = () => {
  if (lockTimer) clearInterval(lockTimer)
  lockTimer = setInterval(async () => {
    try {
      await apiClient.post(`/excels/${currentExcelId.value}/lock`)
    } catch (e) {
      isLockedByOthers.value = true
      lockMessage.value = "锁失效"
      if (spreadsheetInstance) spreadsheetInstance.setMode('read')
      clearInterval(lockTimer)
    }
  }, 15000)
}

const saveExcelData = async () => {
  if (!spreadsheetInstance || isLockedByOthers.value) return
  isSaving.value = true
  saveStatus.value = '正在保存...'
  try {
    const data = spreadsheetInstance.getData()
    await apiClient.put(`/excels/${currentExcelId.value}`, {
      content: JSON.stringify(data)
    })
    saveStatus.value = '所有更改已保存'
  } catch (e) { alert('保存失败') } finally { isSaving.value = false }
}

const closeExcel = async () => {
  if (lockTimer) clearInterval(lockTimer)
  try {
    if (!isLockedByOthers.value) await apiClient.post(`/excels/${currentExcelId.value}/unlock`)
  } catch (e) {}
  currentExcelId.value = null
  spreadsheetInstance = null
  fetchList()
}

const formatDate = (d: string) => d ? new Date(d).toLocaleDateString() : '-'

onMounted(fetchList)
onBeforeUnmount(() => { if (lockTimer) clearInterval(lockTimer) })
</script>

<style scoped>
.excel-manager-modern {
  height: 100%;
  background: #f8f9fa;
  color: #202124;
  display: flex;
  flex-direction: column;
}

.view-header {
  padding: 40px 60px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.title-group h1 { font-size: 28px; font-weight: 500; margin: 0; color: #1a73e8; }
.title-group p { margin: 8px 0 0; color: #5f6368; font-size: 14px; }

.btn-md-primary {
  background: #1a73e8; color: white; border: none; padding: 10px 24px;
  border-radius: 24px; font-weight: 500; cursor: pointer;
  box-shadow: 0 1px 3px rgba(60,64,67,0.3); transition: 0.2s;
}

.excel-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 24px;
  padding: 0 60px 40px;
}
.md-card {
  background: white; border-radius: 12px; padding: 20px;
  display: flex; align-items: center; gap: 20px;
  cursor: pointer; transition: 0.2s; border: 1px solid #dadce0;
}
.md-card:hover { box-shadow: 0 4px 12px rgba(0,0,0,0.1); border-color: #1a73e8; }

.icon-bg {
  width: 48px; height: 48px; background: #e8f0fe;
  border-radius: 8px; position: absolute;
}
.card-icon { position: relative; width: 48px; height: 48px; display: flex; align-items: center; justify-content: center; }
.card-icon .symbol { font-size: 24px; z-index: 1; }

.card-body { flex: 1; }
.card-body h3 { margin: 0; font-size: 16px; font-weight: 500; }
.meta { font-size: 12px; color: #70757a; margin-top: 4px; display: flex; align-items: center; gap: 8px; }
.status.busy { color: #d93025; font-weight: 500; }

.editor-nav {
  background: white; height: 64px; border-bottom: 1px solid #dadce0;
  display: flex; align-items: center; justify-content: space-between; padding: 0 20px;
}
.nav-left { display: flex; align-items: center; gap: 16px; }
.btn-icon-back {
  background: none; border: none; font-size: 20px; color: #5f6368;
  cursor: pointer; width: 40px; height: 40px; border-radius: 50%;
}
.btn-icon-back:hover { background: #f1f3f4; }
.divider { width: 1px; height: 32px; background: #dadce0; }

.sheet-info h3 { margin: 0; font-size: 18px; font-weight: 400; }
.sync-indicator { font-size: 12px; color: #1e8e3e; display: flex; align-items: center; gap: 6px; }
.sync-indicator.warning { color: #d93025; }
.status-dot { width: 8px; height: 8px; background: currentColor; border-radius: 50%; }

.btn-md-save {
  background: #1a73e8; color: white; border: none; padding: 8px 20px;
  border-radius: 4px; font-weight: 500; cursor: pointer;
}
.btn-md-save:disabled { background: #ccc; cursor: not-allowed; }

/* 🌟 核心修改：只读 Banner 样式 */
.spreadsheet-container {
  display: flex;
  flex-direction: column;
  position: relative;
  background: white;
  margin: 20px;
  flex: 1;
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid #dadce0;
}

.readonly-banner {
  background: #fff4e5;
  color: #663c00;
  padding: 10px 24px;
  display: flex;
  align-items: center;
  gap: 12px;
  border-bottom: 1px solid #ffe2b3;
  font-size: 13px;
}
.readonly-banner strong { color: #e65100; margin-right: 4px; }
.banner-icon { font-size: 16px; }

#x-spreadsheet-demo {
  flex: 1;
}

.empty-state { text-align: center; padding: 80px; color: #5f6368; }
.empty-icon { font-size: 48px; margin-bottom: 16px; }
</style>