<template>
  <div class="univer-container">
    <div class="toolbar" v-if="showToolbar">
      <button @click="getData" class="action-btn">获取数据预览</button>
      
      <button @click="saveData" :disabled="isSaving || !!lockedMessage" class="action-btn primary">
        {{ isSaving ? '保存中...' : '保存至云端' }}
      </button>
      
      <button @click="forceResetTable" class="action-btn danger" style="margin-left: auto;">
        🛠️ 强制重置 (修复无法编辑)
      </button>
    </div>

    <div v-if="lockedMessage" class="lock-warning">
      <span>🔒 {{ lockedMessage }}。您当前处于【只读预览模式】，无法保存任何修改。</span>
      <button @click="startLockHeartbeat" class="retry-lock-btn">尝试重新抢锁</button>
    </div>

    <div ref="containerRef" class="sheet-box" v-once></div>

    <div v-if="showDataPreview && sheetData" class="data-preview">
      <h3>数据预览</h3>
      <div class="preview-content">
        <pre>{{ JSON.stringify(sheetData, null, 2) }}</pre>
      </div>
      <button @click="showDataPreview = false" class="close-btn">关闭预览</button>
    </div>
  </div>
</template>

<script setup>
import { ref, markRaw, onMounted, onUnmounted } from 'vue';
import apiClient from '@/utils/api';

// 🌟 核心与门面 API
import { Univer, UniverInstanceType, LocaleType } from '@univerjs/core';
import { defaultTheme } from '@univerjs/themes';
import { FUniver } from '@univerjs/core/facade';
import '@univerjs/sheets/facade';
import '@univerjs/sheets-ui/facade';

// 🌟 仅引入拥有 UI 界面的模块中文包
import designZhCN from '@univerjs/design/locale/zh-CN';
import uiZhCN from '@univerjs/ui/locale/zh-CN';
import docsUIZhCN from '@univerjs/docs-ui/locale/zh-CN';
import sheetsUIZhCN from '@univerjs/sheets-ui/locale/zh-CN';
import sheetsFormulaUIZhCN from '@univerjs/sheets-formula-ui/locale/zh-CN'; 
import sheetsZenEditorZhCN from '@univerjs/sheets-zen-editor/locale/zh-CN'; 

// 🌟 底层引擎与 UI 插件
import { UniverRenderEnginePlugin } from '@univerjs/engine-render';
import { UniverFormulaEnginePlugin } from '@univerjs/engine-formula';
import { UniverUIPlugin } from '@univerjs/ui';
import { UniverDocsPlugin } from '@univerjs/docs';
import { UniverDocsUIPlugin } from '@univerjs/docs-ui';
import { UniverSheetsPlugin } from '@univerjs/sheets';
import { UniverSheetsUIPlugin } from '@univerjs/sheets-ui';
import { UniverSheetsFormulaPlugin } from '@univerjs/sheets-formula';
import { UniverSheetsFormulaUIPlugin } from '@univerjs/sheets-formula-ui';
import { UniverSheetsZenEditorPlugin } from '@univerjs/sheets-zen-editor';

// 🌟 CSS 样式表
import '@univerjs/design/lib/index.css';
import '@univerjs/ui/lib/index.css';
import '@univerjs/docs-ui/lib/index.css';
import '@univerjs/sheets-ui/lib/index.css';
import '@univerjs/sheets-formula-ui/lib/index.css';
import '@univerjs/sheets-zen-editor/lib/index.css';

const props = defineProps({
  showToolbar: { type: Boolean, default: true },
  projectId: { type: String, required: true }
});

const containerRef = ref(null);
let rawUniverInstance = null;
let rawUniverAPI = null;

const isSaving = ref(false);
const showDataPreview = ref(false);
const sheetData = ref(null);
const currentExcelId = ref(null);
const isLoading = ref(true);
let lockTimer = null;

// 🌟 新增：存储被锁定的提示信息
const lockedMessage = ref(null);

const deepMergeLocales = (...locales) => {
  const target = {};
  for (const locale of locales) {
    if (!locale) continue;
    for (const key of Object.keys(locale)) {
      if (typeof locale[key] === 'object' && locale[key] !== null && !Array.isArray(locale[key])) {
        target[key] = deepMergeLocales(target[key] || {}, locale[key]);
      } else {
        target[key] = locale[key];
      }
    }
  }
  return target;
};

