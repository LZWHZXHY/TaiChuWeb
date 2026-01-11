<template>
  <div class="financial-container">
    <div class="stats-cards">
      <el-card class="stat-card">
        <div class="stat-content">
          <div class="stat-label">总收入</div>
          <div class="stat-value income">{{ formatCurrency(totalIncome) }}</div>
        </div>
      </el-card>
      <el-card class="stat-card">
        <div class="stat-content">
          <div class="stat-label">总支出</div>
          <div class="stat-value expense">{{ formatCurrency(totalExpense) }}</div>
        </div>
      </el-card>
      <el-card class="stat-card">
        <div class="stat-content">
          <div class="stat-label">纯利润</div>
          <div class="stat-value profit" :class="{ negative: netProfit < 0 }">
            {{ formatCurrency(netProfit) }}
          </div>
        </div>
      </el-card>
    </div>

    <div class="main-content">
      <div class="chart-section">
        <el-card class="chart-card">
          <template #header>
            <div class="chart-header">
              <span>年度收支趋势</span>
            </div>
          </template>
          <div class="chart-container">
            <div ref="lineChartRef" style="width: 100%; height: 400px;"></div>
          </div>
        </el-card>

        <el-card class="chart-card">
          <template #header>
             <div class="chart-header">
              <span>支出者分布</span>
            </div>
          </template>
          <div class="chart-container">
            <div ref="pieChartRef" style="width: 100%; height: 400px;"></div>
          </div>
        </el-card>
      </div>

      <div class="table-section">
        <el-card>
          <template #header>
            <div class="table-header">
              <span>财务数据明细</span>
              <div class="sort-buttons">
                <el-button @click="sortByDate" size="small">
                  {{ dateSort === 'desc' ? '日期: 最新在前' : '日期: 最旧在前' }}
                </el-button>
                <el-button @click="sortByAmount" size="small">
                  {{ amountSort === 'desc' ? '金额: 从大到小' : '金额: 从小到大' }}
                </el-button>
              </div>
            </div>
          </template>
          <el-table :data="sortedData" style="width: 100%" stripe>
            <el-table-column prop="date" label="日期" width="120">
              <template #default="{ row }">
                {{ formatDate(row.date) }}
              </template>
            </el-table-column>
            <el-table-column prop="zhiChu" label="支出者" width="100" />
            <el-table-column prop="zhiChuXiangMu" label="项目" min-width="180" show-overflow-tooltip />
            <el-table-column prop="shouKuan" label="收款方" width="120" />
            <el-table-column prop="amount" label="金额" width="140">
              <template #default="{ row }">
                <span :class="row.payReceive === 0 ? 'income-text' : 'expense-text'">
                  {{ (row.payReceive === 0 ? '+' : '-') + formatCurrency(row.amount) }}
                </span>
              </template>
            </el-table-column>
            <el-table-column prop="payReceive" label="类型" width="100">
              <template #default="{ row }">
                <el-tag :type="row.payReceive === 0 ? 'success' : 'danger'" effect="dark">
                  {{ row.payReceive === 0 ? '收入' : '支出' }}
                </el-tag>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { ElMessage } from 'element-plus'
import * as echarts from 'echarts'
import apiClient from '@/utils/api'

// --- 状态定义 ---
const financialData = ref([])
const dateSort = ref('desc')
const amountSort = ref('desc')

// 图表 DOM 引用
const pieChartRef = ref(null)
const lineChartRef = ref(null)

// ECharts 实例
let pieChartInstance = null
let lineChartInstance = null

// --- 计算属性 ---
const totalIncome = computed(() => {
  return financialData.value
    .filter(item => item.payReceive === 0)
    .reduce((sum, item) => sum + (item.amount || 0), 0)
})

const totalExpense = computed(() => {
  return financialData.value
    .filter(item => item.payReceive === 1)
    .reduce((sum, item) => sum + (item.amount || 0), 0)
})

