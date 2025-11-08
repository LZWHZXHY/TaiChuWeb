<template>
  <div class="year-daka-container">
    <div class="year-select">
      <button @click="prevYear">&lt;</button>
      <span>{{ year }}年</span>
      <button @click="nextYear">&gt;</button>
    </div>
    <div class="months-grid">
      <div v-for="m in 12" :key="m" class="month-panel">
        <div class="month-title">{{ m }}月</div>
        <table class="calendar-table">
          <thead>
            <tr>
              <th v-for="w in weekDays" :key="w">{{ w }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(row, rowIdx) in getMonthRows(m)" :key="rowIdx">
              <td
                v-for="(cell, colIdx) in row"
                :key="colIdx"
                :class="{
                  checked: cell && isChecked(m, cell),
                  'checkin-normal': cell && getCheckinType(m, cell) === 1,
                  'checkin-buqian': cell && getCheckinType(m, cell) === 2,
                  today: cell && isToday(cell, m),
                  disabled: !cell
                }"
              >
                <span v-if="cell">{{ cell.getDate() }}</span>
              </td>
            </tr>
          </tbody>
        </table>
        <!-- 只在当前月显示打卡按钮 -->
        <div v-if="isCurrentMonth(m)" class="daka-btn-row">
          <button
            class="daka-btn"
            :disabled="isTodayChecked"
            @click="checkIn"
          >
            {{ isTodayChecked ? '已打卡' : '今日打卡' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import axios from 'axios';
import { ref, watchEffect, computed } from 'vue'
import { useUserStore } from '../../store/user';

const userStore = useUserStore()
const userID = computed(() => userStore.user ? userStore.user.id : 0);

const today = new Date()
const year = ref(today.getFullYear())
const weekDays = ['日','一','二','三','四','五','六']
const checkedDays = ref({})

// 新增状态变量
const expGained = ref(0);
const hasLeveledUp = ref(false);
const showExpNotice = ref(false);

// 根据环境设置基础URL
const baseApiUrl = import.meta.env.DEV 
  ? 'https://localhost:44321' 
  : 'https://bianyuzhou.com';




// 拉取打卡数据函数
async function fetchCheckinData() {
  if (!userID.value) return;
  const res = await axios.get('https://bianyuzhou.com/api/CheckIn', {
    params: {
      userId: userID.value,
      year: year.value
    }
  });
  const byMonth = {};
  res.data.forEach(record => {
    const date = new Date(record.checkinDate);
    const y = date.getFullYear();
    const m = date.getMonth() + 1;
    const mm = String(m).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');
    const dateStr = `${y}-${mm}-${dd}`;
    if (!byMonth[m]) byMonth[m] = [];
    byMonth[m].push({ date: dateStr, type: record.type });
  });
  checkedDays.value = { ...byMonth };
}

// 监听年份和用户变化自动拉取
watchEffect(() => {
  fetchCheckinData();
});

// 构造某月的日历二维数组
function getMonthRows(m) {
  const y = year.value
  const firstDay = new Date(y, m - 1, 1)
  const lastDay = new Date(y, m, 0)
  const days = []
  let row = []
  for (let i = 0; i < firstDay.getDay(); i++) row.push(null)
  for (let d = 1; d <= lastDay.getDate(); d++) {
    row.push(new Date(y, m - 1, d))
    if (row.length === 7) {
      days.push(row)
      row = []
    }
  }
  if (row.length > 0) {
    while (row.length < 7) row.push(null)
    days.push(row)
  }
  return days
}

// 获取某天的打卡类型 1普通 2补签
function getCheckinType(m, date) {
  if (!date) return null;
  const y = date.getFullYear();
  const mm = String(date.getMonth() + 1).padStart(2, '0');
  const dd = String(date.getDate()).padStart(2, '0');
  const dateStr = `${y}-${mm}-${dd}`;
  const monthArr = checkedDays.value[m];
  if (!monthArr) return null;
  const record = monthArr.find(item => item.date === dateStr);
  return record ? record.type : null;
}

// 判断某天是否已打卡
function isChecked(m, date) {
  return getCheckinType(m, date) !== null;
}

// 判断是否是当前月面板
function isCurrentMonth(m) {
  return year.value === today.getFullYear() && m === today.getMonth() + 1
}

// 判断某天是否为今天
function isToday(date, m) {
  if (!date) return false
  return (
    year.value === today.getFullYear() &&
    m === today.getMonth() + 1 &&
    date.getDate() === today.getDate()
  )
}

// 当前月今天是否已打卡
const isTodayChecked = computed(() => {
  const m = today.getMonth() + 1;
  const y = today.getFullYear();
  const mm = String(m).padStart(2, '0');
  const dd = String(today.getDate()).padStart(2, '0');
  const monthArr = checkedDays.value[m];
  if (!monthArr) return false;
  return monthArr.some(item => item.date === `${y}-${mm}-${dd}`);
})


async function checkIn() {
  if (!userID.value) return;

  try {
    const res = await axios.post(
      `${baseApiUrl}/api/CheckIn`, // 使用动态的 baseApiUrl
      { UserId: userID.value },
      { headers: { "Content-Type": "application/json" } }
    );

    if (res.data) {
      // 直接更新 Pinia
      userStore.user.level = res.data.newLevel;
      userStore.user.exp = res.data.newExp;

      // ⚡ 确保 localStorage 也更新，否则刷新/重登会覆盖
      localStorage.setItem("user", JSON.stringify(userStore.user));
    }

    await fetchCheckinData();

    const { newExp, newLevel } = res.data;

    // 1. 保存旧等级
    const oldLevel = userStore.user.level || 0;

    // 2. 更新经验 & 等级
    userStore.user.currentExp = newExp;
    userStore.user.level = newLevel;

    // 3. 提示逻辑
    expGained.value = 10;
    hasLeveledUp.value = newLevel > oldLevel;
    showExpNotice.value = true;

    console.log("打卡成功，经验+10，当前等级:", newLevel, "当前经验:", newExp);
  } catch (e) {
    alert(e?.response?.data?.message || "打卡失败");
  }
}





function prevYear() { year.value-- }
function nextYear() { year.value++ }
</script>

<style scoped>
.year-daka-container {
  max-width: 1150px;
  margin: 0 auto;
}
.year-select {
  display: flex;
  justify-content: center;
  align-items: center;
  margin-bottom: 18px;
  gap: 22px;
  font-size: 1.3em;
  font-weight: bold;
}
.year-select button {
  background: linear-gradient(120deg, #d4f8e8 0%, #b6e4bc 100%);
  border: none;
  padding: 7px 16px;
  border-radius: 12px;
  cursor: pointer;
  font-size: 1em;
  transition: background .15s;
  box-shadow: 0 2px 8px 0 rgba(88, 88, 88, 0.06);
}
.year-select button:hover {
  background: #b6e4bc;
}

.months-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 18px;
}
.month-panel {
  background: rgba(255,255,255,0.93);
  border-radius: 12px;
  box-shadow: 0 3px 14px 0 rgba(68, 128, 88, 0.07);
  padding: 10px;
}
.month-title {
  text-align: center;
  font-weight: 600;
  color: #4a7057;
  margin-bottom: 4px;
  letter-spacing: 2px;
}
.calendar-table {
  width: 100%;
  border-collapse: collapse;
  background: rgba(250,255,250,0.7);
  border-radius: 7px;
  font-size: 13px;
}
.calendar-table th,
.calendar-table td {
  width: 22px;
  height: 22px;
  text-align: center;
  border: 1px solid #e2edec;
  padding: 0;
}
.calendar-table th {
  background: #f3f8f6;
  color: #7fa390;
}
.calendar-table td.checked {
  font-weight: bold;
  border: 1.5px solid #bde9d3;
}
.calendar-table td.checkin-normal {
  background: #82e7ad !important; /* 绿色 */
  color: #245c3b;
}
.calendar-table td.checkin-buqian {
  background: #5ab7f7 !important; /* 蓝色 */
  color: #fff;
}
.calendar-table td.disabled {
  background: transparent;
  border: none;
}
.calendar-table td.today {
  background: #ffe48b;
  font-weight: bold;
  border: 2px solid #ffd600;
}
/* 打卡按钮样式 */
.daka-btn-row {
  text-align: center;
  margin-top: 8px;
}
.daka-btn {
  background: linear-gradient(90deg, #88e7ad 0%, #c3ede3 100%);
  color: #222;
  border: none;
  border-radius: 9px;
  padding: 7px 24px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.18s, color 0.18s;
  box-shadow: 0 4px 20px 0 rgba(105, 208, 168, 0.09);
  letter-spacing: 2px;
}
.daka-btn:disabled {
  background: #d8e8e3;
  color: #96a3a2;
  cursor: not-allowed;
  box-shadow: none;
}
</style>