<template>
  <section class="taichu">
    <header class="taichu__header">
      <div class="title-row">
        <h1>太初约战场</h1>
        <small class="beta">BETA</small>
      </div>
      <p class="taichu__subtitle">企划介绍 · 上传角色 · 角色列表与约战 · 战绩</p>

      <!-- 页内导航 -->
      <nav class="tabs">
        <button class="tab" :class="{ active: activeSection === 'intro' }" @click="switchSection('intro')">
          <i class="fas fa-book-open"></i><span>企划介绍</span>
        </button>
        <button class="tab" :class="{ active: activeSection === 'upload' }" @click="switchSection('upload')">
          <i class="fas fa-upload"></i><span>上传角色</span>
        </button>
        <button class="tab" :class="{ active: activeSection === 'gallery' }" @click="switchSection('gallery')">
          <i class="fas fa-id-card"></i><span>角色列表与约战</span>
        </button>
        <button class="tab" :class="{ active: activeSection === 'history' }" @click="switchSection('history')">
          <i class="fas fa-history"></i><span>战绩</span>
        </button>
      </nav>
    </header>

    <main class="taichu__view">
      <!-- 企划介绍 -->
      <section v-show="activeSection === 'intro'" class="panel">
        <BackgroundIntrol
          @go-upload="switchSection('upload')"
          @go-gallery="switchSection('gallery')"
        />
      </section>

      <!-- 上传角色 -->
      <section v-show="activeSection === 'upload'" class="panel">
        <OC_Upload :preset-tags="presetTags" @submit="onCreatePersona" @reset="onResetPersonaForm" />
        <div class="tip">
          <i class="fas fa-lightbulb"></i>
          表单仅收集数据，提交事件将交由父层接入后端保存。
        </div>
      </section>

      <!-- 角色列表与约战 -->
      <section v-show="activeSection === 'gallery'" class="panel">
        <OC_Gallery
          :personas="personas"
          :current-user-id="CURRENT_USER.id"
          :search="filters.search"
          :tags="filters.tags"
          :preset-tags="presetTags"
          @update:search="filters.search = $event"
          @update:tags="filters.tags = $event"
          @fetch="emitFetchPersonas"
          @view="openPreview"
          @challenge="openBattleDialog"
        >
          <template #empty>暂无人设数据，请接入后端后展示</template>
        </OC_Gallery>
      </section>

      <!-- 战绩 -->
      <section v-show="activeSection === 'history'" class="panel">
        <BattleRecord
          :scope="battleScope"
          :battles="battles"
          :personas="personas"
          :current-user-id="CURRENT_USER.id"
          @update:scope="battleScope = $event"
          @fetch="emitFetchBattles"
          @update-status="onUpdateBattleStatus"
        >
          <template #empty>暂无约战数据，请接入后端后展示</template>
        </BattleRecord>
      </section>
    </main>
  </section>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'

// 与本文件同级的组件路径（根据你的实际目录调整）
import BackgroundIntrol from './components/BackgroundIntro.vue'
import OC_Upload from './components/OC_Upload.vue'
import OC_Gallery from './components/OC_Gallery.vue'
import BattleRecord from './components/BattleRecord.vue'

/**
 * 仅框架：无模拟数据
 * - 数据通过 props 注入
 * - 所有读写通过 emits 抛给父层接入后端
 */

type Section = 'intro' | 'upload' | 'gallery' | 'history'
type BattleScope = 'all' | 'incoming' | 'outgoing'
type Persona = { id: string; ownerId: string; name: string }
type ID = string

const emit = defineEmits<{
  (e: 'fetch-personas', query: { search?: string; tags?: string[] }): void
  (e: 'fetch-battles', scope: BattleScope): void
  (e: 'create-persona', payload: any): void
  (e: 'create-battle', payload: any): void
  (e: 'update-battle-status', payload: { id: ID; status: string }): void
}>()

const props = defineProps<{
  personas?: any[]
  battles?: any[]
  currentUserId?: string
  currentUserName?: string
  presetTags?: string[]
}>()

/* 当前用户（父层注入） */
const CURRENT_USER = {
  id: props.currentUserId ?? '',
  name: props.currentUserName ?? '',
}

/* 四个功能导航（默认进入企划介绍） */
const activeSection = ref<Section>('intro')
function switchSection(s: Section) {
  activeSection.value = s
  if (s === 'gallery') emitFetchPersonas()
  if (s === 'history') emitFetchBattles(battleScope.value)
}

/* 标签字典（可由父层注入） */
const presetTags = computed(() => props.presetTags ?? ['剑士', '法师', '敏捷', '近战', '控制', '远程'])

/* 数据源（完全外部注入） */
const personas = computed<any[]>(() => props.personas ?? [])
const battles = computed<any[]>(() => props.battles ?? [])

