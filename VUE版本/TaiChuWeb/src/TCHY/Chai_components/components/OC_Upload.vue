<template>
  <form class="oc-upload" @submit.prevent="onSubmit" novalidate>
    <!-- Two-column layout: left = textual inputs, right = media uploads & preview -->
    <div class="two-column">
      <!-- LEFT column: all textual inputs in one panel -->
      <section class="panel info-panel">
        <div class="info-top">
          <div class="name-block">
            <label class="label">人设名称</label>
            <input v-model.trim="form.name" class="field name-input" required placeholder="例如：斩风" :aria-invalid="!!errors.name" />
            <div v-if="errors.name" class="field-error">{{ errors.name }}</div>
          </div>

          <div class="meta-inline">
            <div class="meta-item">
              <label class="label small">性别</label>
              <select v-model.number="form.gender" class="select compact">
                <option :value="0">男</option>
                <option :value="1">女</option>
                <option :value="2">未知</option>
              </select>
              <div v-if="errors.gender" class="field-error">{{ errors.gender }}</div>
            </div>

            <!-- 年龄输入：没有上限 -->
            <div class="meta-item age">
              <label class="label small">年龄</label>
              <input type="number" min="0" v-model.number="form.age" class="field compact age-input" :aria-invalid="!!errors.age" />
              <div v-if="errors.age" class="field-error">{{ errors.age }}</div>
            </div>

            <div class="meta-item species-item">
              <label class="label small">物种</label>
              <input v-model.trim="form.species" class="field compact" placeholder="物种" :aria-invalid="!!errors.species" />
              <div v-if="errors.species" class="field-error">{{ errors.species }}</div>
            </div>

            <!-- Author input (editable). Prefilled from prop if provided -->
            <div class="meta-item author-item">
              <label class="label small">作者</label>
              <input v-model.trim="form.authorName" class="field compact" placeholder="OC 作者名（可编辑）" :aria-invalid="!!errors.authorName" />
              <div v-if="errors.authorName" class="field-error">{{ errors.authorName }}</div>
            </div>
          </div>
        </div>

        <div class="info-bottom">
          <div class="field-col">
            <label class="label small">性格（简短）</label>
            <textarea v-model.trim="form.character" class="field area" rows="2" placeholder="性格要点"></textarea>
          </div>

          <div class="field-col">
            <label class="label small">能力（剧情描述，非数值）</label>
            <textarea v-model.trim="form.ability.freeText" class="field area" rows="3" placeholder="例如：风斩：短距离斩击并能位移"></textarea>
            <div v-if="errors.ability" class="field-error">{{ errors.ability }}</div>
          </div>

          <div class="field-row">
            <div>
              <label class="label small">一句话简介</label>
              <input v-model.trim="form.experience" class="field" placeholder="一句话 / 精要" />
            </div>
            <div>
              <label class="label small">配色（简洁）</label>
              <input v-model.trim="form.colors" class="field" placeholder='例如: {"hair":"#111"} 或 简短描述' :aria-invalid="!!errors.colors" />
              <div v-if="errors.colors" class="field-error">{{ errors.colors }}</div>
            </div>
          </div>

          <div>
            <label class="label small">背景（精简）</label>
            <textarea v-model.trim="form.background" class="field area" rows="3" placeholder="背景要点"></textarea>
          </div>

          <div>
            <label class="label small">额外说明</label>
            <textarea v-model.trim="form.ExtraDesc" class="field area" rows="2"></textarea>
          </div>

          <div class="compact-row">
            <div>
              <label class="label small">世界时间节点</label>
              <input type="number" min="0" v-model.number="form.OC_Current_Time" class="field compact" :aria-invalid="!!errors.OC_Current_Time" />
              <div v-if="errors.OC_Current_Time" class="field-error">{{ errors.OC_Current_Time }}</div>
            </div>
            <div>
              <label class="label small">POO（自定义，可空）</label>
              <input v-model.trim="form.POO" class="field compact" />
            </div>
          </div>
        </div>
      </section>

      <!-- RIGHT column: media uploads + preview -->
      <aside class="panel media-panel">
        <div class="media-group">
          <div class="media-row">
            <label class="label small">人物立绘（上传）</label>
            <div class="upload-line">
              <input type="file" accept="image/*" @change="onAvatarChange" />
              <div class="preview-small" v-if="avatarPreview">
                <img :src="avatarPreview" alt="avatar" />
                <button type="button" class="btn tiny danger" @click="removeAvatar">删除</button>
              </div>
              <div class="preview-empty" v-else>未选择图片</div>
            </div>
            <p class="hint">建议正面/半身立绘，PNG 或 JPG</p>
            <div v-if="errors.avatarFile" class="field-error">{{ errors.avatarFile }}</div>
          </div>

          <div class="media-row">
            <label class="label small">画廊（多张上传）</label>
            <div class="upload-line">
              <input type="file" accept="image/*" multiple @change="onGalleryChange" />
              <div class="gallery-previews">
                <div v-for="(p, i) in galleryPreviews" :key="i" class="thumb">
                  <img :src="p" />
                  <button type="button" class="btn tiny" @click="removeGallery(i)">×</button>
                </div>
              </div>
            </div>
            <div v-if="errors.galleryFiles" class="field-error">{{ errors.galleryFiles }}</div>
          </div>

          <div class="media-row">
            <label class="label small">武器图片（上传）</label>
            <div class="upload-line">
              <input type="file" accept="image/*" @change="onWeaponChange" />
              <div class="preview-small" v-if="weaponPreview">
                <img :src="weaponPreview" alt="weapon" />
                <button type="button" class="btn tiny danger" @click="removeWeapon">删除</button>
              </div>
              <div class="preview-empty" v-else>未选择图片</div>
            </div>
          </div>
        </div>

        <div class="preview-block">
          <div class="avatar-preview">
            <div class="avatar">
              <img v-if="avatarPreview" :src="avatarPreview" />
              <div v-else class="avatar-empty">立绘预览</div>
            </div>
            <div class="meta">
              <h4 class="name">{{ form.name || '未命名' }}</h4>
              <div class="sub">{{ form.species || '物种' }} · {{ genderLabel }}</div>
              <p class="excerpt">{{ form.experience }}</p>
              <p class="author-line" v-if="form.authorName">作者：{{ form.authorName }}</p>
            </div>
          </div>

          <div class="thumbs" v-if="galleryPreviews.length">
            <div class="thumb-grid">
              <img v-for="(u,i) in galleryPreviews" :key="i" :src="u" class="thumb-item" />
            </div>
          </div>

          <div class="preview-ability">
            <h5 class="small-title">能力摘要</h5>
            <p class="small-text" v-if="form.ability.freeText">{{ form.ability.freeText }}</p>
            <p class="small-text muted" v-else>暂无能力描述</p>
          </div>

          <div class="preview-weapon">
            <h5 class="small-title">武器</h5>
            <img v-if="weaponPreview" :src="weaponPreview" class="weapon-thumb" />
            <p class="small-text muted" v-else>无武器图片</p>
            <p class="small-text">{{ form.OC_WeapenDesc }}</p>
          </div>

          <div class="actions">
            <button type="button" class="btn muted" @click="onReset" :disabled="isSubmitting">重置</button>
            <button type="submit" class="btn primary" :disabled="!form.name || isSubmitting">提交并创建</button>
          </div>

          <div v-if="submitError" class="field-error" style="margin-top:8px; white-space:pre-wrap">{{ submitError }}</div>
          <div v-if="uploadProgress !== null" style="margin-top:8px">
            上传进度: {{ uploadProgress }}%
          </div>
        </div>
      </aside>
    </div>
  </form>
