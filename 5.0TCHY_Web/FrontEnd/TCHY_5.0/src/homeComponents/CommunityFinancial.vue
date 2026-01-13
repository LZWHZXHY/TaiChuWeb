<template>
  <div class="finance-industrial">
    <div class="grid-bg moving-grid"></div>

    <div class="finance-wrapper">
      
      <header class="sys-header">
        <div class="header-main">
          <h1 class="giant-text">
            <span class="prefix">///</span> LEDGER_DATA
          </h1>
          <div class="sub-text">财政档案 // FINANCIAL_RECORDS</div>
        </div>
        <div class="header-meta">
          <div class="meta-row">
            <span>AUDIT_STATUS:</span>
            <span class="status-badge">CLEARED</span>
          </div>
          <div class="meta-row">
             <span>DATA_POINTS:</span>
             <span class="mono-val">{{ financialData.length }}</span>
          </div>
        </div>
      </header>

      <div class="stats-grid">
        <div class="cyber-stat-card income-mode">
          <div class="card-deco-corner"></div>
          <div class="stat-label">TOTAL_INCOME // 总收入</div>
          <div class="stat-value">
            <span class="currency-symbol">¥</span>
            {{ formatNumber(totalIncome) }}
          </div>
          <div class="stat-bar-bg"><div class="stat-bar-fill income-fill"></div></div>
        </div>

        <div class="cyber-stat-card expense-mode">
          <div class="card-deco-corner"></div>
          <div class="stat-label">TOTAL_EXPENSE // 总支出</div>
          <div class="stat-value">
             <span class="currency-symbol">¥</span>
             {{ formatNumber(totalExpense) }}
          </div>
          <div class="stat-bar-bg"><div class="stat-bar-fill expense-fill"></div></div>
        </div>

        <div class="cyber-stat-card profit-mode" :class="{ 'is-negative': netProfit < 0 }">
          <div class="card-deco-corner"></div>
          <div class="stat-header-row">
            <div class="stat-label">NET_PROFIT // 纯利润</div>
            <div class="profit-rate">MARGIN: {{ profitMargin }}%</div>
          </div>
          <div class="stat-value">
             <span class="currency-symbol">¥</span>
             {{ formatNumber(netProfit) }}
          </div>
          <div class="stat-status">
            {{ netProfit >= 0 ? '>>> PROFITABLE' : '>>> DEFICIT_WARNING' }}
          </div>
        </div>
      </div>

      <div class="charts-dashboard">
        <div class="cyber-panel chart-panel">
          <div class="panel-header">
            <span class="icon">■</span> TREND_ANALYSIS // 年度趋势
          </div>
          <div class="chart-box">
             <div ref="lineChartRef" class="echart-instance"></div>
          </div>
        </div>

        <div class="cyber-panel chart-panel">
          <div class="panel-header">
            <span class="icon">■</span> EXPENSE_DIST // 支出分布
          </div>
          <div class="chart-box">
             <div ref="pieChartRef" class="echart-instance"></div>
          </div>
        </div>
      </div>

      <div class="cyber-panel table-panel">
        <div class="panel-header table-header-flex">
          <div>
            <span class="icon">■</span> TRANSACTION_LOGS // 明细
          </div>
          <div class="control-group">
            <button class="cyber-btn-sm" @click="sortByDate" :class="{ active: dateSort }">
              <span class="btn-text">SORT: DATE [{{ dateSort === 'desc' ? '▼' : '▲' }}]</span>
            </button>
            <button class="cyber-btn-sm" @click="sortByAmount" :class="{ active: amountSort }">
               <span class="btn-text">SORT: AMOUNT [{{ amountSort === 'desc' ? '▼' : '▲' }}]</span>
            </button>
          </div>
        </div>

        <div class="table-container custom-scroll">
          <el-table 
            :data="sortedData" 
            style="width: 100%" 
            :row-class-name="tableRowClassName"
            class="cyber-table"
          >
            <el-table-column prop="date" label="TIMESTAMP" width="140">
              <template #default="{ row }">
                <span class="mono-text">{{ formatDate(row.date) }}</span>
              </template>
            </el-table-column>
            
            <el-table-column prop="zhiChu" label="PAYER" width="120">
               <template #default="{ row }">
                 <span class="bold-text">{{ row.zhiChu }}</span>
               </template>
            </el-table-column>

            <el-table-column prop="zhiChuXiangMu" label="ITEM / MEMO" min-width="200" show-overflow-tooltip />

            <el-table-column prop="shouKuan" label="RECEIVER" width="140" />

            <el-table-column prop="amount" label="AMOUNT" width="160">
              <template #default="{ row }">
                <span class="amount-cell" :class="row.payReceive === 0 ? 'inc' : 'exp'">
                  {{ row.payReceive === 0 ? '+' : '-' }} ¥{{ formatNumberSimple(row.amount) }}
                </span>
              </template>
            </el-table-column>

            <el-table-column prop="payReceive" label="TYPE" width="120">
              <template #default="{ row }">
                <div class="type-badge" :class="row.payReceive === 0 ? 'bg-inc' : 'bg-exp'">
                   {{ row.payReceive === 0 ? 'INCOME' : 'EXPENSE' }}
                </div>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount, nextTick } from 'vue'
