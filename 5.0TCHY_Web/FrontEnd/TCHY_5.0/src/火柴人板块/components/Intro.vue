<template>
  <section class="intro" aria-labelledby="intro-title">
    <!-- 顶部说明 -->
    <header class="hero">
      <div class="hero-top">
        <span class="badge">Taichu · 约战场</span>
      </div>
      <h2 id="intro-title" class="hero-title">企划背景与设定</h2>
      <p class="hero-subtitle">
        这是由太初寰宇社区为火柴人圈所提供的一个会长期运营和维护的新型混合成长性企划。<br />
        未来这个企划的内容和要求会随着发展和经历进行调整，所以请以最新的要求为主。<br />
        如果有意见或者想法，欢迎在太虚绘院或者八卦反馈群进行反馈！
      </p>
      <div class="alert warn" role="note" aria-label="版本提示">
        <div class="alert-icon">⚠️</div>
        <div class="alert-body">
          <strong>注意</strong>
          <p>当前版本尚未实现实际的约战功能，仅实现了人设上传；下一版本将上线约战流程。若发现问题，欢迎及时反馈。</p>
        </div>
      </div>
    </header>

    <!-- 三列信息块 -->
    <div class="cards">
      <article class="card">
        <header class="card-head">
          <div class="card-icon">📜</div>
          <h3 class="card-title">企划介绍</h3>
        </header>
        <ul class="kv">
          <li>
            <span class="k">基本概念：</span>
            <span class="v">
              混合成长型企划是太初寰宇提出的新概念，不同于只围绕单一 IP 的创作，也不同于临时性的联合或接力。
              它通过将众多创作者的内容以某种方式长期汇聚与维护，更像是一种圈内的固定娱乐活动。
            </span>
          </li>
          <li>
            <span class="k">参与方式：</span>
            <span class="v">
              画师可上传一个或多个 OC（外观、能力等基础信息）。上传后先进入审核，确保不含敏感或不当内容。
              若发现不合适的条目可举报，平台会暂下架并复审。
              创作者不得强制修改他人的既有设定；若出现严重冲突，官方将组织讨论处理。
              后续将开放更丰富的设定/故事提交入口。
            </span>
          </li>
          <li>
            <span class="k">战斗！爽！</span>
            <span class="v">
              在约战区可查看公开 OC 并发起约战（参考 dojo 道馆与 RHG）。
              完成约定内容并通过审核后，结果将被记录归档；未被官方记录的内容不纳入正式剧情。
              载体形式不限：插画、短漫、动画、文章均可，规则以双方约定为准。
            </span>
          </li>
          <li>
            <span class="k">世界填充：</span>
            <span class="v">
              企划保持“空旷”，鼓励成员在开放平台中自发填充与发展。
              只要不违背法律且不涉敏感，原则上不作限制；具体以审核标准为准。
            </span>
          </li>
        </ul>
      </article>

      <article class="card">
        <header class="card-head">
          <div class="card-icon">💡</div>
          <h3 class="card-title">为什么要做这个</h3>
        </header>
        <ul class="kv">
          <li>
            <span class="k">活跃圈子：</span>
            <span class="v">
              国内柴圈存在资源分散与活力不足的问题。我们希望通过一个长期、统一的平台激活创作与交流。
            </span>
          </li>
          <li>
            <span class="k">固定娱乐活动：</span>
            <span class="v">
              类 RHG 的活动在近年式微，缺少持续维护的平台。我们尝试提供一个“属于国内圈子”的固定活动土壤。
            </span>
          </li>
        </ul>
      </article>

      <article class="card">
        <header class="card-head">
          <div class="card-icon">🎯</div>
          <h3 class="card-title">企划目标</h3>
        </header>
        <ul class="kv">
          <li>
            <span class="k">形成闭环：</span>
            <span class="v">
              将“上传—匹配—约战—结算—归档”打通为可追溯闭环，打造随时可参与、长期运营的固定活动。
            </span>
          </li>
          <li>
            <span class="k">公平公开：</span>
            <span class="v">
              明确规则与时间节点，降低纠纷与成本，鼓励健康竞技与合作。
            </span>
          </li>
          <li>
            <span class="k">可扩展性：</span>
            <span class="v">
              为排行、赛事、联动活动等玩法预留接口与空间。
            </span>
          </li>
          <li>
            <span class="k">未来计划：</span>
            <span class="v">
              官方将发布更完整的世界观与设定说明，并纳入参与者的优秀内容。
            </span>
          </li>
        </ul>
      </article>
    </div>

    <!-- 规则与参与方式 -->
    <div class="grid">
      <article class="card">
        <header class="card-head">
          <div class="card-icon">⚖️</div>
          <h3 class="card-title">规则与约定（概要）</h3>
        </header>
        <ul class="bullets">
          <li>尊重原创与署名：自行拥有版权或具备授权，引用需注明来源。</li>
          <li>公平对战：遵循约定的能力边界与场景限制，避免“无解设定”。</li>
          <li>时间节点：按期交付；如需延期，请提前沟通并更新记录。</li>
          <li>展示与使用：默认可展示并归档为战绩，特殊限制须在约定中说明。</li>
          <li>社区规范：禁止违规内容；平台可对恶意行为进行警示、屏蔽或封禁。</li>
          <li>以上为基础模板，后续将持续迭代与完善。</li>
        </ul>
        <p class="tip">详细条款以平台正式规则与活动细则为准（后续将以独立页面呈现）。</p>
      </article>

      <article class="card">
        <header class="card-head">
          <div class="card-icon">🧭</div>
          <h3 class="card-title">参与方式</h3>
        </header>
        <ol class="steps">
          <li>
            <strong>上传人设：</strong>准备头像、简介、能力、标签与自定义规则，提交后进入人设广场（审核后展示）。
          </li>
          <li>
            <strong>浏览与约战：</strong>在列表中挑选对手，发起约战，双方确定标题、主题、规则与截止时间。
          </li>
          <li>
            <strong>完成与记录：</strong>按约定提交作品，经审核后归档为“战绩”，便于检索与展示。
          </li>
        </ol>
      </article>
    </div>

    <!-- FAQ -->
    <article class="card">
      <header class="card-head">
        <div class="card-icon">❓</div>
        <h3 class="card-title">常见问答（FAQ）</h3>
      </header>
      <div class="faq">
        <details>
          <summary>一定要公开人设吗？</summary>
          <p>建议公开。非公开将无法匹配；如确不希望公开，则无需上传。</p>
        </details>
        <details>
          <summary>作品需要统一画风或长度吗？</summary>
          <p>不需要。作品形式与复杂度由双方事先约定。</p>
        </details>
        <details>
          <summary>如果临时无法完成怎么办？</summary>
          <p>仅在提交作品后才会启动审核与记录；未完成状态下，角色默认保持在约战中，直至完成或取消。</p>
        </details>
      </div>
    </article>

    <!-- 行动按钮 -->
    <div class="actions">
      <button class="btn" type="button" @click="$emit('go-upload')">
        <span class="btn-ico">⬆️</span>
        <span>立即上传人设</span>
      </button>
      <button class="btn primary" type="button" @click="$emit('go-gallery')">
        <span class="btn-ico">🪪</span>
        <span>浏览人设与约战</span>
      </button>
    </div>
  </section>
