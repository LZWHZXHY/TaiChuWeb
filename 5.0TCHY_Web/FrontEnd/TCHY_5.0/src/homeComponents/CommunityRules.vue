<template>
  <div class="compact-rules">
    <!-- 顶部信息栏 -->
    <div class="rules-header">
      <div class="header-main">
        <h1>📜 社区规则</h1>
        <div class="header-info">
          <span class="stats">{{ categoryCount }}个分类 {{ totalRules }}条规则</span>
        </div>
      </div>
    </div>

    <!-- 分类导航 -->
    <div class="category-nav">
      <div class="nav-container">
        <button
          v-for="categoryName in categoryNames"
          :key="categoryName"
          class="nav-item"
          :class="{ 'nav-item--active': activeCategory === categoryName }"
          @click="setActiveCategory(categoryName)"
        >
          <span class="nav-name">{{ categoryName.split(' ')[0] }}</span>
          <span class="nav-version">{{ categoryName.split(' ')[1] }}</span>
          <span class="nav-count">{{ rulesData[categoryName]?.length }}</span>
        </button>
      </div>
    </div>

    <!-- 规则内容 -->
    <div class="rules-content">
      <div v-if="activeCategory && rulesData[activeCategory]" class="category-section">
        <div class="rules-list">
          <div
            v-for="(rule, index) in rulesData[activeCategory]"
            :key="index"
            class="rule-item"
          >
            <div class="rule-main">
              <div class="rule-header">
                <span class="rule-number">第{{ index + 1 }}条</span>
                <span class="rule-penalty" v-if="getPenaltyText(rule)">
                  {{ getPenaltyText(rule) }}
                </span>
              </div>
              <div class="rule-text">{{ rule.text }}</div>
              
              <!-- 子规则 -->
              <div v-if="rule.children && rule.children.length" class="sub-rules">
                <div
                  v-for="(child, childIndex) in rule.children"
                  :key="childIndex"
                  class="sub-rule"
                >
                  <span class="sub-marker">•</span>
                  <span class="sub-text">{{ child.text }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 底部说明 -->
    <div class="rules-footer">
      <p>规则解释权归太初寰宇社区管理团队所有 • 反馈请通过八卦乾听司或意见箱</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'

const rulesData = ref({})
const activeCategory = ref('')

// 计算分类名称列表
const categoryNames = computed(() => {
  return Object.keys(rulesData.value).filter(key => key !== 'meta')
})

// 计算分类数量
const categoryCount = computed(() => {
  return categoryNames.value.length
})

// 计算总规则数
const totalRules = computed(() => {
  let count = 0
  for (const categoryName of categoryNames.value) {
    if (Array.isArray(rulesData.value[categoryName])) {
      count += rulesData.value[categoryName].length
    }
  }
  return count
})

// 根据规则内容推断处罚文本
const getPenaltyText = (rule) => {
  const text = rule.text.toLowerCase()
  
  if (text.includes('法律法规') || text.includes('违法') || text.includes('色情') || text.includes('暴力')) {
    return '直接封禁'
  } else if (text.includes('人身攻击') || text.includes('辱骂') || text.includes('歧视')) {
    return '第一次警告，第二次封禁'
  } else if (text.includes('广告') || text.includes('引流') || text.includes('诈骗')) {
    return '第一次警告并删除，第二次封禁'
  } else if (text.includes('管理员') && text.includes('职权')) {
    return '管理员违规从重处理'
  } else if (text.includes('金额交易')) {
    return '第一次警告，第二次封禁'
  } else if (text.includes('矛盾') && text.includes('私聊')) {
    return '第一次警告，第二次处理'
  } else if (text.includes('警告') && text.includes('处理')) {
    return '视情节警告处理'
  }
  
  return null
}

const loadRulesData = async () => {
  try {
    const response = await fetch('/src/data/rules.json')
    if (response.ok) {
      const data = await response.json()
      rulesData.value = data
      
      // 设置第一个分类为激活状态
      if (categoryNames.value.length > 0) {
        activeCategory.value = categoryNames.value[0]
      }
    }
  } catch (error) {
    console.error('加载规则数据失败:', error)
    // 使用默认数据
    rulesData.value = {
      "通用规则 V1.0": [
        { "text": "数据加载中，请稍候..." }
      ]
    }
    activeCategory.value = "通用规则 V1.0"
  }
}

const setActiveCategory = (categoryName) => {
  activeCategory.value = categoryName
}

onMounted(() => {
  loadRulesData()
})
</script>

<style scoped>
.compact-rules {
  max-width: 900px;
  margin: 0 auto;
  padding: 1rem;
  font-family: 'Segoe UI', system-ui, sans-serif;
  color: #333;
  line-height: 1.5;
}

/* 顶部信息栏 */
.rules-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 1.5rem;
  border-radius: 12px;
  margin-bottom: 1rem;
}

