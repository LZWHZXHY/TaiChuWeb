<template>
  <div class="univer-container">
    <div class="toolbar" v-if="showToolbar">
      <button @click="getData" class="action-btn">获取数据</button>
      <button @click="saveData" :disabled="isSaving" class="action-btn primary">
        {{ isSaving ? '保存中...' : '保存' }}
      </button>
    </div>

    <div ref="containerRef" class="sheet-box"></div>

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
import { ref, shallowRef, onMounted, onUnmounted } from 'vue';

// 1. 核心与主题
import { Univer, UniverInstanceType, LocaleType } from '@univerjs/core';
import { defaultTheme } from '@univerjs/themes'; // ✅ 修复：从主题包导入

// 🌟 2. 引入各个模块的中文语言包 (防止 Locale not initialized 报错)
import designZhCN from '@univerjs/design/locale/zh-CN';
import uiZhCN from '@univerjs/ui/locale/zh-CN';
import docsUIZhCN from '@univerjs/docs-ui/locale/zh-CN';
import sheetsUIZhCN from '@univerjs/sheets-ui/locale/zh-CN';

// 3. 引擎 (渲染与公式)
import { UniverRenderEnginePlugin } from '@univerjs/engine-render';
import { UniverFormulaEnginePlugin } from '@univerjs/engine-formula';

// 4. 基础 UI
import { UniverUIPlugin } from '@univerjs/ui';

// 5. 文档插件 (表格内的富文本排版依赖此插件)
import { UniverDocsPlugin } from '@univerjs/docs';
import { UniverDocsUIPlugin } from '@univerjs/docs-ui';

// 6. 表格核心与 UI
import { UniverSheetsPlugin } from '@univerjs/sheets';
import { UniverSheetsUIPlugin } from '@univerjs/sheets-ui';
import { UniverSheetsFormulaPlugin } from '@univerjs/sheets-formula';

// 🌟 7. 必须引入的样式表 (防止白屏)
import '@univerjs/design/lib/index.css';
import '@univerjs/ui/lib/index.css';
import '@univerjs/docs-ui/lib/index.css';
import '@univerjs/sheets-ui/lib/index.css';

const props = defineProps({
  showToolbar: { type: Boolean, default: true },
  projectId: { type: String, default: '' }
});

const containerRef = ref(null);
const univerRef = shallowRef(null);
const isSaving = ref(false);
const showDataPreview = ref(false);
const sheetData = ref(null);

const initUniver = () => {
  if (!containerRef.value) return;

  if (univerRef.value) {
    univerRef.value.dispose();
  }

  // ✅ 初始化 Univer，注入主题和完整的中文语言包
  const univer = new Univer({
    theme: defaultTheme,
    locale: LocaleType.ZH_CN,
    locales: {
      [LocaleType.ZH_CN]: {
        ...designZhCN,
        ...uiZhCN,
        ...docsUIZhCN,
        ...sheetsUIZhCN,
      },
    },
  });
  univerRef.value = univer;

  // 注册引擎插件
  univer.registerPlugin(UniverRenderEnginePlugin);
  univer.registerPlugin(UniverFormulaEnginePlugin);

  // 注册 UI 插件并绑定 DOM
  univer.registerPlugin(UniverUIPlugin, {
    container: containerRef.value,
    header: true,
    toolbar: true,
    footer: true,
  });

  // 注册文档插件
  univer.registerPlugin(UniverDocsPlugin, { hasScroll: false });
  univer.registerPlugin(UniverDocsUIPlugin);

  // 注册表格相关插件
  univer.registerPlugin(UniverSheetsPlugin);
  univer.registerPlugin(UniverSheetsUIPlugin);
  univer.registerPlugin(UniverSheetsFormulaPlugin);

  // ✅ v0.20.0 规范的创建工作簿 API
  univer.createUnit(UniverInstanceType.UNIVER_SHEET, {
    id: 'project-workbook',
    name: '项目数据表',
    sheets: {
      'sheet-01': {
        name: 'Sheet1',
        cellData: {
          0: { // 第 1 行
            0: { v: '姓名' }, // 第 1 列
            1: { v: '职能' }
          },
          1: { // 第 2 行
            0: { v: 'Eric' },
            1: { v: '项目调度' }
          }
        }
      }
    }
  });
};

const getData = () => {
  if (!univerRef.value) return;
  const workbook = univerRef.value.getCurrentUnitForType(UniverInstanceType.UNIVER_SHEET);
  if (workbook) {
    const snapshot = workbook.getSnapshot();
    sheetData.value = snapshot;
    showDataPreview.value = true;
  }
};

const saveData = () => {
  isSaving.value = true;
  setTimeout(() => {
    alert('✅ Univer 数据已保存！');
    isSaving.value = false;
  }, 500);
};

onMounted(() => {
  setTimeout(() => initUniver(), 0);
});

onUnmounted(() => {
  univerRef.value?.dispose();
});
</script>

<style scoped>
.univer-container {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  background: #fff;
}

.toolbar {
  padding: 10px 16px;
  background: #f8f9fa;
  border-bottom: 1px solid #ddd;
  display: flex;
  gap: 12px;
  flex-shrink: 0;
}

.action-btn {
  padding: 6px 14px;
  border: 1px solid #ddd;
  background: #fff;
  border-radius: 4px;
  cursor: pointer;
  font-size: 13px;
  transition: 0.2s;
}

.action-btn:hover { background: #eee; }
.action-btn.primary { background: #0052CC; color: #fff; border-color: #0052CC; }
.action-btn.primary:hover { background: #0047B3; }

.sheet-box {
  flex: 1;
  position: relative;
  overflow: hidden;
}

.data-preview {
  position: absolute;
  top: 60px;
  right: 20px;
  width: 400px;
  max-height: 80%;
  background: #fff;
  border: 1px solid #ddd;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  z-index: 1000;
}

.data-preview h3 { padding: 12px 16px; margin: 0; border-bottom: 1px solid #ddd; font-size: 15px; }
.preview-content { flex: 1; overflow: auto; padding: 16px; }
.preview-content pre { margin: 0; font-size: 12px; font-family: monospace; white-space: pre-wrap; word-wrap: break-word; }
.close-btn { margin: 12px; padding: 8px; background: #0052CC; color: #fff; border: none; border-radius: 4px; cursor: pointer; }
</style>