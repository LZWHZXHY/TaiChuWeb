<template>
  <div class="user-card">
    <div class="uc-left">
      <div class="top-row">
        <div class="identity-info">
          <div class="name-wrapper">
            <h1 class="user-name">{{ name }}</h1>
            <span class="gender-badge" :class="gender === 'Male' ? 'male' : 'female'">
              {{ gender === 'Male' ? '男' : '女' }}
            </span>
          </div>
          <div class="job-title">资深神经架构师 // ARCHITECT</div>
        </div>

        <div class="action-group">
          <div class="primary-btns">
            <button 
              class="u-btn btn-focus" 
              :class="{ 'is-active': isFollowed }"
              @click="toggleFollow"
            >
              <span v-if="!isFollowed" class="btn-text">+ 关注</span>
              <span v-else class="btn-text fans-num">粉丝 {{ formatFans(fansCount) }}</span>
            </button>
            <button class="u-btn btn-msg">
              <span class="btn-text">私信</span>
            </button>
          </div>
          <div class="secondary-btns">
            <button class="round-btn block-btn" title="拉黑">
              <span class="icon">⊘</span>
            </button>
            <button class="round-btn report-btn" title="举报">
              <span class="icon">⚠</span>
            </button>
          </div>
        </div>
      </div>

      <div class="bio-container">
        <div class="tags-list">
          <span v-for="(tag, index) in tags" :key="index" class="tag-item">
            #{{ tag }}
          </span>
        </div>
        <p class="bio-text">{{ bio }}</p>
      </div>

      <div class="footer-row">
        <span class="ft-item">📍 {{ location }}</span>
        <span class="ft-divider">|</span>
        <span class="ft-item">✉ {{ contact }}</span>
      </div>
      
      <div class="grid-bg"></div>
    </div>

    <div class="uc-center">
      <div class="avatar-wrapper">
        <img :src="avatar" class="big-avatar" />
        <div class="level-pill">
          <span class="lv-txt">LV.</span>
          <span class="lv-num">{{ level }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, defineProps } from 'vue'

const props = defineProps({
  name: { type: String, default: 'K_Runner' },
  gender: { type: String, default: 'Male' }, 
  bio: { type: String, default: '义体维修专家，专注于老式神经网络接口的调试。寻找电子幽灵的踪迹。' },
  tags: { type: Array, default: () => ['赛博义体', '全栈开发', '黑客'] },
  location: { type: String, default: '夜之城·第七区' },
  contact: { type: String, default: 'k_dev@net.connect' },
  avatar: { type: String, default: 'https://image2url.com/r2/default/images/1769279786393-674d59cf-5620-4bde-9d31-c66661aa72f2.jpg' },
  level: { type: Number, default: 42 },
  fansCount: { type: Number, default: 12580 }
})

const isFollowed = ref(false)
const toggleFollow = () => {
  isFollowed.value = !isFollowed.value
}

const formatFans = (num) => {
  return num > 9999 ? (num / 10000).toFixed(1) + 'w' : num
}
</script>

<style scoped>
/* 引入字体 */
@import url('https://fonts.googleapis.com/css2?family=Share+Tech+Mono&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Noto+Sans+SC:wght@400;700;900&display=swap');

.user-card {
  width: 100%;
  height: 100%;
  max-height: 178px; /* 严格限高 */
  display: flex;
  background: transparent;
  font-family: 'Noto Sans SC', sans-serif;
  color: #2c3e50;
  overflow: hidden;
  position: relative;
}

/* === 左侧 uc-left (Flex 2) === */
.uc-left {
  flex: 2;
  padding: 12px 16px;
  display: flex;
  flex-direction: column;
  position: relative;
  z-index: 1;
}

/* 顶部布局 */
.top-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 8px;
}

.identity-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.name-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
}

.user-name {
  margin: 0;
  font-size: 26px; /* 加大名字 */
  font-weight: 900;
  line-height: 1.1;
  color: #1a1a1a;
}

