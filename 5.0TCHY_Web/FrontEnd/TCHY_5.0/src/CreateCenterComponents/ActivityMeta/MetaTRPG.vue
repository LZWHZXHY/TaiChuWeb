<template>
  <div class="meta-grid trpg-grid">
    <div class="md-input-box">
      <label>使用规则 // SYSTEM</label>
      <input v-model="meta.system" type="text" placeholder="例: COC 7th, DND 5e..." />
    </div>
    <div class="md-input-box">
      <label>模组难度 // DIFFICULTY</label>
      <input v-model="meta.difficulty" type="text" placeholder="例: 新人模组 / 刁钻..." />
    </div>
    <div class="md-input-box">
      <label>时代与地点 // SETTING</label>
      <input v-model="meta.era" type="text" placeholder="例: 现代日本 / 1920s美国..." />
    </div>
    <div class="md-input-box">
      <label>模组时长 // DURATION</label>
      <input v-model="meta.duration" type="text" placeholder="例: 约5-10小时 / 3个晚上..." />
    </div>
    <div class="md-input-box">
      <label>推荐人数 // PLAYER_LIMIT</label>
      <input v-model="meta.playerLimit" type="text" placeholder="例: 1-4人" />
    </div>
    <div class="md-input-box">
      <label>强制携卡报名 // REQUIRE_OC_CARD</label>
      <select v-model="meta.requireOC">
        <option :value="true">是 (申请者必须挂载已有OC角色卡)</option>
        <option :value="false">否 (入群后发车卡)</option>
      </select>
    </div>
    <div class="md-input-box" style="grid-column: span 2;">
      <label>车卡要求与技能推荐 // BUILD_RULES</label>
      <input v-model="meta.buildRules" type="text" placeholder="例: 520点不含运，技能上限80/70。推荐侦查、图书馆..." />
    </div>
    <div class="md-input-box" style="grid-column: span 2;">
      <label>排雷与不适预警 // WARNINGS</label>
      <input v-model="meta.warnings" type="text" placeholder="有无引起不适的内容？例: 含有轻微血腥、精神控制元素..." />
    </div>
  </div>
</template>

<script setup>
defineProps({
  meta: { type: Object, required: true }
})
</script>

<style scoped>
.meta-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0 20px; }
.md-input-box { margin-bottom: 20px; flex: 1; }
.md-input-box label { font-size: 10px; font-weight: 900; color: #888; margin-bottom: 8px; display: block; text-transform: uppercase; }
.md-input-box input, .md-input-box select { width: 100%; box-sizing: border-box; border: 2px solid #121212; background: #fff; padding: 12px; font-family: inherit; font-size: 14px; transition: 0.2s; }
</style>