.header-main h1 {
  margin: 0 0 0.5rem 0;
  font-size: 1.8rem;
  font-weight: 700;
}

.header-info {
  display: flex;
  gap: 1rem;
  font-size: 0.9rem;
  opacity: 0.9;
}

.stats {
  background: rgba(255,255,255,0.2);
  padding: 0.2rem 0.6rem;
  border-radius: 8px;
}

/* 分类导航 */
.category-nav {
  margin-bottom: 1.5rem;
}

.nav-container {
  display: flex;
  gap: 0.5rem;
  overflow-x: auto;
  padding: 0.5rem 0;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.7rem 1rem;
  background: #f8f9fa;
  border: 2px solid #e9ecef;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.2s ease;
  white-space: nowrap;
  flex-shrink: 0;
  font-size: 0.9rem;
}

.nav-item:hover {
  border-color: #667eea;
  transform: translateY(-1px);
}

.nav-item--active {
  background: #667eea;
  color: white;
  border-color: #667eea;
}

.nav-name {
  font-weight: 600;
}

.nav-version {
  font-size: 0.8rem;
  opacity: 0.8;
}

.nav-count {
  background: rgba(255,255,255,0.2);
  padding: 0.15rem 0.5rem;
  border-radius: 8px;
  font-size: 0.8rem;
  font-weight: 600;
}

.nav-item--active .nav-count {
  background: rgba(255,255,255,0.3);
}

/* 规则内容 */
.rules-content {
  min-height: 400px;
}

.rules-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.rule-item {
  background: white;
  border: 1px solid #e9ecef;
  border-radius: 10px;
  padding: 1.2rem;
  transition: all 0.2s ease;
}

.rule-item:hover {
  border-color: #667eea;
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.1);
}

.rule-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.8rem;
}

.rule-number {
  font-weight: 600;
  color: #667eea;
  font-size: 0.9rem;
}

.rule-penalty {
  background: #fff5f5;
  color: #e53e3e;
  padding: 0.3rem 0.6rem;
  border-radius: 6px;
  font-size: 0.8rem;
  font-weight: 600;
  border: 1px solid #fed7d7;
}

.rule-text {
  font-size: 1rem;
  line-height: 1.5;
  color: #2d3748;
  margin-bottom: 0.5rem;
}

/* 子规则 */
.sub-rules {
  margin-top: 1rem;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 8px;
  border-left: 3px solid #667eea;
}

.sub-rule {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
  font-size: 0.9rem;
  color: #4a5568;
}

.sub-rule:last-child {
  margin-bottom: 0;
}

.sub-marker {
  color: #667eea;
  font-weight: bold;
  flex-shrink: 0;
}

.sub-text {
  line-height: 1.4;
}

/* 底部说明 */
.rules-footer {
  margin-top: 2rem;
  padding: 1rem;
  text-align: center;
  color: #718096;
  font-size: 0.9rem;
  border-top: 1px solid #e2e8f0;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .compact-rules {
    padding: 0.5rem;
  }
  
  .rules-header {
    padding: 1rem;
  }
  
  .header-info {
    flex-direction: column;
    gap: 0.5rem;
  }
  
  .nav-container {
    gap: 0.3rem;
  }
  
  .nav-item {
    padding: 0.6rem 0.8rem;
    font-size: 0.85rem;
  }
  
  .rule-item {
    padding: 1rem;
  }
  
  .rule-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
  
  .rule-penalty {
    align-self: flex-start;
  }
}

@media (max-width: 480px) {
  .header-main h1 {
    font-size: 1.5rem;
  }
  
  .rule-text {
    font-size: 0.95rem;
  }
  
  .sub-text {
    font-size: 0.85rem;
  }
}
</style>