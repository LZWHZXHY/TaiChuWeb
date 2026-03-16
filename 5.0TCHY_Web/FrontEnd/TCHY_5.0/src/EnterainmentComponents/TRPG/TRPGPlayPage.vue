<template>
  <div class="play-zone" :class="{ 'is-gm-mode': isGM }">
    <aside class="status-sidebar">
      <div class="monitor-header">
        <span class="icon">{{ isGM ? '👑' : '📡' }}</span>
        SQUAD_MONITOR // {{ isGM ? '指挥官权限已激活' : '战术小组' }}
      </div>
      
      <div class="unit-scroll custom-scroll">
        <div 
          v-for="p in participants" 
          :key="p.id" 
          class="unit-status-card"
          :class="{ 'can-edit': isGM }"
          @click="gmEditStat(p)"
        >
          <div class="u-brief">
            <CommonAvatar :userId="p.id" :passedAvatar="p.avatar" class="u-avatar" />
            <div class="u-names">
              <span class="p-name">{{ p.username }}</span>
              <span class="oc-name">PC: {{ p.ocName || '载入中...' }}</span>
            </div>
          </div>
          
          <div class="status-bars">
            <div class="bar-row">
              <span class="label">HP</span>
              <div class="bar-bg">
                <div class="bar-val hp" :style="{ width: (p.hp / p.hpMax * 100) + '%' }"></div>
              </div>
              <span class="num">{{ p.hp }}/{{ p.hpMax }}</span>
            </div>
            <div class="bar-row">
              <span class="label">SAN</span>
              <div class="bar-bg">
                <div class="bar-val san" :style="{ width: (p.san / p.sanMax * 100) + '%' }"></div>
              </div>
              <span class="num">{{ p.san }}/{{ p.sanMax }}</span>
            </div>
          </div>

          <div v-if="isGM" class="gm-hint">点击进行数值干预 // OVERRIDE_STATS</div>
        </div>
        
        <div v-if="participants.length === 0" class="empty-msg">等待信号接入...</div>
      </div>
    </aside>

    <main class="tactical-map-area">
      <div class="map-container" :style="{ backgroundImage: `url(${mapUrl})` }">
        <div class="grid-overlay"></div>
        
        <div class="map-controls">
          <button v-if="isGM" class="m-btn admin" @click="changeMap">🖼️ 更换战术图纸</button>
          <button class="m-btn">🌓 迷雾系统</button>
          <button class="m-btn">📏 战术测距</button>
        </div>
      </div>
    </main>

    <aside class="comms-sidebar">
      <div class="log-header">COMMAND_LOG // 指令流</div>
      <div class="log-body custom-scroll" ref="logContainer">
        <div v-for="(log, i) in diceLogs" :key="i" class="log-entry" :class="{ 'gm-post': log.isGM }">
          <div class="log-meta">
            <span class="log-time">{{ log.time }}</span>
            <span class="log-user">[{{ log.isGM ? 'GM' : 'UNIT' }}] {{ log.user }}</span>
          </div>
          <p class="log-msg" :class="{ 'critical': log.isCritical }">{{ log.content }}</p>
        </div>
      </div>
      
      <div class="dice-console">
        <div class="dice-quick-btns">
          <button v-for="d in [100, 20, 10, 6]" :key="d" @click="roll(d)" class="d-btn">D{{ d }}</button>
        </div>
        <div class="input-row">
          <input 
            type="text" 
            :placeholder="isGM ? '输入叙事指令或环境描述...' : '输入对话/扮演内容...'" 
            v-model="message" 
            @keyup.enter="sendMsg" 
          />
          <button class="send-btn" :class="{ 'gm': isGM }" @click="sendMsg">
            {{ isGM ? 'BROADCAST' : 'SEND' }}
          </button>
        </div>
      </div>
    </aside>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import apiClient from '@/utils/api'
import { useAuthStore } from '@/utils/auth'
import CommonAvatar from '@/GeneralComponents/UserAvatar.vue'
import { ElMessageBox, ElMessage } from 'element-plus'

const route = useRoute()
const auth = useAuthStore()
const logContainer = ref(null)

// 状态定义
const data = ref(null)
const participants = ref([])
const diceLogs = ref([])
const message = ref('')
const mapUrl = ref('https://img.bianyuzhou.com/assets/default_map.jpg')

