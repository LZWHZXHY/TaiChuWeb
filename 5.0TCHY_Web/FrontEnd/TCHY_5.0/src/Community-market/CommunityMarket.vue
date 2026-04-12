<template>
  <div class="community-market">
    <header class="market-top-bar">
      <div class="user-finance">
        <div class="finance-item">
          <span class="label">当前可用余额</span>
          <div class="amount-group">
            <span class="currency-icon">💠</span>
            <span class="amount">12,840</span>
            <span class="unit">太初点</span>
          </div>
        </div>
        <button class="btn-recharge">充值接入</button>
      </div>
      
      <div class="top-actions">
        <button class="btn-ghost">我的仓库</button>
        <button class="btn-ghost">交易记录</button>
        <div class="search-box">
          <input type="text" placeholder="搜索道具名称..." />
        </div>
      </div>
    </header>

    <div class="market-body">
      <aside class="market-nav">
        <section class="nav-group">
          <h3 class="nav-title">装扮中心</h3>
          <nav class="nav-links">
            <button class="nav-link active">头像边框</button>
            <button class="nav-link">个性挂件</button>
            <button class="nav-link">聊天气泡</button>
            <button class="nav-link">主页背景</button>
          </nav>
        </section>

        <section class="nav-group">
          <h3 class="nav-title">功能道具</h3>
          <nav class="nav-links">
            <button class="nav-link">改名卡片</button>
            <button class="nav-link">全服广播</button>
            <button class="nav-link">身份认证</button>
          </nav>
        </section>

        <section class="nav-group">
          <h3 class="nav-title">限定收藏</h3>
          <nav class="nav-links">
            <button class="nav-link highlight">年度勋章</button>
            <button class="nav-link">绝版套装</button>
          </nav>
        </section>
      </aside>

      <main class="item-grid custom-scroll">
        <div class="prop-card" v-for="n in 12" :key="n">
          <div class="rarity-line" :class="{ rare: n % 4 === 0 }"></div>
          
          <div class="prop-preview">
            <div class="avatar-demo">
              <img src="https://via.placeholder.com/60" class="base-avatar" />
              <div class="frame-overlay" :style="{ borderColor: n % 4 === 0 ? '#0891B2' : '#E2E8F0' }"></div>
            </div>
            <span class="prop-tag" v-if="n % 4 === 0">限定</span>
          </div>

          <div class="prop-details">
            <div class="prop-header">
              <h4 class="prop-name">核心观测者 框架</h4>
              <span class="prop-id">#编号-{{ 1000 + n }}</span>
            </div>
            
            <p class="prop-desc">应用此边框后，您的头像将在社区全域获得青蓝色动态流光效果。</p>

            <div class="prop-footer">
              <div class="price-box">
                <span class="price">500</span>
                <span class="unit">太初点</span>
              </div>
              <button class="btn-buy" :class="{ 'btn-rare': n % 4 === 0 }">
                {{ n % 4 === 0 ? '申领' : '购买' }}
              </button>
            </div>
          </div>
        </div>
      </main>
    </div>
  </div>
</template>

<style scoped>
.community-market {
  --market-white: #FFFFFF;
  --market-bg: #F8FAFC;
  --market-cyan: #0891B2;
  --market-text: #1E293B;
  --market-sub: #64748B;
  --market-border: #E2E8F0;

  display: flex;
  flex-direction: column;
  height: 100%;
  background: var(--market-white);
  color: var(--market-text);
  font-family: 'PingFang SC', sans-serif;
}

/* 顶部栏 */
.market-top-bar {
  padding: 15px 30px;
  border-bottom: 1px solid var(--market-border);
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #FFF;
}

.user-finance { display: flex; align-items: center; gap: 20px; }
.finance-item .label { font-size: 0.7rem; color: var(--market-sub); font-weight: 600; }
.amount-group { display: flex; align-items: center; gap: 5px; margin-top: 2px; }
.amount { font-size: 1.2rem; font-weight: 800; color: var(--market-cyan); font-family: monospace; }
.unit { font-size: 0.7rem; color: var(--market-sub); font-weight: 600; }