import * as echarts from 'echarts'
import apiClient from '@/utils/api'

// --- 状态定义 ---
const financialData = ref([])
const dateSort = ref('desc')
const amountSort = ref(null) // 默认不按金额排序

// 图表 DOM
const pieChartRef = ref(null)
const lineChartRef = ref(null)
let pieChartInstance = null
let lineChartInstance = null

// --- 计算属性 ---
const totalIncome = computed(() => {
  return financialData.value.filter(i => i.payReceive === 0).reduce((s, i) => s + (i.amount || 0), 0)
})
const totalExpense = computed(() => {
  return financialData.value.filter(i => i.payReceive === 1).reduce((s, i) => s + (i.amount || 0), 0)
})
const netProfit = computed(() => totalIncome.value - totalExpense.value)

// 新增功能：利润率计算
const profitMargin = computed(() => {
  if (totalIncome.value === 0) return 0
  return ((netProfit.value / totalIncome.value) * 100).toFixed(1)
})

const sortedData = computed(() => {
  let sorted = [...financialData.value]
  if (amountSort.value) {
    sorted.sort((a, b) => amountSort.value === 'desc' ? b.amount - a.amount : a.amount - b.amount)
  } else if (dateSort.value) {
    sorted.sort((a, b) => {
      const dA = new Date(a.date || 0), dB = new Date(b.date || 0)
      return dateSort.value === 'desc' ? dB - dA : dA - dB
    })
  }
  return sorted
})

