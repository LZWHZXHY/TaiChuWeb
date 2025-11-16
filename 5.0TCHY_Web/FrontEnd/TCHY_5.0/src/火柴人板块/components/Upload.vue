<template>
  <section class="card" aria-labelledby="upload-title">
    <header class="card-head">
      <h3 id="upload-title">2. 人设上传</h3>
    </header>

    <!-- 仅表单框架，不提交、不调用任何 API -->
    <form class="form" @submit.prevent>
      <div class="grid two">
        <div class="field">
          <label for="role-name">角色名称<span class="req">*</span></label>
          <input id="role-name" type="text" class="input" placeholder="例如：炎刃·赤霄" required />
        </div>
        <div class="field">
          <label for="role-faction">所属阵营/组织</label>
          <input id="role-faction" type="text" class="input" placeholder="可选项，例：太初研究会" />
        </div>
      </div>

      <div class="grid two">
        <div class="field">
          <label for="role-type">角色类型</label>
          <select id="role-type" class="select">
            <option value="">未选择</option>
            <option value="近战">近战</option>
            <option value="远程">远程</option>
            <option value="法术">法术</option>
            <option value="辅助">辅助</option>
            <option value="多面手">多面手</option>
          </select>
        </div>
        <div class="field">
          <label for="role-tier">强度/定位</label>
          <select id="role-tier" class="select">
            <option value="">未选择</option>
            <option value="入门">入门</option>
            <option value="标准">标准</option>
            <option value="高强度">高强度</option>
            <option value="超规格">超规格</option>
          </select>
        </div>
      </div>

      <div class="field">
        <label for="role-abilities">能力与设定<span class="req">*</span></label>
        <textarea id="role-abilities" class="textarea" rows="6" placeholder="描述角色的能力、限制、招式、战术偏好等…" required />
      </div>

      <div class="grid two">
        <div class="field">
          <label>立绘/头像上传</label>
          <div class="uploader" @dragover.prevent @drop.prevent>
            <div class="uploader-inner">
              <div class="uploader-icon">🖼️</div>
              <div class="uploader-text">
                <strong>点击选择图片（暂未接入）</strong>
                <span class="muted">建议 PNG/JPG，最大 5MB</span>
              </div>
            </div>
          </div>
        </div>
        <div class="field">
          <label for="role-link">参考链接（可选）</label>
          <input id="role-link" type="url" class="input" placeholder="例如：作品集/设定图地址" />
        </div>
      </div>

      <div class="field check">
        <label class="checkbox">
          <input type="checkbox" />
          <span>我已阅读并遵守社区与约战规则</span>
        </label>
      </div>

      <div class="form-actions">
        <button class="btn primary" type="submit" disabled>提交人设</button>
        <button class="btn ghost" type="button" disabled>清空</button>
      </div>

      <p class="tip">说明：此表单为静态占位。接入 API 后启用提交与校验。</p>
    </form>
  </section>
</template>

<script setup>
</script>

<style scoped>
:root { --card:#fff; --ring:#e6ebf3; --ring-2:#d6deea; --r-lg:16px; --shadow-sm:0 2px 10px rgba(2,6,23,.06); --muted:#5a6478; --brand:#2563eb; }
.card { background:var(--card); border:1px solid var(--ring); border-radius:var(--r-lg); box-shadow:var(--shadow-sm); padding:12px; }
.card-head { border-bottom:1px solid var(--ring); padding-bottom:8px; margin-bottom:10px; }
.card-head h3 { margin:0; font-size:18px; font-weight:900; }

.form { display:grid; gap:12px; }
.grid.two { display:grid; grid-template-columns:1fr 1fr; gap:10px; }
@media (max-width:640px){ .grid.two { grid-template-columns:1fr; } }

.field { display:grid; gap:6px; }
label { font-weight:800; color:#0f1e3a; }
.req { color:#ef4444; margin-left:2px; }

.input, .select, .textarea {
  background:#fff; border:1px solid var(--ring); border-radius:10px;
  padding:10px 12px; outline:none; transition:border-color .15s, box-shadow .15s;
}
.input:focus, .select:focus, .textarea:focus { border-color:var(--ring-2); box-shadow:0 0 0 3px rgba(37,99,235,.12); }

.field.check { margin-top:2px; }
.checkbox { display:inline-flex; align-items:center; gap:8px; cursor:default; }

.uploader {
  border:2px dashed var(--ring-2); border-radius:12px; background:#fcfdff;
  padding:14px; user-select:none;
}
.uploader-inner { display:grid; grid-auto-flow:column; gap:10px; align-items:center; justify-content:start; }
.uploader-icon { font-size:20px; }
.uploader-text strong { display:block; }
.uploader-text .muted { font-size:12px; color:var(--muted); }

.form-actions { display:flex; gap:10px; align-items:center; }
.btn { appearance:none; border:1px solid var(--ring); border-radius:10px; padding:10px 14px; font-weight:800; background:#fff; color:#0f172a; }
.btn.primary { background: var(--brand); color:#fff; border-color:transparent; }
.btn.ghost { background:#fff; }
.tip { color:var(--muted); }
</style>