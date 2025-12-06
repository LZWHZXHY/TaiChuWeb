<template>
  <aside class="cart-sidebar" :class="{open: visible}">
    <div class="header">
      <h3>购物车</h3>
      <button class="close" @click="$emit('close')">关闭</button>
    </div>

    <div class="items" v-if="items && items.length">
      <div class="entry" v-for="c in items" :key="c.id">
        <div class="info">
          <div class="name">{{ c.item.title }}</div>
          <div class="qty">x{{ c.qty }}</div>
        </div>
        <div class="right">
          <div class="price">{{ c.item.price * c.qty }} 金币</div>
          <button class="remove" @click="$emit('remove', c.id)">移除</button>
        </div>
      </div>
    </div>

    <div v-else class="empty">购物车空空如也，快去挑选道具吧！</div>

    <div class="summary">
      <div>小计：<strong>{{ subtotal }} 金币</strong></div>
      <div>余额：<strong>{{ balance }} 金币</strong></div>
      <button class="checkout" :disabled="items.length===0 || balance < subtotal" @click="checkout">结算</button>
    </div>
  </aside>
</template>

<script setup>
import { computed } from 'vue'
const props = defineProps({
  visible: { type: Boolean, default: false },
  items: { type: Array, default: () => [] },
  balance: { type: Number, default: 0 },
})
const emit = defineEmits(['close', 'remove', 'checkout'])

const subtotal = computed(() => {
  return props.items.reduce((s, c) => s + c.item.price * c.qty, 0)
})

function checkout() {
  // For simplicity, trigger checkout for first item in cart
  emit('checkout', props.items[0])
}
</script>

<style scoped>
.cart-sidebar {
  background: #ffffff;
  border:1px solid #e6e9ef;
  border-radius:10px;
  padding:14px;
  width:100%;
  max-width:340px;
  transform: translateX(8px);
  box-shadow: 0 10px 30px rgba(16,24,40,0.06);
}
.header { display:flex; justify-content:space-between; align-items:center; margin-bottom:12px; }
.close { background:transparent; border:1px solid #e6e9ef; padding:8px 10px; border-radius:8px; cursor:pointer; }
.items { display:flex; flex-direction:column; gap:10px; max-height:320px; overflow:auto; padding-right:6px; }
.entry { display:flex; justify-content:space-between; align-items:center; gap:8px; padding:10px; border-radius:8px; background: transparent; border-bottom: 1px dashed #f1f3f5; }
.name { font-weight:700; color:#111827; }
.qty { color:#6b7280; }
.remove { background:transparent; border:1px solid #e6e9ef; padding:6px 8px; border-radius:8px; cursor:pointer; }
.summary { margin-top:14px; display:flex; flex-direction:column; gap:10px; }
.checkout {
  padding:10px; border-radius:8px; background: var(--accent); border:none; cursor:pointer; font-weight:700; color:#000000;
  box-shadow: 0 8px 22px rgba(6,182,212,0.12);
}
.empty { color:#6b7280; padding:20px; text-align:center; background: transparent; border-radius:8px; }
</style>