.gender-badge {
  font-size: 11px;
  padding: 1px 4px;
  border-radius: 2px;
  color: #fff;
  font-weight: bold;
}
.male { background-color: #3498db; }
.female { background-color: #e91e63; }

.job-title {
  font-size: 10px;
  color: #e67e22;
  font-weight: bold;
  letter-spacing: 0.5px;
  font-family: 'Share Tech Mono', monospace;
}

/* --- 右上角按钮组 --- */
.action-group {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 6px;
}

.primary-btns {
  display: flex;
  gap: 8px;
}

/* 大按钮样式 */
.u-btn {
  height: 32px;
  padding: 0 16px;
  border: 2px solid #2c3e50;
  background: transparent;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
  /* 赛博切角 */
  clip-path: polygon(10px 0, 100% 0, 100% calc(100% - 10px), calc(100% - 10px) 100%, 0 100%, 0 10px);
  transition: all 0.4s ease-in-out;
}

.btn-text {
  font-size: 14px;
  font-weight: bold;
  color: #ec5353;
}

.btn-focus:hover { background: rgba(224, 35, 35, 1); border-color: #e63922; }
.btn-focus:hover .btn-text { color: #ffffff; }

/* 关注激活态 */
.btn-focus.is-active {
  background: #2c3e50;
  border-color: #2c3e50;
  transition: all 0.4s ease-in-out;
}
.btn-focus.is-active .btn-text { color: #fff; }
.fans-num { font-family: 'Share Tech Mono'; font-size: 13px; }

.btn-msg {
  border-color: #2c3e50;
}
.btn-msg .btn-text { color: #2c3e50; }
.btn-msg:hover { border-color: #ffffff;background: #000000; }
.btn-msg:hover .btn-text { color: #ffffff; }

/* 小圆按钮组 */
.secondary-btns {
  display: flex;
  gap: 8px;
  padding-right: 4px; /* 微调对齐 */
}

.round-btn {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  border: 1px solid #000000;
  background: white;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: #f70000;
  transition: all 0.2s;
}
.round-btn:hover { transform: scale(1.1); }

.block-btn:hover { border-color: #ffffff; color: #ffffff; background: #000000; }
.report-btn:hover { border-color: #f39c12; color: #f39c12; background: #fef9e7; }
.icon { font-size: 12px; font-weight: bold; }


/* --- 中间简介区 --- */
.bio-container {
  flex: 1; /* 占据剩余高度 */
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-bottom: 4px;
  overflow: hidden;
}

.tags-list {
  display: flex;
  gap: 6px;
}
.tag-item {
  font-size: 12px;
  background: rgba(0,0,0,0.05);
  color: #555;
  padding: 1px 6px;
  border-radius: 2px;
}

.bio-text {
  margin: 0;
  font-size: 13px; /* 字体加大 */
  line-height: 1.5;
  color: #444;
  /* 限制3行 */
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
}


/* --- 底部信息 --- */
.footer-row {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  color: #777;
  font-family: 'Share Tech Mono', 'Noto Sans SC', sans-serif;
  border-top: 1px dashed rgba(0,0,0,0.1);
  padding-top: 6px;
}
.ft-divider { opacity: 0.3; font-size: 10px; }


/* 背景装饰 */
.grid-bg {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  background-image: radial-gradient(#ccc 1px, transparent 1px);
  background-size: 20px 20px;
  opacity: 0.2;
  z-index: -1;
  pointer-events: none;
}


/* === 右侧 uc-center (Flex 1) - 专属头像区 === */
.uc-center {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  /* 左边框分割 */
  border-left: 1px solid rgba(0,0,0,0.05);
  border-right: 1px solid rgba(0,0,0,0.05);
}

.avatar-wrapper {
  position: relative;
  width: 130px; /* 大尺寸 */
  height: 130px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.big-avatar {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  border: 4px solid #fff;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  object-fit: cover;
  background: #f0f0f0;
}

/* 等级胶囊 */
.level-pill {
  position: absolute;
  bottom: 0px;
  background: #2c3e50;
  color: #fff;
  padding: 2px 10px;
  border-radius: 12px;
  border: 2px solid #fff;
  display: flex;
  align-items: baseline;
  gap: 2px;
  box-shadow: 0 2px 6px rgba(0,0,0,0.15);
  z-index: 2;
}

.lv-txt { font-size: 10px; opacity: 0.8; font-family: 'Share Tech Mono'; }
.lv-num { font-size: 14px; font-weight: bold; font-family: 'Share Tech Mono'; color: #e67e22; }
</style>