const generatePerfectTemplate = () => ({
  id: 'project-workbook',
  appVersion: '3.0.0-alpha',
  sheetOrder: ['sheet-01'],
  name: '项目数据表',
  sheets: {
    'sheet-01': {
      id: 'sheet-01',
      name: 'Sheet1',
      status: 1, 
      rowCount: 100,
      columnCount: 26,
      cellData: {
        "0": { "0": { "v": "干员名录" }, "1": { "v": "当前状态" } },
        "1": { "0": { "v": "Amiya" }, "1": { "v": "工作中" } }
      }
    }
  }
});

const fetchOrInitializeExcel = async () => {
  try {
    isLoading.value = true;
    const listRes = await apiClient.get(`/projects/${props.projectId}/excels`);
    if (listRes.data && listRes.data.length > 0) {
      currentExcelId.value = listRes.data[0].Id || listRes.data[0].id;
    } else {
      const createRes = await apiClient.post(`/projects/${props.projectId}/excels`, { Name: "默认数据表" });
      currentExcelId.value = createRes.data.Id || createRes.data.id;
    }
    await loadExcelData();
  } catch (error) {
    console.error("获取表格信息失败:", error);
  } finally {
    isLoading.value = false;
  }
};

const loadExcelData = async () => {
  try {
    const detailRes = await apiClient.get(`/excels/${currentExcelId.value}`);
    const dbContent = detailRes.data.Content || detailRes.data.content;
    let snapshotData = null;

    try {
      if (dbContent && typeof dbContent === 'string' && dbContent.length > 10) {
        const parsed = JSON.parse(dbContent);
        if (parsed.sheets && Object.values(parsed.sheets)[0]?.status === 1) {
          snapshotData = parsed;
        } else {
          throw new Error("残缺数据");
        }
      } else {
        throw new Error("空数据");
      }
    } catch (e) {
      snapshotData = generatePerfectTemplate();
    }

    initUniver(snapshotData);
    startLockHeartbeat();
  } catch (error) {
    console.error("加载详情失败:", error);
  }
};

const initUniver = (initialData) => {
  if (!containerRef.value) return;
  if (rawUniverInstance) rawUniverInstance.dispose();

  const univer = new Univer({
    theme: defaultTheme,
    locale: LocaleType.ZH_CN,
    locales: {
      [LocaleType.ZH_CN]: deepMergeLocales(
        designZhCN,
        uiZhCN,
        docsUIZhCN,
        sheetsUIZhCN,
        sheetsFormulaUIZhCN,
        sheetsZenEditorZhCN
      ),
    },
  });
  
  rawUniverInstance = markRaw(univer);

  univer.registerPlugin(UniverRenderEnginePlugin);
  univer.registerPlugin(UniverFormulaEnginePlugin);
  univer.registerPlugin(UniverUIPlugin, { container: containerRef.value, header: true, toolbar: true, footer: true });
  univer.registerPlugin(UniverDocsPlugin, { hasScroll: false });
  univer.registerPlugin(UniverDocsUIPlugin);
  univer.registerPlugin(UniverSheetsPlugin);
  univer.registerPlugin(UniverSheetsUIPlugin);
  univer.registerPlugin(UniverSheetsFormulaPlugin);
  univer.registerPlugin(UniverSheetsFormulaUIPlugin);
  univer.registerPlugin(UniverSheetsZenEditorPlugin);

  univer.createUnit(UniverInstanceType.UNIVER_SHEET, initialData);
  rawUniverAPI = markRaw(FUniver.newAPI(univer));
};

const saveData = async () => {
  if (!currentExcelId.value || !rawUniverAPI) return;
  
  // 防御性判断：如果被锁定就不让保存
  if (lockedMessage.value) {
    alert("当前处于只读模式，无法保存");
    return;
  }

  const workbook = rawUniverAPI.getActiveWorkbook();
  if (!workbook) return;

  isSaving.value = true;
  try {
    const snapshot = workbook.getSnapshot();
    await apiClient.put(`/excels/${currentExcelId.value}`, { content: JSON.stringify(snapshot) });
    alert('✅ 数据已成功同步至云端！');
  } catch (error) {
    if (error.response?.status === 423) alert(`保存失败：${error.response.data.message}`);
  } finally {
    isSaving.value = false;
  }
};