// 💡 权限判断逻辑
const isGM = computed(() => data.value?.userId === auth.user?.id)

// 初始化战术数据
const fetchCombatData = async () => {
  try {
    // 1. 获取行动详情，确定发起人身份
    const resDetail = await apiClient.get(`Activity/${route.params.id}`)
    data.value = resDetail.data.data
    
    // 2. 获取参战名单
    const resMembers = await apiClient.get(`Activity/${route.params.id}/participants`)
    
    // 模拟解包 OC 的 MetaJson 数值
    participants.value = resMembers.data.data.map(p => ({
      ...p,
      hp: 12, hpMax: 15, // 后续建议改为从 p.ocMetaJson 中解析
      san: 45, sanMax: 60
    }))
  } catch (e) {
    ElMessage.error("指挥链路初始化失败")
  }
}

// GM 修改数值：点击头像区域触发
const gmEditStat = async (player) => {
  if (!isGM.value) return
  
  try {
    const { value: statType } = await ElMessageBox.confirm(
      `正在干预单位 [${player.username}] 的数值，请选择目标属性：`,
      '战术干预 // GM_OVERRIDE',
      { confirmButtonText: 'HP (生命)', cancelButtonText: 'SAN (理智)', distinguishCancelAndClose: true }
    ).then(() => ({ value: 'HP' })).catch(action => action === 'cancel' ? ({ value: 'SAN' }) : Promise.reject())

    const { value: newVal } = await ElMessageBox.prompt(`请输入新的 ${statType} 数值`, '执行修正', {
      inputPattern: /^-?\d+$/,
      inputErrorMessage: '必须为整数'
    })

    if (statType === 'HP') player.hp = parseInt(newVal)
    else player.san = parseInt(newVal)

    addLog(`修正了单位 [${player.username}] 的 ${statType} 为 ${newVal}`, true)
    ElMessage.success("数值修正指令已下达")
  } catch (e) { /* 取消操作 */ }
}

const changeMap = async () => {
  const { value: url } = await ElMessageBox.prompt('请输入新的战术图纸 (图像URL)', '更换地图', {
    inputPlaceholder: 'https://...'
  })
  if (url) {
    mapUrl.value = url
    addLog(`更换了区域战术图纸`, true)
  }
}

const roll = (max) => {
  const result = Math.floor(Math.random() * max) + 1
  // COC 规则判定大成功逻辑
  const isCritical = (max === 100 && result <= 5) || (max === 20 && result === 20)
  addLog(`掷出了 D${max}: ${result} ${isCritical ? '!!! 关键成功 !!!' : ''}`, isGM.value, isCritical)
}

const sendMsg = () => {
  if (!message.value.trim()) return
  addLog(message.value, isGM.value)
  message.value = ''
}

const addLog = (content, gm = false, critical = false) => {
  diceLogs.value.push({
    time: new Date().toLocaleTimeString('zh-CN', { hour12: false }),
    user: auth.user?.username || 'UNKNOWN',
    content,
    isGM: gm,
    isCritical: critical
  })
  // 滚动到底部
  nextTick(() => {
    if (logContainer.value) logContainer.value.scrollTop = logContainer.value.scrollHeight
  })
}

onMounted(fetchCombatData)
</script>

<style scoped>
.play-zone {
  display: grid; grid-template-columns: 280px 1fr 340px; height: 100vh;
  background: #050505; color: #eee; font-family: 'JetBrains Mono', 'PingFang SC', monospace;
  overflow: hidden;
}

