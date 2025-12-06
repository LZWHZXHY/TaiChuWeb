<template>
  <div class="glass-page">
    <header class="topbar container" role="banner">
      <div class="brand">
        <h1 class="title">太初绘画展厅</h1>
        <p class="tagline muted">玻璃质感 · 强化毛玻璃层次 · 瀑布流展示</p>
      </div>

      <div class="controls" role="region" aria-label="搜索与筛选">
        <input
          v-model="query"
          @input="onQuery"
          class="search"
          type="search"
          placeholder="搜索 标题 / 作者 / 标签…"
          aria-label="搜索"
        />

        <select v-model="filterTag" @change="applyFilters" aria-label="按标签筛选" class="select">
          <option value="">所有标签</option>
          <option v-for="t in tags" :key="t" :value="t">{{ t }}</option>
        </select>

        <select v-model="sortMode" @change="onSortChange" aria-label="排序" class="select">
          <option value="new">最新</option>
          <option value="hot">最热</option>
          <option value="alpha">标题 A→Z</option>
          <option value="random">随机</option>
        </select>

        <button class="btn subtle" @click="clearFilters" aria-label="清除筛选">清除</button>
      </div>
    </header>

    <main class="container main-grid" role="main">
      <!-- Gallery -->
      <section class="gallery" aria-live="polite" aria-label="作品画廊">
        <div class="gallery-head">
          <h2>瀑布展示</h2>
          <p class="muted">向下滚动加载更多 · 新上传即时置顶</p>
        </div>

        <section class="masonry" role="list">
          <article
            v-for="(it, idx) in visiblePaged"
            :key="it.id"
            class="card"
            role="listitem"
            tabindex="0"
            @click="openPreviewByIndex(idx)"
            @keydown.enter.prevent="openPreviewByIndex(idx)"
            :style="cardStyle(idx)"
          >
            <div class="frost-plate" aria-hidden="true"></div>

            <div class="panel-surface">
              <div class="glass-frame">
                <img class="thumb" :src="it.src" :alt="it.title || '作品图像'" loading="lazy" />
              </div>

              <div class="caption">
                <div class="left">
                  <strong class="card-title" :title="it.title">{{ it.title || '无标题' }}</strong>
                  <small class="muted author">by {{ it.author || '匿名' }}</small>
                  <div v-if="it.desc" class="card-desc" :title="it.desc">{{ truncate(it.desc, 80) }}</div>
                </div>
                <div class="right">
                  <span class="likes" aria-hidden="true">❤ {{ it.likes }}</span>
                </div>
              </div>
            </div>
          </article>

          <div ref="sentinel" class="sentinel" aria-hidden="true"></div>
        </section>

        <div class="more" v-if="hasMore">
          <button class="btn" @click="loadMore" :disabled="loading">加载更多</button>
        </div>
        <div class="more" v-else>
          <small class="muted">没有更多了</small>
        </div>
      </section>

      <!-- Aside: Upload & tools -->
      <aside class="aside" aria-label="上传与工具">
        <div class="panel upload" role="region" aria-label="上传图片">
          <h3>上传图片</h3>
          <p class="muted">拖放或点击选择（支持多张）</p>

          <div class="meta-inputs">
            <label>
              <small class="muted">作者姓名（必填）</small>
              <input v-model="authorName" type="text" class="meta-text" placeholder="作者姓名" />
            </label>
            <label>
              <small class="muted">作品描述（必填）</small>
              <textarea v-model="desc" class="meta-textarea" placeholder="作品描述"></textarea>
            </label>
          </div>

          <label class="dropzone" for="files" @dragover.prevent="onDragOver" @drop.prevent="onDrop">
            <input id="files" ref="fileInput" class="file" type="file" accept="image/*" multiple @change="onFiles" />
            <div class="dz-inner">
              <svg class="icon" viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M5 20h14v-2H5v2zm7-16l5 5h-3v4h-4V9H7l5-5z"/></svg>
              <span>拖放图片或点击选择</span>
            </div>
          </label>

          <div class="pending" v-if="pending.length">
            <strong>预览</strong>
            <div class="previews">
              <div v-for="p in pending" :key="p.id" class="mini">
                <img :src="p.src" :alt="p.name" />
                <button class="tiny" @click="removePending(p.id)" aria-label="移除">✕</button>
              </div>
            </div>

            <div class="actions">
              <button class="btn primary" @click="commitPending" :disabled="uploading">发布到画廊</button>
              <button class="btn subtle" @click="clearPending" :disabled="uploading">清空</button>
            </div>
          </div>

          <p class="muted small">作者姓名与作品描述为后端必填；上传时会把这两项随每个文件一并发送。</p>
        </div>

        <div class="panel tools">
          <h4>快速操作</h4>
          <div class="chips">
            <button class="chip" @click="setSort('new')">最新</button>
            <button class="chip" @click="setSort('hot')">最热</button>
            <button class="chip" @click="shuffle">随机</button>
          </div>

          <div class="stats">
            <div>总数 <strong>{{ total }}</strong></div>
            <div>显示 <strong>{{ visiblePaged.length }}</strong></div>
          </div>
        </div>
      </aside>
    </main>

    <!-- Independent modal preview -->
    <div v-if="isPreviewOpen" class="modal-overlay" role="dialog" aria-modal="true" aria-label="作品预览" @click.self="closePreview">
      <div class="modal-content" role="document">
        <button class="modal-close" @click="closePreview" aria-label="关闭预览">✕</button>

        <div class="modal-body">
          <div class="modal-image-wrap">
            <button v-if="hasPrev" class="modal-nav left" @click="showPrev" aria-label="上一张">‹</button>
            <img class="modal-image" :src="preview?.src" :alt="preview?.title || '预览图像'" />
            <button v-if="hasNext" class="modal-nav right" @click="showNext" aria-label="下一张">›</button>
          </div>

          <aside class="modal-info">
            <h3 class="modal-title">{{ preview?.title || '无标题' }}</h3>
            <p class="muted">作者：{{ preview?.author || '匿名' }}</p>
            <p class="muted" v-if="preview?.uploadAt">上传时间：{{ formatUploadAt(preview.uploadAt) }}</p>
            <p v-if="previewDesc" class="desc">{{ previewDesc }}</p>

            <div class="modal-actions">
              <button class="btn" @click="downloadPreview">下载</button>
              <button class="btn primary" @click="likePreview">❤ {{ preview?.likes ?? 0 }}</button>
            </div>
          </aside>
        </div>
      </div>
    </div>

    <footer class="footer container">
      <small class="muted">© 2025 毛玻璃画廊</small>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import apiClient from '@/utils/api'

