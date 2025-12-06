<template>
  <div class="detail-backdrop" @click.self="$emit('close')">
    <div class="detail">
      <button class="close" @click="$emit('close')">✕</button>
      <div class="top">
        <div class="thumb">{{ item.title }}</div>
        <div class="info">
          <h2>{{ item.title }}</h2>
          <p class="type">{{ item.type }}</p>
          <p class="desc">{{ item.description }}</p>
          <div class="meta">
            <div class="price">{{ item.price }} 金币</div>
            <div class="stock">库存: {{ item.stock }}</div>
          </div>
          <div class="actions">
            <button @click="$emit('add-to-cart', item, 1)" :disabled="item.stock===0" class="buy">加入购物车</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  item: { type: Object, required: true },
})
</script>

<style scoped>
.detail-backdrop { position:fixed; inset:0; display:flex; align-items:center; justify-content:center; background: rgba(16,24,40,0.32); z-index:60; }
.detail { width:880px; max-width:95%; background: #ffffff; border-radius:12px; padding:18px; color:var(--text); border:1px solid #e6e9ef; position:relative; box-shadow: 0 24px 70px rgba(16,24,40,0.08); }
.close { position:absolute; right:16px; top:16px; background:transparent; border:none; color:#6b7280; font-size:18px; cursor:pointer; }
.top { display:flex; gap:16px; }
.thumb { width:300px; height:180px; border-radius:10px; background: linear-gradient(180deg,#fbfdff,#f7fbfd); display:flex; align-items:center; justify-content:center; color:#6b7280; font-weight:700; }
.info h2 { margin:0; color:#111827; }
.type { color:#6b7280; margin:6px 0; }
.desc { color:#374151; }
.meta { display:flex; gap:12px; margin-top:8px; }
.buy { padding:10px 14px; border-radius:8px; background: var(--accent); border:none; color:#fff; font-weight:700; cursor:pointer; }
</style>