</template>

<script setup lang="ts">
import { reactive, ref, computed } from 'vue'
// 使用你已有的封装 request 实例，确保路径和别名正确
// 如果你的路径不同，请根据项目结构调整导入路径
import request from '../../../../server/UserInfoApi'

/* single props declaration */
const props = defineProps<{ currentUserName?: string }>()
const emit = defineEmits<{ (e: 'submit', payload: any): void; (e: 'reset'): void }>()

const isSubmitting = ref(false)
const submitError = ref<string | null>(null)
const uploadProgress = ref<number | null>(null)

const form = reactive({
  name: '',
  gender: 2 as number,
  age: 0 as number,
  species: '',
  authorName: props.currentUserName ?? '', // prefill, editable (user-provided author)
  avatarFile: null as File | null,
  galleryFiles: [] as File[],
  weaponFile: null as File | null,
  experience: '',
  character: '',
  background: '',
  colors: '',
  OC_WeapenDesc: '',
  ExtraDesc: '',
  POO: '',
  ability: { freeText: '' },
  OC_Current_Time: 0 as number
})

const errors = reactive<Record<string,string>>({})
const avatarPreview = ref<string | null>(null)
const galleryPreviews = ref<string[]>([])
const weaponPreview = ref<string | null>(null)

const genderLabel = computed(() => form.gender === 0 ? '男' : form.gender === 1 ? '女' : '未知')