/* --- 左侧状态栏 --- */
.status-sidebar { background: #0d0d0d; border-right: 1px solid #222; display: flex; flex-direction: column; }
.monitor-header { padding: 20px; font-weight: 900; font-size: 11px; background: #151515; letter-spacing: 1px; border-bottom: 2px solid #222; }
.is-gm-mode .monitor-header { background: #D92323; color: #fff; }

.unit-scroll { flex: 1; overflow-y: auto; padding: 10px; }
.unit-status-card { 
  padding: 15px; background: #111; border: 1px solid #222; margin-bottom: 10px;
  position: relative; overflow: hidden;
}
.unit-status-card.can-edit:hover { border-color: #D92323; background: #1a1010; cursor: pointer; }

.u-brief { display: flex; gap: 12px; align-items: center; margin-bottom: 15px; }
.u-avatar { width: 45px; height: 45px; border: 1px solid #333; }
.p-name { font-size: 13px; font-weight: 900; color: #fff; display: block; }
.oc-name { font-size: 10px; color: #666; font-style: italic; }

.bar-row { display: flex; align-items: center; gap: 10px; margin-bottom: 6px; font-size: 10px; }
.label { width: 30px; font-weight: 900; color: #888; }
.bar-bg { flex: 1; height: 8px; background: #000; border-radius: 4px; overflow: hidden; }
.bar-val { height: 100%; transition: width 0.6s cubic-bezier(0.22, 1, 0.36, 1); }
.hp { background: #D92323; box-shadow: 0 0 10px rgba(217, 35, 35, 0.4); }
.san { background: #00F0FF; box-shadow: 0 0 10px rgba(0, 240, 255, 0.4); }
.num { width: 45px; text-align: right; color: #aaa; font-family: monospace; }
.gm-hint { font-size: 8px; color: #D92323; margin-top: 10px; font-weight: 900; text-align: center; opacity: 0.5; }

/* --- 中间地图区 --- */
.tactical-map-area { position: relative; background: #000; }
.map-container { 
  width: 100%; height: 100%; background-size: contain; background-repeat: no-repeat; 
  background-position: center; position: relative; transition: 0.3s;
}
.grid-overlay { 
  position: absolute; inset: 0; pointer-events: none;
  background-image: linear-gradient(rgba(255,255,255,0.03) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.03) 1px, transparent 1px);
  background-size: 40px 40px;
}
.map-controls { position: absolute; top: 20px; left: 20px; display: flex; gap: 10px; }
.m-btn { 
  background: rgba(0,0,0,0.8); border: 1px solid #444; color: #fff; 
  padding: 8px 15px; font-size: 10px; font-weight: 900; cursor: pointer;
}
.m-btn.admin { border-color: #D92323; color: #D92323; }
.m-btn:hover { background: #fff; color: #000; border-color: #fff; }

/* --- 右侧日志区 --- */
.comms-sidebar { background: #0d0d0d; border-left: 1px solid #222; display: flex; flex-direction: column; }
.log-header { padding: 15px 20px; font-weight: 900; font-size: 11px; background: #151515; }
.log-body { flex: 1; padding: 15px; font-size: 13px; line-height: 1.6; }
.log-entry { margin-bottom: 12px; padding: 10px; border-left: 2px solid #333; background: rgba(255,255,255,0.02); }
.log-meta { margin-bottom: 5px; font-size: 11px; }
.log-time { color: #555; margin-right: 8px; }
.log-user { color: #00F0FF; font-weight: 900; }
.log-msg { margin: 0; color: #bbb; word-break: break-all; }

/* GM 日志特有样式 */
.gm-post { background: rgba(217, 35, 35, 0.08); border-left-color: #D92323; }
.gm-post .log-user { color: #D92323; }
.gm-post .log-msg { color: #fff; font-weight: bold; }
.critical { color: #ff0055 !important; text-decoration: underline; font-weight: 900; }

/* 交互终端 */
.dice-console { padding: 20px; background: #111; border-top: 2px solid #222; }
.dice-quick-btns { display: grid; grid-template-columns: repeat(4, 1fr); gap: 10px; margin-bottom: 15px; }
.d-btn { 
  background: #000; border: 1px solid #333; color: #fff; padding: 10px 0; 
  font-weight: 900; cursor: pointer; transition: 0.2s; font-size: 11px;
}
.d-btn:hover { border-color: #00F0FF; color: #00F0FF; background: rgba(0, 240, 255, 0.05); }

.input-row { display: flex; background: #000; border: 1px solid #333; padding: 5px; }
.input-row input { 
  flex: 1; background: transparent; border: none; color: #fff; 
  padding: 10px; font-family: inherit; outline: none; font-size: 13px;
}
.send-btn { 
  background: #222; border: none; color: #888; padding: 0 20px; 
  font-weight: 900; cursor: pointer; font-size: 10px;
}
.send-btn.gm { background: #D92323; color: #fff; }
.send-btn:hover { background: #333; color: #fff; }

.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #333; }
</style>