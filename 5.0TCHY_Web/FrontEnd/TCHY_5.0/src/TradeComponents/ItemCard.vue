<template>
  <div class="card" role="article" :aria-label="item.title">
    <div class="media" :style="thumbStyle" :aria-hidden="!!propsImage">
      <div class="media-overlay">
        <div class="price-badge">{{ item.price }} <small>金币</small></div>
        <div class="badges">
          <span v-for="b in item.badges" :key="b" class="badge">{{ b }}</span>
        </div>
      </div>
      <img v-if="propsImage" :src="item.image" alt="" class="thumb-img" />
      <div v-else class="thumb-fallback">{{ placeholder }}</div>
    </div>

    <div class="body">
      <h3 class="title" @click="$emit('view', item)">{{ item.title }}</h3>
      <p class="desc">{{ item.description }}</p>

      <div class="meta">
        <div class="stock" :class="{ out: item.stock === 0 }">
          {{ item.stock > 0 ? ('库存 ' + item.stock) : '已售罄' }}
        </div>
        <div class="spacer"></div>
      </div>

      <div class="actions">
        <button
          :disabled="item.stock===0"
          @click="$emit('add', item, 1)"
          class="buy"
          aria-disabled="item.stock===0"
        >
          加入购物车
        </button>
        <button class="details" @click="$emit('view', item)">详情</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  item: { type: Object, required: true },
})
const placeholder = '预览'
const propsImage = computed(() => !!props.item.image)

const thumbStyle = computed(() => {
  // background fallback for non-image state removed; use <img> for better performance/alt handling
  return props.item.image ? { backgroundColor: 'transparent' } : { background: 'linear-gradient(180deg,#f7fafc,#ffffff)' }
})
</script>

<style scoped>
:root{
  --card-bg: #ffffff;
  --muted: #6b7280;
  --text: #111827;
  --accent: #06b6d4;
  --accent-2: #0ea5a7;
  --border: #e6e9ef;
  --radius: 12px;
  --soft-shadow: 0 8px 24px rgba(16,24,40,0.06);
  --hover-shadow: 0 18px 50px rgba(16,24,40,0.09);
}

/* Card base */
.card{
  display:flex;
  flex-direction:column;
  align-self: start;
  background: var(--card-bg);
  border-radius: var(--radius);
  padding:12px;
  gap:12px;
  box-sizing:border-box;
  border: 1px solid var(--border);
  transition: transform .16s cubic-bezier(.2,.9,.2,1), box-shadow .16s;
  box-shadow: var(--soft-shadow);
  overflow: hidden;
  min-height: 220px;
}
.card:focus-within,
.card:hover{
  transform: translateY(-6px);
  box-shadow: var(--hover-shadow);
  outline: none;
}

/* media area */
.media{
  position: relative;
  width: 100%;
  height: 140px;
  border-radius: 10px;
  overflow: hidden;
  display:flex;
  align-items:center;
  justify-content:center;
  background: linear-gradient(180deg,#fbfdff,#f3f6f9);
  border: 1px solid var(--border);
}

/* real image (better for performance & accessibility) */
.thumb-img{
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
  object-position: center;
  display:block;
}

/* fallback content when no image */
.thumb-fallback{
  color: var(--muted);
  font-weight:700;
  font-size:14px;
  z-index: 1;
  padding: 8px 12px;
}

/* overlay contains price and badges (keeps them readable on images) */
.media-overlay{
  position: absolute;
  inset: 0;
  display:flex;
  flex-direction:column;
  justify-content:space-between;
  padding:10px;
  pointer-events: none; /* keep overlay non-interactive */
  background: linear-gradient(180deg, rgba(255,255,255,0.00) 30%, rgba(255,255,255,0.02) 100%);
}

/* price badge top-right */
.price-badge{
  pointer-events: auto;
  align-self: flex-end;
  background: linear-gradient(90deg, rgba(6,182,212,0.95), rgba(6,182,212,0.85));
  color: #fff;
  font-weight: 800;
  padding:6px 10px;
  border-radius: 999px;
  font-size: 13px;
  box-shadow: 0 6px 18px rgba(6,182,212,0.12);
  border: 1px solid rgba(255,255,255,0.1);
}
.price-badge small{
  font-weight:600;
  font-size:10px;
  opacity:0.9;
  margin-left:6px;
}

/* badges group bottom-left */
.badges{
  display:flex;
  gap:8px;
  align-items:center;
}
.badge{
  pointer-events: auto;
  background: rgba(255,255,255,0.88);
  color: var(--text);
  font-weight:700;
  padding:6px 8px;
  border-radius:999px;
  font-size:12px;
  border: 1px solid rgba(16,24,40,0.04);
  box-shadow: 0 4px 10px rgba(2,6,23,0.04);
}

/* body */
.body{
  display:flex;
  flex-direction:column;
  gap:8px;
}

/* title */
.title{
  margin:0;
  cursor:pointer;
  font-size:15px;
  color:var(--text);
  font-weight:700;
  line-height:1.15;
}
.title:hover{ text-decoration: underline; }

/* description with clamp to 3 lines */
.desc{
  margin:0;
  color:var(--muted);
  font-size:13px;
  line-height:1.5;
  display:-webkit-box;
  -webkit-line-clamp:3;
  -webkit-box-orient:vertical;
  overflow:hidden;
  text-overflow:ellipsis;
}

/* meta row */
.meta{
  display:flex;
  align-items:center;
  gap:8px;
  margin-top:4px;
}
.stock{
  color:var(--muted);
  font-weight:600;
  font-size:13px;
}
.stock.out{ color:#ef4444; }

/* push actions to bottom if card taller */
.spacer{ flex:1 1 auto; }

/* actions row */
.actions{
  display:flex;
  gap:8px;
  align-items:center;
}
button{
  padding:9px 12px;
  border-radius:10px;
  border: 1px solid var(--border);
  cursor: pointer;
  font-weight:700;
  background: transparent;
  color: var(--text);
  transition: transform .08s, box-shadow .08s, opacity .12s;
}
.buy{
  background: linear-gradient(90deg, var(--accent), var(--accent-2));
  color: #000000;
  border: none;
  box-shadow: 0 8px 22px rgba(6,182,212,0.12);
}
.buy:active{ transform: translateY(1px) scale(.998); }
.buy:disabled{ opacity:0.6; cursor:not-allowed; box-shadow:none; }
.details{
  background: transparent;
  color: var(--muted);
}

/* focus states for accessibility */
button:focus, .title:focus{
  outline: 3px solid rgba(6,182,212,0.14);
  outline-offset: 2px;
}

/* responsive */
@media (max-width: 600px){
  .media{ height: 110px; }
  .card{ padding:10px; min-height:unset; }
  .price-badge{ font-size:12px; padding:5px 8px; }
  .badge{ font-size:11px; padding:5px 7px; }
}
</style>