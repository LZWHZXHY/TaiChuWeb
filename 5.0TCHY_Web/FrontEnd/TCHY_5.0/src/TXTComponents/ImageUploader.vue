<template>
  <div class="image-uploader">
    <input ref="inputRef" type="file" :accept="accept" multiple @change="onSelect" style="display:none" />
    <div class="uploader-ui">
      <button type="button" class="btn btn-secondary" @click="trigger">选择图片</button>
      <span class="muted">最多 {{ maxFiles }} 张，单张 ≤ {{ (maxSize/1024/1024).toFixed(1) }}MB</span>
    </div>

    <div v-if="previews.length" class="previews" role="list">
      <div v-for="(p, i) in previews" :key="i" class="preview-item" role="listitem">
        <div class="preview-inner" tabindex="0">
          <img :src="p.url" class="preview-image" alt="图片预览" loading="lazy" />
          <button type="button" class="remove-btn" @click="remove(i)" aria-label="移除图片">×</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
const props = defineProps({
  maxFiles: { type: Number, default: 10 },
  maxSize: { type: Number, default: 10 * 1024 * 1024 },
  accept: { type: String, default: 'image/*,.gif' }
})
const emit = defineEmits(['change'])
const inputRef = ref(null)
const files = ref([])
const previews = ref([])

const trigger = () => inputRef.value && inputRef.value.click()

const onSelect = (e) => {
  const selected = Array.from(e.target.files || [])
  for (const f of selected) {
    if (files.value.length >= props.maxFiles) break
    if (!validate(f)) continue
    files.value.push(f)
    const url = URL.createObjectURL(f)
    previews.value.push({ file: f, url })
  }
  emit('change', files.value)
  e.target.value = ''
}

const validate = (f) => {
  if (!f) return false
  if (f.size > props.maxSize) { alert(`文件 ${f.name} 太大`); return false }
  // basic mime check
  return true
}

const remove = (index) => {
  const p = previews.value.splice(index,1)[0]
  if (p && p.url) URL.revokeObjectURL(p.url)
  files.value.splice(index,1)
  emit('change', files.value)
}

const reset = () => {
  previews.value.forEach(p => p.url && URL.revokeObjectURL(p.url))
  previews.value = []
  files.value = []
  emit('change', files.value)
}

defineExpose({ reset })
</script>

<style scoped>
/* Variables for easy theming */
.image-uploader {
  --border: #e6eef6;
  --muted: #7b8794;
  --accent: 96, 106, 234; /* rgb for semi-transparent focus */
  --bg-chip: #fbfdff;
  --danger: #ef4444;
  display: flex;
  flex-direction: column;
  gap: 12px;
  font-family: inherit;
}

/* Uploader UI (button + hint) */
.uploader-ui {
  display: flex;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;
}

/* Button styles kept consistent with form buttons */
.btn {
  --btn-radius: 8px;
  padding: 8px 12px;
  border-radius: var(--btn-radius);
  border: 1px solid #d6e1f2;
  background: var(--bg-chip);
  color: #0b1220;
  cursor: pointer;
  font-weight: 600;
  font-size: 0.92rem;
  transition: transform 140ms ease, box-shadow 140ms ease;
}
.btn:hover { transform: translateY(-1px); box-shadow: 0 8px 20px rgba(9,30,66,0.06); }
.btn:active { transform: translateY(0); }

/* Muted helper text */
.muted {
  color: var(--muted);
  font-size: 0.86rem;
  white-space: nowrap;
}

/* Previews container: responsive grid */
.previews {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(92px, 1fr));
  gap: 10px;
  align-items: start;
}

/* Each preview item */
.preview-item {
  width: 100%;
  display: block;
}

/* Inner wrapper for consistent aspect ratio and keyboard focus */
.preview-inner {
  position: relative;
  width: 100%;
  padding-top: 66.66%; /* 3:2 aspect ratio; adjust as needed */
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid var(--border);
  background: linear-gradient(180deg, rgba(250,252,255,0.6), rgba(248,250,255,0.4));
  box-shadow: 0 6px 18px rgba(9,30,66,0.03);
  transition: transform 140ms ease, box-shadow 140ms ease, border-color 140ms ease;
  outline: none;
}

/* Keyboard focus ring */
.preview-inner:focus {
  border-color: rgba(var(--accent), 0.95);
  box-shadow: 0 10px 30px rgba(96,106,234,0.08), 0 0 0 6px rgba(var(--accent), 0.06);
  transform: translateY(-2px);
}

/* Image fills the wrapper */
.preview-image {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
  transition: transform 220ms ease;
  backface-visibility: hidden;
}

/* Hover to slightly zoom image */
.preview-inner:hover .preview-image {
  transform: scale(1.06);
}

/* Remove button: more accessible and visible */
.remove-btn {
  position: absolute;
  top: 8px;
  right: 8px;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  border: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  background: rgba(0,0,0,0.55);
  color: #fff;
  cursor: pointer;
  box-shadow: 0 6px 18px rgba(9,30,66,0.12);
  transition: background 140ms ease, transform 120ms ease;
}
.remove-btn:hover { background: rgba(0,0,0,0.72); transform: translateY(-1px); }
.remove-btn:active { transform: translateY(0); }

/* If you'd like the remove button to be visible only on hover uncomment below:
.preview-inner .remove-btn { opacity: 0; transform: translateY(-6px); transition: opacity 120ms ease, transform 120ms ease; }
.preview-inner:hover .remove-btn { opacity: 1; transform: translateY(0); }
*/

/* Small screens: make previews larger and stack controls */
@media (max-width: 520px) {
  .uploader-ui { gap: 8px; }
  .muted { font-size: 0.82rem; }
  .previews { grid-template-columns: repeat(2, 1fr); gap: 8px; }
  .preview-inner { padding-top: 75%; } /* squarer on small screens */
}

/* Very small / single-column fallback */
@media (max-width: 360px) {
  .previews { grid-template-columns: repeat(1, 1fr); }
}

/* Visually hidden but accessible file input (kept inline style hidden for compatibility) */
input[type="file"] {
  position: absolute !important;
  width: 1px !important;
  height: 1px !important;
  padding: 0 !important;
  margin: -1px !important;
  overflow: hidden !important;
  clip: rect(0,0,0,0) !important;
  white-space: nowrap !important;
  border: 0 !important;
}
</style>