function revokeAllPreviews() {
  if (avatarPreview.value) { URL.revokeObjectURL(avatarPreview.value); avatarPreview.value = null }
  galleryPreviews.value.forEach(u => { try { URL.revokeObjectURL(u) } catch {} })
  galleryPreviews.value = []
  if (weaponPreview.value) { URL.revokeObjectURL(weaponPreview.value); weaponPreview.value = null }
}

function onAvatarChange(e: Event) {
  const input = e.target as HTMLInputElement
  const f = input.files?.[0] ?? null
  if (!f) return
  if (avatarPreview.value) URL.revokeObjectURL(avatarPreview.value)
  form.avatarFile = f
  avatarPreview.value = URL.createObjectURL(f)
  input.value = ''
}

function onGalleryChange(e: Event) {
  const input = e.target as HTMLInputElement
  const files = input.files ? Array.from(input.files) : []
  if (!files.length) return
  files.forEach(f => {
    form.galleryFiles.push(f)
    galleryPreviews.value.push(URL.createObjectURL(f))
  })
  input.value = ''
}

function onWeaponChange(e: Event) {
  const input = e.target as HTMLInputElement
  const f = input.files?.[0] ?? null
  if (!f) return
  if (weaponPreview.value) URL.revokeObjectURL(weaponPreview.value)
  form.weaponFile = f
  weaponPreview.value = URL.createObjectURL(f)
  input.value = ''
}

function removeAvatar() {
  if (avatarPreview.value) URL.revokeObjectURL(avatarPreview.value)
  avatarPreview.value = null
  form.avatarFile = null
}
function removeWeapon() {
  if (weaponPreview.value) URL.revokeObjectURL(weaponPreview.value)
  weaponPreview.value = null
  form.weaponFile = null
}
function removeGallery(index: number) {
  const u = galleryPreviews.value[index]
  if (u) URL.revokeObjectURL(u)
  galleryPreviews.value.splice(index, 1)
  form.galleryFiles.splice(index, 1)
}

function onReset() {
  form.name = ''
  form.gender = 2
  form.age = 0
  form.species = ''
  form.authorName = props.currentUserName ?? ''
  revokeAllPreviews()
  form.avatarFile = null
  form.galleryFiles = []
  form.weaponFile = null
  form.experience = ''
  form.character = ''
  form.background = ''
  form.colors = ''
  form.OC_WeapenDesc = ''
  form.ExtraDesc = ''
  form.POO = ''
  form.ability.freeText = ''
  form.OC_Current_Time = 0
  submitError.value = null
  uploadProgress.value = null
  emit('reset')
}

function validate(): boolean {
  // clear
  for (const k in errors) delete errors[k]
  if (!form.name || !form.name.trim()) errors.name = '人设名称为必填'
  if (!form.authorName || !form.authorName.trim()) errors.authorName = '作者为必填'
  if (!form.ability?.freeText || !form.ability.freeText.trim()) errors.ability = '能力为必填'
  if (!form.colors || !form.colors.trim()) errors.colors = '配色为必填'
  if (form.OC_Current_Time == null) errors.OC_Current_Time = '世界时间节点为必填'
  // avatar must exist (per your requirement)
  if (!form.avatarFile) errors.avatarFile = '请上传人物立绘'
  // age: require non-negative integer (no upper bound)
  if (form.age == null || form.age === '' || Number.isNaN(form.age)) {
    errors.age = '年龄必须为数字'
  } else if (!Number.isInteger(form.age)) {
    errors.age = '年龄必须为整数'
  } else if (form.age < 0) {
    errors.age = '年龄必须为非负数'
  }
  // species required
  if (!form.species || !form.species.trim()) errors.species = '物种为必填'
  // gallery size optional but check each file size
  const maxBytes = 10 * 1024 * 1024 // 10MB per file
  form.galleryFiles.forEach((f, idx) => {
    if (f.size > maxBytes) errors.galleryFiles = `第 ${idx + 1} 张画廊图片大小超过 10MB`
  })
  return Object.keys(errors).length === 0
}