// --- 格式化工具 ---
const formatNumber = (num) => Number(num).toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
const formatNumberSimple = (num) => Number(num).toLocaleString('zh-CN', { minimumFractionDigits: 2 })
const formatDate = (str) => {
  if (!str) return 'N/A'
  try { return new Date(str).toLocaleDateString('zh-CN').replace(/\//g, '.') } catch { return str }
}

const sortByDate = () => {
  dateSort.value = dateSort.value === 'desc' ? 'asc' : 'desc'
  amountSort.value = null
}
const sortByAmount = () => {
  amountSort.value = amountSort.value === 'desc' ? 'asc' : 'desc'
  dateSort.value = null
}

const tableRowClassName = ({ row }) => {
  return row.payReceive === 0 ? 'row-income' : 'row-expense'
}

// --- 数据获取 ---
const fetchData = async () => {
  try {
    const res = await apiClient.get('/Financial/all')
    financialData.value = res.data.map((item, idx) => ({
      id: idx, // 虚拟ID
      zhiChu: item.ZhiChu,
      zhiChuXiangMu: item.ZhiChuXiangMu,
      date: item.date,
      shouKuan: item.ShouKuan,
      amount: Number(item.Amount) || 0,
      payReceive: item.PayReceive
    }))
    nextTick(() => { initPieChart(); initLineChart() })
  } catch (e) {
    console.error("Data Load Error", e)
  }
}

// --- ECharts 配置 (硬核风格) ---
const fontMono = "'JetBrains Mono', monospace"
const colorIncome = '#239b56' // 工业绿
const colorExpense = '#D92323' // 工业红
const colorBlack = '#111111'

const initPieChart = () => {
  if (!pieChartRef.value) return
  const payerMap = {}
  financialData.value.forEach(item => {
    if (item.payReceive === 1) {
      const p = item.zhiChu || 'UNKNOWN'
      payerMap[p] = (payerMap[p] || 0) + item.amount
    }
  })
  const data = Object.entries(payerMap).map(([k, v]) => ({ name: k, value: v }))

  if (pieChartInstance) pieChartInstance.dispose()
  pieChartInstance = echarts.init(pieChartRef.value)
  
  pieChartInstance.setOption({
    tooltip: { 
      trigger: 'item', 
      backgroundColor: 'rgba(255,255,255,0.9)',
      borderColor: colorBlack,
      borderWidth: 2,
      textStyle: { fontFamily: fontMono, color: colorBlack },
      formatter: '{b}: ¥{c} ({d}%)'
    },
    series: [{
      type: 'pie',
      radius: ['45%', '70%'],
      itemStyle: { 
        borderRadius: 0, 
        borderColor: '#fff', 
        borderWidth: 2 
      },
      label: { 
        show: true, 
        fontFamily: fontMono,
        formatter: '{b}\n{d}%',
        color: colorBlack
      },
      data: data.length ? data : [{name:'NO_DATA', value:0}]
    }]
  })
}

const initLineChart = () => {
  if (!lineChartRef.value) return
  const yearMap = {}
  financialData.value.forEach(item => {
    if (!item.date) return
    const y = new Date(item.date).getFullYear()
    if (!yearMap[y]) yearMap[y] = { inc: 0, exp: 0 }
    item.payReceive === 0 ? yearMap[y].inc += item.amount : yearMap[y].exp += item.amount
  })
  
  const years = Object.keys(yearMap).sort()
  const incData = years.map(y => yearMap[y].inc)
  const expData = years.map(y => yearMap[y].exp)

  if (lineChartInstance) lineChartInstance.dispose()
  lineChartInstance = echarts.init(lineChartRef.value)

  lineChartInstance.setOption({
    tooltip: { 
      trigger: 'axis',
      backgroundColor: '#fff',
      borderColor: colorBlack,
      borderWidth: 2,
      textStyle: { fontFamily: fontMono },
      axisPointer: { type: 'cross', label: { backgroundColor: colorBlack } }
    },
    legend: { 
      data: ['INCOME', 'EXPENSE'], 
      top: 0, 
      textStyle: { fontFamily: fontMono, fontWeight: 'bold' } 
    },
    grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
    xAxis: { 
      type: 'category', 
      data: years,
      axisLine: { lineStyle: { color: colorBlack } },
      axisLabel: { fontFamily: fontMono }
    },
    yAxis: { 
      type: 'value',
      splitLine: { lineStyle: { type: 'dashed' } },
      axisLabel: { fontFamily: fontMono }
    },
    series: [
      { name: 'INCOME', type: 'line', data: incData, itemStyle: { color: colorIncome }, symbol: 'rect', symbolSize: 6 },
      { name: 'EXPENSE', type: 'line', data: expData, itemStyle: { color: colorExpense }, symbol: 'rect', symbolSize: 6 }
    ]
  })
}

const handleResize = () => {
  pieChartInstance?.resize()
  lineChartInstance?.resize()
}

onMounted(() => {
  fetchData()
  window.addEventListener('resize', handleResize)
})
onBeforeUnmount(() => {
  pieChartInstance?.dispose()
  lineChartInstance?.dispose()
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=JetBrains+Mono:wght@400;700&display=swap');

/* --- 全局变量 --- */
.finance-industrial {
  --red: #D92323; 
  --green: #239b56;
  --black: #111111; 
  --white: #F4F1EA;
  --mono: 'JetBrains Mono', monospace; 
  --heading: 'Anton', sans-serif;
  
  width: 100%;
  min-height: 100vh;
  background-color: var(--white);
  color: var(--black);
  font-family: var(--mono);
  position: relative;
  overflow: hidden;
}

.grid-bg { 
  position: absolute; inset: 0; 
  background-image: linear-gradient(rgba(17, 17, 17, 0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(17, 17, 17, 0.05) 1px, transparent 1px); 
  background-size: 40px 40px; 
  z-index: 0; pointer-events: none;
}
.moving-grid { animation: gridScroll 60s linear infinite; }
@keyframes gridScroll { 0% { transform: translateY(0); } 100% { transform: translateY(-40px); } }

/* --- 主容器 --- */
.finance-wrapper {
  position: relative;
  z-index: 1;
  max-width: 1200px;
  margin: 0 auto;
  padding: 40px 20px;
  display: flex;
  flex-direction: column;
  gap: 30px;
}

/* --- Header --- */
.sys-header {
  border-bottom: 4px solid var(--black);
  padding-bottom: 20px;
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
}
.giant-text {
  font-family: var(--heading);
  font-size: 3rem; margin: 0; line-height: 1;
}
.prefix { color: var(--red); margin-right: 10px; }
.sub-text { font-weight: bold; color: #666; margin-top: 5px; }
.header-meta { text-align: right; font-size: 0.85rem; font-weight: bold; }
.meta-row { margin-bottom: 5px; }
.status-badge { background: var(--black); color: var(--white); padding: 0 5px; margin-left: 5px; }
.mono-val { font-family: var(--mono); color: var(--red); }

/* --- Stats Grid --- */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
}
.cyber-stat-card {
  border: 2px solid var(--black);
  background: #fff;
  padding: 20px;
  position: relative;
  box-shadow: 4px 4px 0 rgba(0,0,0,0.1);
  transition: transform 0.2s;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  min-height: 140px;
}
.cyber-stat-card:hover {
  transform: translateY(-5px);
  box-shadow: 8px 8px 0 rgba(0,0,0,0.2);
}
/* 卡片装饰角 */
.card-deco-corner {
  position: absolute; top: 0; right: 0;
  width: 0; height: 0;
  border-style: solid;
  border-width: 0 20px 20px 0;
  border-color: transparent var(--black) transparent transparent;
}
.stat-label { font-size: 0.8rem; font-weight: bold; color: #666; margin-bottom: 10px; }
.stat-value {
  font-family: var(--heading);
  font-size: 2.5rem;
  line-height: 1;
  margin-bottom: 15px;
}
.currency-symbol { font-size: 1.5rem; vertical-align: top; color: #888; }
.stat-bar-bg { height: 6px; background: #eee; width: 100%; }
.stat-bar-fill { height: 100%; width: 0; animation: fillBar 1s forwards; }
@keyframes fillBar { to { width: 100%; } }

/* 配色 */
.income-mode .stat-value { color: var(--green); }
.income-fill { background: var(--green); }
.expense-mode .stat-value { color: var(--red); }
.expense-fill { background: var(--red); }

.profit-mode { border-color: var(--black); }
.profit-mode.is-negative .stat-value { color: var(--red); }
.profit-mode.is-negative .stat-status { color: var(--red); border-color: var(--red); }
.stat-header-row { display: flex; justify-content: space-between; }
.profit-rate { font-size: 0.75rem; background: #eee; padding: 2px 6px; }
.stat-status {
  font-size: 0.8rem; font-weight: bold;
  border-top: 1px dashed #ccc; padding-top: 5px;
  color: var(--green);
}

/* --- Charts --- */
.charts-dashboard {
  display: grid; grid-template-columns: 2fr 1fr; gap: 20px;
}
.cyber-panel {
  border: 2px solid var(--black);
  background: #fff;
  display: flex; flex-direction: column;
}
.panel-header {
  background: var(--black); color: var(--white);
  padding: 8px 15px; font-weight: bold; font-size: 0.9rem;
  display: flex; align-items: center; gap: 10px;
}
.icon { color: var(--red); font-size: 0.8rem; }
.chart-box { padding: 20px; height: 400px; }
.echart-instance { width: 100%; height: 100%; }

/* --- Table Section --- */
.table-panel { min-height: 400px; }
.table-header-flex { display: flex; justify-content: space-between; align-items: center; }
.control-group { display: flex; gap: 10px; }
.cyber-btn-sm {
  background: var(--white); border: 1px solid var(--black);
  padding: 4px 10px; cursor: pointer; font-family: var(--mono);
  font-size: 0.75rem; font-weight: bold;
}
.cyber-btn-sm:hover, .cyber-btn-sm.active {
  background: var(--red); color: var(--white); border-color: var(--red);
}

.table-container { padding: 0; }
/* 深度定制 El Table */
:deep(.cyber-table) {
  --el-table-border-color: var(--black);
  --el-table-header-bg-color: #eee;
  --el-table-row-hover-bg-color: #f0f0f0;
  font-family: var(--mono);
  color: var(--black);
}
:deep(.el-table th.el-table__cell) {
  background-color: #eee !important;
  color: var(--black);
  font-weight: 800;
  border-bottom: 2px solid var(--black) !important;
  text-transform: uppercase;
}
:deep(.el-table td.el-table__cell) {
  border-bottom: 1px solid #ccc;
}
/* 去除默认边框 */
:deep(.el-table__inner-wrapper::before) { display: none; }

.amount-cell { font-weight: bold; }
.amount-cell.inc { color: var(--green); }
.amount-cell.exp { color: var(--red); }
.type-badge {
  display: inline-block; padding: 2px 8px; font-size: 0.7rem; font-weight: bold;
  border: 1px solid var(--black);
}
.bg-inc { background: #dfffe0; }
.bg-exp { background: #ffe0e0; }
.bold-text { font-weight: 700; }

/* 响应式 */
@media (max-width: 1024px) {
  .stats-grid { grid-template-columns: 1fr; }
  .charts-dashboard { grid-template-columns: 1fr; }
  .table-header-flex { flex-direction: column; align-items: flex-start; gap: 10px; }
}
</style>