.btn-recharge {
  background: var(--market-cyan); color: #FFF; border: none;
  padding: 6px 15px; border-radius: 4px; font-size: 0.8rem; font-weight: 600; cursor: pointer;
}

.top-actions { display: flex; gap: 10px; align-items: center; }
.btn-ghost {
  background: transparent; border: 1px solid var(--market-border);
  padding: 6px 12px; border-radius: 4px; font-size: 0.8rem; color: var(--market-sub);
  cursor: pointer; transition: 0.2s;
}
.btn-ghost:hover { border-color: var(--market-cyan); color: var(--market-cyan); }

.search-box input {
  border: 1px solid var(--market-border); padding: 6px 12px; border-radius: 4px;
  font-size: 0.8rem; outline: none; width: 180px;
}

/* 主体布局 */
.market-body { flex: 1; display: flex; overflow: hidden; }

/* 导航项 */
.market-nav { width: 200px; border-right: 1px solid var(--market-border); padding: 25px 15px; }
.nav-group { margin-bottom: 30px; }
.nav-title { font-size: 0.75rem; font-weight: 800; color: var(--market-sub); margin-bottom: 10px; padding-left: 10px; }
.nav-links { display: flex; flex-direction: column; gap: 4px; }
.nav-link {
  text-align: left; padding: 10px 12px; border: none; background: transparent;
  font-size: 0.9rem; color: var(--market-sub); border-radius: 6px; cursor: pointer; transition: 0.2s;
}
.nav-link:hover { background: var(--market-bg); color: var(--market-text); }
.nav-link.active { background: rgba(8, 145, 178, 0.08); color: var(--market-cyan); font-weight: 600; }
.nav-link.highlight { color: #D97706; }

/* 道具卡片网格 */
.item-grid {
  flex: 1; padding: 25px; display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 20px; overflow-y: auto; background: var(--market-bg);
}

.prop-card {
  background: #FFF; border: 1px solid var(--market-border); border-radius: 8px;
  display: flex; flex-direction: column; position: relative; transition: 0.3s;
}
.prop-card:hover { transform: translateY(-3px); box-shadow: 0 8px 20px rgba(0,0,0,0.05); border-color: var(--market-cyan); }

.rarity-line { height: 3px; background: #E2E8F0; width: 100%; }
.rarity-line.rare { background: var(--market-cyan); }

.prop-preview {
  height: 120px; background: #FAFAFA; display: flex; align-items: center;
  justify-content: center; position: relative; border-bottom: 1px solid #F1F5F9;
}
.avatar-demo { position: relative; width: 60px; height: 60px; }
.base-avatar { width: 100%; height: 100%; border-radius: 50%; }
.frame-overlay {
  position: absolute; inset: -5px; border: 3px solid #E2E8F0; border-radius: 50%;
}

.prop-tag {
  position: absolute; top: 10px; right: 10px; background: var(--market-cyan);
  color: #FFF; font-size: 0.6rem; padding: 2px 6px; border-radius: 3px;
}

.prop-details { padding: 15px; flex: 1; display: flex; flex-direction: column; }
.prop-header { display: flex; justify-content: space-between; align-items: baseline; margin-bottom: 8px; }
.prop-name { font-size: 1rem; font-weight: 700; }
.prop-id { font-size: 0.65rem; color: var(--market-sub); font-family: monospace; }
.prop-desc { font-size: 0.75rem; color: var(--market-sub); line-height: 1.5; margin-bottom: 15px; }

.prop-footer {
  margin-top: auto; border-top: 1px solid #F1F5F9; padding-top: 12px;
  display: flex; justify-content: space-between; align-items: center;
}
.price { font-size: 1.1rem; font-weight: 800; color: var(--market-text); font-family: monospace; }
.btn-buy {
  background: #F1F5F9; color: var(--market-text); border: none;
  padding: 6px 16px; border-radius: 4px; font-size: 0.8rem; font-weight: 600; cursor: pointer;
  transition: 0.2s;
}
.btn-buy:hover { background: #E2E8F0; }
.btn-buy.btn-rare { background: var(--market-cyan); color: #FFF; }
.btn-buy.btn-rare:hover { background: #0e7490; }
</style>