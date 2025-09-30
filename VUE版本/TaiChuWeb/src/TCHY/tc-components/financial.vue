<template>
  <div class="financial-page">
    <!-- 检索区域 -->
    <div class="search-bar">
      <input v-model="search.zhiChu" placeholder="支出" />
      <input v-model="search.zhiChuXiangMu" placeholder="项目" />
      <input type="date" v-model="search.dateFrom" />
      <input type="date" v-model="search.dateTo" />
      <select v-model="search.payReceive">
        <option value="">全部类型</option>
        <option value="1">支出</option>
        <option value="0">收入</option>
      </select>
      <!-- 新增支出人筛选 -->
      <select v-model="search.person">
        <option value="">全部支出人</option>
        <option v-for="person in allPersons" :key="person" :value="person">{{ person }}</option>
      </select>
      <button @click="fetchFinancials">检索</button>
      <button @click="resetFilter">重置</button>
    </div>

    <!-- 统计板块 -->
    <div class="summary-section">
      <div class="summary-card">
        <div class="summary-label">总支出</div>
        <div class="summary-value out">{{ totalOutcome }}</div>
      </div>
      <div class="summary-card">
        <div class="summary-label">总收益</div>
        <div class="summary-value in">{{ totalIncome }}</div>
      </div>
      <div class="summary-card">
        <div class="summary-label">净收益</div>
        <div class="summary-value net" :class="{positive: netIncome >= 0, negative: netIncome < 0}">
          {{ netIncome }}
        </div>
      </div>
    </div>

    <!-- 主内容并排 -->
    <div class="main-content">
      <!-- 表格区域 -->
      <div class="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>序号</th>
              <th>支出</th>
              <th>项目</th>
              <th>日期</th>
              <th>收款方</th>
              <th @click="toggleAmountSort" style="cursor:pointer;">
                金额
                <span v-if="amountSort === 'asc'">▲</span>
                <span v-else-if="amountSort === 'desc'">▼</span>
                <span v-else style="color:#ccc;">⇅</span>
              </th>
              <th>类型</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in sortedFinancials" :key="item.index">
              <td>{{ item.index }}</td>
              <td>{{ item.zhiChu || '-' }}</td>
              <td>{{ item.zhiChuXiangMu || '-' }}</td>
              <td>{{ item.date ? item.date.split('T')[0] : '-' }}</td>
              <td>{{ item.shouKuan || '-' }}</td>
              <td>{{ item.amount !== undefined ? item.amount : '-' }}</td>
              <td>
                <span :class="item.payReceive === 1 ? 'out' : 'in'">
                  {{ item.payReceive === 1 ? '支出' : '收入' }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- 饼图区域（右侧/左侧可调）-->
      <div class="chart-section">
        <PieChart
          v-if="pieLabels.length"
          :labels="pieLabels"
          :data="pieData"
          :colors="pieColors"
        />
        <div v-else style="color: #999; text-align:center;">暂无支出数据</div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import axios from 'axios'
import PieChart from './PieChart.vue'
import { Chart, ArcElement, Tooltip, Legend } from 'chart.js'
Chart.register(ArcElement, Tooltip, Legend)

const baseApiUrl = import.meta.env.DEV
  ? 'https://localhost:44321'
  : 'https://bianyuzhou.com';

const financials = ref([])
const search = ref({
  zhiChu: '',
  zhiChuXiangMu: '',
  dateFrom: '',
  dateTo: '',
  payReceive: '',
  person: ''
})

// 排序状态：'asc' 升序, 'desc' 降序, '' 不排序
const amountSort = ref('')

// 排序切换函数
function toggleAmountSort() {
  amountSort.value = amountSort.value === 'asc' ? 'desc'
                      : amountSort.value === 'desc' ? '' : 'asc'
}

// 支出人列表（自动去重）
const allPersons = computed(() => {
  const set = new Set()
  financials.value.forEach(item => {
    if (item.payReceive === 1 && item.zhiChu) set.add(item.zhiChu)
  })
  return Array.from(set)
})

// 按前端筛选支出人
const filteredFinancials = computed(() => {
  if (!search.value.person) return financials.value
  return financials.value.filter(item => item.payReceive === 1 && item.zhiChu === search.value.person || (item.payReceive !== 1 && !search.value.payReceive))
})

// 排序后的数据
const sortedFinancials = computed(() => {
  let arr = [...filteredFinancials.value]
  if (amountSort.value === 'asc') {
    arr.sort((a, b) => (Number(a.amount) || 0) - (Number(b.amount) || 0))
  } else if (amountSort.value === 'desc') {
    arr.sort((a, b) => (Number(b.amount) || 0) - (Number(a.amount) || 0))
  }
  return arr
})

const fetchFinancials = async () => {
  const params = {}
  if (search.value.zhiChu) params.zhiChu = search.value.zhiChu
  if (search.value.zhiChuXiangMu) params.zhiChuXiangMu = search.value.zhiChuXiangMu
  if (search.value.dateFrom) params.dateFrom = search.value.dateFrom
  if (search.value.dateTo) params.dateTo = search.value.dateTo
  if (search.value.payReceive !== '') params.payReceive = search.value.payReceive

  // 不传person，前端本地筛选（后端没有person参数）
  const { data } = await axios.get(`${baseApiUrl}/api/financial`, { params })
  financials.value = data
}

const resetFilter = () => {
  search.value = {
    zhiChu: '',
    zhiChuXiangMu: '',
    dateFrom: '',
    dateTo: '',
    payReceive: '',
    person: ''
  }
  fetchFinancials()
}

// 总支出
const totalOutcome = computed(() =>
  filteredFinancials.value
    .filter(item => item.payReceive === 1)
    .reduce((sum, item) => sum + (Number(item.amount) || 0), 0)
    .toFixed(2)
)
// 总收益
const totalIncome = computed(() =>
  filteredFinancials.value
    .filter(item => item.payReceive === 0)
    .reduce((sum, item) => sum + (Number(item.amount) || 0), 0)
    .toFixed(2)
)
// 净收益
const netIncome = computed(() =>
  (Number(totalIncome.value) - Number(totalOutcome.value)).toFixed(2)
)

// 饼图数据（分支出人统计）
const pieLabels = computed(() => {
  const map = {}
  filteredFinancials.value.forEach(item => {
    if (item.payReceive === 1) {
      const user = item.zhiChu || '未知'
      map[user] = (map[user] || 0) + (Number(item.amount) || 0)
    }
  })
  return Object.keys(map)
})
const pieData = computed(() => {
  const map = {}
  filteredFinancials.value.forEach(item => {
    if (item.payReceive === 1) {
      const user = item.zhiChu || '未知'
      map[user] = (map[user] || 0) + (Number(item.amount) || 0)
    }
  })
  return pieLabels.value.map(l => map[l])
})
const pieColors = computed(() => [
  '#FF6384', '#36A2EB', '#FFCE56', '#8BC34A', '#F44336', '#9C27B0', '#00BCD4',
  '#E91E63', '#009688', '#FFC107', '#CDDC39', '#607D8B'
])

// 初始化加载全部数据
fetchFinancials()
</script>

<style scoped>
.financial-page {
  max-width: 1250px;
  margin: 32px auto;
  padding: 24px 18px;
  background: #f8fafc;
  border-radius: 16px;
  box-shadow: 0 2px 16px #d7e3fa44;
}
.search-bar {
  display: flex;
  gap: 12px;
  margin-bottom: 18px;
  flex-wrap: wrap;
  padding: 8px;
  background: #f4f6fb;
  border-radius: 8px;
}
.search-bar input,
.search-bar select {
  padding: 6px 10px;
  border: 1px solid #d9e1ec;
  border-radius: 6px;
  font-size: 15px;
  background: #fff;
  outline: none;
  transition: border 0.2s;
}
.search-bar input:focus,
.search-bar select:focus {
  border-color: #4c8bf5;
}
.search-bar button {
  padding: 7px 18px;
  border: none;
  border-radius: 6px;
  font-size: 15px;
  background: #e4e9f1;
  color: #444;
  cursor: pointer;
  transition: background 0.2s;
}
.search-bar button.primary {
  background: #4c8bf5;
  color: #fff;
}
.search-bar button:hover {
  opacity: 0.85;
}

.summary-section {
  display: flex;
  gap: 30px;
  margin: 24px 0 20px 0;
  flex-wrap: wrap;
}
.summary-card {
  background: linear-gradient(96deg,#f4f6fb 60%,#e2e9f6);
  padding: 18px 28px;
  border-radius: 12px;
  box-shadow: 0 1px 8px #e3e9f4;
  text-align: center;
  min-width: 150px;
}
.summary-label {
  color: #7a8ca5;
  font-size: 14px;
  margin-bottom: 7px;
}
.summary-value {
  font-size: 25px;
  font-weight: bold;
  letter-spacing: 1px;
}
.summary-value.in {
  color: #19b574;
}
.summary-value.out {
  color: #f25d5d;
}
.summary-value.net {
  color: #607d8b;
}
.summary-value.net.positive {
  color: #19b574;
}
.summary-value.net.negative {
  color: #f25d5d;
}

.main-content {
  display: flex;
  gap: 36px;
  align-items: flex-start;
}
.table-outer {
  flex: 2.5;
  min-width: 0;
  background: #fff;
  border-radius: 10px;
  box-shadow: 0 2px 8px #ecf1f8;
  padding: 0;
  margin: 0;
  /* 让表格区域整体更有卡片感 */
}
.table-wrapper {
  /* 限高 + 滚动 */
  max-height: 420px;
  overflow-y: auto;
  border-radius: 10px;
}
.table-wrapper table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
  min-width: 800px;
}
.table-wrapper thead tr {
  position: sticky;
  top: 0;
  background: #f4f6fb;
  z-index: 2;
}
th, td {
  padding: 9px 12px;
  border-bottom: 1px solid #f2f5fa;
  text-align: center;
  font-size: 15px;
}
th {
  background-color: #f1f6fc;
  color: #3c5773;
  font-weight: 700;
  letter-spacing: 1px;
  position: sticky;
  top: 0;
  z-index: 1;
  box-shadow: 0 2px 2px -2px #e9eef5;
}
tr {
  transition: background 0.2s;
}
tbody tr:hover {
  background: #eaf3fc88;
}
td.in {
  color: #19b574;
}
td.out {
  color: #f25d5d;
}

.chart-section {
  flex: 1;
  min-width: 220px;
  min-height: 220px;
  max-width: 360px;
  background: #fff;
  border-radius: 10px;
  box-shadow: 0 2px 8px #ecf1f8;
  padding: 24px 8px 16px 8px;
  display: flex;
  justify-content: center;
  align-items: center;
  height: 360px; /* 从320px提升到360px */
}
:deep(canvas) {
  max-width: 260px !important;    /* 从180提升到260 */
  max-height: 260px !important;
}

@media (max-width: 900px) {
  .main-content {
    flex-direction: column;
  }
  .chart-section, .table-outer {
    max-width: 100%;
    min-width: 0;
    width: 100%;
    margin-bottom: 24px;
    height: auto;
  }
  .table-wrapper {
    max-height: 300px;
  }
}
</style>