const netProfit = computed(() => totalIncome.value - totalExpense.value)

const sortedData = computed(() => {
  let sorted = [...financialData.value]
  
  // 日期排序
  if (dateSort.value) {
    sorted.sort((a, b) => {
      const dateA = a.date ? new Date(a.date) : new Date(0)
      const dateB = b.date ? new Date(b.date) : new Date(0)
      return dateSort.value === 'desc' ? dateB - dateA : dateA - dateB
    })
  }
  
  // 金额排序 (如果激活)
  if (amountSort.value) {
    sorted.sort((a, b) => {
      const amountA = a.amount || 0
      const amountB = b.amount || 0
      return amountSort.value === 'desc' ? amountB - amountA : amountA - amountB
    })
  }
  
  return sorted
})

// --- 工具方法 ---
const formatCurrency = (amount) => {
  if (amount === undefined || amount === null || isNaN(amount)) return '¥0.00'
  return '¥' + Number(amount).toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

const formatDate = (dateString) => {
  if (!dateString) return '-'
  try {
    const date = new Date(dateString)
    if (isNaN(date.getTime())) return '-'
    return date.toLocaleDateString('zh-CN')
  } catch (error) {
    return '-'
  }
}

const sortByDate = () => {
  dateSort.value = dateSort.value === 'desc' ? 'asc' : 'desc'
  amountSort.value = null // 互斥排序
}

const sortByAmount = () => {
  amountSort.value = amountSort.value === 'desc' ? 'asc' : 'desc'
  dateSort.value = null // 互斥排序
}

// --- 数据获取与处理 ---
const fetchData = async () => {
  try {
    const response = await apiClient.get('/Financial/all')
    
    // 🔥 关键：字段映射 (后端大写 -> 前端小写)
    financialData.value = response.data.map(item => ({
      index: item.index,
      zhiChu: item.ZhiChu,                // 映射 ZhiChu
      zhiChuXiangMu: item.ZhiChuXiangMu,  // 映射 ZhiChuXiangMu
      date: item.date,
      shouKuan: item.ShouKuan,            // 映射 ShouKuan
      amount: Number(item.Amount) || 0,   // 映射 Amount 并转数字
      payReceive: item.PayReceive         // 映射 PayReceive
    }))
    
    // 等待 DOM 更新后初始化图表
    nextTick(() => {
      initPieChart()
      initLineChart()
    })
  } catch (error) {
    console.error('获取数据失败:', error)
    ElMessage.error('获取数据失败')
  }
}

// --- 图表 1: 支出分布饼图 ---
const initPieChart = () => {
  if (!pieChartRef.value) return

  // 聚合数据：按支出者(zhiChu)统计总金额
  const payerMap = {}
  financialData.value.forEach(item => {
    if (item.payReceive === 1) { // 只看支出
      const payer = item.zhiChu || '其他'
      payerMap[payer] = (payerMap[payer] || 0) + item.amount
    }
  })
  
  const chartData = Object.entries(payerMap).map(([name, value]) => ({ name, value }))
  
  if (chartData.length === 0) {
     chartData.push({ name: '暂无数据', value: 0 })
  }

  if (pieChartInstance) pieChartInstance.dispose()
  pieChartInstance = echarts.init(pieChartRef.value)
  
  pieChartInstance.setOption({
    tooltip: { trigger: 'item', valueFormatter: (val) => formatCurrency(val) },
    legend: { orient: 'vertical', left: 'left' },
    series: [{
      name: '支出分布',
      type: 'pie',
      radius: ['40%', '70%'],
      center: ['60%', '50%'],
      avoidLabelOverlap: false,
      itemStyle: { borderRadius: 10, borderColor: '#fff', borderWidth: 2 },
      label: { show: false, position: 'center' },
      emphasis: {
        label: { show: true, fontSize: 16, fontWeight: 'bold' }
      },
      labelLine: { show: false },
      data: chartData
    }]
  })
}

// --- 图表 2: 年度收支折线图 ---
const initLineChart = () => {
  if (!lineChartRef.value) return

  // 聚合数据：按年份统计收入和支出
  const yearMap = {} // { '2024': { income: 0, expense: 0 } }

  financialData.value.forEach(item => {
    if (!item.date) return
    const d = new Date(item.date)
    if (isNaN(d.getTime())) return
    
    const year = d.getFullYear()
    if (!yearMap[year]) yearMap[year] = { income: 0, expense: 0 }

    if (item.payReceive === 0) {
      yearMap[year].income += item.amount
    } else {
      yearMap[year].expense += item.amount
    }
  })

  // 排序年份
  const years = Object.keys(yearMap).sort((a, b) => a - b)
  const incomeData = years.map(y => yearMap[y].income)
  const expenseData = years.map(y => yearMap[y].expense)

  if (lineChartInstance) lineChartInstance.dispose()
  lineChartInstance = echarts.init(lineChartRef.value)

  lineChartInstance.setOption({
    tooltip: { trigger: 'axis' },
    legend: { data: ['收入', '支出'], top: 0 },
    grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
    xAxis: { type: 'category', boundaryGap: false, data: years },
    yAxis: { type: 'value' },
    series: [
      {
        name: '收入',
        type: 'line',
        smooth: true,
        data: incomeData,
        itemStyle: { color: '#67c23a' },
        areaStyle: { opacity: 0.2, color: '#67c23a' }
      },
      {
        name: '支出',
        type: 'line',
        smooth: true,
        data: expenseData,
        itemStyle: { color: '#f56c6c' },
        areaStyle: { opacity: 0.2, color: '#f56c6c' }
      }
    ]
  })
}

const handleResize = () => {
  pieChartInstance && pieChartInstance.resize()
  lineChartInstance && lineChartInstance.resize()
}

// --- 生命周期 ---
onMounted(() => {
  fetchData()
  window.addEventListener('resize', handleResize)
})

onBeforeUnmount(() => {
  pieChartInstance && pieChartInstance.dispose()
  lineChartInstance && lineChartInstance.dispose()
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
.financial-container {
  padding: 20px;
  background-color: #f5f7fa;
  min-height: 100vh;
}

/* 1. 统计卡片样式 */
.stats-cards {
  display: flex;
  gap: 20px;
  margin-bottom: 20px;
}

.stat-card {
  flex: 1;
  border-radius: 8px;
  transition: all 0.3s;
}

.stat-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 16px rgba(0,0,0,0.1);
}

.stat-content {
  text-align: center;
  padding: 15px;
}

.stat-label { color: #909399; font-size: 14px; margin-bottom: 8px; }
.stat-value { font-size: 28px; font-weight: bold; font-family: 'Helvetica Neue', sans-serif; }

.stat-value.income { color: #67c23a; }
.stat-value.expense { color: #f56c6c; }
.stat-value.profit { color: #409eff; }
.stat-value.profit.negative { color: #f56c6c; }

/* 2. 图表区域样式 */
.chart-section {
  display: grid;
  grid-template-columns: 1fr 1fr; /* 电脑端两列 */
  gap: 20px;
  margin-bottom: 20px;
}

.chart-card {
  border-radius: 8px;
}

.chart-header {
  font-weight: bold;
  font-size: 16px;
  color: #303133;
}

/* 3. 表格区域样式 */
.table-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.table-header span {
  font-size: 16px;
  font-weight: bold;
}

.income-text { color: #67c23a; font-weight: bold; }
.expense-text { color: #f56c6c; font-weight: bold; }

/* 响应式适配 */
@media (max-width: 992px) {
  .chart-section {
    grid-template-columns: 1fr; /* 平板/手机端变为一列 */
  }
}

@media (max-width: 768px) {
  .stats-cards {
    flex-direction: column;
  }
  .table-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }
}
</style>