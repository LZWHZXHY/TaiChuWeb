<template>
  <div class="grid">
    <item-card
      v-for="it in items"
      :key="it.id"
      :item="it"
      @add="(i, qty)=>$emit('add-to-cart', i, qty)"
      @view="(i)=>$emit('view-item', i)"
    />
    <div v-if="items.length === 0" class="empty">未找到符合条件的商品。</div>
  </div>
</template>

<script setup>
import ItemCard from './ItemCard.vue'
const props = defineProps({
  items: { type: Array, default: () => [] },
})
</script>

<style scoped>
.grid {
  display:grid;
  grid-template-columns: repeat(auto-fill,minmax(220px,1fr));
  gap:16px;
  align-items: start;
  align-content: start;
  padding:6px;
}
.empty {
  color:#6b7280;
  padding:20px;
  grid-column:1/-1;
  text-align:center;
  background: transparent;
  border-radius:8px;
}
</style>