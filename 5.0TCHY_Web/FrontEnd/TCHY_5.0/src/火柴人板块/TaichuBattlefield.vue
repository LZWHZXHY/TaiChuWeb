<template>
  <div class="battle">
    <!-- 顶部标题区 -->
    <header class="page-hero">
      <h1 class="title">太初约战场</h1>
      <p class="subtitle">通过选项卡在各模块间进行切换：介绍 / 人设上传 / 人设列表 / 战斗记录</p>
    </header>

    <!-- 选项卡导航（仅显示一个模块，避免占用空间） -->
    <nav
      class="tabs"
      role="tablist"
      aria-label="太初约战场模块"
      @keydown.left.prevent="move(-1)"
      @keydown.right.prevent="move(1)"
    >
      <button
        v-for="t in tabs"
        :key="t.key"
        role="tab"
        type="button"
        class="tab"
        :id="`tab-${t.key}`"
        :aria-controls="`panel-${t.key}`"
        :aria-selected="active === t.key"
        :tabindex="active === t.key ? 0 : -1"
        :class="{ active: active === t.key }"
        @click="setTab(t.key)"
      >
        <span class="idx">{{ t.idx }}</span>
        <span class="label">{{ t.label }}</span>
      </button>
    </nav>

    <!-- 主内容区：仅渲染当前选中的一个模块 -->
    <section
      class="panel"
      role="tabpanel"
      :id="`panel-${active}`"
      :aria-labelledby="`tab-${active}`"
    >
      <Transition name="fade-slide" mode="out-in">
        <KeepAlive>
          <component :is="currentComp" :key="active" />
        </KeepAlive>
      </Transition>
    </section>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'

// 子模块组件（已拆分成4个独立文件，无模拟数据）
import BattlefieldIntro from './components/Intro.vue'
import BattlefieldUpload from './components/Upload.vue'
import BattlefieldRoster from './components/Roster.vue'
import BattlefieldRecords from './components/Records.vue'

// 选项卡配置
const tabs = [
  { key: 'intro',   label: '约战场介绍', idx: 1, comp: BattlefieldIntro },
  { key: 'upload',  label: '人设上传',   idx: 2, comp: BattlefieldUpload },
  { key: 'roster',  label: '人设列表',   idx: 3, comp: BattlefieldRoster },
  { key: 'records', label: '战斗记录',   idx: 4, comp: BattlefieldRecords }
]

const active = ref('intro')

const currentComp = computed(() => {
  const found = tabs.find(t => t.key === active.value)
  return found ? found.comp : BattlefieldIntro
})

// 切换选项卡
function setTab(key) {
  if (tabs.some(t => t.key === key)) {
    active.value = key
  }
}

// 键盘左右方向键切换
function move(delta) {
  const i = tabs.findIndex(t => t.key === active.value)
  if (i === -1) return
  const nxt = (i + delta + tabs.length) % tabs.length
  setTab(tabs[nxt].key)
}

// 初始根据 URL hash 定位（如 #upload）
onMounted(() => {
  const hash = (location.hash || '').replace('#', '')
  if (tabs.some(t => t.key === hash)) {
    active.value = hash
  } else {
    // 也可读取上次选中的模块（可选）
    const last = sessionStorage.getItem('taichu_tab')
    if (tabs.some(t => t.key === last)) active.value = last
  }
})

// 同步 URL hash 与临时存储
watch(active, (v) => {
  try {
    history.replaceState(null, '', `#${v}`)
    sessionStorage.setItem('taichu_tab', v)
  } catch {}
})
</script>

<style scoped>
:root {
  --bg: #f7f9fc;
  --ink: #0f172a;
  --muted: #5a6478;
  --card: #ffffff;
  --ring: #e6ebf3;
  --ring-strong: #d6deea;
  --brand: #2563eb;
  --radius-lg: 16px;
  --radius-md: 12px;
  --shadow-sm: 0 2px 10px rgba(2,6,23,.06);
  --shadow-md: 0 12px 34px rgba(2,6,23,.08);
}

.battle {
  background: var(--bg);
  color: var(--ink);
  min-height: 100%;
  padding: clamp(14px, 3vw, 20px);
  display: grid;
  gap: 12px;
}

/* 顶部标题 */
.page-hero {
  text-align: center;
}
.title {
  margin: 0;
  font-weight: 900;
  font-size: clamp(20px, 2.6vw, 26px);
}
.subtitle {
  margin: 6px 0 0;
  color: var(--muted);
}

/* 选项卡（粘顶部，单行滚动，不占空间） */
.tabs {
  position: sticky;
  top: 0;
  z-index: 5;
  background: var(--card);
  border: 1px solid var(--ring);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
  padding: 6px;
  display: grid;
  grid-auto-flow: column;
  grid-auto-columns: max-content;
  gap: 6px;
  overflow-x: auto;
}
.tabs::-webkit-scrollbar { height: 8px; }
.tabs::-webkit-scrollbar-thumb { background: #e5eaf2; border-radius: 8px; }

.tab {
  display: grid;
  grid-auto-flow: column;
  grid-auto-columns: max-content 1fr;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: 10px;
  border: 1px solid transparent;
  background: #fff;
  color: var(--ink);
  font-weight: 800;
  cursor: pointer;
  transition: background .15s ease, border-color .15s ease;
  white-space: nowrap;
}
.tab .idx {
  display: inline-block;
  min-width: 22px;
  height: 22px;
  line-height: 22px;
  border-radius: 999px;
  text-align: center;
  background: #eef4ff;
  color: #1e3a8a;
  font-size: 12px;
}
.tab:hover { background: #f7faff; border-color: var(--ring); }
.tab.active {
  background: #eef4ff;
  border-color: #d9e6ff;
}

/* 主内容容器 */
.panel {
  background: var(--card);
  border: 1px solid var(--ring);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  padding: 10px;
}

/* 动画 */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: opacity .2s ease, transform .2s ease;
}
.fade-slide-enter-from,
.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(6px);
}
</style>