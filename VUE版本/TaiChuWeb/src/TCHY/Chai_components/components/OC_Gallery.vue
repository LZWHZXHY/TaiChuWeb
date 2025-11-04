<template>
  <section class="oc-persona-list">
    <div class="filter-bar">
      <div class="search">
        <i class="fas fa-search" aria-hidden="true"></i>
        <input
          v-model="localSearch"
          type="text"
          placeholder="搜索人设名称 / 简介 / 标签"
          @keyup.enter="fetchList"
          aria-label="搜索人设"
        />
      </div>

      <div class="chips" role="list">
        <button
          v-for="t in presetTags"
          :key="t"
          class="chip"
          :class="{ active: localTags.includes(t) }"
          @click.prevent="toggleTag(t)"
          type="button"
          role="listitem"
          :aria-pressed="localTags.includes(t)"
        >
          #{{ t }}
        </button>
      </div>

      <div class="spacer"></div>

      <div class="view-tip">
        <template v-if="loading">加载中…</template>
        <template v-else>共 {{ total }} 个公开人设</template>
      </div>
    </div>

    <div class="grid">
      <article
        v-for="p in personas"
        :key="p.id"
        class="card"
        :class="{ 'card-has-media': p.avatarUrl || p.weaponImg }"
        role="article"
        :aria-label="p.name"
      >
        <div class="card-inner">
          <!-- CONTENT COLUMN (left) -->
          <div class="card-content">
            <div class="content-panel">
              <div class="title-row">
                <h3 class="name" :title="p.name">{{ p.name }}</h3>
                <span v-if="p.alias" class="alias">/{{ p.alias }}</span>
                <span v-if="isMine(p)" class="badge">我的</span>
              </div>

              <div class="owner" v-if="p.ownerName">作者：@{{ p.ownerName }}</div>

              <div
                class="basic"
                v-if="p.age !== undefined || p.gender !== undefined || p.POO || p.OC_Current_Time !== undefined"
              >
                <span v-if="p.age !== undefined && p.age !== null" class="meta-item">年龄：{{ p.age }}</span>
                <span v-if="p.gender !== undefined && p.gender !== null" class="meta-item">性别：{{ genderLabel(p.gender) }}</span>
                <span v-if="p.POO" class="meta-item">POO：{{ p.POO }}</span>
                <span v-if="p.OC_Current_Time !== undefined && p.OC_Current_Time !== null" class="meta-item">世界时间：{{ p.OC_Current_Time }}</span>
              </div>

              <p v-if="p.background" class="bio clamp-3"><strong>背景：</strong> <span class="text-block">{{ p.background }}</span></p>
              <p v-if="p.character" class="bio clamp-2"><strong>性格：</strong> <span class="text-block">{{ p.character }}</span></p>

              <div v-if="p.OC_WeapenDesc" class="weapon-desc"><strong>武器描述：</strong> <span class="text-block">{{ p.OC_WeapenDesc }}</span></div>

              <div v-if="p.tags && p.tags.length" class="tags" aria-hidden="false">
                <span v-for="t in p.tags" :key="t" class="tag">#{{ t }}</span>
              </div>

              <div v-if="p.ability" class="ability"><strong>能力描述：</strong> <span class="text-block">{{ p.ability }}</span></div>

              <div v-if="p.experience && p.experience.length" class="experience"><strong>经历：</strong> <span class="text-block">{{ p.experience.join(', ') }}</span></div>

              <div v-if="p.ExtraDesc" class="extra"><strong>额外说明：</strong> <span class="text-block">{{ p.ExtraDesc }}</span></div>
            </div>

            <div class="card-actions">
              <button class="btn" type="button" @click="$emit('view', p)">
                <i class="fas fa-eye" aria-hidden="true"></i> 查看
              </button>
              <button class="btn primary" type="button" :disabled="isMine(p)" @click="$emit('challenge', p)">
                <i class="fas fa-swords" aria-hidden="true"></i> 约战
              </button>
            </div>
          </div> <!-- /.card-content -->

          <!-- MEDIA COLUMN (right, only if exists) -->
          <div v-if="p.avatarUrl || p.weaponImg" class="card-media" aria-hidden="false">
            <div v-if="p.avatarUrl" class="media-block portrait">
              <div class="label">立绘</div>
              <img :src="p.avatarUrl" class="media-img" alt="立绘" loading="lazy" />
            </div>

            <div v-if="p.weaponImg" class="media-block weapon">
              <div class="label">武器图</div>
              <img :src="p.weaponImg" class="media-img" alt="武器图" loading="lazy" />
            </div>
          </div>
        </div> <!-- /.card-inner -->
      </article>
    </div>

    <div v-if="!loading && !personas.length" class="empty">
      <i class="fas fa-box-open" aria-hidden="true"></i>
      <span><slot name="empty">暂无人设数据</slot></span>
    </div>

    <div class="pager" v-if="total > pageSize">
      <button :disabled="page <= 1 || loading" @click="changePage(page - 1)">上一页</button>
      <span>第 {{ page }} 页 / 共 {{ Math.max(1, Math.ceil(total / pageSize)) }} 页</span>
      <button :disabled="page >= Math.ceil(total / pageSize) || loading" @click="changePage(page + 1)">下一页</button>
    </div>

    <div v-if="error" class="error">{{ error }}</div>
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import request, { baseApiUrl } from '../../../../server/UserInfoApi'

