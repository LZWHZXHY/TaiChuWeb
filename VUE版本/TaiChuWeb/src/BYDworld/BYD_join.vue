<template>
  <div class="BydPortal">
    <!-- 顶部：品牌 + 当前模块标题（不再放导航项） -->
    <header class="TopBar">
      <div class="TopBarInner">
        <div class="Brand">BYD Portal</div>
        <div class="Current">{{ currentLabel }}</div>
      </div>
    </header>

    <!-- 主体布局 -->
    <main class="MainGrid">
      <!-- 左侧功能列表：唯一的导航入口 -->
      <aside class="SidePanel">
        <div class="PanelTitle">功能选项</div>
        <ul class="OptionList">
          <li
            v-for="item in sections"
            :key="item.key"
            :class="{ active: activeKey === item.key }"
            @click="select(item.key)"
          >
            <span>{{ item.label }}</span>
          </li>
        </ul>
      </aside>

      <!-- 中间内容窗口：动态渲染当前组件 -->
      <section class="ViewWindow">
        <keep-alive>
          <component :is="currentComponent" />
        </keep-alive>
      </section>

      <!-- 右侧：仅展示公告等信息（去掉快速入口，避免重复） -->
      <aside class="RightPanel">
        <div class="RightCard">
          <h3>公告</h3>
          <p>这里可以放置制作须知、里程碑、常用链接等。</p>
        </div>
        <!-- <div v-if="activeKey === 'project'" class="RightCard">项目提示...</div> -->
      </aside>
    </main>
  </div>
</template>

<script setup>
import { ref, computed, defineAsyncComponent, onMounted } from 'vue'

// 子模块按需加载（保持原路径即可）
const SectionAgreement = defineAsyncComponent(() => import('./components/Agreement.vue'))
const SectionWeekly    = defineAsyncComponent(() => import('./components/WeeklyReport.vue'))
const SectionMembers   = defineAsyncComponent(() => import('./components/Participants.vue'))
const SectionProject   = defineAsyncComponent(() => import('./components/ProjectManagement.vue'))
const SectionLinks     = defineAsyncComponent(() => import('./components/Links.vue'))
const SectionUpload    = defineAsyncComponent(() => import('./BYD_production/UploadInfo.vue'))

// 单一数据源：导航项（唯一的真相来源）
const sections = [
  { key: 'agreement', label: '协议（必看）', component: SectionAgreement },
  { key: 'weekly',    label: '周报',        component: SectionWeekly },
  { key: 'members',   label: '参与成员',    component: SectionMembers },
  { key: 'project',   label: '项目管理',    component: SectionProject },
  { key: 'links',     label: '链接合集',    component: SectionLinks },
  { key: 'upload',    label: '上传通道',    component: SectionUpload },
]

// 当前激活模块
const activeKey = ref('agreement')

// 当前组件与标题
const currentComponent = computed(() => {
  return sections.find(s => s.key === activeKey.value)?.component ?? SectionAgreement
})
const currentLabel = computed(() => {
  return sections.find(s => s.key === activeKey.value)?.label ?? ''
})

// 切换并把 key 写入 URL hash，刷新/分享可保持
function select(key) {
  if (!sections.some(s => s.key === key)) return
  activeKey.value = key
  writeKeyToHash(key)
}

function readKeyFromHash() {
  try {
    const hash = window.location.hash || ''
    const idx = hash.indexOf('?')
    if (idx >= 0) {
      const params = new URLSearchParams(hash.substring(idx + 1))
      const tab = params.get('tab')
      if (tab && sections.some(s => s.key === tab)) {
        activeKey.value = tab
      }
    }
  } catch { /* ignore */ }
}

function writeKeyToHash(key) {
  try {
    const hash = window.location.hash || '#'
    const [pathPart, queryPart] = hash.split('?')
    const params = new URLSearchParams(queryPart || '')
    params.set('tab', key)
    const next = `${pathPart}?${params.toString()}`
    if (next !== hash) {
      history.replaceState(null, '', next)
    }
  } catch { /* ignore */ }
}