/* 角色列表筛选器（仅UI状态） */
const filters = reactive<{ search: string; tags: string[] }>({ search: '', tags: [] })
function emitFetchPersonas() {
  emit('fetch-personas', {
    search: filters.search || undefined,
    tags: filters.tags.length ? filters.tags : undefined,
  })
}
watch(
  () => [filters.search, filters.tags],
  () => {
    if (activeSection.value === 'gallery') emitFetchPersonas()
  },
  { deep: true },
)

/* 我的角色和便捷工具（供发起约战等处使用） */
const myPersonas = computed<Persona[]>(() => personas.value.filter((p: any) => p.ownerId === CURRENT_USER.id))
function personaName(id: ID | null) {
  if (!id) return '未知人设'
  return personas.value.find((p: any) => p.id === id)?.name || '未知人设'
}

/* 上传角色（透传给父层） */
function onCreatePersona(payload: any) {
  emit('create-persona', payload)
  activeSection.value = 'gallery'
}
function onResetPersonaForm() {
  /* 可按需扩展 */
}

/* 战绩（历史约战记录） */
const battleScope = ref<BattleScope>('all')
function emitFetchBattles(scope: BattleScope) {
  emit('fetch-battles', scope)
}
watch(battleScope, (s) => {
  if (activeSection.value === 'history') emitFetchBattles(s)
})
function onUpdateBattleStatus(payload: { id: ID; status: string }) {
  emit('update-battle-status', payload)
}

/* 约战弹窗/预览（后续启用可接入） */
const battleDialog = reactive<{ open: boolean; opponentPersonaId: ID | null }>({ open: false, opponentPersonaId: null })
function openBattleDialog(opponent: Persona) {
  if (opponent.ownerId === CURRENT_USER.id) return
  battleDialog.open = true
  battleDialog.opponentPersonaId = opponent.id
}
function closeBattleDialog() {
  battleDialog.open = false
  battleDialog.opponentPersonaId = null
}
function onCreateBattle(payload: any) {
  emit('create-battle', payload)
  closeBattleDialog()
  activeSection.value = 'history'
}

const preview = reactive<{ open: boolean; data: any | null }>({ open: false, data: null })
function openPreview(p: any) { preview.open = true; preview.data = p }
function closePreview() { preview.open = false; preview.data = null }

/* 初次进入时加载（intro 无需加载） */
watch(
  activeSection,
  (s) => {
    if (s === 'gallery') emitFetchPersonas()
    if (s === 'history') emitFetchBattles(battleScope.value)
  },
  { immediate: true },
)
</script>