async function uploadFilesAndAssociate(ocId: number) {
  const fd = new FormData()
  if (form.avatarFile) fd.append('avatar', form.avatarFile)
  if (form.weaponFile) fd.append('weapon', form.weaponFile)
  if (form.galleryFiles && form.galleryFiles.length) {
    form.galleryFiles.forEach(f => fd.append('gallery', f))
  }

  uploadProgress.value = 0

  const config = {
    headers: { 'Content-Type': 'multipart/form-data' },
    onUploadProgress: (progressEvent: ProgressEvent) => {
      if (!progressEvent.lengthComputable) return
      uploadProgress.value = Math.round((progressEvent.loaded / progressEvent.total) * 100)
    },
    timeout: 2 * 60 * 1000 // 2 minutes
  }

  const resp = await request.post(`/api/OCBattle/${ocId}/upload-files`, fd, config)
  return resp.data
}

async function onSubmit() {
  submitError.value = null
  if (!validate()) {
    // focus first error (simple)
    const first = Object.keys(errors)[0]
    submitError.value = errors[first]
    return
  }

  isSubmitting.value = true
  try {
    // 1) create metadata (authorName included as user requested)
    const meta = {
      Name: form.name,
      Gender: form.gender,
      Age: form.age,
      Species: form.species,
      POO: form.POO,
      OC_Current_Time: form.OC_Current_Time,
      ability: JSON.stringify({ freeText: form.ability.freeText }), // backend expects string
      Colors: form.colors,
      Experience: form.experience,
      Character: form.character,
      Background: form.background,
      OC_WeapenDesc: form.OC_WeapenDesc,
      ExtraDesc: form.ExtraDesc,
      // include authorName from front-end (backend should still validate/override if required)
      AuthorName: form.authorName
    }

    const createResp = await request.post('/api/OCBattle/create', meta)
    const createdId = createResp.data.id ?? createResp.data.Id ?? createResp.data.Id
    if (!createdId) throw new Error('创建失败：未返回 id')

    // 2) upload files
    const uploadResult = await uploadFilesAndAssociate(createdId)

    // success - you can emit or navigate
    emit('submit', { ocId: createdId, uploaded: uploadResult })
    // reset UI
    onReset()
  } catch (err: any) {
    console.error('请求错误：', err)
    const data = err?.response?.data
    if (data) {
      // Prefer human-readable message if provided, otherwise stringify details
      if (data.message) {
        submitError.value = data.message + (data.errors ? '\n' + JSON.stringify(data.errors, null, 2) : '')
      } else if (data.errors) {
        submitError.value = JSON.stringify(data.errors, null, 2)
      } else {
        submitError.value = JSON.stringify(data, null, 2)
      }
      console.error('后端响应 body:', data)
    } else {
      submitError.value = err.message || '提交失败'
    }
  } finally {
    isSubmitting.value = false
    uploadProgress.value = null
  }
}
</script>

<style scoped>
:root {
  --bg: #ffffff;
  --panel: #ffffff;
  --border: #e9eef6;
  --muted: #6b7280;
  --text: #0f172a;
  --primary: #0ea5e9;
  --danger: #ef4444;
  --radius: 10px;
}

.field-error { color: var(--danger); font-size: 13px; margin-top:6px; white-space:normal; }