onMounted(() => {
  readKeyFromHash()
})
</script>

<style scoped>
.BydPortal {
  --bg: #f7f9fc;
  --card: #ffffff;
  --text: #1f2937;
  --muted: #6b7280;
  --brand: #2563eb;
  --brand-weak: #dbeafe;

  background: var(--bg);

  /* 高度上下文：保证中间区域可以滚动 */
  height: 100dvh; /* 或 100vh */
  min-height: 0;

  display: flex;
  flex-direction: column;
}

/* 顶部：仅展示品牌与当前模块标题 */
.TopBar {
  position: sticky;
  top: 0;
  z-index: 100;
  background: linear-gradient(90deg, #616161 0%, #505050 100%);
  box-shadow: 0 2px 8px rgba(31, 41, 55, 0.18);
}
.TopBarInner {
  height: 56px;
  padding: 0 20px;
  display: grid;
  grid-template-columns: auto 1fr;
  align-items: center;
  gap: 16px;
}
.Brand {
  color: #ffe082;
  font-weight: 800;
  letter-spacing: .6px;
}
.Current {
  color: #f5f5f5;
  opacity: .9;
  font-weight: 600;
}

/* 三列布局 */
.MainGrid {
  display: grid;
  grid-template-columns: 240px 1fr 300px;
  gap: 16px;
  padding: 20px;

  /* 占据 TopBar 之外的剩余空间 */
  flex: 1;

  /* 关键：允许子项在剩余空间内收缩，保证内部滚动生效 */
  min-height: 0;
}

.SidePanel,
.RightPanel {
  background: var(--card);
  border-radius: 16px;
  box-shadow: 0 1px 6px rgba(31, 41, 55, 0.08);
  padding: 14px 12px;
  display: flex;
  flex-direction: column;

  /* 允许在父容器中收缩，避免挤压中间滚动 */
  min-height: 0;
  overflow: hidden; /* 如需侧栏也可滚动，改为 overflow-y: auto; */
}

.PanelTitle {
  text-align: center;
  font-size: 15px;
  font-weight: 700;
  color: var(--brand);
  margin-bottom: 10px;
  letter-spacing: .5px;
}

.OptionList {
  list-style: none;
  margin: 0;
  padding: 0;
}

.OptionList li {
  padding: 10px 12px;
  border-radius: 8px;
  font-size: 14px;
  color: var(--text);
  cursor: pointer;
  transition: background .18s, color .18s;
}

.OptionList li:hover { background: var(--brand-weak); color: var(--brand); }
.OptionList li.active { background: var(--brand-weak); color: var(--brand); font-weight: 700; }

/* 中间视图窗口：作为唯一滚动容器 */
.ViewWindow {
  background: #ffffff;
  border-radius: 16px;
  box-shadow: 0 2px 12px rgba(31, 41, 55, 0.10);
  padding: 20px;

  /* 关键两行：允许在父容器空间内收缩 + 开启垂直滚动 */
  min-height: 0;
  overflow-y: auto;
}

/* 右侧卡片 */
.RightCard {
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 1px 8px rgba(31, 41, 55, 0.08);
  padding: 12px;
  margin-bottom: 12px;
}
.RightCard h3 {
  margin: 0 0 8px 0;
  font-size: 15px;
  color: var(--text);
}

/* 响应式 */
@media (max-width: 1100px) {
  .MainGrid { grid-template-columns: 220px 1fr; }
  .RightPanel { display: none; }
}

@media (max-width: 768px) {
  .MainGrid {
    grid-template-columns: 1fr;
    min-height: 0; /* 继续保证内部可滚动 */
  }
  .SidePanel { order: 1; }
  .ViewWindow { order: 2; min-height: 0; }
}
</style>