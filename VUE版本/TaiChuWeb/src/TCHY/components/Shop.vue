<template>
  <div class="delta-shop-root">
    <header class="delta-shop-header">
      <div class="delta-shop-title">太初交易行</div>
      <div class="delta-shop-desc">消耗间隙粒子，兑换专属道具，定制你的身份！</div>
      <div class="delta-shop-user-particle">
        <span class="delta-particle-label">我的间隙粒子：</span>
        <img src="#" class="delta-particle-icon" />
        <span class="delta-particle-value">{{ particle }}</span>
      </div>
    </header>
    <div class="delta-shop-toolbar">
      <div class="delta-shop-categories">
        <button
          v-for="cat in ['全部', ...categories]"
          :key="cat"
          @click="selectedCategory = (cat === '全部' ? '' : cat)"
          :class="['delta-cat-btn', { active: selectedCategory === cat || (!selectedCategory && cat === '全部') }]"
        >{{ cat }}</button>
      </div>
      <input v-model="search" placeholder="搜索道具/用途" class="delta-shop-search" />
    </div>
    <div class="delta-shop-items-scroll">
      <div class="delta-shop-items">
        <transition-group name="delta-shop-item" tag="div" class="delta-shop-items-grid">
          <div v-for="item in filteredItems"
               :key="item.id"
               class="delta-shop-item-card"
               :class="{ 'red-level': item.level === 7 }"
               :style="{ background: getLevelBg(item.level) }"
          >
            <div class="delta-shop-item-img-wrap">
              <img :src="getItemIconUrl(item.icon)"
                class="delta-shop-item-img"
                @error="event => event.target.src = '/static/img/default-item.png'" />
            </div>
            <div class="delta-shop-item-info">
              <div class="delta-shop-item-name">{{ item.name }}</div>
              <div class="delta-shop-item-level">
                等级：
                <span :style="{color: item.level === 7 ? '#ff3940' : '#fff', fontWeight: item.level === 7 ? 'bold' : 'normal'}">
                  {{ item.level }}
                </span>
                <span v-if="item.requireLevel" style="color:#ffe66d; margin-left: 8px;">
                  (购买需等级{{ item.requireLevel }})
                </span>
              </div>
              <div class="delta-shop-item-desc">{{ item.desc }}</div>
              <div class="delta-shop-item-stock">
                库存: {{ item.stock === -1 ? '无限' : item.stock }}
              </div>
            </div>
            <div class="delta-shop-item-bottom">
              <div class="delta-shop-item-cost">
                <img src="#" class="delta-particle-icon" />
                <span>{{ item.cost }}</span>
              </div>
              <button class="delta-shop-buy-btn"
                      :disabled="userLevel < (item.requireLevel || 1)"
                      @click="buy(item)"
                      :class="{ 'disabled-btn': userLevel < (item.requireLevel || 1) }"
              >
                {{ userLevel < (item.requireLevel || 1) ? '等级不足' : '兑换' }}
              </button>
            </div>
          </div>
        </transition-group>
        <div v-if="filteredItems.length === 0" class="delta-shop-empty">
          <img src="#" class="delta-shop-empty-img" />
          <div>未找到相关道具</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import api from '../../../server/api'

const categories = ['功能卡', '装饰', '称号']
const shopItems = ref([])
const selectedCategory = ref('')
const search = ref('')
const particle = ref('...')
const userLevel = ref(1) // 用户等级

const baseApiUrl = import.meta.env.DEV
  ? "https://localhost:44321"
  : "https://bianyuzhou.com";



function getItemIconUrl(iconPath) {
  if (!iconPath) return '/static/img/default-item.png'
  return `${baseApiUrl}/api/UserInfo/imageproxy?path=${encodeURIComponent(iconPath)}`
}



// 获取用户等级，假设接口 /api/Trade/userinfo 返回 { level: 5 }
async function fetchUserLevel() {
  try {
    const res = await api.get('/api/Trade/userinfo')
    userLevel.value = res?.level ?? 1
  } catch (e) {
    userLevel.value = 1
  }
}

async function fetchShopItems() {
  try {
    const res = await api.get('/api/Trade/items')
    shopItems.value = res || []
  } catch (e) {
    alert('获取商品列表失败')
  }
}

async function fetchParticle() {
  try {
    const res = await api.get('/api/Trade/Points')
    particle.value = res?.points ?? 0
  } catch (e) {
    particle.value = '获取失败'
  }
}

onMounted(() => {
  fetchUserLevel()
  fetchShopItems()
  fetchParticle()
})

const filteredItems = computed(() => {
  let items = shopItems.value
  if (selectedCategory.value) {
    items = items.filter(i => i.category === selectedCategory.value)
  }
  if (search.value.trim()) {
    const key = search.value.trim()
    items = items.filter(
      i => i.name.includes(key) || i.desc.includes(key)
    )
  }
  return items
})

