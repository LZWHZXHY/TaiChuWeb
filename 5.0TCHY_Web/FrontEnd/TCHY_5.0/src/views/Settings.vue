<template>
  <div class="user-view-page">
    <!-- 页面头部 -->
    <div class="page-header">
      <h1 class="page-title">用户信息查看</h1>
      <div class="page-actions">
        <button class="action-btn" @click="refreshPage">
          <i>🔄</i> 刷新
        </button>
        <button class="action-btn" @click="toggleTheme">
          <i>{{ themeIcon }}</i> 主题
        </button>
      </div>
    </div>

    <!-- 主要内容区域 -->
    <div class="main-content">
      <!-- 左侧用户信息区域 -->
      <div class="left-panel">
        <!-- 用户基本信息卡片 -->
        <div class="user-basic-card">
          <div class="user-avatar-section">
            <img :src="userInfo.avatar" :alt="userInfo.name" class="user-avatar-large" />
            <div class="avatar-status" :class="userInfo.status"></div>
          </div>
          
          <div class="user-details">
            <h2 class="user-name">{{ userInfo.name }}</h2>
            <p class="user-username">@{{ userInfo.username }}</p>
            
            <div class="user-bio">{{ userInfo.bio }}</div>
            
            <div class="user-meta">
              <div class="meta-item">
                <i>📍</i> {{ userInfo.location }}
              </div>
              <div class="meta-item">
                <i>🔗</i> 
                <a :href="userInfo.website" target="_blank" class="website-link">
                  {{ userInfo.websiteDisplay }}
                </a>
              </div>
              <div class="meta-item">
                <i>📅</i> 加入于 {{ formatJoinDate(userInfo.joinDate) }}
              </div>
            </div>

            <div class="user-stats-grid">
              <div class="stat-card" v-for="stat in userStats" :key="stat.label">
                <div class="stat-value">{{ stat.value }}</div>
                <div class="stat-label">{{ stat.label }}</div>
              </div>
            </div>

            <div class="user-actions">
              <button class="primary-btn" @click="followUser">
                <span v-if="!isFollowing">关注用户</span>
                <span v-else>取消关注</span>
              </button>
              <button class="secondary-btn" @click="sendMessage">
                <i>💬</i> 发送消息
              </button>
              <button class="secondary-btn" @click="showMoreActions">
                <i>⋯</i> 更多
              </button>
            </div>
          </div>
        </div>

        <!-- 用户标签区域 -->
        <div class="user-tags-section">
          <h3 class="section-title">用户标签</h3>
          <div class="tags-container">
            <span v-for="tag in userTags" :key="tag" class="user-tag">
              {{ tag }}
            </span>
            <button class="add-tag-btn" @click="suggestTag">+ 建议标签</button>
          </div>
        </div>

        <!-- 用户技能区域 -->
        <div class="user-skills-section">
          <h3 class="section-title">技能与专长</h3>
          <div class="skills-list">
            <div v-for="skill in userSkills" :key="skill.name" class="skill-item">
              <div class="skill-name">{{ skill.name }}</div>
              <div class="skill-level">
                <div class="level-bar" :style="{ width: skill.level + '%' }"></div>
              </div>
              <span class="skill-years">{{ skill.years }}年经验</span>
            </div>
          </div>
        </div>

        <!-- 用户认证信息 -->
        <div class="user-verification-section" v-if="userInfo.verified">
          <h3 class="section-title">认证信息</h3>
          <div class="verification-badge">
            <i>✅</i> 已认证用户
            <span class="verification-info">{{ userInfo.verificationInfo }}</span>
          </div>
        </div>

        <!-- 用户联系方式（管理员可见） -->
        <div class="user-contact-section" v-if="isAdmin">
          <h3 class="section-title">联系方式（仅管理员可见）</h3>
          <div class="contact-info">
            <p><strong>邮箱：</strong>{{ userInfo.contact.email }}</p>
            <p><strong>电话：</strong>{{ userInfo.contact.phone }}</p>
            <p><strong>最后登录：</strong>{{ userInfo.lastLogin }}</p>
          </div>
        </div>
      </div>

      <!-- 右侧内容区域 -->
      <div class="right-panel">
        <!-- 标签导航 -->
        <div class="tabs-navigation">
          <button 
            v-for="tab in tabs" 
            :key="tab.id"
            class="tab-btn"
            :class="{ 'active': activeTab === tab.id }"
            @click="activeTab = tab.id"
          >
            {{ tab.name }}
            <span class="tab-badge">{{ getTabCount(tab.id) }}</span>
          </button>
        </div>

        <!-- 动态内容区 -->
        <div class="content-area">
          <!-- 动态列表 -->
          <div v-if="activeTab === 'activities'" class="activities-list">
            <div v-for="activity in userActivities" :key="activity.id" class="activity-item">
              <div class="activity-header">
                <span class="activity-type">{{ getActivityType(activity.type) }}</span>
                <span class="activity-time">{{ formatActivityTime(activity.time) }}</span>
              </div>
              <div class="activity-content">
                {{ activity.content }}
              </div>
              <div v-if="activity.attachments" class="activity-attachments">
                <img 
                  v-for="(img, idx) in activity.attachments" 
                  :key="idx"
                  :src="img"
                  class="attachment-img"
                  @click="viewImage(img)"
                />
              </div>
              <div class="activity-stats">
                <button class="stat-btn" @click="likeActivity(activity)">
                  <i>👍</i> {{ activity.likes }}
                </button>
                <button class="stat-btn" @click="commentOnActivity(activity)">
                  <i>💬</i> {{ activity.comments }}
                </button>
                <button class="stat-btn" @click="shareActivity(activity)">
                  <i>↪️</i> {{ activity.shares }}
                </button>
              </div>
            </div>
          </div>

          <!-- 作品展示 -->
          <div v-if="activeTab === 'works'" class="works-grid">
            <div v-for="work in userWorks" :key="work.id" class="work-card">
              <div class="work-image">
                <img :src="work.image" :alt="work.title" />
                <div class="work-category">{{ work.category }}</div>
              </div>
              <div class="work-info">
                <h4 class="work-title">{{ work.title }}</h4>
                <p class="work-desc">{{ work.description }}</p>
                <div class="work-meta">
                  <span class="work-date">{{ formatDate(work.date) }}</span>
                  <span class="work-views">👁️ {{ work.views }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- 粉丝列表 -->
          <div v-if="activeTab === 'followers'" class="followers-list">
            <div v-for="follower in userFollowers" :key="follower.id" class="follower-card">
              <img :src="follower.avatar" :alt="follower.name" class="follower-avatar" />
              <div class="follower-info">
                <h4 class="follower-name">{{ follower.name }}</h4>
                <p class="follower-bio">{{ follower.bio }}</p>
                <div class="follower-stats">
                  <span>👥 {{ follower.followers }} 粉丝</span>
                  <span>📝 {{ follower.posts }} 作品</span>
                </div>
              </div>
              <button 
                class="follow-btn"
                :class="{ 'following': follower.isFollowing }"
                @click="toggleFollowFollower(follower)"
              >
                {{ follower.isFollowing ? '已关注' : '关注' }}
              </button>
            </div>
          </div>

          <!-- 关注列表 -->
          <div v-if="activeTab === 'following'" class="following-list">
            <div v-for="following in userFollowing" :key="following.id" class="following-card">
              <img :src="following.avatar" :alt="following.name" class="following-avatar" />
              <div class="following-info">
                <h4 class="following-name">{{ following.name }}</h4>
                <p class="following-bio">{{ following.bio }}</p>
                <div class="following-stats">
                  <span>👥 {{ following.followers }} 粉丝</span>
                  <span>📝 {{ following.posts }} 作品</span>
                </div>
              </div>
              <button 
                class="follow-btn"
                :class="{ 'following': following.isFollowing }"
                @click="toggleFollowFollowing(following)"
              >
                {{ following.isFollowing ? '已关注' : '关注' }}
              </button>
            </div>
          </div>

          <!-- 收藏内容 -->
          <div v-if="activeTab === 'collections'" class="collections-grid">
            <div v-for="collection in userCollections" :key="collection.id" class="collection-card">
              <div class="collection-cover">
                <img :src="collection.cover" :alt="collection.title" />
                <div class="collection-count">{{ collection.itemCount }} 个项目</div>
              </div>
              <div class="collection-info">
                <h4 class="collection-title">{{ collection.title }}</h4>
                <p class="collection-desc">{{ collection.description }}</p>
                <div class="collection-meta">
                  <span>创建于 {{ formatDate(collection.createdAt) }}</span>
                  <span>👁️ {{ collection.views }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- 成就与徽章 -->
          <div v-if="activeTab === 'achievements'" class="achievements-section">
            <div class="achievements-stats">
              <div class="achievement-stat">
                <div class="stat-number">{{ userInfo.achievementScore }}</div>
                <div class="stat-label">成就积分</div>
              </div>
              <div class="achievement-stat">
                <div class="stat-number">{{ userAchievements.length }}</div>
                <div class="stat-label">获得徽章</div>
              </div>
              <div class="achievement-stat">
                <div class="stat-number">{{ userInfo.ranking }}</div>
                <div class="stat-label">全站排名</div>
              </div>
            </div>
            
            <div class="badges-grid">
              <div v-for="badge in userAchievements" :key="badge.id" class="badge-card">
                <div class="badge-icon">{{ badge.icon }}</div>
                <div class="badge-info">
                  <h5 class="badge-title">{{ badge.title }}</h5>
                  <p class="badge-desc">{{ badge.description }}</p>
                  <span class="badge-date">获得于 {{ formatDate(badge.earnedAt) }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- 数据分析 -->
          <div v-if="activeTab === 'analytics'" class="analytics-section">
            <div class="analytics-card">
              <h4 class="analytics-title">活跃度统计</h4>
              <div class="analytics-chart">
                <!-- 简单的柱状图模拟 -->
                <div class="chart-bars">
                  <div v-for="day in activityData" :key="day.day" class="chart-bar-container">
                    <div class="chart-bar" :style="{ height: day.activity + '%' }"></div>
                    <div class="chart-label">{{ day.day }}</div>
                  </div>
                </div>
              </div>
            </div>

            <div class="analytics-stats-grid">
              <div class="analytics-stat">
                <div class="stat-label">本周活跃天数</div>
                <div class="stat-value">{{ analytics.weeklyActiveDays }}</div>
              </div>
              <div class="analytics-stat">
                <div class="stat-label">平均日活</div>
                <div class="stat-value">{{ analytics.avgDailyActivity }}</div>
              </div>
              <div class="analytics-stat">
                <div class="stat-label">峰值活跃时间</div>
                <div class="stat-value">{{ analytics.peakTime }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 图片查看模态框 -->
    <div v-if="viewingImage" class="image-view-modal" @click="closeImageView">
      <div class="modal-content" @click.stop>
        <button class="modal-close" @click="closeImageView">×</button>
        <img :src="viewingImage" alt="查看的图片" />
      </div>
    </div>

    <!-- 更多操作菜单 -->
    <div v-if="showActionsMenu" class="actions-menu">
      <div class="menu-content">
        <h4>更多操作</h4>
        <button class="menu-item" @click="blockUser">🚫 屏蔽用户</button>
        <button class="menu-item" @click="reportUser">⚠️ 举报用户</button>
        <button class="menu-item" @click="exportUserData">📥 导出数据</button>
        <button class="menu-item" @click="showActionsMenu = false">取消</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

// 主题状态
const darkMode = ref(false)
const themeIcon = computed(() => darkMode.value ? '🌙' : '☀️')

// 用户基本信息
const userInfo = ref({
  id: 1,
  name: '张三',
  username: 'zhangsan',
  avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=200&h=200&fit=crop&crop=face',
  bio: '全栈开发者 | UI/UX设计师 | 技术博主 | 开源爱好者',
  location: '北京, 中国',
  website: 'https://zhangsan.dev',
  websiteDisplay: 'zhangsan.dev',
  joinDate: '2020-03-15',
  status: 'online',
  verified: true,
  verificationInfo: '技术领域专家认证',
  lastLogin: '2024-01-15 14:30',
  achievementScore: 1250,
  ranking: 42,
  contact: {
    email: 'zhangsan@example.com',
    phone: '+86 13800138000'
  }
})

// 用户统计
const userStats = ref([
  { label: '作品', value: 128 },
  { label: '粉丝', value: 2345 },
  { label: '关注', value: 156 },
  { label: '收藏', value: 89 },
  { label: '获赞', value: 12560 },
  { label: '评论', value: 3456 }
])

// 用户标签
const userTags = ref(['全栈开发', 'Vue.js', 'React', 'Node.js', 'UI设计', '用户体验', '开源', '技术写作'])

// 用户技能
const userSkills = ref([
  { name: 'JavaScript', level: 90, years: 5 },
  { name: 'Vue.js', level: 85, years: 4 },
  { name: 'React', level: 80, years: 3 },
  { name: 'Node.js', level: 75, years: 4 },
  { name: 'UI/UX设计', level: 70, years: 3 },
  { name: 'Python', level: 60, years: 2 }
])

// 标签页配置
const tabs = ref([
  { id: 'activities', name: '动态' },
  { id: 'works', name: '作品' },
  { id: 'followers', name: '粉丝' },
  { id: 'following', name: '关注' },
  { id: 'collections', name: '收藏' },
  { id: 'achievements', name: '成就' },
  { id: 'analytics', name: '数据' }
])

const activeTab = ref('activities')

// 用户动态
const userActivities = ref([
  { 
    id: 1, 
    type: 'post', 
    content: '刚刚发布了一篇关于Vue3性能优化的新文章，欢迎大家阅读讨论！', 
    time: '2024-01-15 14:30:00',
    likes: 124,
    comments: 23,
    shares: 8,
    attachments: [
      'https://images.unsplash.com/photo-1513475382585-d06e58bcb0e0?w=400&h=300&fit=crop'
    ]
  },
  { 
    id: 2, 
    type: 'like', 
    content: '点赞了李四的"前端工程化实践"项目', 
    time: '2024-01-14 10:15:00',
    likes: 0,
    comments: 0,
    shares: 0
  },
  { 
    id: 3, 
    type: 'comment', 
    content: '在"React Hooks最佳实践"文章中评论：这个方案很实用，感谢分享！', 
    time: '2024-01-13 16:45:00',
    likes: 5,
    comments: 2,
    shares: 0
  },
  { 
    id: 4, 
    type: 'share', 
    content: '分享了王五的"Web性能优化指南"', 
    time: '2024-01-12 11:20:00',
    likes: 8,
    comments: 3,
    shares: 2
  }
])

// 用户作品
const userWorks = ref([
  { 
    id: 1, 
    title: 'Vue3企业级后台管理系统', 
    description: '基于Vue3 + TypeScript + Pinia开发的企业级后台管理系统模板',
    image: 'https://images.unsplash.com/photo-1551650975-87deedd944c3?w=400&h=300&fit=crop',
    category: '前端项目',
    date: '2024-01-10',
    views: 1250
  },
  { 
    id: 2, 
    title: 'Node.js微服务架构实践', 
    description: '使用Node.js构建可扩展的微服务架构的完整指南和示例代码',
    image: 'https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=400&h=300&fit=crop',
    category: '后端项目',
    date: '2024-01-05',
    views: 890
  },
  { 
    id: 3, 
    title: '响应式UI组件库', 
    description: '一套现代化、可定制的React UI组件库',
    image: 'https://images.unsplash.com/photo-1551650975-87deedd944c3?w=401&h=301&fit=crop',
    category: 'UI组件',
    date: '2023-12-28',
    views: 1560
  },
  { 
    id: 4, 
    title: '数据可视化仪表盘', 
    description: '使用D3.js和ECharts构建的交互式数据可视化仪表盘',
    image: 'https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=400&h=300&fit=crop',
    category: '数据可视化',
    date: '2023-12-20',
    views: 2100
  }
])

// 用户粉丝
const userFollowers = ref([
  { 
    id: 1, 
    name: '李四', 
    avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100&h=100&fit=crop',
    bio: '前端开发者',
    followers: 1200,
    posts: 45,
    isFollowing: true
  },
  { 
    id: 2, 
    name: '王五', 
    avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=101&h=101&fit=crop',
    bio: '全栈工程师',
    followers: 890,
    posts: 32,
    isFollowing: false
  },
  { 
    id: 3, 
    name: '赵六', 
    avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=102&h=102&fit=crop',
    bio: 'UI设计师',
    followers: 560,
    posts: 28,
    isFollowing: true
  }
])

// 用户关注
const userFollowing = ref([
  { 
    id: 1, 
    name: '技术大佬A', 
    avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=103&h=103&fit=crop',
    bio: '资深架构师',
    followers: 5200,
    posts: 120,
    isFollowing: true
  },
  { 
    id: 2, 
    name: '设计专家B', 
    avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=104&h=104&fit=crop',
    bio: '资深UI/UX设计师',
    followers: 3100,
    posts: 85,
    isFollowing: true
  },
  { 
    id: 3, 
    name: '开源贡献者C', 
    avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=105&h=105&fit=crop',
    bio: '知名开源项目维护者',
    followers: 8900,
    posts: 210,
    isFollowing: false
  }
])

// 用户收藏
const userCollections = ref([
  { 
    id: 1, 
    title: '前端学习资源', 
    description: '收集优质的前端开发学习资源和教程',
    cover: 'https://images.unsplash.com/photo-1551650975-87deedd944c3?w-400&h=300&fit=crop',
    itemCount: 45,
    views: 1200,
    createdAt: '2024-01-10'
  },
  { 
    id: 2, 
    title: '设计灵感', 
    description: '优秀的UI/UX设计案例和灵感收集',
    cover: 'https://images.unsplash.com/photo-1551288049-bebda4e38f71?w-401&h=301&fit=crop',
    itemCount: 68,
    views: 890,
    createdAt: '2024-01-05'
  }
])

// 用户成就
const userAchievements = ref([
  { 
    id: 1, 
    icon: '🏆', 
    title: '活跃贡献者', 
    description: '连续30天发布优质内容',
    earnedAt: '2024-01-15'
  },
  { 
    id: 2, 
    icon: '⭐', 
    title: '技术专家', 
    description: '获得1000个技术类回答的赞同',
    earnedAt: '2024-01-10'
  },
  { 
    id: 3, 
    icon: '👑', 
    title: '社区领袖', 
    description: '帮助100位用户解决问题',
    earnedAt: '2024-01-05'
  },
  { 
    id: 4, 
    icon: '🚀', 
    title: '快速学习者', 
    description: '完成所有新手任务',
    earnedAt: '2024-01-01'
  }
])

// 活动数据
const activityData = ref([
  { day: '周一', activity: 80 },
  { day: '周二', activity: 60 },
  { day: '周三', activity: 90 },
  { day: '周四', activity: 70 },
  { day: '周五', activity: 95 },
  { day: '周六', activity: 50 },
  { day: '周日', activity: 40 }
])

// 分析数据
const analytics = ref({
  weeklyActiveDays: 5,
  avgDailyActivity: 72,
  peakTime: '14:00-16:00'
})

// 状态
const isFollowing = ref(false)
const isAdmin = ref(true) // 模拟管理员身份
const viewingImage = ref(null)
const showActionsMenu = ref(false)

// 计算方法
const getTabCount = (tabId) => {
  switch(tabId) {
    case 'activities': return userActivities.value.length
    case 'works': return userWorks.value.length
    case 'followers': return userFollowers.value.length
    case 'following': return userFollowing.value.length
    case 'collections': return userCollections.value.length
    case 'achievements': return userAchievements.value.length
    default: return 0
  }
}

// 工具方法
const formatJoinDate = (dateStr) => {
  const date = new Date(dateStr)
  return date.toLocaleDateString('zh-CN', { 
    year: 'numeric', 
    month: 'long'
  })
}

const formatActivityTime = (timeStr) => {
  const date = new Date(timeStr)
  const now = new Date()
  const diff = now - date
  const hours = Math.floor(diff / (1000 * 60 * 60))
  
  if (hours < 1) {
    return '刚刚'
  } else if (hours < 24) {
    return `${hours}小时前`
  } else {
    return date.toLocaleDateString('zh-CN', { 
      month: 'short', 
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  }
}

const formatDate = (dateStr) => {
  return new Date(dateStr).toLocaleDateString('zh-CN', { 
    year: 'numeric',
    month: 'short', 
    day: 'numeric'
  })
}

const getActivityType = (type) => {
  const types = {
    'post': '发布',
    'like': '点赞',
    'comment': '评论',
    'share': '分享'
  }
  return types[type] || '动态'
}

// 操作方法
const refreshPage = () => {
  console.log('刷新页面数据')
  // 在实际应用中，这里可以重新获取用户数据
}

const toggleTheme = () => {
  darkMode.value = !darkMode.value
  document.documentElement.setAttribute('data-theme', darkMode.value ? 'dark' : 'light')
}

const followUser = () => {
  isFollowing.value = !isFollowing.value
  console.log(isFollowing.value ? '关注用户' : '取消关注')
}

const sendMessage = () => {
  console.log('发送消息给', userInfo.value.name)
}

const showMoreActions = () => {
  showActionsMenu.value = true
}

const blockUser = () => {
  console.log('屏蔽用户')
  showActionsMenu.value = false
}

const reportUser = () => {
  console.log('举报用户')
  showActionsMenu.value = false
}

const exportUserData = () => {
  console.log('导出用户数据')
  showActionsMenu.value = false
}

const suggestTag = () => {
  const newTag = prompt('请输入要添加的标签：')
  if (newTag && !userTags.value.includes(newTag)) {
    userTags.value.push(newTag)
  }
}

const likeActivity = (activity) => {
  activity.likes++
  console.log('点赞动态', activity.id)
}

const commentOnActivity = (activity) => {
  const comment = prompt('请输入评论：')
  if (comment) {
    activity.comments++
    console.log('评论动态', activity.id, ':', comment)
  }
}

const shareActivity = (activity) => {
  activity.shares++
  console.log('分享动态', activity.id)
}

const viewImage = (imgUrl) => {
  viewingImage.value = imgUrl
}

const closeImageView = () => {
  viewingImage.value = null
}

const toggleFollowFollower = (follower) => {
  follower.isFollowing = !follower.isFollowing
  console.log(follower.isFollowing ? '关注粉丝' : '取消关注粉丝', follower.id)
}

const toggleFollowFollowing = (following) => {
  following.isFollowing = !following.isFollowing
  console.log(following.isFollowing ? '关注用户' : '取消关注用户', following.id)
}

// 生命周期
onMounted(() => {
  console.log('用户查看页面加载完成')
})
</script>

<style scoped>
.user-view-page {
  font-family: Arial, sans-serif;
  color: #333;
  max-width: 1400px;
  margin: 0 auto;
  padding: 20px;
  background: #f5f7fa;
  min-height: 100vh;
}

/* 页面头部 */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
  padding-bottom: 15px;
  border-bottom: 2px solid #e0e6ed;
}

.page-title {
  font-size: 24px;
  font-weight: bold;
  color: #2c3e50;
  margin: 0;
}

.page-actions {
  display: flex;
  gap: 10px;
}

.action-btn {
  padding: 8px 16px;
  background: #fff;
  border: 1px solid #d1d9e6;
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  transition: all 0.2s;
}

.action-btn:hover {
  background: #f0f4f8;
  border-color: #b8c2cc;
}

/* 主要内容区域布局 */
.main-content {
  display: flex;
  gap: 30px;
}

.left-panel {
  width: 350px;
  flex-shrink: 0;
}

.right-panel {
  flex: 1;
  min-width: 0;
}

/* 用户基本信息卡片 */
.user-basic-card {
  background: #fff;
  border-radius: 12px;
  padding: 25px;
  margin-bottom: 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  border: 1px solid #e0e6ed;
}

.user-avatar-section {
  position: relative;
  width: 120px;
  height: 120px;
  margin: 0 auto 20px;
}

.user-avatar-large {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
  border: 4px solid #fff;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}

.avatar-status {
  position: absolute;
  bottom: 8px;
  right: 8px;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  border: 3px solid #fff;
}

.avatar-status.online {
  background: #4caf50;
}

.avatar-status.offline {
  background: #9e9e9e;
}

.avatar-status.busy {
  background: #f44336;
}

.user-details {
  text-align: center;
}

.user-name {
  font-size: 24px;
  font-weight: bold;
  color: #2c3e50;
  margin: 0 0 5px 0;
}

.user-username {
  color: #7f8c8d;
  margin: 0 0 15px 0;
  font-size: 16px;
}

.user-bio {
  color: #5a6c7d;
  line-height: 1.6;
  margin: 0 0 20px 0;
  padding: 0 10px;
}

.user-meta {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 20px;
  color: #6c757d;
  font-size: 14px;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 8px;
  justify-content: center;
}

.website-link {
  color: #3498db;
  text-decoration: none;
}

.website-link:hover {
  text-decoration: underline;
}

.user-stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 15px;
  margin-bottom: 25px;
  padding: 15px 0;
  border-top: 1px solid #e0e6ed;
  border-bottom: 1px solid #e0e6ed;
}

.stat-card {
  text-align: center;
  padding: 10px;
  background: #f8fafc;
  border-radius: 8px;
  border: 1px solid #e0e6ed;
}

.stat-value {
  font-size: 20px;
  font-weight: bold;
  color: #2c3e50;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 12px;
  color: #7f8c8d;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.user-actions {
  display: flex;
  gap: 10px;
  justify-content: center;
  margin-top: 20px;
}

.primary-btn, .secondary-btn {
  padding: 10px 20px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s;
  border: none;
  display: flex;
  align-items: center;
  gap: 6px;
}

.primary-btn {
  background: #3498db;
  color: white;
  flex: 1;
}

.primary-btn:hover {
  background: #2980b9;
}

.secondary-btn {
  background: #fff;
  color: #5a6c7d;
  border: 1px solid #d1d9e6;
}

.secondary-btn:hover {
  background: #f0f4f8;
  border-color: #b8c2cc;
}

/* 通用部分样式 */
.section-title {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 15px 0;
  padding-bottom: 8px;
  border-bottom: 2px solid #e0e6ed;
}

/* 用户标签区域 */
.user-tags-section,
.user-skills-section,
.user-verification-section,
.user-contact-section {
  background: #fff;
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  border: 1px solid #e0e6ed;
}

.tags-container {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.user-tag {
  background: #e8f4fd;
  color: #3498db;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 14px;
  border: 1px solid #d1e9ff;
}

.add-tag-btn {
  background: #fff;
  color: #7f8c8d;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 14px;
  border: 1px dashed #d1d9e6;
  cursor: pointer;
}

.add-tag-btn:hover {
  background: #f0f4f8;
  border-color: #b8c2cc;
}

/* 用户技能区域 */
.skills-list {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.skill-item {
  display: flex;
  align-items: center;
  gap: 15px;
}

.skill-name {
  width: 100px;
  font-weight: 500;
  color: #2c3e50;
}

.skill-level {
  flex: 1;
  height: 8px;
  background: #e0e6ed;
  border-radius: 4px;
  overflow: hidden;
}

.level-bar {
  height: 100%;
  background: #3498db;
  border-radius: 4px;
  transition: width 0.3s;
}

.skill-years {
  width: 80px;
  text-align: right;
  color: #7f8c8d;
  font-size: 14px;
}

/* 用户认证信息 */
.verification-badge {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px;
  background: #f0f9ff;
  border-radius: 8px;
  border: 1px solid #d1e9ff;
  color: #0369a1;
}

.verification-info {
  font-size: 14px;
  color: #64748b;
}

/* 联系方式 */
.contact-info {
  background: #f8fafc;
  padding: 15px;
  border-radius: 8px;
  border: 1px solid #e0e6ed;
  font-size: 14px;
  line-height: 1.6;
}

.contact-info p {
  margin: 8px 0;
}

/* 标签导航 */
.tabs-navigation {
  display: flex;
  background: #fff;
  border-radius: 12px 12px 0 0;
  overflow: hidden;
  border: 1px solid #e0e6ed;
  border-bottom: none;
  margin-bottom: 0;
}

.tab-btn {
  flex: 1;
  padding: 15px 20px;
  background: #fff;
  border: none;
  cursor: pointer;
  font-weight: 500;
  color: #7f8c8d;
  border-bottom: 3px solid transparent;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: all 0.2s;
}

.tab-btn:hover {
  background: #f0f4f8;
  color: #2c3e50;
}

.tab-btn.active {
  color: #3498db;
  border-bottom-color: #3498db;
  background: #f0f9ff;
}

.tab-badge {
  background: #e0e6ed;
  color: #5a6c7d;
  padding: 2px 8px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: normal;
}

.tab-btn.active .tab-badge {
  background: #3498db;
  color: white;
}

/* 内容区域 */
.content-area {
  background: #fff;
  border-radius: 0 0 12px 12px;
  padding: 25px;
  border: 1px solid #e0e6ed;
  min-height: 500px;
}

/* 动态列表 */
.activities-list {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.activity-item {
  padding: 20px;
  border: 1px solid #e0e6ed;
  border-radius: 8px;
  background: #f8fafc;
}

.activity-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.activity-type {
  background: #e8f4fd;
  color: #3498db;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.activity-time {
  color: #7f8c8d;
  font-size: 13px;
}

.activity-content {
  color: #2c3e50;
  line-height: 1.6;
  margin-bottom: 15px;
}

.activity-attachments {
  display: flex;
  gap: 10px;
  margin-bottom: 15px;
  flex-wrap: wrap;
}

.attachment-img {
  width: 120px;
  height: 80px;
  object-fit: cover;
  border-radius: 6px;
  cursor: pointer;
  border: 1px solid #e0e6ed;
  transition: transform 0.2s;
}

.attachment-img:hover {
  transform: scale(1.05);
}

.activity-stats {
  display: flex;
  gap: 15px;
  padding-top: 15px;
  border-top: 1px solid #e0e6ed;
}

.stat-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  background: #fff;
  border: 1px solid #d1d9e6;
  border-radius: 6px;
  padding: 6px 12px;
  cursor: pointer;
  color: #5a6c7d;
  transition: all 0.2s;
}

.stat-btn:hover {
  background: #f0f4f8;
  border-color: #b8c2cc;
}

/* 作品网格 */
.works-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.work-card {
  border: 1px solid #e0e6ed;
  border-radius: 8px;
  overflow: hidden;
  background: #fff;
  transition: transform 0.2s, box-shadow 0.2s;
}

.work-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 20px rgba(0,0,0,0.1);
}

.work-image {
  position: relative;
  height: 180px;
  overflow: hidden;
}

.work-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.work-category {
  position: absolute;
  top: 10px;
  right: 10px;
  background: rgba(52, 152, 219, 0.9);
  color: white;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
}

.work-info {
  padding: 15px;
}

.work-title {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 8px 0;
}

.work-desc {
  color: #5a6c7d;
  font-size: 14px;
  line-height: 1.5;
  margin: 0 0 12px 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.work-meta {
  display: flex;
  justify-content: space-between;
  color: #7f8c8d;
  font-size: 13px;
  padding-top: 12px;
  border-top: 1px solid #e0e6ed;
}

/* 粉丝和关注列表 */
.followers-list,
.following-list {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.follower-card,
.following-card {
  display: flex;
  align-items: center;
  gap: 15px;
  padding: 15px;
  border: 1px solid #e0e6ed;
  border-radius: 8px;
  background: #fff;
}

.follower-avatar,
.following-avatar {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  object-fit: cover;
  border: 2px solid #e0e6ed;
}

.follower-info,
.following-info {
  flex: 1;
}

.follower-name,
.following-name {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 5px 0;
}

.follower-bio,
.following-bio {
  color: #5a6c7d;
  font-size: 14px;
  margin: 0 0 8px 0;
  line-height: 1.4;
}

.follower-stats,
.following-stats {
  display: flex;
  gap: 15px;
  color: #7f8c8d;
  font-size: 13px;
}

.follow-btn {
  padding: 8px 20px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
  background: #3498db;
  color: white;
  border: none;
  transition: all 0.2s;
  white-space: nowrap;
}

.follow-btn.following {
  background: #fff;
  color: #7f8c8d;
  border: 1px solid #d1d9e6;
}

.follow-btn:hover {
  opacity: 0.9;
}

.follow-btn.following:hover {
  background: #fee;
  color: #e74c3c;
  border-color: #f5c6cb;
}

/* 收藏网格 */
.collections-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.collection-card {
  border: 1px solid #e0e6ed;
  border-radius: 8px;
  overflow: hidden;
  background: #fff;
  transition: transform 0.2s;
}

.collection-card:hover {
  transform: translateY(-4px);
}

.collection-cover {
  position: relative;
  height: 150px;
  overflow: hidden;
}

.collection-cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.collection-count {
  position: absolute;
  bottom: 10px;
  right: 10px;
  background: rgba(0,0,0,0.7);
  color: white;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
}

.collection-info {
  padding: 15px;
}

.collection-title {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 8px 0;
}

.collection-desc {
  color: #5a6c7d;
  font-size: 14px;
  line-height: 1.5;
  margin: 0 0 12px 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.collection-meta {
  display: flex;
  justify-content: space-between;
  color: #7f8c8d;
  font-size: 13px;
  padding-top: 12px;
  border-top: 1px solid #e0e6ed;
}

/* 成就与徽章 */
.achievements-section {
  display: flex;
  flex-direction: column;
  gap: 25px;
}

.achievements-stats {
  display: flex;
  justify-content: space-around;
  padding: 20px;
  background: #f8fafc;
  border-radius: 8px;
  border: 1px solid #e0e6ed;
}

.achievement-stat {
  text-align: center;
}

.achievement-stat .stat-number {
  font-size: 28px;
  font-weight: bold;
  color: #2c3e50;
  margin-bottom: 5px;
}

.achievement-stat .stat-label {
  font-size: 14px;
  color: #7f8c8d;
}

.badges-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 20px;
}

.badge-card {
  display: flex;
  align-items: center;
  gap: 15px;
  padding: 15px;
  border: 1px solid #e0e6ed;
  border-radius: 8px;
  background: #fff;
  transition: transform 0.2s;
}

.badge-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 6px 15px rgba(0,0,0,0.1);
}

.badge-icon {
  font-size: 32px;
  width: 60px;
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f0f9ff;
  border-radius: 50%;
  border: 2px solid #d1e9ff;
}

.badge-info {
  flex: 1;
}

.badge-title {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 5px 0;
}

.badge-desc {
  color: #5a6c7d;
  font-size: 14px;
  line-height: 1.4;
  margin: 0 0 8px 0;
}

.badge-date {
  color: #7f8c8d;
  font-size: 12px;
}

/* 数据分析 */
.analytics-section {
  display: flex;
  flex-direction: column;
  gap: 25px;
}

.analytics-card {
  padding: 20px;
  border: 1px solid #e0e6ed;
  border-radius: 8px;
  background: #fff;
}

.analytics-title {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 20px 0;
  padding-bottom: 10px;
  border-bottom: 2px solid #e0e6ed;
}

.analytics-chart {
  height: 200px;
  display: flex;
  align-items: flex-end;
  padding: 20px 0;
}

.chart-bars {
  display: flex;
  justify-content: space-around;
  align-items: flex-end;
  width: 100%;
  height: 100%;
}

.chart-bar-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  height: 100%;
  width: 30px;
}

.chart-bar {
  width: 20px;
  background: #3498db;
  border-radius: 4px 4px 0 0;
  min-height: 5px;
  transition: height 0.3s;
}

.chart-label {
  margin-top: 8px;
  font-size: 12px;
  color: #7f8c8d;
}

.analytics-stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
}

.analytics-stat {
  padding: 20px;
  border: 1px solid #e0e6ed;
  border-radius: 8px;
  background: #fff;
  text-align: center;
}

.analytics-stat .stat-label {
  font-size: 14px;
  color: #7f8c8d;
  margin-bottom: 8px;
}

.analytics-stat .stat-value {
  font-size: 24px;
  font-weight: bold;
  color: #2c3e50;
}

/* 模态框 */
.image-view-modal {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0,0,0,0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  position: relative;
  max-width: 90%;
  max-height: 90%;
  background: white;
  border-radius: 8px;
  overflow: hidden;
}

.modal-close {
  position: absolute;
  top: 10px;
  right: 10px;
  width: 40px;
  height: 40px;
  background: rgba(0,0,0,0.5);
  color: white;
  border: none;
  border-radius: 50%;
  font-size: 24px;
  cursor: pointer;
  z-index: 1001;
}

.modal-content img {
  max-width: 100%;
  max-height: 80vh;
  display: block;
}

/* 操作菜单 */
.actions-menu {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.menu-content {
  background: white;
  border-radius: 8px;
  padding: 20px;
  min-width: 300px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.2);
}

.menu-content h4 {
  margin: 0 0 20px 0;
  color: #2c3e50;
  text-align: center;
}

.menu-item {
  display: block;
  width: 100%;
  padding: 12px 15px;
  margin-bottom: 10px;
  text-align: left;
  background: #fff;
  border: 1px solid #e0e6ed;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 16px;
}

.menu-item:hover {
  background: #f0f4f8;
  border-color: #b8c2cc;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .main-content {
    flex-direction: column;
  }
  
  .left-panel {
    width: 100%;
  }
  
  .works-grid,
  .collections-grid,
  .badges-grid {
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  }
}

@media (max-width: 768px) {
  .user-view-page {
    padding: 10px;
  }
  
  .page-header {
    flex-direction: column;
    gap: 15px;
    align-items: flex-start;
  }
  
  .page-actions {
    width: 100%;
    justify-content: flex-start;
  }
  
  .tabs-navigation {
    flex-wrap: wrap;
  }
  
  .tab-btn {
    flex: 1 0 calc(50% - 5px);
  }
  
  .user-stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .user-actions {
    flex-direction: column;
  }
  
  .works-grid,
  .collections-grid,
  .badges-grid {
    grid-template-columns: 1fr;
  }
  
  .analytics-stats-grid {
    grid-template-columns: 1fr;
  }
  
  .follower-card,
  .following-card {
    flex-direction: column;
    text-align: center;
  }
  
  .follower-info,
  .following-info {
    text-align: center;
  }
}
</style>