type Persona = {
  id: string
  ownerId: string
  ownerName?: string | null
  name: string
  alias?: string | null
  avatarUrl?: string | null
  weaponImg?: string | null
  background?: string | null
  character?: string | null
  tags?: string[]
  OC_WeapenDesc?: string | null
  OC_Current_TIME?: number | null
  OC_Current_Time?: number | null
  POO?: string | null
  ExtraDesc?: string | null
  ability?: string | null
  experience?: string[] | null
  age?: number | null
  gender?: number | null
}

// props/events/state
const props = defineProps<{
  currentUserId?: string | null
  presetTags?: string[] | null
  initialSearch?: string | null
  initialTags?: string[] | null
  initialPage?: number | null
  initialPageSize?: number | null
}>()

const emit = defineEmits<{
  (e: 'view', p: Persona): void
  (e: 'challenge', p: Persona): void
  (e: 'fetched', payload: { total: number; page: number; pageSize: number }): void
}>()

const localSearch = ref(props.initialSearch ?? '')
const localTags = ref<string[]>(Array.isArray(props.initialTags) ? props.initialTags.slice() : [])
const presetTags = ref(props.presetTags ?? [])
const page = ref(props.initialPage ?? 1)
const pageSize = ref(props.initialPageSize ?? 24)

const personas = ref<Persona[]>([])
const total = ref(0)
const loading = ref(false)
const error = ref<string | null>(null)

function isMine(p: Persona) { 
  return !!props.currentUserId && p.ownerId === props.currentUserId 
}
function genderLabel(g?: number | null) { 
  if (g === 0) return '男'
  if (g === 1) return '女' 
  return '未知' 
}
function tagsParam() { 
  return localTags.value.length ? localTags.value.join(',') : undefined 
}

function parseTags(raw: unknown): string[] {
  if (!raw) return []
  if (Array.isArray(raw)) return raw.map((x) => String(x).trim()).filter(Boolean)
  const s = String(raw).trim()
  if (!s) return []
  if ((s.startsWith('[') && s.endsWith(']')) || s.startsWith('{')) {
    try { 
      const parsed = JSON.parse(s)
      if (Array.isArray(parsed)) return parsed.map((x) => String(x).trim()).filter(Boolean) 
    } catch {}
  }
  return s.split(/[,; ]+/).map((t) => t.trim()).filter(Boolean)
}

function tryParseJsonArrayAsStringList(raw: string): string[] {
  try { 
    const parsed = JSON.parse(raw)
    if (Array.isArray(parsed)) return parsed.map((v) => String(v)) 
  } catch {}
  return raw.split(/[,;|\n]+/).map((s) => s.trim()).filter(Boolean)
}

function extractAbilityText(raw: string): string {
  if (!raw) return ''
  const s = raw.trim()
  if ((s.startsWith('{') && s.endsWith('}')) || s.startsWith('["') || s.startsWith('[')) {
    try {
      const parsed = JSON.parse(s)
      if (parsed && typeof parsed === 'object') {
        if (Array.isArray(parsed)) return parsed.join(', ')
        if (parsed.freeText) return String(parsed.freeText)
        return String(parsed)
      }
    } catch {}
  }
  return raw
}