<!-- 保持非 scoped + .taichu 前缀，确保样式可作用到子组件 -->
<style>
.taichu { color: #fff; min-height: 100%; display: flex; flex-direction: column; gap: 12px; }
.taichu .title-row { display: flex; align-items: center; gap: 8px; }
.taichu .beta { padding: 2px 6px; border-radius: 6px; font-size: 11px; border: 1px solid rgba(255,255,255,0.25); color: rgba(255,255,255,0.85); background: rgba(255,255,255,0.08); }
.taichu .taichu__header h1 { margin: 0 0 6px 0; font-size: 22px; font-weight: 700; }
.taichu .taichu__subtitle { margin: 0 0 10px; color: rgba(255,255,255,0.7); font-size: 14px; }

.taichu .tabs { display: flex; gap: 8px; flex-wrap: wrap; }
.taichu .tab { display: inline-flex; align-items: center; gap: 8px; padding: 8px 12px; border: 1px solid rgba(255,255,255,0.2); border-radius: 10px; color: rgba(255,255,255,0.9); background: rgba(255,255,255,0.04); transition: all .2s ease; cursor: pointer; }
.taichu .tab:hover { background: rgba(255,255,255,0.12); color: #fff; }
.taichu .tab.active { background: rgba(255,255,255,0.18); color: #fff; }

.taichu .panel { border: 1px solid rgba(255,255,255,0.15); border-radius: 12px; padding: 12px; background: rgba(255,255,255,0.04); }
.taichu .tip { margin-top: 10px; font-size: 12px; color: rgba(255,255,255,0.75); display: flex; align-items: center; gap: 6px; }

/* 过滤条和搜索（供 OC_Gallery 使用） */
.taichu .filter-bar { display: flex; align-items: center; gap: 12px; flex-wrap: wrap; margin-bottom: 12px; }
.taichu .search { display: inline-flex; align-items: center; gap: 8px; background: rgba(255,255,255,0.06); border: 1px solid rgba(255,255,255,0.2); padding: 6px 10px; border-radius: 8px; }
.taichu .search input { background: transparent; border: none; outline: none; color: #fff; width: 240px; }

/* 标签 chips */
.taichu .chips { display: flex; gap: 6px; flex-wrap: wrap; }
.taichu .chip { font-size: 12px; padding: 4px 8px; border-radius: 999px; cursor: pointer; background: rgba(255,255,255,0.06); border: 1px solid rgba(255,255,255,0.2); color: #fff; }
.taichu .chip.filled { background: rgba(255,255,255,0.18); }
.taichu .chip.active { background: #2b7cff; border-color: #2b7cff; }
.taichu .spacer { flex: 1; }
.taichu .view-tip { color: rgba(255,255,255,0.7); font-size: 12px; }

/* 卡片网格与卡片 */
.taichu .grid { display: grid; grid-template-columns: repeat( auto-fill, minmax(280px, 1fr) ); gap: 12px; }
.taichu .card { border: 1px solid rgba(255,255,255,0.15); border-radius: 12px; padding: 12px; display: grid; grid-template-columns: 72px 1fr; gap: 12px; background: rgba(255,255,255,0.04); }
.taichu .avatar { width: 72px; height: 72px; border-radius: 10px; overflow: hidden; background: rgba(255,255,255,0.08); display: grid; place-items: center; }
.taichu .avatar img { width: 100%; height: 100%; object-fit: cover; }
.taichu .placeholder { color: rgba(255,255,255,0.8); font-size: 20px; }

.taichu .meta .title-row { display: flex; align-items: baseline; gap: 6px; }
.taichu .meta h3 { margin: 0; font-size: 18px; }
.taichu .alias { color: rgba(255,255,255,0.7); }
.taichu .badge { margin-left: 6px; font-size: 11px; padding: 1px 6px; border-radius: 999px; background: rgba(62,201,92,0.2); border: 1px solid rgba(62,201,92,0.4); color: #c4f5d0; }
.taichu .owner { color: rgba(255,255,255,0.6); font-size: 12px; margin-top: 2px; }
.taichu .bio { color: rgba(255,255,255,0.85); font-size: 13px; margin: 4px 0; }
.taichu .tags { display: flex; gap: 6px; flex-wrap: wrap; margin-top: 6px; }
.taichu .tag { font-size: 12px; color: rgba(255,255,255,0.8); background: rgba(255,255,255,0.08); padding: 2px 6px; border-radius: 6px; }
.taichu .powers { display: flex; gap: 10px; margin-top: 8px; font-size: 12px; color: rgba(255,255,255,0.9); }
.taichu .rules { margin-top: 6px; font-size: 12px; color: rgba(255,255,255,0.75); }
.taichu .actions { display: flex; gap: 8px; align-items: center; grid-column: 1 / -1; }

.taichu .btn { background: rgba(255,255,255,0.1); color: #fff; border: 1px solid rgba(255,255,255,0.2); padding: 6px 10px; border-radius: 8px; cursor: pointer; }
.taichu .btn:hover { background: rgba(255,255,255,0.18); }
.taichu .btn.primary { background: #2b7cff; border-color: #2b7cff; }
.taichu .btn.primary:hover { filter: brightness(1.05); }
.taichu .btn.danger { background: #e24a4a; border-color: #e24a4a; }

/* 战绩列表 */
.taichu .battle-filter { display: flex; gap: 8px; align-items: center; margin-bottom: 10px; }
.taichu .list { display: flex; flex-direction: column; gap: 10px; }
.taichu .battle-item { display: grid; grid-template-columns: 1fr auto; gap: 10px; border: 1px solid rgba(255,255,255,0.15); border-radius: 10px; padding: 10px; background: rgba(255,255,255,0.04); }
.taichu .battle-item .title { display: flex; gap: 8px; align-items: center; }
.taichu .battle-item .title i { color: #ffb86c; }
.taichu .battle-item .sub { display: flex; gap: 12px; color: rgba(255,255,255,0.75); font-size: 12px; margin-top: 4px; flex-wrap: wrap; }
.taichu .battle-item .desc { color: rgba(255,255,255,0.85); font-size: 13px; margin-top: 2px; }
.taichu .status { font-size: 12px; padding: 2px 6px; border-radius: 999px; border: 1px solid rgba(255,255,255,0.2); color: rgba(255,255,255,0.95); }
.taichu .status[data-status="pending"] { background: rgba(255,255,255,0.08); }
.taichu .status[data-status="accepted"] { background: rgba(62, 201, 92, 0.25); border-color: rgba(62, 201, 92, 0.4); }
.taichu .status[data-status="rejected"] { background: rgba(226, 74, 74, 0.25); border-color: rgba(226, 74, 74, 0.4); }
.taichu .status[data-status="withdrawn"] { background: rgba(255, 184, 108, 0.25); border-color: rgba(255, 184, 108, 0.4); }

.taichu .empty { border: 1px dashed rgba(255,255,255,0.2); border-radius: 10px; padding: 24px; display: grid; justify-items: center; gap: 8px; color: rgba(255,255,255,0.8); }
</style>