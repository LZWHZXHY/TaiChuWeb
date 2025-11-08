<template>
  <section>
    <div class="battle-filter">
      <label>范围</label>
      <select :value="scope" @change="onScopeChange">
        <option value="all">全部</option>
        <option value="incoming">我收到的</option>
        <option value="outgoing">我发起的</option>
      </select>
    </div>

    <div class="list">
      <div v-for="b in items" :key="b.id" class="battle-item">
        <div class="left">
          <div class="title">
            <i class="fas fa-fire"></i>
            <strong>{{ b.title }}</strong>
            <span class="status" :data-status="b.status">{{ statusText(b.status) }}</span>
          </div>
          <div class="sub">
            <span>发起：{{ b.initiatorUserName }}（{{ personaName(b.initiatorPersonaId) }}）</span>
            <span>对手：{{ b.opponentUserName }}（{{ personaName(b.opponentPersonaId) }}）</span>
          </div>
          <div class="desc" v-if="b.brief">{{ b.brief }}</div>
          <div class="desc" v-if="b.customRules">规则：{{ b.customRules }}</div>
          <div class="desc" v-if="b.deadlineAt">截止：{{ b.deadlineAt }}</div>
        </div>
        <div class="right">
          <button
            v-if="b.opponentUserId === currentUserId && b.status === 'pending'"
            class="btn"
            @click="$emit('update-status', { id: b.id, status: 'accepted' })"
          >
            接受
          </button>
          <button
            v-if="b.opponentUserId === currentUserId && b.status === 'pending'"
            class="btn danger"
            @click="$emit('update-status', { id: b.id, status: 'rejected' })"
          >
            拒绝
          </button>
          <button
            v-if="b.initiatorUserId === currentUserId && b.status === 'pending'"
            class="btn"
            @click="$emit('update-status', { id: b.id, status: 'withdrawn' })"
          >
            撤回
          </button>
        </div>
      </div>
    </div>

    <div v-if="!items.length" class="empty">
      <i class="fas fa-box-open"></i>
      <span><slot name="empty">暂无约战数据</slot></span>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'

type BattleStatus = 'pending' | 'accepted' | 'rejected' | 'withdrawn' | 'completed'
type BattleRequest = {
  id: string
  title: string
  theme?: string
  brief?: string
  customRules?: string
  deadlineAt?: string
  initiatorUserId: string
  initiatorUserName: string
  initiatorPersonaId: string
  opponentUserId: string
  opponentUserName: string
  opponentPersonaId: string
  status: BattleStatus
  createdAt?: string
  updatedAt?: string
}
type PersonaRef = { id: string; name: string }

const props = defineProps<{
  scope: 'all' | 'incoming' | 'outgoing'
  battles: BattleRequest[]
  personas: PersonaRef[]
  currentUserId?: string
}>()
const emit = defineEmits<{
  (e: 'update:scope', v: 'all' | 'incoming' | 'outgoing'): void
  (e: 'fetch', v: 'all' | 'incoming' | 'outgoing'): void
  (e: 'update-status', p: { id: string; status: BattleStatus }): void
}>()

const items = computed(() => {
  if (props.scope === 'incoming') return props.battles.filter((b) => b.opponentUserId === props.currentUserId)
  if (props.scope === 'outgoing') return props.battles.filter((b) => b.initiatorUserId === props.currentUserId)
  return props.battles
})
function personaName(id?: string) {
  if (!id) return '未知人设'
  return props.personas.find((p) => p.id === id)?.name || '未知人设'
}
function statusText(s: BattleStatus) {
  switch (s) {
    case 'pending': return '待确认'
    case 'accepted': return '已接受'
    case 'rejected': return '已拒绝'
    case 'withdrawn': return '已撤回'
    case 'completed': return '已完成'
  }
}
function onScopeChange(e: Event) {
  const v = (e.target as HTMLSelectElement).value as 'all' | 'incoming' | 'outgoing'
  emit('update:scope', v)
  emit('fetch', v)
}
</script>