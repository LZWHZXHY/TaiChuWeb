<template>
  <section>
    <div class="filter-bar">
      <div class="search">
        <i class="fas fa-search"></i>
        <input
          :value="search"
          type="text"
          placeholder="搜索人设名称 / 简介 / 标签"
          @input="onSearchInput"
          @keyup.enter="emitFetch"
        />
      </div>
      <div class="chips">
        <button
          v-for="t in presetTags"
          :key="t"
          class="chip"
          :class="{ active: tags.includes(t) }"
          @click="toggleTag(t)"
        >
          #{{ t }}
        </button>
      </div>
      <div class="spacer"></div>
      <div class="view-tip">共 {{ filtered.length }} 个公开人设</div>
    </div>

    <div class="grid">
      <div v-for="p in filtered" :key="p.id" class="card">
        <div class="avatar">
          <img v-if="p.avatarUrl" :src="p.avatarUrl" alt="avatar" />
          <div v-else class="placeholder"><i class="fas fa-user"></i></div>
        </div>

        <div class="meta">
          <div class="title-row">
            <h3>{{ p.name }}</h3>
            <span v-if="p.alias" class="alias">/{{ p.alias }}</span>
            <span v-if="isMine(p)" class="badge">我的</span>
          </div>
          <div class="owner">@{{ p.ownerName }}</div>
          <p v-if="p.bio" class="bio clamp-2">{{ p.bio }}</p>
          <div class="tags">
            <span v-for="t in p.tags" :key="t" class="tag">#{{ t }}</span>
          </div>
          <div v-if="p.power" class="powers">
            <span>ATK {{ p.power.attack }}</span>
            <span>DEF {{ p.power.defense }}</span>
            <span>SPD {{ p.power.speed }}</span>
            <span>SKL {{ p.power.skill }}</span>
          </div>
          <div v-if="p.rules" class="rules clamp-1">规则：{{ p.rules }}</div>
        </div>

        <div class="actions">
          <button class="btn" @click="$emit('view', p)">
            <i class="fas fa-eye"></i> 查看
          </button>
          <button class="btn primary" :disabled="isMine(p)" @click="$emit('challenge', p)">
            <i class="fas fa-swords"></i> 约战
          </button>
        </div>
      </div>
    </div>

    <div v-if="!filtered.length" class="empty">
      <i class="fas fa-box-open"></i>
      <span><slot name="empty">暂无人设数据</slot></span>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'

type PersonaPower = { attack: number; defense: number; speed: number; skill: number }
type Persona = {
  id: string
  ownerId: string
  ownerName: string
  name: string
  alias?: string
  avatarUrl?: string
  galleryUrls?: string[]
  bio?: string
  tags: string[]
  power?: PersonaPower
  rules?: string
  visibility?: 'public' | 'private'
  createdAt?: string
  updatedAt?: string
}

const props = defineProps<{
  personas: Persona[]
  currentUserId?: string
  search: string
  tags: string[]
  presetTags: string[]
}>()
const emit = defineEmits<{
  (e: 'update:search', v: string): void
  (e: 'update:tags', v: string[]): void
  (e: 'fetch'): void
  (e: 'view', p: Persona): void
  (e: 'challenge', p: Persona): void
}>()

function onSearchInput(e: Event) {
  emit('update:search', (e.target as HTMLInputElement).value)
}
function toggleTag(t: string) {
  const set = new Set(props.tags)
  set.has(t) ? set.delete(t) : set.add(t)
  emit('update:tags', Array.from(set))
  emit('fetch')
}
function isMine(p: Persona): boolean {
  // 保证返回值为纯 boolean，避免出现 "" 或 undefined
  return !!props.currentUserId && p.ownerId === props.currentUserId
}
const filtered = computed(() => {
  let list = props.personas.filter((p) => !p.visibility || p.visibility === 'public')
  if (props.search) {
    const s = props.search.toLowerCase()
    list = list.filter((p) =>
      [p.name, p.alias, p.bio, ...(p.tags || [])].join(' ').toLowerCase().includes(s),
    )
  }
  if (props.tags.length) list = list.filter((p) => props.tags.every((t) => p.tags.includes(t)))
  return list
})
function emitFetch() {
  emit('fetch')
}
</script>