/* container */
.oc-upload { max-width: 1200px; margin: 16px auto; font-family: Inter, system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", Arial; color: var(--text); }

/* two column layout */
.two-column { display: grid; grid-template-columns: 1fr 420px; gap: 18px; align-items: start; }
@media (max-width: 980px) { .two-column { grid-template-columns: 1fr; } }

/* panels */
.panel { background: var(--panel); border: 1px solid var(--border); border-radius: var(--radius); padding: 14px; box-shadow: 0 6px 18px rgba(15,23,42,0.04); }

/* info panel specifics */
.info-panel { display: flex; flex-direction: column; gap: 10px; }
.info-top { display:grid; grid-template-columns: 1fr minmax(220px, 420px); gap:12px; align-items:end; }
.name-block { display:flex; flex-direction:column; gap:6px; min-width:160px; }
.name-input { padding:12px 14px; font-size:16px; border-radius:10px; border:1px solid var(--border); width:100%; box-sizing:border-box; }
.meta-inline { display:flex; gap:10px; align-items:end; flex-wrap:wrap; justify-content:flex-end; }
.meta-item { display:flex; flex-direction:column; gap:6px; min-width:64px; max-width:220px; flex:0 1 auto; }
.meta-item.age { min-width:56px; max-width:88px; width:72px; }
.meta-item.species-item { min-width:120px; max-width:220px; }
.meta-item.author-item { min-width:120px; max-width:220px; }
.field, .select { width:100%; padding:8px 10px; border-radius:8px; border:1px solid var(--border); background:#fff; box-sizing:border-box; font-size:13px; }
.field.compact, .select.compact { padding:6px 8px; height:36px; }
.field.compact.age-input { padding:6px 6px; height:34px; width:100%; box-sizing:border-box; text-align:center; font-size:13px; }
.area { min-height:44px; resize:vertical; padding:8px 10px; border-radius:8px; }
.field-row { display:flex; gap:12px; }
.field-row > * { flex:1; }
.info-bottom { display:flex; flex-direction:column; gap:10px; }
.compact-row { display:flex; gap:12px; align-items:flex-end; }
.compact-row > * { flex:1; }

/* media panel specifics */
.media-panel { display:flex; flex-direction:column; gap:12px; }
.upload-line { display:flex; gap:12px; align-items:center; flex-wrap:wrap; }
.preview-small { display:flex; gap:8px; align-items:center; }
.preview-small img { width:64px; height:64px; object-fit:cover; border-radius:6px; border:1px solid var(--border); }
.preview-empty { color:var(--muted); font-size:13px; }
.gallery-previews { display:flex; gap:8px; flex-wrap:wrap; margin-top:8px; }
.thumb { position:relative; width:84px; height:84px; border-radius:6px; overflow:hidden; border:1px solid var(--border); }
.thumb img { width:100%; height:100%; object-fit:cover; display:block; }
.thumb button { position:absolute; top:6px; right:6px; background:rgba(0,0,0,0.55); color:#fff; border:none; border-radius:50%; width:22px; height:22px; cursor:pointer; }

/* preview area */
.preview-block { display:flex; flex-direction:column; gap:12px; }
.avatar-preview { display:flex; gap:12px; align-items:center; }
.avatar { width:84px; height:84px; border-radius:8px; background:#f8fafc; border:1px solid var(--border); display:grid; place-items:center; overflow:hidden; }
.avatar img { width:100%; height:100%; object-fit:cover; }
.avatar-empty { color:var(--muted); font-size:13px; }
.meta .name { margin:0; font-size:16px; font-weight:700; }
.sub { color:var(--muted); font-size:13px; }
.excerpt { color:#334155; font-size:13px; margin-top:8px; }
.author-line { color:var(--muted); font-size:13px; margin-top:6px; }

/* gallery grid */
.thumb-grid { display:grid; grid-template-columns: repeat(3, 1fr); gap:8px; }
.thumb-item { width:100%; height:70px; object-fit:cover; border-radius:6px; border:1px solid var(--border); }

.preview-ability, .preview-weapon { border-top:1px dashed var(--border); padding-top:8px; }

.actions { display:flex; gap:10px; justify-content:flex-end; margin-top:6px; }
.btn { padding:8px 12px; border-radius:8px; border:1px solid #cbd5e1; background:#fff; cursor:pointer; font-weight:600; }
.btn.primary { background:var(--primary); color:#fff; border-color:var(--primary); box-shadow:0 6px 18px rgba(14,165,233,0.12); }
.btn.muted { background:#f8fafc; color:#374151; border-color:#e6eef6; }
.btn.tiny { padding:6px 8px; font-size:13px; }
.btn.danger { background:var(--danger); color:#fff; border-color:var(--danger); }

.small-title { font-size:13px; margin:0 0 6px 0; }
.small-text { font-size:13px; color:#374151; }
.muted { color:var(--muted); }
</style>