// 7级专属红色渐变+发光，其它等级自定义
// getLevelBg 代码
function getLevelBg(level) {
  if (level === 7) {
    // 低调暗红渐变
    return `linear-gradient(135deg, #2b161b 0%, #6d222e 60%, #a93a4e 100%)`;
  }
  if (level === 6) return 'linear-gradient(120deg, #d4aaff 76%, #24355b 100%)'
  if (level === 5) return 'linear-gradient(120deg, #ffe2f7 76%, #24355b 100%)'
  if (level === 4) return 'linear-gradient(120deg, #ffdba6 76%, #24355b 100%)'
  if (level === 3) return 'linear-gradient(120deg, #f4fdad 76%, #24355b 100%)'
  if (level === 2) return 'linear-gradient(120deg, #b8ffd1 76%, #24355b 100%)'
  if (level === 1) return 'linear-gradient(120deg, #bcdcff 76%, #24355b 100%)'
  return 'linear-gradient(120deg,#24355b 90%,#181c23 100%)'
}

async function buy(item) {
  if (userLevel.value < (item.requireLevel || 1)) {
    alert('您的账户等级不足，无法兑换该道具！')
    return
  }
  try {
    const res = await api.post('/api/Trade/buy', {
      ItemId: item.id,
      Count: 1
    })
    alert(res.message || '兑换成功')
    fetchParticle()
    fetchShopItems()
  } catch (e) {
    alert(e.message || '兑换失败')
  }
}
</script>