const forceResetTable = async () => {
  if (!confirm("⚠️ 警告：这会彻底清空当前表格，并写入一份全新可编辑的模板，确定要执行吗？")) return;
  initUniver(generatePerfectTemplate());
  await saveData();
  alert("已完成强制修复，现在可以自由编辑了！");
};

// 🌟 修改：带状态感知的心跳锁机制
const startLockHeartbeat = () => {
  const tryLock = async () => {
    try {
      await apiClient.post(`/excels/${currentExcelId.value}/lock`);
      // 如果成功没报错，说明抢到了锁，清空锁定警告
      lockedMessage.value = null; 
    } catch (error) {
      if (error.response?.status === 423) {
        // 如果报 423，说明锁在别人那里，把后端的警告语提取出来
        lockedMessage.value = error.response.data.message;
      }
    }
  };

  tryLock(); // 首次执行
  if (lockTimer) clearInterval(lockTimer);
  lockTimer = setInterval(tryLock, 20000); // 20秒一续约
};

const releaseLock = async () => {
  if (currentExcelId.value) try { await apiClient.post(`/excels/${currentExcelId.value}/unlock`); } catch (e) {}
};

const getData = () => {
  if (!rawUniverAPI) return;
  const workbook = rawUniverAPI.getActiveWorkbook();
  if (workbook) {
    sheetData.value = workbook.getSnapshot();
    showDataPreview.value = true;
  }
};

onMounted(() => fetchOrInitializeExcel());
onUnmounted(() => {
  if (lockTimer) clearInterval(lockTimer);
  releaseLock();
  if (rawUniverInstance) rawUniverInstance.dispose();
});
</script>

<style scoped>
.univer-container { width: 100%; height: 100%; display: flex; flex-direction: column; background: #fff; }
.toolbar { padding: 10px 16px; background: #f8f9fa; border-bottom: 1px solid #ddd; display: flex; gap: 12px; flex-shrink: 0; align-items: center; }
.action-btn { padding: 6px 14px; border: 1px solid #ddd; background: #fff; border-radius: 4px; cursor: pointer; font-size: 13px; transition: 0.2s; font-weight: 500;}
.action-btn:hover { background: #eee; }
.action-btn.primary { background: #0052CC; color: #fff; border-color: #0052CC; }
.action-btn.primary:hover { background: #0047B3; }
.action-btn.primary:disabled { background: #B3D4FF; border-color: #B3D4FF; cursor: not-allowed; }
.action-btn.danger { background: #ffebe6; color: #de350b; border-color: #ffbdad; }
.action-btn.danger:hover { background: #ffbdad; }
.sheet-box { flex: 1; position: relative; overflow: hidden; width: 100%; height: 100%; } 
.data-preview { position: absolute; top: 60px; right: 20px; width: 400px; max-height: 80%; background: #fff; border: 1px solid #ddd; box-shadow: 0 4px 12px rgba(0,0,0,0.1); border-radius: 8px; display: flex; flex-direction: column; z-index: 1000; }
.data-preview h3 { padding: 12px 16px; margin: 0; border-bottom: 1px solid #ddd; font-size: 15px; }
.preview-content { flex: 1; overflow: auto; padding: 16px; }
.preview-content pre { margin: 0; font-size: 12px; font-family: monospace; white-space: pre-wrap; word-wrap: break-word; }
.close-btn { margin: 12px; padding: 8px; background: #0052CC; color: #fff; border: none; border-radius: 4px; cursor: pointer; }

/* 🌟 新增：锁定警告条样式 */
.lock-warning {
  background-color: #FFF0B3; 
  color: #172B4D;
  padding: 8px 16px;
  font-size: 13px;
  font-weight: 600;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #FFE380;
  animation: slideDown 0.3s ease-out;
}
.retry-lock-btn {
  background: transparent;
  border: 1px solid #172B4D;
  border-radius: 4px;
  padding: 4px 10px;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: 0.2s;
}
.retry-lock-btn:hover {
  background: #172B4D;
  color: #FFF0B3;
}
@keyframes slideDown {
  from { transform: translateY(-10px); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}
</style>