function toProxyUrl(rel?: string | null): string | null {
  if (!rel) return null
  const s = rel.trim()
  try { new URL(s); return s } catch {}
  if (s.includes('/imageproxy')) {
    const base = (baseApiUrl ?? '').replace(/\/+$/, '')
    if (!base) return s.startsWith('/') ? s : `/${s}`
    return s.startsWith('/') ? `${base}${s}` : `${base}/${s}`
  }
  const normalized = s.replace(/^\/+/, '')
  const base = (baseApiUrl ?? '').replace(/\/+$/, '')
  if (!base) return `/api/OCBattle/imageproxy?path=${encodeURIComponent(normalized)}`
  return `${base}/api/OCBattle/imageproxy?path=${encodeURIComponent(normalized)}`
}

async function fetchList() {
  loading.value = true
  error.value = null
  try {
    const params: any = { page: page.value, pageSize: pageSize.value }
    if (localSearch.value) params.search = localSearch.value
    const tp = tagsParam()
    if (tp) params.tags = tp

    const resp = await request.get('/api/OCBattle/list', { params })
    const data = resp.data
    total.value = Number(data.total ?? 0)
    page.value = Number(data.page ?? page.value)
    pageSize.value = Number(data.pageSize ?? pageSize.value)

    const items = Array.isArray(data.items) ? data.items : []
    personas.value = items.map((x: any) => ({
      id: String(x.id ?? ''),
      ownerId: String(x.ownerId ?? x.authorID ?? ''),
      ownerName: x.authorName ?? x.ownerName ?? null,
      name: x.name ?? '',
      alias: x.alias ?? null,
      avatarUrl: toProxyUrl(x.avatarUrl ?? x.avatar ?? x.OC_image_url ?? null),
      weaponImg: toProxyUrl(x.weaponImg ?? x.OC_WeapenImgUrl ?? null),
      background: x.background ?? x.bio ?? null,
      character: x.character ?? null,
      tags: Array.isArray(x.tags) ? x.tags : parseTags(x.tags ?? x.colors),
      OC_WeapenDesc: x.OC_WeapenDesc ?? x.weaponDesc ?? null,
      OC_Current_Time: x.OC_Current_Time ?? null,
      POO: x.POO ?? null,
      ExtraDesc: x.ExtraDesc ?? null,
      ability: (typeof x.ability === 'string') ? extractAbilityText(x.ability) : (x.ability?.freeText ?? null),
      experience: Array.isArray(x.experience) ? x.experience.map((v: any) => String(v)) : (typeof x.experience === 'string' ? tryParseJsonArrayAsStringList(x.experience) : []),
      age: x.age ?? null,
      gender: (x.gender === undefined ? x.Gender ?? null : x.gender)
    }))

    emit('fetched', { total: total.value, page: page.value, pageSize: pageSize.value })
  } catch (err: any) {
    console.error('获取人设列表失败:', err)
    error.value = err.response?.data?.message || err.message || '获取数据失败'
    personas.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

function changePage(newPage: number) {
  if (newPage < 1 || newPage > Math.ceil(total.value / pageSize.value)) return
  page.value = newPage
  fetchList()
}

function toggleTag(tag: string) {
  const index = localTags.value.indexOf(tag)
  if (index > -1) {
    localTags.value.splice(index, 1)
  } else {
    localTags.value.push(tag)
  }
  page.value = 1
  fetchList()
}

onMounted(() => {
  fetchList()
})

let searchTimeout: number | undefined
watch(localSearch, (newVal, oldVal) => {
  if (newVal !== oldVal) {
    if (searchTimeout) window.clearTimeout(searchTimeout)
    searchTimeout = window.setTimeout(() => {
      page.value = 1
      fetchList()
    }, 300)
  }
})

watch(localTags, () => {
  page.value = 1
  fetchList()
}, { deep: true })
</script>

<style scoped>
/* Container */
.oc-persona-list {
  font-family: Inter, system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", Arial;
  color: #e6eef6;
  padding: 12px;
}

/* Filter bar */
.filter-bar {
  display: flex;
  gap: 12px;
  align-items: center;
  margin-bottom: 12px;
}
.search input {
  padding: 8px 10px;
  border-radius: 8px;
  border: 1px solid rgba(255,255,255,0.12);
  background: rgba(255,255,255,0.04);
  color: #fff;
}

/* Grid */
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(520px, 1fr));
  gap: 18px;
}