<style scoped>
.delta-shop-root {
  min-height: 100vh;
  background: radial-gradient(ellipse at 60% 40%, #30455e 0%, #111927 85%) fixed;
  padding-bottom: 80px;
  font-family: "Segoe UI", "PingFang SC", "Microsoft YaHei", sans-serif;
}
.delta-shop-header {
  text-align: left;
  padding: 56px 0 18px 32px;
  background: linear-gradient(100deg,rgba(80,120,200,.12) 65%,rgba(70,110,180,.09) 100%);
  border-bottom: 1.5px solid #22304677;
  margin-bottom: 18px;
  box-shadow: 0 12px 36px #1a2b4b11;
  position: relative;
}
.delta-shop-user-particle {
  position: absolute;
  right: 36px;
  top: 46px;
  background: rgba(50,80,140,0.32);
  border-radius: 14px;
  padding: 7px 24px 7px 12px;
  font-size: 1.13rem;
  color: #ffe66d;
  font-weight: bold;
  box-shadow: 0 2px 12px #38e6e055;
  display: flex;
  align-items: center;
  z-index: 2;
  letter-spacing: 1.3px;
}
.delta-particle-label {
  color: #c8e3ff;
  font-size: 1.04rem;
  margin-right: 5px;
}
.delta-particle-icon {
  width: 22px;
  height: 22px;
  vertical-align: middle;
  margin-right: 4px;
}
.delta-particle-value {
  font-size: 1.18rem;
  color: #ffe66d;
  font-weight: 900;
}
.delta-shop-title {
  font-size: 2.46rem;
  font-weight: bold;
  letter-spacing: 5px;
  color: #e4eaff;
  text-shadow: 0 6px 28px #1e47a099, 0 1px 2px #223;
}
.delta-shop-desc {
  color: #8bb4ff;
  margin-top: 7px;
  font-size: 1.11rem;
  letter-spacing: 2px;
  text-shadow: 0 2px 10px #4a9aff44;
}
.delta-shop-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 28px;
  align-items: center;
  justify-content: space-between;
  max-width: 1080px;
  margin: 0 auto 26px auto;
  padding: 0 18px;
}
.delta-shop-categories {
  display: flex;
  gap: 16px;
}
.delta-cat-btn {
  border: none;
  outline: none;
  background: #253958;
  color: #a4c6ff;
  border-radius: 20px;
  font-size: 1.09rem;
  padding: 8px 32px;
  font-weight: bold;
  letter-spacing: 1.5px;
  box-shadow: 0 2px 12px #1348a033;
  transition: all 0.18s;
  cursor: pointer;
  position: relative;
  border-bottom: 2.5px solid #2f4d77;
}
.delta-cat-btn.active, .delta-cat-btn:hover {
  background: linear-gradient(100deg, #3478fc 60%, #3be6e0 120%);
  color: #fff;
  border-bottom: 2.5px solid #42ffe3;
  box-shadow: 0 8px 32px #1cd0ff44;
  transform: scale(1.06);
}
.delta-shop-search {
  padding: 8px 20px;
  border-radius: 16px;
  border: 1.7px solid #355085;
  background: #1c2941cc;
  color: #e1eaff;
  font-size: 1.09rem;
  width: 240px;
  outline: none;
  box-shadow: 0 2px 12px #2c87fe33;
  transition: border 0.15s, box-shadow 0.16s;
}
.delta-shop-search:focus {
  border: 2px solid #38d6f7;
  box-shadow: 0 4px 18px #38d6f733;
}
.delta-shop-items-scroll {
  max-width: 1160px;
  margin: 0 auto;
  padding: 0 12px;
}
.delta-shop-items {
  margin-top: 12px;
}
.delta-shop-items-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(310px, 1fr));
  gap: 38px;
}
.delta-shop-item-card {
  background: linear-gradient(120deg,rgba(40,80,130,0.95) 76%,rgba(28,44,67,.98) 100%);
  border-radius: 22px;
  box-shadow: 0 8px 32px #46b1ff44, 0 2.5px 12px #0c1d2c77;
  border: 1.6px solid #2d5b9b88;
  padding: 22px 20px 22px 22px;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  min-height: 270px;
  position: relative;
  overflow: hidden;
  transition: box-shadow 0.18s, transform 0.18s, border 0.14s;
}
/* style scoped，插入到你的 style 里 */
.delta-shop-item-card.red-level {
  box-shadow: 0 0 8px 0 #a93a4e88, 0 4px 32px #2b161b66, 0 2.5px 12px #a93a4e44;
  border: 2px solid #b05a5a;
  filter: brightness(1) saturate(1.07);
  color: #ffeaea;
}
.delta-shop-item-card.red-level .delta-shop-item-level,
.delta-shop-item-card.red-level .delta-shop-item-name {
  color: #ffd6d6 !important;
  text-shadow: 0 1px 6px #2b161b77, 0 1px 0 #000;
}
.delta-shop-item-card.red-level .delta-shop-item-desc {
  color: #ffbfc7 !important;
}
.delta-shop-item-card.red-level .delta-shop-item-stock {
  color: #ffbfa6 !important;
}
.delta-shop-item-card.red-level .delta-shop-buy-btn {
  background: linear-gradient(90deg, #6d222e, #a93a4e 90%);
  color: #fff0f0 !important;
  border: 1px solid #c57272;
  box-shadow: 0 0 8px #a93a4e55;
}
.delta-shop-item-card.red-level .delta-shop-buy-btn:hover:enabled {
  background: linear-gradient(90deg, #a93a4e 60%, #6d222e 120%);
}
.delta-shop-item-card:hover {
  box-shadow: 0 18px 46px #29bcff77, 0 2.5px 12px #38e6e077;
  border: 2.5px solid #38e6e0;
  transform: translateY(-7px) scale(1.035);
}
.delta-shop-item-img-wrap {
  position: relative;
  margin-bottom: 12px;
}
.delta-shop-item-img {
  width: 84px;
  height: 84px;
  border-radius: 16px;
  background: #24355b;
  object-fit: contain;
  border: 2.5px solid #5db5ff33;
  box-shadow: 0 2px 16px #2f9dff3a;
}
.delta-shop-item-info {
  width: 100%;
  margin-bottom: 11px;
}
.delta-shop-item-name {
  font-size: 1.19rem;
  font-weight: bold;
  color: #eaf4ff;
  margin-bottom: 7px;
  letter-spacing: 2px;
  text-shadow: 0 2px 8px #1e47a055;
}
.delta-shop-item-level {
  font-size: 0.95rem;
  font-weight: bold;
  margin-bottom: 4px;
  letter-spacing: 1px;
}
.delta-shop-item-desc {
  color: #a2d2ff;
  font-size: 1.01rem;
  line-height: 1.8;
  min-height: 38px;
  letter-spacing: 0.5px;
}
.delta-shop-item-stock {
  color: #ffe66d;
  font-size: 0.98rem;
  margin-top: 4px;
  font-weight: bold;
  letter-spacing: 1.5px;
}
.delta-shop-item-bottom {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
  gap: 8px;
}
.delta-shop-item-cost {
  color: #38e6e0;
  font-size: 1.09rem;
  font-weight: bold;
  display: flex;
  align-items: center;
  gap: 4px;
  text-shadow: 0 1px 12px #38e6e066;
}
.delta-shop-buy-btn {
  padding: 8px 28px;
  border: none;
  border-radius: 14px;
  background: linear-gradient(90deg, #38e6e0, #3478fc 80%);
  color: #fff;
  font-size: 1.08rem;
  font-weight: bold;
  box-shadow: 0 2px 12px #38e6e055;
  cursor: pointer;
  letter-spacing: 2px;
  transition: background 0.17s, transform 0.12s, box-shadow 0.16s;
}
.delta-shop-buy-btn:hover:enabled {
  background: linear-gradient(90deg, #3478fc 60%, #38e6e0 120%);
  box-shadow: 0 4px 18px #38e6e033;
  transform: scale(1.08);
}
.delta-shop-buy-btn.disabled-btn,
.delta-shop-buy-btn:disabled {
  background: #bbb !important;
  color: #fff !important;
  cursor: not-allowed;
  box-shadow: none;
  opacity: 0.85;
}
.delta-shop-empty {
  text-align: center;
  color: #6cbfff;
  font-size: 1.13rem;
  margin: 80px 0 0 0;
}
.delta-shop-empty-img {
  width: 120px;
  opacity: 0.55;
  margin-bottom: 18px;
}
.delta-shop-item-enter-active, .delta-shop-item-leave-active {
  transition: all .38s cubic-bezier(.43,1.4,.47,.98);
}
.delta-shop-item-enter-from, .delta-shop-item-leave-to {
  opacity: 0;
  transform: translateY(32px) scale(.93);
}
</style>