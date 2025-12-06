<template>
  <header class="header">
    <div class="left">
      <h1 class="logo">虚拟道具商店</h1>
      <div class="controls">
        <input v-model="localQuery" placeholder="搜索皮肤 / 道具 / 名称" @keyup.enter="apply" />
        <select v-model="localCategory" @change="apply">
          <option v-for="c in categories" :key="c">{{ c }}</option>
        </select>
      </div>
    </div>

    <div class="right">
      <div class="balance">余额：<strong>{{ balance }}</strong> 金币</div>
      <button class="cart-btn" @click="$emit('open-cart')">购物车</button>
    </div>
  </header>
</template>

<script setup>
import { ref, watch, toRefs } from 'vue'
const props = defineProps({
  balance: { type: Number, default: 0 },
  categories: { type: Array, default: () => [] },
  query: { type: String, default: '' },
  category: { type: String, default: '全部' },
})
const emit = defineEmits(['update:query', 'update:category', 'open-cart'])

const localQuery = ref(props.query)
const localCategory = ref(props.category)

watch(() => props.query, (v) => (localQuery.value = v))
watch(() => props.category, (v) => (localCategory.value = v))

function apply() {
  emit('update:query', localQuery.value)
  emit('update:category', localCategory.value)
}
</script>

<style scoped>
.header {
  display:flex;
  justify-content:space-between;
  align-items:center;
  gap:12px;
  padding:12px 14px;
  border-radius:10px;
  background: #ffffff;
  border: 1px solid #e6e9ef;
  box-shadow: 0 8px 20px rgba(16,24,40,0.04);
}
.logo { margin:0; font-size:18px; color:#111827; font-weight:700; }
.controls { display:flex; gap:10px; align-items:center; }
.controls input, .controls select {
  padding:8px 10px; border-radius:8px; border: 1px solid #e6e9ef; outline: none;
  background: #fff; color: #111827; font-size:13px;
}
.controls input::placeholder { color: #9ca3af; }
.right { display:flex; align-items:center; gap:12px; }
.balance { color: #374151; font-size:14px; }
.cart-btn {
  padding:9px 12px; border-radius:8px; border:none; cursor:pointer;
  background: var(--accent);
  color:#fff; font-weight:700; box-shadow: 0 8px 20px rgba(6,182,212,0.12);
  transition: transform .12s;
}
.cart-btn:active { transform: translateY(1px) scale(.998); }
@media (max-width: 720px) {
  .controls input { width: 120px; }
  .header { padding:10px; border-radius:8px; }
}
</style>