/* Card - glass background */
.card {
  position: relative;
  border-radius: 12px;
  overflow: hidden;
  /* soft border */
  border: 1px solid rgba(255,255,255,0.06);
  /* glass effect */
  background: linear-gradient(135deg, rgba(255,255,255,0.04), rgba(255,255,255,0.02));
  backdrop-filter: blur(10px) saturate(120%);
  -webkit-backdrop-filter: blur(10px) saturate(120%);
  padding: 14px;
  box-shadow: 0 6px 20px rgba(2,6,23,0.4);
}

/* Inner layout: content left, media right when there is media */
.card-inner {
  display: grid;
  gap: 14px;
  align-items: start;
  grid-template-columns: 1fr;
}

/* when card has media, use two columns: content (flexible) on left, media fixed on right */
.card-has-media .card-inner {
  grid-template-columns: 1fr 200px; /* content left, media right */
}

/* Card content: improve readability with a dark translucent panel */
.card-content {
  display: flex;
  flex-direction: column;
  gap: 10px;
  min-width: 0; /* allow proper truncation inside grid */
}

/* content panel - stronger contrast for text */
.content-panel {
  background: rgba(6, 8, 12, 0.72); /* darker translucent layer */
  padding: 14px;
  border-radius: 10px;
  color: #eef6ff;
  line-height: 1.6;
  box-shadow: inset 0 1px 0 rgba(255,255,255,0.02);
  max-height: 420px;
  overflow: auto;
}

/* make long text wrap normally (avoid vertical stacking) */
.content-panel .bio,
.content-panel .text-block,
.content-panel .ability,
.content-panel .experience,
.content-panel .weapon-desc,
.content-panel .extra {
  white-space: normal;
  word-break: break-word;
  color: #e6eef6;
  margin: 0 0 8px 0;
  font-size: 14px;
}

/* Title row */
.title-row {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: nowrap;
}
.name {
  margin: 0;
  font-size: 18px;
  font-weight: 700;
  color: #fff;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: calc(100% - 140px); /* leave room for badge/alias */
}
.alias {
  color: rgba(230,238,246,0.85);
  font-size: 13px;
}
.badge {
  background: rgba(255,255,255,0.06);
  color: #cde9ff;
  padding: 4px 8px;
  border-radius: 8px;
  font-size: 12px;
}

/* meta row */
.basic {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  color: rgba(230,238,246,0.9);
  font-size: 13px;
}

/* tags */
.tags {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}
.tag {
  display: inline-block;
  padding: 6px 10px;
  border-radius: 12px;
  background: rgba(14,165,233,0.12);
  color: #8be0ff;
  font-weight: 600;
  font-size: 13px;
}

/* card actions */
.card-actions {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
  margin-top: 6px;
}
.btn {
  padding: 8px 12px;
  border-radius: 8px;
  border: 1px solid rgba(255,255,255,0.06);
  background: rgba(255,255,255,0.02);
  color: #eef6ff;
  cursor: pointer;
}
.btn.primary {
  background: linear-gradient(90deg,#0ea5e9,#0891b2);
  border: none;
  color: #fff;
}

/* MEDIA column styling */
.card-media {
  display: flex;
  flex-direction: column;
  gap: 10px;
  align-items: center;
  justify-content: flex-start;
  padding-left: 12px;
}
.media-block {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 8px;
  align-items: center;
}
.media-block .label {
  font-size: 12px;
  color: rgba(230,238,246,0.8);
}
.media-img {
  width: 100%;
  height: 240px;
  object-fit: cover;
  border-radius: 10px;
  border: 1px solid rgba(255,255,255,0.04);
  box-shadow: 0 6px 18px rgba(2,6,23,0.45);
}

/* Responsive: stack on narrow screens */
@media (max-width: 960px) {
  .card-has-media .card-inner {
    grid-template-columns: 1fr;
  }
  .media-img {
    height: 200px;
  }
  .grid {
    grid-template-columns: 1fr;
  }
}

/* small screens */
@media (max-width: 520px) {
  .media-img {
    height: 160px;
  }
  .name { font-size: 16px; }
  .content-panel { padding: 12px; }
}

/* truncation helpers */
.clamp-3 {
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>