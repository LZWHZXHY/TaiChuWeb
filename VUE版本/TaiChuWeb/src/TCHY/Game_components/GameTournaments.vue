<script setup>
import { ref, onMounted, computed } from 'vue'
import api from '../../../server/api' // 这里根据你的实际路径调整

const baseApiUrl = import.meta.env.VITE_API_BASE_URL

console.log('baseApiUrl:', baseApiUrl)
const tournaments = ref([])
const selectedTournament = ref(null)
const detailDialogVisible = ref(false)
const champion = ref(null)
const isSignedUp = ref(false)
const signupList = ref([])


const fetchSignupList = async (tournamentId) => {
  try {
    signupList.value = await api.get(`/api/tournaments/${tournamentId}/signups`)
  } catch {
    signupList.value = []
  }
}


const statusMap = {
  0: '未开始',
  1: '举办中',
  2: '已结束',
  3: '已取消'
}
const gameTypeMap = {
  0: '街霸6',
  1: 'CSGO'
}

function formatDate(dateStr) {
  if (!dateStr) return ''
  return dateStr.split('T')[0]
}

const fetchTournaments = async () => {
  tournaments.value = await api.get('/api/tournaments')
}

const canSignUp = computed(() => {
  if (!selectedTournament.value) return false
  const now = new Date()
  const regDate = new Date(selectedTournament.value.registrationDate)
  return now <= regDate
})

const checkSignUpStatus = async (tournamentId) => {
  try {
    const res = await api.get(`/api/tournaments/${tournamentId}/isSignedUp`)
    isSignedUp.value = !!res.isSignedUp
  } catch {
    isSignedUp.value = false
  }
}

const refreshJoinAmount = async () => {
  const data = await api.get(`/api/tournaments/${selectedTournament.value.id}`)
  selectedTournament.value.joinAmount = data.joinAmount
}

const openDetail = async (tournament) => {
  selectedTournament.value = tournament
  detailDialogVisible.value = true
  if (tournament.status === 2) {
    const data = await api.get(`/api/champions?tournamentId=${tournament.id}`)
    champion.value = data && data[0]
  } else {
    champion.value = null
  }
  await checkSignUpStatus(tournament.id)
  await fetchSignupList(tournament.id) // 新增
}

const signUp = async () => {
  try {
    await api.post(`/api/tournaments/${selectedTournament.value.id}/signup`)
    isSignedUp.value = true
    await refreshJoinAmount()
    await fetchSignupList(selectedTournament.value.id)
    // 弹出 joinUrl
    if (selectedTournament.value.joinUrl) {
      window.open(selectedTournament.value.joinUrl, '_blank')
    }
  } catch (e) {
    alert(e?.message || '报名失败')
  }
}

const cancelSignUp = async () => {
  try {
    await api.post(`/api/tournaments/${selectedTournament.value.id}/cancel-signup`)
    isSignedUp.value = false
    await refreshJoinAmount()
    await fetchSignupList(selectedTournament.value.id) // 这里加上
  } catch (e) {
    alert(e?.message || '取消报名失败')
  }
}

onMounted(fetchTournaments)
</script>

