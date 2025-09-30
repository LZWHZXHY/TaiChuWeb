<template>
  <div class="community-rules">
    <h1>太初寰宇社区规则</h1>
    <div
      v-for="(rulesArr, section) in rules"
      :key="section"
      class="rule-card"
    >
      <div class="card-header">
        <span class="section-title">{{ section }}</span>
      </div>
      <ol class="rule-list">
        <template v-for="(rule, idx) in rulesArr" :key="idx">
          <li class="rule-item">
            <span class="rule-text">
              <b>{{ idx + 1 }}.</b> {{ rule.text }}
            </span>
            <!-- 子层 -->
            <ol
              v-if="rule.children && rule.children.length"
              class="rule-list child"
            >
              <template v-for="(child, cidx) in rule.children" :key="cidx">
                <li class="rule-item">
                  <span class="rule-text">
                    <b>{{ (idx + 1) + '.' + (cidx + 1) }}.</b> {{ child.text }}
                  </span>
                  <!-- 孙层 -->
                  <ol
                    v-if="child.children && child.children.length"
                    class="rule-list child"
                  >
                    <li
                      v-for="(gchild, gcidx) in child.children"
                      :key="gcidx"
                      class="rule-item"
                    >
                      <span class="rule-text">
                        <b>{{ (idx + 1) + '.' + (cidx + 1) + '.' + (gcidx + 1) }}.</b>
                        {{ gchild.text }}
                      </span>
                    </li>
                  </ol>
                </li>
              </template>
            </ol>
          </li>
        </template>
      </ol>
    </div>
  </div>
</template>

<script setup>
import rules from '@/assets/rules.json'
</script>

<style scoped>
:root {
  --main-bg: #f7f9fb;
  --card-bg: #fff;
  --card-border: #50b883;
  --title-color: #222;
  --section-color: #2c3e50;
  --text-color: #223045;
  --index-color: #266c36;
  --sub-text: #546e7a;
  --shadow: 0 2px 10px rgba(80, 120, 130, 0.07);
  --shadow-hover: 0 6px 28px rgba(80, 120, 130, 0.13);
}

.community-rules {
  background: var(--main-bg);
  min-height: 100vh;
  max-width: 800px;
  margin: 0 auto 64px auto;
  padding: 0 18px 60px 18px;
  font-family: 'HarmonyOS Sans', 'PingFang SC', 'Microsoft YaHei', Arial, sans-serif;
  color: var(--text-color);
}

h1 {
  text-align: center;
  margin: 48px 0 36px 0;
  font-weight: 800;
  font-size: 2.35rem;
  color: var(--title-color);
  letter-spacing: 2.2px;
  text-shadow: 0 2px 8px #0001;
}

.rule-card {
  background: var(--card-bg);
  border-radius: 15px;
  box-shadow: var(--shadow);
  margin-bottom: 38px;
  padding: 28px 34px 20px 34px;
  border-left: 7px solid var(--card-border);
  transition: box-shadow 0.18s;
}

.rule-card:hover {
  box-shadow: var(--shadow-hover);
}

.card-header {
  margin-bottom: 16px;
  display: flex;
  align-items: center;
}

.section-title {
  font-size: 1.22em;
  font-weight: bold;
  color: var(--section-color);
  letter-spacing: 1.5px;
}

.rule-list {
  margin-left: 0;
  padding-left: 23px;
  list-style: none;
}

.rule-item {
  margin-bottom: 13px;
  font-size: 1.08em;
  line-height: 1.85;
  position: relative;
  list-style: none;
  color: var(--text-color);
}

.rule-text {
  display: inline-block;
  margin-bottom: 5px;
  font-size: 1em;
  font-weight: 400;
  word-break: break-word;
}

.rule-index {
  color: var(--index-color);
  font-weight: 700;
  font-size: 1.04em;
  margin-right: 0.4em;
  text-shadow: 0 1px 2px #0001;
}

.child {
  margin-top: 5px;
  margin-bottom: 5px;
  padding-left: 28px;
  border-left: 2px dotted #e3eaea;
}

@media (max-width: 700px) {
  .community-rules {
    padding: 0 3vw 36px 3vw;
  }
  .rule-card {
    padding: 18px 7px 10px 12px;
  }
  h1 {
    font-size: 1.5rem;
    margin: 32px 0 24px 0;
  }
  .section-title {
    font-size: 1.05em;
  }
}

@media (max-width: 420px) {
  .rule-card {
    padding: 12px 0 7px 5px;
  }
  .community-rules {
    padding: 0 1vw;
  }
}
</style>