type Item = {
  id: number | string
  title?: string
  author?: string
  likes: number
  src: string
  tags?: string[]
  uploadAt?: string
  desc?: string | null
}

/* state */
const items = ref<Item[]>([])
const query = ref('')
const filterTag = ref('')
const sortMode = ref<'new'|'hot'|'alpha'|'random'>('new')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const hasMore = ref(true)
const loading = ref(false)
const pending = ref<{ id: string; src: string; name: string; file: File }[]>([])
const fileInput = ref<HTMLInputElement | null>(null)
const sentinel = ref<HTMLElement | null>(null)
const observer = ref<IntersectionObserver | null>(null)
const previewIndex = ref<number | null>(null)
const preview = computed(() => previewIndex.value !== null ? items.value[previewIndex.value] : null)
const uploading = ref(false)
const authorName = ref('')
const desc = ref('')
const nextCursor = ref<string | null>(null)

/* Allow use of Vite env (ensure jsconfig/module set to ESNext) */
const backendBase = (import.meta.env.VITE_BACKEND_BASE as string) || window.location.origin

/* helpers */
const normalizeUrl = (u?: string | null) => {
  if (!u) return ''
  const s = u.trim()
  if (/^http:\/\//i.test(s)) return s.replace(/^http:\/\//i, 'https://')
  return s
}
const buildFullImageUrl = (relativeOrFull: string | null | undefined) => {
  if (!relativeOrFull) return ''
  if (/^https?:\/\//i.test(relativeOrFull)) return normalizeUrl(relativeOrFull)
  const rel = relativeOrFull.replace(/^\/+/, '')
  return normalizeUrl(`${backendBase.replace(/\/$/, '')}/uploads/${rel}`)
}

/* map server item -> client Item (ensures src and desc) */
const mapServerItem = (it: any): Item => {
  const srcRaw = it.imageUrlFull ?? it.imageUrl ?? it.src ?? ''
  return {
    id: it.id,
    title: it.title,
    author: it.authorName ?? it.author ?? '匿名',
    likes: it.likes ?? 0,
    src: buildFullImageUrl(srcRaw),
    tags: it.tags ?? [],
    uploadAt: it.uploadAt,
    desc: it.desc ?? it.description ?? null
  }
}

/* derived */
const tags = computed(() => {
  const s = new Set<string>()
  items.value.forEach(i => (i.tags || []).forEach(t => s.add(t)))
  return Array.from(s)
})

const visible = computed(() => {
  const q = query.value.trim().toLowerCase()
  let arr = items.value.slice()
  if (filterTag.value) arr = arr.filter(it => (it.tags || []).includes(filterTag.value))
  if (q) arr = arr.filter(it => (it.title || '').toLowerCase().includes(q) || (it.author || '').toLowerCase().includes(q))
  if (sortMode.value === 'hot') arr = arr.sort((a,b)=>b.likes - a.likes)
  else if (sortMode.value === 'alpha') arr = arr.sort((a,b)=> (a.title||'').localeCompare(b.title||''))
  else if (sortMode.value === 'random') arr = arr.sort(()=>Math.random() - 0.5)
  return arr
})

const visiblePaged = computed(() => visible.value.slice(0, pageSize.value * page.value))

/* utilities */
const truncate = (s?: string | null, n = 80) => {
  if (!s) return ''
  return s.length <= n ? s : s.slice(0, n - 1) + '…'
}

/* fetch & pagination (keyset cursor for new) */
const fetchListInitial = async () => {
  nextCursor.value = null
  items.value = []
  page.value = 1
  await fetchList({ cursor: null, replace: true, pageNum: 1 })
}

const fetchList = async (options: { cursor?: string | null, replace?: boolean, pageNum?: number } = {}) => {
  if (loading.value) return
  loading.value = true
  try {
    const { cursor = null, replace = false, pageNum = page.value } = options
    let url = ''
    if (sortMode.value === 'new') {
      url = `/Drawing/list?pageSize=${pageSize.value}&sort=new`
      if (cursor) url += `&cursor=${encodeURIComponent(cursor)}`
    } else {
      url = `/Drawing/list?page=${pageNum}&pageSize=${pageSize.value}&sort=${encodeURIComponent(sortMode.value)}`
    }

    const res = await apiClient.get(url)
    const payload = res?.data
    if (payload?.success && payload.data) {
      const serverItems = (payload.data.items || []).map(mapServerItem)
      if (replace) items.value = serverItems
      else items.value.push(...serverItems)

      if (payload.data.nextCursor !== undefined) {
        nextCursor.value = payload.data.nextCursor ?? null
        hasMore.value = !!payload.data.hasMore
      } else {
        total.value = payload.data.totalCount ?? total.value
        const totalPages = payload.data.totalPages ?? Math.ceil((total.value || 0) / pageSize.value)
        hasMore.value = pageNum < totalPages
        nextCursor.value = null
      }

      page.value = pageNum
    } else {
      if (replace) items.value = []
      hasMore.value = false
      nextCursor.value = null
    }
  } catch (err) {
    console.error('fetchList error', err)
  } finally {
    loading.value = false
  }
}

const loadMore = () => {
  if (!hasMore.value || loading.value) return
  if (sortMode.value === 'new') fetchList({ cursor: nextCursor.value, replace: false, pageNum: page.value + 1 })
  else fetchList({ cursor: null, replace: false, pageNum: page.value + 1 })
}

/* intersection observer */
const setupObserver = () => {
  if (!sentinel.value) return
  observer.value = new IntersectionObserver(entries => {
    for (const e of entries) if (e.isIntersecting) loadMore()
  }, { root: null, rootMargin: '400px', threshold: 0.1 })
  if (sentinel.value) observer.value.observe(sentinel.value)
}

/* preview modal controls */
const isPreviewOpen = computed(() => previewIndex.value !== null)

const onKeydown = (e: KeyboardEvent) => {
  if (!isPreviewOpen.value) return
  if (e.key === 'Escape') closePreview()
  else if (e.key === 'ArrowLeft') showPrev()
  else if (e.key === 'ArrowRight') showNext()
}

const openPreviewByIndex = (idx: number) => {
  previewIndex.value = idx
  document.body.style.overflow = 'hidden'
  window.addEventListener('keydown', onKeydown)
}

const closePreview = () => {
  previewIndex.value = null
  document.body.style.overflow = ''
  window.removeEventListener('keydown', onKeydown)
}

const hasPrev = computed(() => previewIndex.value !== null && previewIndex.value > 0)
const hasNext = computed(() => previewIndex.value !== null && previewIndex.value < items.value.length - 1)

const showPrev = () => { if (!hasPrev.value) return; previewIndex.value = (previewIndex.value as number) - 1 }
const showNext = () => { if (!hasNext.value) return; previewIndex.value = (previewIndex.value as number) + 1 }

const formatUploadAt = (s?: string) => {
  if (!s) return ''
  try { return new Date(s).toLocaleString() } catch { return s }
}

const previewDesc = computed(() => preview.value?.desc ?? null)

const downloadPreview = () => {
  if (!preview.value) return
  const a = document.createElement('a')
  a.href = preview.value.src
  a.download = (preview.value.title || 'image').replace(/\s+/g, '_') + '.jpg'
  document.body.appendChild(a)
  a.click()
  a.remove()
}

const likePreview = () => {
  if (!preview.value) return
  const idx = items.value.findIndex(i => i.id === preview.value?.id)
  if (idx !== -1) items.value[idx].likes = (items.value[idx].likes || 0) + 1
}

/* upload helpers */
const onFiles = (e: Event) => {
  const input = e.target as HTMLInputElement
  if (!input?.files) return
  handleFiles(input.files)
  input.value = ''
}
const onDragOver = (e: DragEvent) => { e.dataTransfer!.dropEffect = 'copy' }
const onDrop = (e: DragEvent) => { if (!e.dataTransfer) return; handleFiles(e.dataTransfer.files) }

const handleFiles = (files: FileList) => {
  Array.from(files).forEach(file => {
    if (!file.type.startsWith('image/')) return
    const id = `u-${Date.now()}-${Math.random().toString(36).slice(2,8)}`
    const src = URL.createObjectURL(file)
    pending.value.unshift({ id, src, name: file.name, file })
    items.value.unshift({ id, title: file.name, author: '你', likes: 0, src, tags: ['上传'], uploadAt: new Date().toISOString(), desc: null })
  })
}

const removePending = (id: string) => {
  const idx = pending.value.findIndex(p => p.id === id)
  if (idx >= 0) { URL.revokeObjectURL(pending.value[idx].src); pending.value.splice(idx, 1) }
}
const clearPending = () => { pending.value.forEach(p => URL.revokeObjectURL(p.src)); pending.value = [] }

const commitPending = async () => {
  if (!pending.value.length) return
  if (!authorName.value || !desc.value) { window.alert('请填写作者姓名与作品描述（为必填项）'); return }
  uploading.value = true
  try {
    for (const p of [...pending.value].reverse()) {
      const form = new FormData()
      form.append('Image', p.file)
      form.append('Title', p.name)
      form.append('AuthorName', authorName.value)
      form.append('Desc', desc.value)
      const res = await apiClient.post('/Drawing/upload', form)
      const payload = res?.data
      if (payload?.success && payload.data) {
        const serverUrl = payload.data.imageUrl ?? payload.data.imageUrlFull ?? payload.data.relativePath ?? ''
        const idx = items.value.findIndex(it => it.src === p.src)
        if (idx !== -1) {
          items.value[idx].src = buildFullImageUrl(serverUrl)
          items.value[idx].id = payload.data.id ?? items.value[idx].id
          items.value[idx].title = payload.data.title ?? items.value[idx].title
          items.value[idx].author = payload.data.author ?? authorName.value
          items.value[idx].uploadAt = payload.data.uploadAt ?? items.value[idx].uploadAt
          items.value[idx].desc = payload.data.desc ?? items.value[idx].desc
        } else {
          items.value.unshift({ id: payload.data.id ?? Date.now(), title: payload.data.title ?? p.name, author: authorName.value, likes: 0, src: buildFullImageUrl(serverUrl), uploadAt: payload.data.uploadAt, desc: payload.data.desc })
        }
        URL.revokeObjectURL(p.src)
        const pendIdx = pending.value.findIndex(x => x.id === p.id)
        if (pendIdx !== -1) pending.value.splice(pendIdx, 1)
      } else {
        console.warn('upload failed for', p.name, payload)
      }
    }
  } catch (err: any) {
    console.error('commitPending error', err)
    if (err.response) {
      const serverMsg = err.response.data?.message || (err.response.data?.errors ? JSON.stringify(err.response.data.errors) : null) || JSON.stringify(err.response.data)
      window.alert('上传失败: ' + (serverMsg ?? `HTTP ${err.response.status}`))
    } else if (err.request) {
      window.alert('网络错误：未收到来自服务器的响应（请检查后端是否启动，或 CORS/网络设置）')
    } else {
      window.alert('上传失败：' + (err.message || String(err)))
    }
  } finally {
    uploading.value = false
  }
}

/* filters & controls (exposed handlers) */
let qTimer2: ReturnType<typeof setTimeout> | null = null
const onQueryHandler = () => {
  if (qTimer2) clearTimeout(qTimer2)
  qTimer2 = setTimeout(() => { applyFilters(); qTimer2 = null }, 200)
}
const onQuery = onQueryHandler

const applyFilters = () => {
  nextCursor.value = null
  if (sortMode.value === 'new') fetchList({ cursor: null, replace: true, pageNum: 1 })
  else { page.value = 1; fetchList({ cursor: null, replace: true, pageNum: 1 }) }
}

const clearFilters = () => {
  query.value = ''
  filterTag.value = ''
  sortMode.value = 'new'
  nextCursor.value = null
  fetchListInitial()
}

const setSort = (s: typeof sortMode.value) => {
  sortMode.value = s
  nextCursor.value = null
  if (s === 'new') fetchListInitial()
  else { page.value = 1; fetchList({ cursor: null, replace: true, pageNum: 1 }) }
}

const onSortChange = () => {
  nextCursor.value = null
  if (sortMode.value === 'new') fetchListInitial()
  else { page.value = 1; fetchList({ cursor: null, replace: true, pageNum: 1 }) }
}

const shuffle = () => { sortMode.value = 'random' }

/* card style */
const cardStyle = (i: number) => {
  const tx = ((i % 4) - 1.5) * 6
  const ty = ((i % 3) - 1) * 4
  const scale = 1 + ((i % 6) - 3) * 0.004
  return { transform: `translate(${tx}px, ${ty}px) scale(${scale})`, zIndex: String(10 + (i % 5)) }
}

/* lifecycle */
onMounted(() => { fetchListInitial(); setupObserver() })
onBeforeUnmount(() => {
  observer.value?.disconnect()
  pending.value.forEach(p => URL.revokeObjectURL(p.src))
  window.removeEventListener('keydown', onKeydown)
  document.body.style.overflow = ''
})
</script>

<style scoped>
:root{
  --bg: linear-gradient(180deg,#f7fbff,#f2f8ff);
  --muted: #6b7280;
  --accent-a: #7fd3ff;
  --accent-b: #b19cff;
  --glass-weak: rgba(255,255,255,0.45);
  --glass-strong: rgba(255,255,255,0.7);
  --edge: rgba(8,18,36,0.06);
  --shadow-1: 0 8px 30px rgba(8,18,36,0.06);
  --shadow-2: 0 20px 60px rgba(8,18,36,0.08);
  --radius: 14px;
}
.glass-page { background: var(--bg); min-height: 100vh; color: #072136; font-family: Inter, ui-sans-serif, system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", Arial; -webkit-font-smoothing: antialiased; -moz-osx-font-smoothing: grayscale; position: relative; }
.container { max-width: 1180px; margin-inline: auto; padding: 24px clamp(16px, 4vw, 48px); box-sizing: border-box; }
.topbar { display:flex; gap: 16px; align-items: center; justify-content: space-between; padding: 18px; border-radius: 16px; background: linear-gradient(180deg, rgba(255,255,255,0.36), rgba(255,255,255,0.28)); border: 1px solid var(--edge); box-shadow: var(--shadow-1); backdrop-filter: blur(10px) saturate(140%); position: relative; z-index: 10; }
.title { margin: 0; font-size: clamp(20px, 3.2vw, 28px); font-weight: 700; color: #052136; }
.tagline { margin: 4px 0 0; color: var(--muted); font-size:13px; }
.controls { display:flex; gap:10px; align-items:center; }
.search { min-width: 260px; padding: 10px 12px; border-radius: 999px; border: 1px solid var(--edge); background: linear-gradient(180deg, rgba(255,255,255,0.55), rgba(255,255,255,0.45)); box-shadow: 0 8px 20px rgba(8,18,36,0.04), inset 0 -3px 10px rgba(8,18,36,0.02); color: #072136; backdrop-filter: blur(6px); }
.select { padding: 8px 10px; border-radius: 10px; border: 1px solid var(--edge); background: rgba(255,255,255,0.92); color: #072136; backdrop-filter: blur(6px); }
.main-grid { display: grid; grid-template-columns: 1fr 320px; gap: 22px; margin-top: 18px; align-items: start; }
.masonry { column-gap: 18px; column-count: 4; margin-top: 12px; }
@media (max-width: 1400px) { .masonry { column-count: 3; } }
@media (max-width: 900px) { .main-grid { grid-template-columns: 1fr; } .masonry { column-count: 2; } }
@media (max-width: 520px) { .masonry { column-count: 1; } }
.card { display: inline-block; width: 100%; margin: 0 0 22px; break-inside: avoid; cursor: pointer; position: relative; transition: transform 200ms cubic-bezier(.2,.9,.25,1), box-shadow 200ms; will-change: transform; }
.frost-plate { position: absolute; inset: -8px; border-radius: 16px; background: linear-gradient(180deg, rgba(255,255,255,0.12), rgba(255,255,255,0.06)); filter: blur(6px); z-index: 0; pointer-events: none; opacity: 0.95; }
.panel-surface { position: relative; border-radius: 12px; overflow: visible; padding: 10px; background: linear-gradient(180deg, rgba(255,255,255,0.48), rgba(255,255,255,0.36)); border: 1px solid var(--edge); box-shadow: var(--shadow-1); backdrop-filter: blur(12px) saturate(140%); z-index: 2; }
.glass-frame { border-radius: 10px; padding: 8px; background: linear-gradient(180deg, rgba(255,255,255,0.12), rgba(255,255,255,0.06)); border: 1px solid rgba(255,255,255,0.16); box-shadow: 0 10px 28px rgba(8,18,36,0.06); backdrop-filter: blur(10px); }
.thumb { display:block; width: 100%; height: auto; border-radius: 8px; object-fit: cover; border: 6px solid rgba(255,255,255,0.88); transition: transform 260ms ease, filter 260ms ease; box-shadow: 0 8px 26px rgba(8,18,36,0.06); }
.card:hover { transform: translateY(-8px) scale(1.012); box-shadow: var(--shadow-2); }
.card:hover .thumb { transform: scale(1.02); filter: saturate(1.04); }
.caption { display:flex; justify-content:space-between; gap: 8px; padding-top: 10px; align-items:center; }
.card-title { font-weight: 700; font-size: 13px; color: #062238; max-width: 72%; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.author { font-size: 12px; color: var(--muted); }
.likes { color: #c34d7a; font-weight: 600; }
.sentinel { height: 2px; width: 100%; }
.panel { background: linear-gradient(180deg, rgba(255,255,255,0.5), rgba(255,255,255,0.42)); border-radius: 12px; padding: 14px; border: 1px solid var(--edge); box-shadow: var(--shadow-1); backdrop-filter: blur(14px) saturate(140%); position: sticky; top: 84px; }
.dropzone { display:block; margin-top: 12px; cursor:pointer; }
.file { display:none; }
.dz-inner { padding: 12px; border-radius: 10px; border: 1px dashed rgba(8,18,36,0.06); background: linear-gradient(135deg, rgba(127,211,255,0.06), rgba(177,156,255,0.04)); display:flex; gap:8px; align-items:center; justify-content:center; color: #062238; backdrop-filter: blur(10px); }
.previews { display:flex; gap:8px; flex-wrap:wrap; margin-top:8px; }
.mini { width:64px; height:64px; border-radius:8px; overflow:hidden; position:relative; border: 1px solid rgba(255,255,255,0.6); background: rgba(255,255,255,0.32); backdrop-filter: blur(8px); box-shadow: 0 8px 18px rgba(8,18,36,0.04); }
.mini img { width:100%; height:100%; object-fit:cover; display:block; }
.tiny { position:absolute; top:6px; right:6px; background: rgba(0,0,0,0.6); color:#fff; border:none; border-radius:6px; width:22px; height:22px; cursor:pointer }
.actions { display:flex; gap:8px; margin-top:8px; justify-content:flex-end; }
.btn { padding:8px 12px; border-radius: 999px; background: linear-gradient(90deg, var(--accent-a), var(--accent-b)); color: #072136; border: none; cursor:pointer; box-shadow: 0 8px 22px rgba(125,101,255,0.06); }
.btn.subtle { background: transparent; border: 1px solid var(--edge); color: var(--muted); }
.btn.primary { background: linear-gradient(90deg, #7fd3ff, #b19cff); color: #041223; border: none; }
.chips { display:flex; gap:8px; margin-bottom:8px; }
.chip { padding:6px 10px; border-radius:999px; background: rgba(255,255,255,0.7); border: 1px solid rgba(8,18,36,0.03); cursor:pointer; color: #062238; }
.stats { margin-top: 8px; color: var(--muted); font-size: 13px; }
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(6,12,24,0.65);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 120000;
  -webkit-backdrop-filter: blur(6px);
  backdrop-filter: blur(6px);
  padding: 20px;
}
.modal-content {
  width: min(1100px, 95vw);
  max-height: 90vh;
  background: linear-gradient(180deg, rgba(255,255,255,0.96), rgba(255,255,255,0.94));
  border-radius: 12px;
  padding: 18px;
  display: flex;
  flex-direction: column;
  box-shadow: 0 30px 80px rgba(6,12,24,0.08);
  position: relative;
}
.modal-close {
  position: absolute;
  top: 10px;
  right: 10px;
  background: rgba(6,12,24,0.06);
  border: none;
  width: 36px;
  height: 36px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 18px;
}
.modal-body {
  display: flex;
  gap: 18px;
  align-items: flex-start;
  overflow: hidden;
}
.modal-image-wrap {
  flex: 1 1 60%;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  min-width: 300px;
}
.modal-image {
  max-width: 100%;
  max-height: 80vh;
  border-radius: 8px;
  box-shadow: 0 20px 60px rgba(6,12,24,0.08);
}
.modal-info {
  width: 320px;
  padding: 8px 6px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.modal-title { margin: 0; font-size: 18px; }
.modal-info .muted { color: #6b7280; font-size: 13px; }
.modal-info .desc { white-space: pre-wrap; color:#233; }
.modal-actions { display:flex; gap:8px; margin-top:12px; }
.modal-nav {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  background: rgba(255,255,255,0.9);
  border: none;
  width: 44px;
  height: 64px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 30px;
  color: #072136;
  display:flex;
  align-items:center;
  justify-content:center;
  opacity: 0.9;
}
.modal-nav.left { left: 8px; }
.modal-nav.right { right: 8px; }
@media (max-width: 900px) {
  .modal-content { width: 94vw; padding: 12px; }
  .modal-body { flex-direction: column; }
  .modal-info { width: 100%; }
  .modal-image { max-height: 50vh; }
}
.footer { margin-top: 28px; padding: 24px 0 56px; text-align: center; color: var(--muted); }
.card-desc {
  margin-top: 6px;
  color: var(--muted);
  font-size: 12px;
  max-height: 1.8em;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.meta-inputs { display:flex; flex-direction:column; gap:8px; margin-bottom:8px; }
.meta-text { width:100%; padding:8px; border-radius:8px; border:1px solid rgba(8,18,36,0.06); }
.meta-textarea { width:100%; min-height:72px; padding:8px; border-radius:8px; border:1px solid rgba(8,18,36,0.06); resize:vertical; }
</style>