</template>

<script setup>
defineEmits(['go-upload', 'go-gallery'])
</script>

<style scoped>
:root{
  --bg:#f7f9fc;
  --ink:#0f172a;
  --muted:#5a6478;
  --card:#ffffff;
  --ring:#e6ebf3;
  --ring-strong:#d6deea;
  --brand:#2563eb;
  --brand-ink:#0b3aa0;
  --accent:#5b8cff;
  --success:#10b981;
  --warn:#f59e0b;
  --warn-bg:#fff7ed;
  --warn-ring:#fde68a;
  --radius-lg:16px;
  --radius-md:12px;
  --shadow-sm:0 2px 10px rgba(2,6,23,.06);
  --shadow-md:0 12px 34px rgba(2,6,23,.08);
}

/* 容器 */
.intro{
  max-width: 1100px;
  margin: 0 auto;
  padding: clamp(14px,3vw,22px);
  color: var(--ink);
}

/* 英雄区 */
.hero{
  display: grid;
  gap: 12px;
  padding: 16px 18px;
  background: linear-gradient(180deg,#fff, #f8fbff);
  border:1px solid var(--ring);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
}
.hero-top{
  display:flex; justify-content:flex-start;
}
.badge{
  display:inline-block;
  padding:4px 10px;
  border-radius: 999px;
  font-weight: 800;
  font-size: 12px;
  color:#173888;
  background: linear-gradient(90deg,#e8f0ff,#eef4ff);
  border:1px solid #dbe6ff;
}
.hero-title{
  margin:0;
  font-size: clamp(20px,2.4vw,26px);
  font-weight: 900;
  letter-spacing:.2px;
}
.hero-subtitle{
  margin:0;
  color: var(--muted);
  line-height: 1.75;
}

/* 提示 */
.alert{
  display:grid;
  grid-template-columns: 22px 1fr;
  gap:10px;
  align-items:start;
  padding:10px 12px;
  border:1px solid var(--ring);
  border-radius: 12px;
  background:#f8fafc;
}
.alert.warn{
  border-color: var(--warn-ring);
  background: var(--warn-bg);
}
.alert-icon{ font-size: 16px; line-height: 22px; }
.alert-body strong{
  display:inline-block; margin-right:8px; font-weight:900; color:#7c2d12;
}
.alert-body p{ margin:.2em 0 0; color:#7c2d12; }

/* 三列卡片 */
.cards{
  margin-top: 14px;
  display:grid;
  gap:14px;
  grid-template-columns: repeat(3,1fr);
}
@media (max-width:1024px){ .cards{ grid-template-columns: repeat(2,1fr);} }
@media (max-width:680px){ .cards{ grid-template-columns: 1fr;} }

/* 通用卡片 */
.card{
  position: relative;
  background: var(--card);
  border:1px solid var(--ring);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  padding: 12px;
}
.card::before{
  content:"";
  position:absolute; inset: -1px -1px auto -1px; height: 4px;
  border-radius: 12px 12px 0 0;
  background: linear-gradient(90deg,#7aa5ff,#a0baff 60%, #d6e2ff);
  opacity: .7;
}
.card-head{
  display:grid; grid-template-columns: 22px 1fr; gap:10px; align-items:center;
  padding-bottom:8px; margin-bottom:10px; border-bottom:1px solid var(--ring);
}
.card-icon{ text-align:center; font-size: 16px; }
.card-title{ margin:0; font-size: 16px; font-weight: 900; }

/* KV 列表 */
.kv{ list-style:none; margin:0; padding:0; display:grid; gap:10px; }
.kv .k{
  display:inline-block; min-width:6em; font-weight:800; color:#0f1e3a;
}
.kv .v{
  color:#334155; line-height:1.8;
  display:inline;
}

/* 规则与参与方式：两列 */
.grid{
  margin-top: 14px;
  display:grid; gap:14px; grid-template-columns: 1.2fr .8fr;
}
@media (max-width:960px){ .grid{ grid-template-columns: 1fr; } }

/* 要点列表 */
.bullets{
  margin:0; padding-left: 1.1em; color:#334155; line-height:1.8;
  display:grid; gap:6px;
}
.bullets li::marker { color:#7aa5ff; }

/* 步骤 */
.steps{
  margin:0; padding-left:1.2em; color:#334155; line-height:1.8;
  display:grid; gap:8px;
}
.steps strong{ color:#0f1e3a; }

/* FAQ */
.faq{ display:grid; gap:10px; }
details{
  border:1px solid var(--ring);
  border-radius: 10px;
  background:#fff;
  padding: 8px 12px;
}
summary{
  cursor:pointer; font-weight:800; color:#0f1e3a; outline:none;
}
details p{ margin:6px 0 0; color:#334155; line-height:1.75; }

/* 行动区 */
.actions{
  margin-top: 16px;
  display:flex; gap:10px; justify-content:center; flex-wrap:wrap;
}
.btn{
  appearance:none; cursor:pointer;
  border:1px solid var(--ring); border-radius:12px;
  padding: 10px 14px;
  background:#fff; color:#0f172a; font-weight:800;
  display:inline-grid; grid-auto-flow: column; gap:8px; align-items:center;
  transition: background .15s, border-color .15s, box-shadow .15s, transform .12s;
}
.btn:hover{ background:#f7faff; border-color: var(--ring-strong); box-shadow: var(--shadow-sm); transform: translateY(-1px); }
.btn.primary{ background: var(--brand); color:#fff; border-color: transparent; }
.btn-ico{ font-size: 16px; line-height: 1; }
</style>