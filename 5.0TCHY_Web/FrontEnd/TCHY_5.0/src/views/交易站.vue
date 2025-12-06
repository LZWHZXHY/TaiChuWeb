<template>
  <div class="shop">
    <shop-header
      :balance="balance"
      :categories="categories"
      v-model:query="query"
      v-model:category="category"
      @open-cart="showCart = true"
    />

    <div class="content">
      <item-grid
        class="items"
        :items="filteredItems"
        @add-to-cart="addToCart"
        @view-item="openDetails"
      />

      <cart-sidebar
        class="cart"
        :visible="showCart"
        :items="cart"
        :balance="balance"
        @close="showCart = false"
        @remove="removeFromCart"
        @checkout="openCheckout"
      />
    </div>

    <purchase-modal
      v-if="checkoutItem"
      :item="checkoutItem"
      :max-quantity="getMaxQuantity(checkoutItem)"
      @cancel="checkoutItem = null"
      @confirm="handleConfirmPurchase"
    />

    <item-details
      v-if="detailItem"
      :item="detailItem"
      @close="detailItem = null"
      @add-to-cart="addToCart"
    />

    <footer class="footer">
      © 2025 虚拟道具商店 — 支持皮肤 / 改名卡 / 补签卡等虚拟商品
    </footer>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import ShopHeader from '@/TradeComponents/ShopHeader.vue'
import ItemGrid from '@/TradeComponents/ItemGrid.vue'
import CartSidebar from '@/TradeComponents/CartSidebar.vue'
import PurchaseModal from '@/TradeComponents/PurchaseModal.vue'
import ItemDetails from '@/TradeComponents/ItemDetails.vue'

const balance = ref(1200) // 用户余额示例（可替换成真实接口）
const categories = ref(['全部', '皮肤', '改名卡', '补签卡', '道具包'])

const query = ref('')
const category = ref('全部')

const items = ref([
  {
    id: 'skin-001',
    type: '皮肤',
    title: '蔚蓝猎手 皮肤',
    price: 150,
    image: '',
    description: '稀有蔚蓝主题皮肤，独特外观+特效',
    stock: 5,
    badges: ['限量', '热销'],
  },
  {
    id: 'rename-01',
    type: '改名卡',
    title: '改名卡（永久）',
    price: 50,
    image: '',
    description: '允许更改一次用户名，永久生效',
    stock: 100,
    badges: [],
  },
  {
    id: 'qian-01',
    type: '补签卡',
    title: '补签卡 x5',
    price: 20,
    image: '',
    description: '补签5次，可在签到界面使用',
    stock: 200,
    badges: ['便宜'],
  },
  {
    id: 'bundle-01',
    type: '道具包',
    title: '新手礼包',
    price: 80,
    image: '',
    description: '包含随机皮肤、补签卡和少量金币',
    stock: 50,
    badges: ['推荐'],
  },
])

const cart = ref([]) // { id, item, qty }

const showCart = ref(false)
const checkoutItem = ref(null)
const detailItem = ref(null)

const filteredItems = computed(() => {
  return items.value.filter((it) => {
    const matchQuery = query.value.trim() === '' || it.title.includes(query.value) || (it.description && it.description.includes(query.value))
    const matchCategory = category.value === '全部' || it.type === category.value
    return matchQuery && matchCategory
  })
})

function addToCart(item, qty = 1) {
  const existing = cart.value.find((c) => c.id === item.id)
  if (existing) {
    existing.qty = Math.min(item.stock, existing.qty + qty)
  } else {
    cart.value.push({ id: item.id, item, qty: Math.min(item.stock, qty) })
  }
  showCart.value = true
}

function removeFromCart(id) {
  cart.value = cart.value.filter((c) => c.id !== id)
}

function openCheckout(cartEntry) {
  // cartEntry may be undefined -> checkout entire cart (not supported in this simple modal)
  checkoutItem.value = cartEntry ? cartEntry.item : cart.value[0]?.item || null
}

function openDetails(item) {
  detailItem.value = item
}

function handleConfirmPurchase({ item, qty, total }) {
  // 简单示例：扣减余额，减少库存，清空对应购物车条目
  if (balance.value < total) {
    alert('余额不足，请充值。')
    return
  }
  balance.value -= total
  const storeItem = items.value.find((it) => it.id === item.id)
  if (storeItem) storeItem.stock = Math.max(0, storeItem.stock - qty)
  cart.value = cart.value.filter((c) => c.id !== item.id)
  checkoutItem.value = null
  alert(`购买成功：${item.title} x${qty}`)
}

function getMaxQuantity(item) {
  return item ? Math.min(item.stock, 99) : 0
}
</script>

<style scoped>
/* 简约主题（Light / Minimal）配色与布局 */
:root {
  --bg: #f6f8fa;
  --panel-bg: #ffffff;
  --muted: #6b7280;
  --text: #111827;
  --accent: #06b6d4; /* 亮色点缀，清爽的青色 */
  --border: #e6e9ef;
  --shadow: 0 6px 20px rgba(16,24,40,0.06);
  --radius: 10px;
}

.shop {
  display:flex;
  flex-direction:column;
  gap:18px;
  min-height:100vh;
  padding:28px;
  background: var(--bg);
  color: var(--text);
  box-sizing:border-box;
  font-family: Inter, ui-sans-serif, system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", Arial;
}

/* content layout with airy spacing */
.content {
  display:grid;
  grid-template-columns: 1fr 340px;
  gap:24px;
  align-items:start;
  min-height: 0;
}

/* subtle panel baseline for reuse */
.panel {
  background: var(--panel-bg);
  border-radius: var(--radius);
  padding:14px;
  box-shadow: var(--shadow);
  border: 1px solid var(--border);
}

/* footer */
.footer {
  text-align:center;
  color:var(--muted);
  font-size:13px;
  padding:12px 0;
}

/* responsiveness */
@media (max-width: 900px) {
  .content { grid-template-columns: 1fr; padding:0; gap:12px; }
  .shop { padding:16px; }
}
</style>