<template>
  <div class="tournament-list">
    <div
      v-for="t in tournaments"
      :key="t.id"
      class="tournament-card"
      @click="openDetail(t)"
      :style="{
  backgroundImage: `url('${baseApiUrl}/api/UserInfo/imageproxy?path=TournamentBackground/bg${t.id}.jpg')`
}"
    >
      <div class="t-card-mask"></div>
      <div class="t-title">{{ t.name }}</div>
      <div class="t-meta">
        <span class="t-status" :class="statusMap[t.status]">{{ statusMap[t.status] }}</span>
        <span class="t-type">{{ gameTypeMap[t.gameType] }}</span>
      </div>
      <div class="t-date">
        {{ formatDate(t.startDate) }} ~ {{ t.endDate ? formatDate(t.endDate) : '进行中' }}
      </div>
    </div>

    <!-- 弹窗详情 -->
    <div v-if="detailDialogVisible" class="tournament-dialog-mask" @click.self="detailDialogVisible = false">
      <div class="tournament-dialog">
        <div class="dialog-header">
          <div class="dialog-title">{{ selectedTournament?.name }}</div>
          <button class="close-btn" @click="detailDialogVisible = false">×</button>
        </div>
        <div class="dialog-content-flex">
          <div class="dialog-body">

            <div class="dialog-summary" v-if="selectedTournament?.summary">
              {{ selectedTournament.summary }}
            </div>

            <div class="dialog-info-row"><b>游戏类型：</b>{{ gameTypeMap[selectedTournament?.gameType] }}</div>
            <div class="dialog-info-row"><b>赛制状态：</b>
              <span :class="statusMap[selectedTournament?.status]">{{ statusMap[selectedTournament?.status] }}</span>
            </div>
            <div class="dialog-info-row"><b>举办时间：</b>
              {{ formatDate(selectedTournament?.startDate) }} ~ {{ selectedTournament?.endDate ? formatDate(selectedTournament?.endDate) : '进行中' }}
            </div>
            <div class="dialog-info-row"><b>赛制规则：</b>
              <pre class="t-rule">{{ selectedTournament?.rules }}</pre>
            </div>
            <div class="dialog-info-row"><b>参赛要求：</b><br>{{ selectedTournament?.requirements }}</div>
            <div class="dialog-info-row"><b>奖励：</b>{{ selectedTournament?.reward }}</div>
            <div class="dialog-info-row"><b>报名截止时间：</b>{{ formatDate(selectedTournament?.registrationDate) }}</div>
            <div class="dialog-info-row"><b>当前参赛人数：</b>{{ selectedTournament?.joinAmount ?? 0 }}</div>
            <div class="dialog-info-row" v-if="champion">
              <b>本届冠军：</b>
              <span class="champion">{{ champion.name }}</span>
            </div>
          </div>
          <div class="signup-list">
      <div class="signup-list-title">已报名用户</div>
      <div v-if="signupList.length === 0" class="signup-list-empty">暂无报名</div>
      <div v-else>
        <div v-for="u in signupList" :key="u.id" class="signup-user">
          <img :src="`${baseApiUrl}/api/UserInfo/imageproxy?path=${u.logo}`" class="signup-avatar" />
          <div class="signup-username">{{ u.username }}</div>
        </div>
      </div>
    </div>




        </div>
        <div class="dialog-footer">
          <button
            v-if="canSignUp && !isSignedUp"
            class="signup-btn"
            @click="signUp"
          >报名参赛</button>
          <button
            v-if="canSignUp && isSignedUp"
            class="signup-btn cancel"
            @click="cancelSignUp"
          >取消报名</button>
          <button
            v-if="!canSignUp"
            class="signup-btn disabled"
            disabled
          >报名已截止</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dialog-summary {
  white-space: pre-line;
  color: #334e77;
  background: #e8f3ff;
  border-radius: 7px;
  padding: 10px 14px;
  margin-bottom: 18px;
  font-size: 1.04rem;
  font-weight: 500;
}
.tournament-list {
  display: flex;
  flex-wrap: wrap;
  gap: 30px;
}
.tournament-card {
  flex: 0 0 270px;
  background: rgba(255,255,255,0.25);
  border-radius: 16px;
  box-shadow: 0 4px 18px 0 rgba(60,90,160,0.11);
  padding: 22px 20px 18px 20px;
  cursor: pointer;
  transition: box-shadow 0.18s, transform 0.16s;
  position: relative;
  border: 1.5px solid #e7eaf5bb;
  backdrop-filter: blur(10px) saturate(1.08);
  overflow: hidden;
  background-size: cover;
  background-position: center;
  color: #fff; /* 白字适应深遮罩 */
}
.tournament-card:hover {
  box-shadow: 0 8px 28px 0 #90caf9aa;
  transform: translateY(-5px) scale(1.028);
  border-color: #7bb1ff66;
}
.t-card-mask {
  position: absolute;
  inset: 0;
  background: rgba(0,0,0,0.42); /* 深色半透明遮罩 */
  z-index: 0;
  pointer-events: none;
}
.t-title,
.t-meta,
.t-date {
  position: relative;
  z-index: 1;
  color: #fff;
  text-shadow: 0 2px 8px rgba(0,0,0,0.46), 0 0px 1px #222;
}
.t-title {
  font-size: 1.28rem;
  font-weight: bold;
  color: #fff;
  margin-bottom: 12px;
  letter-spacing: 1.5px;
  text-shadow: 0 3px 12px #000a, 0 1px 2px #222;
}
.t-meta {
  display: flex;
  gap: 16px;
  font-size: 1.01rem;
  margin-bottom: 6px;
  color: #efefef;
  text-shadow: 0 2px 6px #000c;
}
.t-status {
  padding: 2px 12px;
  border-radius: 8px;
  font-size: 0.99rem;
  color: #fff;
  box-shadow: 0 1px 6px #0004;
  border: 1px solid #fff4;
  background: linear-gradient(90deg,#1565c0,#26c6da);
}
.t-status.举办中 { background: linear-gradient(90deg,#4fc3f7,#1976d2); }
.t-status.已结束 { background: linear-gradient(90deg,#90a4ae,#bdbdbd); }
.t-type {
  color: #ffecb3;
  font-weight: 500;
  text-shadow: 0 1px 4px #000a;
}
.t-date {
  font-size: 0.97rem;
  color: #f1f1f1;
  margin-bottom: 2px;
  text-shadow: 0 2px 6px #000c;
}

/* 弹窗样式 */
.tournament-dialog-mask {
  position: fixed;
  z-index: 1510;
  left: 0; right: 0; top: 0; bottom: 0;
  background: rgba(40,60,90,0.28);
  display: flex;
  align-items: center;
  justify-content: center;
  /* 新增：允许弹窗内容超出时滚动 */
  overflow-y: auto;
}

.tournament-dialog {
  width: 800px;
  max-height: 90vh; /* 关键：最大高度为视口高度的90% */
  background: #f6fafd;
  border-radius: 16px;
  box-shadow: 0 8px 32px 0 #84b7f333;
  padding: 0;
  overflow: auto; /* 超出部分弹窗内部滚动 */
  animation: dialog-in .22s cubic-bezier(.32,1.38,.42,.97);
  display: flex;
  flex-direction: column;
}
@keyframes dialog-in {
  0% { opacity: 0; transform: translateY(50px) scale(0.98);}
  100% { opacity: 1; transform: none;}
}
.dialog-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 22px 30px 12px 28px;
  background: linear-gradient(90deg,#e3f2fd 60%,#fafffd 100%);
}
.dialog-title {
  font-size: 1.22rem;
  font-weight: bold;
  color: #263a67;
  letter-spacing: 1.2px;
}
.close-btn {
  background: none;
  border: none;
  font-size: 2.1rem;
  line-height: 1;
  color: #6b7ebc;
  cursor: pointer;
  padding: 0 2px;
  margin-left: 10px;
  transition: color 0.18s;
}
.close-btn:hover { color: #e57373; }
.dialog-body {
  padding: 18px 28px 26px 28px;
  font-size: 1.01rem;
  color: #20407c;
}
.dialog-info-row {
  margin-bottom: 14px;
  line-height: 1.7;
  white-space: pre-line;
  word-break: break-all;
}
.t-rule {
  background: #f2f6fa;
  border-radius: 7px;
  padding: 8px 12px;
  margin-top: 5px;
  font-size: 0.97rem;
  color: #37474f;
  white-space: pre-line;
}
.champion {
  color: #d32f2f;
  font-weight: bold;
  font-size: 1.05rem;
}
.dialog-footer {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  padding: 18px 28px 20px 28px;
  border-top: 1px solid #e0e6ef;
  background: #f2f7fd;
}
.signup-btn {
  padding: 8px 28px;
  border: none;
  border-radius: 8px;
  background: #1976d2;
  color: #fff;
  font-size: 1.09rem;
  font-weight: bold;
  cursor: pointer;
  transition: background 0.17s;
}
.signup-btn.cancel {
  background: #f25d5d;
}
.signup-btn.disabled,
.signup-btn:disabled {
  background: #bdbdbd;
  cursor: not-allowed;
}
.dialog-content-flex {
  display: flex;
  gap: 24px;
}
.dialog-body {
  flex: 1 1 0;
  min-width: 0;
}
.signup-list {
  width: 200px;
  max-height: 350px;
  overflow-y: auto;
  background: #f9fbfd;
  border-radius: 8px;
  padding: 10px 6px 10px 6px;
  box-shadow: 0 2px 8px #e3edfa33;
  display: flex;
  flex-direction: column;
  align-items: center;
}
.signup-list-title {
  font-size: 1.04rem;
  font-weight: 600;
  color: #1565c0;
  margin-bottom: 12px;
  letter-spacing: 1px;
}
.signup-list-empty {
  color: #c0c7d1;
  font-size: 0.97rem;
  margin-top: 12px;
}
.signup-user {
  display: flex;
  flex-direction: row; 
  align-items: center;
  margin-bottom: 11px;
}
.signup-avatar {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  object-fit: cover;
  margin-bottom: 3px;
  box-shadow: 0 2px 8px #cfe5ff44;
  border: 2px solid #fff;
  background: #eee;
}
.signup-username {
  font-size: 0.95rem;
  color: #263a67;
  word-break: break-all;
  text-align: center;
  max-width: 90px;
}
</style>