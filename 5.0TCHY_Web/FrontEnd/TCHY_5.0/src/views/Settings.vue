<template>
  <div class="cyber-profile">
    <div class="grid-bg moving-grid"></div>

    <!-- 悬浮语言切换按钮 -->
    <button
      class="floating-lang-toggle"
      @click="langPriority = langPriority === 'zh' ? 'en' : 'zh'"
      :title="langPriority === 'zh' ? '切换到英文优先' : 'Switch to Chinese Priority'"
    >
      <div class="toggle-icon">
        <span class="lang-primary">{{ langPriority === 'zh' ? '中' : 'EN' }}</span>
        <span class="lang-secondary">{{ langPriority === 'zh' ? 'EN' : '中' }}</span>
      </div>
      <div class="toggle-label">{{ langPriority === 'zh' ? '语言' : 'LANG' }}</div>
    </button>

    <div class="profile-scroll-container custom-scroll">

      <!-- 顶部导航栏 -->
      <header class="profile-header">
        <div class="header-left">
          <button class="back-btn" @click="router.push('/comCenter')">
            <i class="fas fa-arrow-left"></i>
            <span class="btn-text-main">{{ t.back }}</span>
            <span class="btn-text-sub" v-if="langPriority === 'zh'">BACK</span>
          </button>
        </div>
        <div class="header-center">
          <h1 class="page-title">
            <span class="title-deco"></span>
            <span class="title-main">{{ isEditMode ? t.profileEditor : t.userProfile }}</span>
            <span class="title-sub">{{ isEditMode ? (langPriority === 'zh' ? 'EDITOR' : '编辑器') : (langPriority === 'zh' ? 'PROFILE' : '主页') }}</span>
          </h1>
        </div>
        <div class="header-right">
          <div class="mode-toggle">
            <button
              class="toggle-btn"
              :class="{ active: !isEditMode }"
              @click="isEditMode = false"
            >
              <span class="btn-main">{{ t.view }}</span>
              <span class="btn-sub">{{ langPriority === 'zh' ? 'VIEW' : '查看' }}</span>
            </button>
            <button
              class="toggle-btn"
              :class="{ active: isEditMode }"
              @click="isEditMode = true"
            >
              <span class="btn-main">{{ t.edit }}</span>
              <span class="btn-sub">{{ langPriority === 'zh' ? 'EDIT' : '编辑' }}</span>
            </button>
          </div>
        </div>
      </header>

      <div class="tech-strip">
        <div class="strip-content">
          // TAICHU_USER_SYSTEM_v5.0 // 太初用户系统 // DATA_SYNCED // 数据已同步 // PERSONALIZATION_ACTIVE // 个性化已启用 //
          // TAICHU_USER_SYSTEM_v5.0 // 太初用户系统 // DATA_SYNCED // 数据已同步 // PERSONALIZATION_ACTIVE // 个性化已启用 //
        </div>
      </div>

      <!-- 主要内容区域 -->
      <div class="profile-main-grid">

        <!-- 左侧栏：名片 + 统计 + 快捷入口 -->
        <aside class="profile-left-column">

          <!-- 用户名片 -->
          <section class="cyber-card user-card">
            <div class="card-deco-corner"></div>
            <div class="taichu-logo-badge">
              <span class="badge-main">太初寰宇</span>
              <span class="badge-sub">TAICHU</span>
            </div>

            <div class="avatar-section">
              <div class="avatar-wrapper" :class="{ 'edit-hover': isEditMode }">
                <img :src="fixAvatarUrl(userProfile.avatar)" @error="handleImgError" />
                <div v-if="isEditMode" class="avatar-overlay" @click="triggerAvatarUpload">
                  <i class="fas fa-camera"></i>
                  <span class="overlay-main">{{ t.changeAvatar }}</span>
                  <span class="overlay-sub">{{ langPriority === 'zh' ? 'CHANGE' : '更换' }}</span>
                </div>
                <input
                  ref="avatarInput"
                  type="file"
                  accept="image/*"
                  style="display: none;"
                  @change="handleAvatarChange"
                />
              </div>
              <div class="level-badge">
                <span class="badge-level">LV.{{ userProfile.level || 1 }}</span>
                <span class="badge-level-sub">{{ langPriority === 'zh' ? 'LEVEL' : '等级' }}</span>
              </div>
            </div>

            <div class="user-info-block">
              <h2 class="username" v-if="!isEditMode">{{ userProfile.username }}</h2>
              <input
                v-else
                type="text"
                v-model="userProfile.username"
                class="edit-input username-input"
                :placeholder="t.usernamePlaceholder"
              />

              <div class="user-id">ID: {{ userProfile.userId || '000000' }}</div>

              <div class="bio-section" v-if="!isEditMode">
                <p class="bio-text">{{ userProfile.bio || t.noBioData }}</p>
              </div>
              <textarea
                v-else
                v-model="userProfile.bio"
                class="edit-textarea"
                :placeholder="t.bioPlaceholder"
                rows="3"
              ></textarea>

              <div class="stats-mini-grid">
                <div class="stat-mini">
                  <span class="num">{{ userProfile.followingCount || 0 }}</span>
                  <span class="label">
                    <span class="label-main">{{ t.following }}</span>
                    <span class="label-sub">{{ langPriority === 'zh' ? 'FOLLOWING' : '关注' }}</span>
                  </span>
                </div>
                <div class="stat-mini">
                  <span class="num">{{ userProfile.followersCount || 0 }}</span>
                  <span class="label">
                    <span class="label-main">{{ t.followers }}</span>
                    <span class="label-sub">{{ langPriority === 'zh' ? 'FOLLOWERS' : '粉丝' }}</span>
                  </span>
                </div>
                <div class="stat-mini">
                  <span class="num">{{ userProfile.postsCount || 0 }}</span>
                  <span class="label">
                    <span class="label-main">{{ t.posts }}</span>
                    <span class="label-sub">{{ langPriority === 'zh' ? 'POSTS' : '帖子' }}</span>
                  </span>
                </div>
              </div>
            </div>

            <button v-if="isEditMode" class="save-profile-btn" @click="saveProfile">
              <i class="fas fa-save"></i>
              <span class="btn-text-main">{{ t.saveProfile }}</span>
              <span class="btn-text-sub">{{ langPriority === 'zh' ? 'SAVE' : '保存' }}</span>
            </button>
          </section>

          <!-- 成就栏 -->
          <section class="cyber-card achievements-card">
            <div class="card-label-strip">
              <span class="strip-main">{{ t.achievements }}</span>
              <span class="strip-sub">{{ langPriority === 'zh' ? 'ACHIEVEMENTS' : '成��' }}</span>
            </div>
            <div class="achievements-grid">
              <div
                v-for="ach in achievements"
                :key="ach.id"
                class="achievement-item"
                :class="{ unlocked: ach.unlocked }"
                :title="ach.description"
              >
                <i :class="ach.icon"></i>
                <span class="ach-name">{{ ach.name }}</span>
              </div>
            </div>
          </section>

          <!-- 仓库入口 -->
          <section class="cyber-card repo-card">
            <div class="card-label-strip">
              <span class="strip-main">{{ t.repositories }}</span>
              <span class="strip-sub">{{ langPriority === 'zh' ? 'REPOSITORIES' : '仓库' }}</span>
            </div>
            <div class="repo-list">
              <div
                v-for="repo in repositories"
                :key="repo.id"
                class="repo-item"
                @click="openRepo(repo)"
              >
                <i class="fas fa-folder"></i>
                <span class="repo-name">{{ repo.name }}</span>
                <span class="repo-count">{{ repo.count }}</span>
              </div>
            </div>
          </section>

        </aside>

        <!-- 中间栏：作品瀑布流 + 博客列表 -->
        <main class="profile-mid-column">

          <!-- Tab切换 -->
          <div class="content-tabs">
            <button
              class="tab-btn"
              :class="{ active: activeTab === 'works' }"
              @click="activeTab = 'works'"
            >
              <i class="fas fa-th"></i>
              <span class="tab-main">{{ t.artworks }}</span>
              <span class="tab-count">({{ artworks.length }})</span>
              <span class="tab-sub">{{ langPriority === 'zh' ? 'WORKS' : '作品' }}</span>
            </button>
            <button
              class="tab-btn"
              :class="{ active: activeTab === 'blogs' }"
              @click="activeTab = 'blogs'"
            >
              <i class="fas fa-book"></i>
              <span class="tab-main">{{ t.blogs }}</span>
              <span class="tab-count">({{ blogs.length }})</span>
              <span class="tab-sub">{{ langPriority === 'zh' ? 'BLOGS' : '博客' }}</span>
            </button>
            <button
              class="tab-btn"
              :class="{ active: activeTab === 'activity' }"
              @click="activeTab = 'activity'"
            >
              <i class="fas fa-history"></i>
              <span class="tab-main">{{ t.activity }}</span>
              <span class="tab-sub">{{ langPriority === 'zh' ? 'ACTIVITY' : '活动' }}</span>
            </button>
          </div>

          <!-- 作品瀑布流 -->
          <Transition name="fade" mode="out-in">
            <div v-if="activeTab === 'works'" class="works-waterfall" key="works">
              <div v-if="loadingWorks" class="loading-state">
                <i class="fas fa-spinner fa-spin"></i> {{ t.loadingArtworks }}
              </div>
              <div v-else-if="artworks.length === 0" class="empty-state">
                <p>{{ t.noArtworksYet }}</p>
                <button class="upload-btn" @click="uploadArtwork">
                  <i class="fas fa-upload"></i>
                  <span class="btn-text-main">{{ t.uploadFirstArtwork }}</span>
                  <span class="btn-text-sub">{{ langPriority === 'zh' ? 'UPLOAD' : '上传' }}</span>
                </button>
              </div>
              <div v-else class="waterfall-container">
                <div
                  v-for="(art, index) in artworks"
                  :key="art.id"
                  class="artwork-card"
                  :style="{ animationDelay: (index * 0.05) + 's' }"
                  @click="openArtworkDetail(art)"
                >
                  <div class="artwork-img-wrapper">
                    <img :src="art.imageUrl" @error="handleImgError" loading="lazy" />
                    <div class="artwork-overlay">
                      <div class="overlay-stats">
                        <span><i class="far fa-heart"></i> {{ art.likes }}</span>
                        <span><i class="far fa-eye"></i> {{ art.views }}</span>
                      </div>
                    </div>
                  </div>
                  <div class="artwork-info">
                    <h4 class="artwork-title">{{ art.title }}</h4>
                    <span class="artwork-date">{{ formatTime(art.createTime) }}</span>
                  </div>
                </div>
              </div>
            </div>

            <!-- 博客列表 -->
            <div v-else-if="activeTab === 'blogs'" class="blogs-list" key="blogs">
              <div v-if="loadingBlogs" class="loading-state">
                <i class="fas fa-spinner fa-spin"></i> {{ t.loadingBlogs }}
              </div>
              <div v-else-if="blogs.length === 0" class="empty-state">
                <p>{{ t.noBlogsYet }}</p>
                <button class="upload-btn" @click="router.push('/blogCreater')">
                  <i class="fas fa-pen"></i>
                  <span class="btn-text-main">{{ t.writeFirstBlog }}</span>
                  <span class="btn-text-sub">{{ langPriority === 'zh' ? 'WRITE' : '写作' }}</span>
                </button>
              </div>
              <div v-else class="blog-list-container">
                <div
                  v-for="blog in blogs"
                  :key="blog.id"
                  class="blog-item-card"
                  @click="openBlogDetail(blog.id)"
                >
                  <div v-if="blog.coverImage" class="blog-cover">
                    <img :src="fixAvatarUrl(blog.coverImage)" @error="handleImgError" />
                  </div>
                  <div class="blog-content-section">
                    <div class="blog-meta">
                      <span class="blog-tag">#{{ blog.tags?.[0] || 'TECH' }}</span>
                      <span class="blog-date">{{ formatTime(blog.createTime) }}</span>
                    </div>
                    <h3 class="blog-title">{{ blog.title }}</h3>
                    <p class="blog-excerpt">{{ blog.excerpt || blog.content?.substring(0, 100) + '...' }}</p>
                    <div class="blog-stats">
                      <span><i class="far fa-eye"></i> {{ blog.views }}</span>
                      <span><i class="far fa-comment"></i> {{ blog.comments }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- 活动记录 -->
            <div v-else-if="activeTab === 'activity'" class="activity-timeline" key="activity">
              <div v-if="loadingActivity" class="loading-state">
                <i class="fas fa-spinner fa-spin"></i> LOADING_ACTIVITY...
              </div>
              <div v-else-if="activities.length === 0" class="empty-state">
                <p>// NO_ACTIVITY_RECORDED //</p>
              </div>
              <div v-else class="timeline-container">
                <div
                  v-for="activity in activities"
                  :key="activity.id"
                  class="activity-item"
                >
                  <div class="activity-dot"></div>
                  <div class="activity-content">
                    <div class="activity-header">
                      <span class="activity-type" :class="activity.type">
                        <i :class="getActivityIcon(activity.type)"></i>
                        {{ activity.type.toUpperCase() }}
                      </span>
                      <span class="activity-time">{{ formatTime(activity.time) }}</span>
                    </div>
                    <p class="activity-desc">{{ activity.description }}</p>
                  </div>
                </div>
              </div>
            </div>
          </Transition>

        </main>

        <!-- 右侧栏：留言板 + 设置 -->
        <aside class="profile-right-column">

          <!-- 留言板 -->
          <section class="cyber-card guestbook-card">
            <div class="card-label-strip">
              <span class="strip-main">{{ langPriority === 'zh' ? '留言板' : 'GUESTBOOK' }}</span>
              <span class="strip-count">({{ guestbookMessages.length }})</span>
              <span class="strip-sub">{{ langPriority === 'zh' ? 'GUESTBOOK' : '留言板' }}</span>
            </div>

            <div class="guestbook-messages custom-scroll">
              <div v-if="guestbookMessages.length === 0" class="empty-guestbook">
                <p>// NO_MESSAGES_YET //</p>
              </div>
              <div
                v-for="msg in guestbookMessages"
                :key="msg.id"
                class="guestbook-msg"
              >
                <div class="msg-header">
                  <div class="msg-avatar">
                    <img :src="fixAvatarUrl(msg.author.avatar)" @error="handleImgError" />
                  </div>
                  <div class="msg-info">
                    <span class="msg-author">{{ msg.author.username }}</span>
                    <span class="msg-time">{{ formatTime(msg.createTime) }}</span>
                  </div>
                </div>
                <p class="msg-content">{{ msg.content }}</p>
              </div>
            </div>

            <div class="guestbook-input-area">
              <textarea
                v-model="newMessage"
                :placeholder="langPriority === 'zh' ? '留下你的足迹...' : 'LEAVE_MESSAGE...'"
                class="msg-input"
                rows="2"
                @keyup.ctrl.enter="submitMessage"
              ></textarea>
              <button
                class="msg-send-btn"
                @click="submitMessage"
                :disabled="!newMessage.trim()"
              >
                <span class="btn-send-main">{{ langPriority === 'zh' ? '发送' : 'SEND' }}</span>
                <span class="btn-send-sub">{{ langPriority === 'zh' ? 'SEND' : '发送' }}</span>
              </button>
            </div>
          </section>

          <!-- 个人设置 -->
          <section v-if="isEditMode" class="cyber-card settings-card">
            <div class="card-label-strip">
              <span class="strip-main">{{ langPriority === 'zh' ? '设置' : 'SETTINGS' }}</span>
              <span class="strip-sub">{{ langPriority === 'zh' ? 'SETTINGS' : '设置' }}</span>
            </div>

            <div class="settings-list">
              <div class="setting-item">
                <label class="setting-label">
                  <i class="fas fa-envelope"></i> Email
                </label>
                <input
                  type="email"
                  v-model="userProfile.email"
                  class="setting-input"
                  placeholder="your@email.com"
                />
              </div>

              <div class="setting-item">
                <label class="setting-label">
                  <i class="fas fa-link"></i> Website
                </label>
                <input
                  type="url"
                  v-model="userProfile.website"
                  class="setting-input"
                  placeholder="https://..."
                />
              </div>

              <div class="setting-item">
                <label class="setting-label">
                  <i class="fas fa-map-marker-alt"></i> Location
                </label>
                <input
                  type="text"
                  v-model="userProfile.location"
                  class="setting-input"
                  placeholder="City, Country"
                />
              </div>

              <div class="setting-item">
                <label class="setting-label">
                  <i class="fas fa-palette"></i>
                  <span class="label-main">{{ langPriority === 'zh' ? '主题颜色' : 'Theme Color' }}</span>
                  <span class="label-sub">{{ langPriority === 'zh' ? 'THEME' : '主题' }}</span>
                </label>
                <div class="color-picker-group">
                  <div
                    v-for="color in themeColors"
                    :key="color"
                    class="color-option"
                    :style="{ background: color }"
                    :class="{ selected: userProfile.themeColor === color }"
                    @click="userProfile.themeColor = color"
                  ></div>
                </div>
              </div>

              <div class="setting-item toggle-row">
                <label class="setting-label">
                  <i class="fas fa-eye"></i> Profile Public
                </label>
                <label class="cyber-switch">
                  <input type="checkbox" v-model="userProfile.isPublic" />
                  <span class="switch-slider"></span>
                </label>
              </div>

              <div class="setting-item toggle-row">
                <label class="setting-label">
                  <i class="fas fa-bell"></i> Notifications
                </label>
                <label class="cyber-switch">
                  <input type="checkbox" v-model="userProfile.notifications" />
                  <span class="switch-slider"></span>
                </label>
              </div>

              <button class="danger-btn" @click="showDeleteConfirm = true">
                <i class="fas fa-exclamation-triangle"></i>
                <span class="btn-text-main">{{ langPriority === 'zh' ? '删除账户' : 'DELETE ACCOUNT' }}</span>
                <span class="btn-text-sub">{{ langPriority === 'zh' ? 'DELETE' : '删除' }}</span>
              </button>
            </div>
          </section>

          <!-- 社交链接 -->
          <section class="cyber-card social-card">
            <div class="card-label-strip">
              <span class="strip-main">{{ langPriority === 'zh' ? '社交链接' : 'SOCIAL LINKS' }}</span>
              <span class="strip-sub">{{ langPriority === 'zh' ? 'SOCIAL' : '社交' }}</span>
            </div>
            <div class="social-links">
              <a v-for="link in socialLinks" :key="link.platform" :href="link.url" target="_blank" class="social-link">
                <i :class="link.icon"></i>
                <span>{{ link.platform }}</span>
              </a>
            </div>
          </section>

        </aside>

      </div>

    </div>

    <!-- 删除确认弹窗 -->
    <Teleport to="body">
      <Transition name="modal-scale">
        <div v-if="showDeleteConfirm" class="cyber-modal-overlay" @click.self="showDeleteConfirm = false">
          <div class="confirm-modal">
            <div class="modal-header">
              <span class="warning-icon"><i class="fas fa-exclamation-triangle"></i></span>
              <h3>CONFIRM DELETION</h3>
            </div>
            <p class="modal-text">
              This action cannot be undone. All your data will be permanently deleted.
            </p>
            <div class="modal-actions">
              <button class="cancel-btn" @click="showDeleteConfirm = false">CANCEL</button>
              <button class="confirm-btn" @click="deleteAccount">DELETE</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import apiClient from '@/utils/api';

const router = useRouter();

const isDev = window.location.hostname === 'localhost';
const BASE_URL = isDev ? 'https://localhost:44359' : 'https://bianyuzhou.com';

const isEditMode = ref(false);
const activeTab = ref('works');
const langPriority = ref('zh'); // 'zh' 中文优先, 'en' 英文优先

// 多语言文本
const t = computed(() => {
  const lang = langPriority.value;
  return {
    back: lang === 'zh' ? '返回' : 'BACK',
    profileEditor: lang === 'zh' ? '资料编辑' : 'PROFILE_EDITOR',
    userProfile: lang === 'zh' ? '用户主页' : 'USER_PROFILE',
    view: lang === 'zh' ? '查看' : 'VIEW',
    edit: lang === 'zh' ? '编辑' : 'EDIT',
    changeAvatar: lang === 'zh' ? '更换头像' : 'CHANGE',
    usernamePlaceholder: lang === 'zh' ? '用户名' : 'Username',
    bioPlaceholder: lang === 'zh' ? '写点什么介绍自己吧...' : 'Write your bio...',
    noBioData: lang === 'zh' ? '// 暂无个人简介' : '// NO_BIO_DATA',
    following: lang === 'zh' ? '关注' : 'Following',
    followers: lang === 'zh' ? '粉丝' : 'Followers',
    posts: lang === 'zh' ? '帖子' : 'Posts',
    saveProfile: lang === 'zh' ? '保存资料' : 'SAVE PROFILE',
    achievements: lang === 'zh' ? '成就' : 'ACHIEVEMENTS',
    repositories: lang === 'zh' ? '仓库' : 'REPOSITORIES',
    artworks: lang === 'zh' ? '作品集' : 'ARTWORKS',
    blogs: lang === 'zh' ? '博客' : 'BLOGS',
    activity: lang === 'zh' ? '活动记录' : 'ACTIVITY',
    loadingArtworks: lang === 'zh' ? '加载作品中...' : 'LOADING_ARTWORKS...',
    noArtworksYet: lang === 'zh' ? '// 还没有作品 //' : '// NO_ARTWORKS_YET //',
    uploadFirstArtwork: lang === 'zh' ? '上传第一件作品' : 'UPLOAD FIRST ARTWORK',
    loadingBlogs: lang === 'zh' ? '加载博客中...' : 'LOADING_BLOGS...',
    noBlogsYet: lang === 'zh' ? '// 还没有博客 //' : '// NO_BLOGS_YET //',
    writeFirstBlog: lang === 'zh' ? '写第一篇博客' : 'WRITE FIRST BLOG',
    loadingActivity: lang === 'zh' ? '加载活动记录中...' : 'LOADING_ACTIVITY...',
    noActivityRecorded: lang === 'zh' ? '// 暂无活动记录 //' : '// NO_ACTIVITY_RECORDED //',
    guestbook: lang === 'zh' ? '留言板' : 'GUESTBOOK',
    noMessagesYet: lang === 'zh' ? '// 还没有留言 //' : '// NO_MESSAGES_YET //',
    leaveMessagePlaceholder: lang === 'zh' ? '留下你的足迹...' : 'LEAVE_MESSAGE...',
    send: lang === 'zh' ? '发送' : 'SEND',
    settings: lang === 'zh' ? '设置' : 'SETTINGS',
    email: lang === 'zh' ? '邮箱' : 'Email',
    website: lang === 'zh' ? '个人网站' : 'Website',
    location: lang === 'zh' ? '所在地' : 'Location',
    themeColor: lang === 'zh' ? '主题颜色' : 'Theme Color',
    profilePublic: lang === 'zh' ? '公开主页' : 'Profile Public',
    notifications: lang === 'zh' ? '通知' : 'Notifications',
    deleteAccount: lang === 'zh' ? '删除账户' : 'DELETE ACCOUNT',
    socialLinks: lang === 'zh' ? '社交链接' : 'SOCIAL_LINKS',
    confirmDeletion: lang === 'zh' ? '确认删除' : 'CONFIRM DELETION',
    deleteWarning: lang === 'zh' ? '此操作无法撤销，您的所有数据将被永久删除。' : 'This action cannot be undone. All your data will be permanently deleted.',
    cancel: lang === 'zh' ? '取消' : 'CANCEL',
    delete: lang === 'zh' ? '删除' : 'DELETE',
    locationPlaceholder: lang === 'zh' ? '城市，国家' : 'City, Country'
  };
});

const userProfile = reactive({
  userId: '000001',
  username: 'TaichuUser',
  avatar: '',
  bio: '探索太初宇宙的旅行者 // Explorer of TAICHU Universe',
  level: 5,
  followingCount: 42,
  followersCount: 128,
  postsCount: 36,
  email: '',
  website: '',
  location: '',
  themeColor: '#D92323',
  isPublic: true,
  notifications: true
});

const themeColors = ['#D92323', '#2196F3', '#4CAF50', '#FF9800', '#9C27B0', '#111111'];

const achievements = ref([
  { id: 1, name: 'First Post', icon: 'fas fa-star', unlocked: true, description: 'Posted your first content' },
  { id: 2, name: 'Socializer', icon: 'fas fa-users', unlocked: true, description: 'Got 100 followers' },
  { id: 3, name: 'Creator', icon: 'fas fa-palette', unlocked: true, description: 'Uploaded 10 artworks' },
  { id: 4, name: 'Writer', icon: 'fas fa-pen', unlocked: false, description: 'Published 20 blogs' },
  { id: 5, name: 'Popular', icon: 'fas fa-fire', unlocked: false, description: 'Got 1000 likes' },
  { id: 6, name: 'Legend', icon: 'fas fa-crown', unlocked: false, description: 'Reach level 10' }
]);

const repositories = ref([
  { id: 1, name: 'Artworks', count: 24, type: 'artwork' },
  { id: 2, name: 'Blogs', count: 12, type: 'blog' },
  { id: 3, name: 'Favorites', count: 56, type: 'favorite' },
  { id: 4, name: 'Collections', count: 8, type: 'collection' }
]);

const artworks = ref([]);
const blogs = ref([]);
const activities = ref([]);
const guestbookMessages = ref([]);

const loadingWorks = ref(false);
const loadingBlogs = ref(false);
const loadingActivity = ref(false);

const newMessage = ref('');
const showDeleteConfirm = ref(false);
const avatarInput = ref(null);

const socialLinks = ref([
  { platform: 'GitHub', url: 'https://github.com', icon: 'fab fa-github' },
  { platform: 'Twitter', url: 'https://twitter.com', icon: 'fab fa-twitter' },
  { platform: 'Bilibili', url: 'https://bilibili.com', icon: 'fas fa-tv' }
]);

// Utility functions
const handleImgError = (e) => {
  if (e.target.src.includes('土豆.jpg')) return;
  e.target.src = '/土豆.jpg';
};

const fixAvatarUrl = (url) => {
  if (!url || typeof url !== 'string') return '/土豆.jpg';
  if (url.startsWith('http') || url.startsWith('data:image')) return url;
  let path = url.replace(/\\/g, '/');
  if (path.startsWith('/')) path = path.substring(1);
  if (!path.startsWith('uploads/')) path = `uploads/${path}`;
  return `${BASE_URL}/${path}`;
};

const formatTime = (t) => {
  if (!t) return 'N/A';
  const date = new Date(t);
  const now = new Date();
  const diff = now - date;
  const days = Math.floor(diff / (1000 * 60 * 60 * 24));

  if (days === 0) return 'Today';
  if (days === 1) return 'Yesterday';
  if (days < 7) return `${days} days ago`;
  return date.toLocaleDateString();
};

const getActivityIcon = (type) => {
  const icons = {
    post: 'fas fa-paper-plane',
    blog: 'fas fa-pen',
    artwork: 'fas fa-image',
    comment: 'fas fa-comment',
    like: 'fas fa-heart',
    follow: 'fas fa-user-plus'
  };
  return icons[type] || 'fas fa-circle';
};

// API functions
const fetchUserProfile = async () => {
  try {
    const res = await apiClient.get('/User/profile');
    if (res.data.success) {
      Object.assign(userProfile, res.data.data);
    }
  } catch (e) {
    console.error('Failed to fetch profile:', e);
  }
};

const fetchArtworks = async () => {
  loadingWorks.value = true;
  try {
    const res = await apiClient.get('/User/artworks', { params: { pageSize: 50 } });
    artworks.value = res.data.list || [];
  } catch (e) {
    console.error('Failed to fetch artworks:', e);
    // Mock data for demo
    artworks.value = Array.from({ length: 12 }, (_, i) => ({
      id: i + 1,
      title: `Artwork ${i + 1}`,
      imageUrl: `https://picsum.photos/300/400?random=${i}`,
      likes: Math.floor(Math.random() * 100),
      views: Math.floor(Math.random() * 500),
      createTime: new Date(Date.now() - Math.random() * 10000000000)
    }));
  } finally {
    loadingWorks.value = false;
  }
};

const fetchBlogs = async () => {
  loadingBlogs.value = true;
  try {
    const res = await apiClient.get('/Blog/user-articles');
    blogs.value = res.data.list || [];
  } catch (e) {
    console.error('Failed to fetch blogs:', e);
    // Mock data
    blogs.value = Array.from({ length: 5 }, (_, i) => ({
      id: i + 1,
      title: `Blog Post ${i + 1}: 探索太初宇宙的奥秘`,
      coverImage: '',
      tags: ['Tech', 'Art', 'Life'][Math.floor(Math.random() * 3)],
      content: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit...',
      views: Math.floor(Math.random() * 200),
      comments: Math.floor(Math.random() * 20),
      createTime: new Date(Date.now() - Math.random() * 10000000000)
    }));
  } finally {
    loadingBlogs.value = false;
  }
};

const fetchActivities = async () => {
  loadingActivity.value = true;
  try {
    const res = await apiClient.get('/User/activities');
    activities.value = res.data.list || [];
  } catch (e) {
    console.error('Failed to fetch activities:', e);
    // Mock data
    activities.value = Array.from({ length: 10 }, (_, i) => ({
      id: i + 1,
      type: ['post', 'blog', 'artwork', 'comment', 'like', 'follow'][Math.floor(Math.random() * 6)],
      description: 'Performed an action in the system',
      time: new Date(Date.now() - Math.random() * 10000000000)
    }));
  } finally {
    loadingActivity.value = false;
  }
};

const fetchGuestbook = async () => {
  try {
    const res = await apiClient.get('/User/guestbook');
    guestbookMessages.value = res.data.list || [];
  } catch (e) {
    console.error('Failed to fetch guestbook:', e);
  }
};

const saveProfile = async () => {
  try {
    const res = await apiClient.put('/User/profile', userProfile);
    if (res.data.success) {
      alert('Profile saved successfully!');
      isEditMode.value = false;
    }
  } catch (e) {
    console.error('Failed to save profile:', e);
    alert('Failed to save profile');
  }
};

const submitMessage = async () => {
  if (!newMessage.value.trim()) return;
  try {
    const res = await apiClient.post('/User/guestbook', { content: newMessage.value });
    if (res.data.success) {
      guestbookMessages.value.unshift(res.data.data);
      newMessage.value = '';
    }
  } catch (e) {
    console.error('Failed to submit message:', e);
    alert('Failed to send message');
  }
};

const triggerAvatarUpload = () => {
  avatarInput.value?.click();
};

const handleAvatarChange = async (e) => {
  const file = e.target.files[0];
  if (!file) return;

  const formData = new FormData();
  formData.append('avatar', file);

  try {
    const res = await apiClient.post('/User/avatar', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    if (res.data.success) {
      userProfile.avatar = res.data.url;
    }
  } catch (e) {
    console.error('Failed to upload avatar:', e);
    alert('Failed to upload avatar');
  }
};

const openArtworkDetail = (art) => {
  router.push(`/artwork/${art.id}`);
};

const openBlogDetail = (id) => {
  router.push(`/blog/${id}`);
};

const openRepo = (repo) => {
  console.log('Opening repo:', repo);
};

const uploadArtwork = () => {
  router.push('/artwork/upload');
};

const deleteAccount = async () => {
  try {
    const res = await apiClient.delete('/User/account');
    if (res.data.success) {
      alert('Account deleted');
      router.push('/');
    }
  } catch (e) {
    console.error('Failed to delete account:', e);
    alert('Failed to delete account');
  }
};

onMounted(() => {
  fetchUserProfile();
  fetchArtworks();
  fetchBlogs();
  fetchActivities();
  fetchGuestbook();
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Anton&family=Inter:wght@400;600;800&family=JetBrains+Mono:wght@400;700&display=swap');

/* 核心变量 */
.cyber-profile {
  --red: #D92323;
  --red-dark: #a01818;
  --black: #111111;
  --black-light: #1a1a1a;
  --off-white: #F4F1EA;
  --gray: #E0DDD5;
  --gray-light: #f9f8f5;
  --gray-dark: #333333;
  --mono: 'JetBrains Mono', monospace;
  --heading: 'Anton', sans-serif;
  --body: 'Inter', sans-serif;
  --gap: 20px;

  width: 100%;
  height: 100vh;
  background: var(--off-white);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  font-family: var(--body);
  color: var(--black);
  position: relative;
}

/* 背景网格 */
.grid-bg {
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(var(--gray) 1px, transparent 1px),
    linear-gradient(90deg, var(--gray) 1px, transparent 1px);
  background-size: 50px 50px;
  opacity: 0.4;
  pointer-events: none;
  z-index: 0;
}

.moving-grid {
  animation: gridScroll 30s linear infinite;
}

@keyframes gridScroll {
  0% { transform: translateY(0); }
  100% { transform: translateY(-50px); }
}

/* 悬浮语言切换按钮 */
.floating-lang-toggle {
  position: fixed;
  bottom: 40px;
  right: 40px;
  z-index: 9999;
  width: 70px;
  height: 70px;
  background: var(--red);
  border: 3px solid var(--black);
  color: var(--off-white);
  cursor: pointer;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 5px;
  font-family: var(--mono);
  font-weight: 700;
  box-shadow: -5px 5px 0 rgba(0, 0, 0, 0.2);
  transition: all 0.3s cubic-bezier(0.68, -0.55, 0.265, 1.55);
  clip-path: polygon(15% 0%, 100% 0%, 100% 85%, 85% 100%, 0% 100%, 0% 15%);
}

.floating-lang-toggle:hover {
  transform: translateY(-5px) rotate(5deg);
  box-shadow: -8px 8px 0 rgba(0, 0, 0, 0.3);
  background: var(--black);
  color: var(--off-white);
}

.toggle-icon {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
}

.lang-primary {
  font-size: 1.2rem;
  line-height: 1;
}

.lang-secondary {
  font-size: 0.65rem;
  opacity: 0.7;
}

.toggle-label {
  font-size: 0.55rem;
  letter-spacing: 1px;
  opacity: 0.8;
}

.profile-scroll-container {
  flex: 1;
  overflow-y: auto;
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
}

/* 顶部导航 */
.profile-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 30px;
  border-bottom: 4px solid var(--black);
  background: #fff;
  position: sticky;
  top: 0;
  z-index: 100;
  clip-path: polygon(0 0, 100% 0, 100% calc(100% - 10px), calc(100% - 10px) 100%, 0 100%);
}

.header-left .back-btn {
  background: var(--black);
  color: var(--off-white);
  border: 2px solid var(--black);
  padding: 12px 24px;
  font-family: var(--mono);
  font-size: 0.9rem;
  font-weight: 700;
  cursor: pointer;
  transition: 0.3s;
  display: flex;
  align-items: center;
  gap: 8px;
  clip-path: polygon(8% 0%, 100% 0%, 92% 100%, 0% 100%);
}

.header-left .back-btn:hover {
  background: var(--red);
  color: var(--off-white);
  transform: translateX(-5px);
  box-shadow: 5px 0 0 rgba(217, 35, 35, 0.3);
}

.btn-text-main {
  font-size: 1rem;
}

.btn-text-sub {
  font-size: 0.7rem;
  opacity: 0.7;
  margin-left: 5px;
}

.page-title {
  font-family: var(--heading);
  font-size: 2rem;
  margin: 0;
  display: flex;
  align-items: center;
  gap: 15px;
  text-transform: uppercase;
  position: relative;
  color: var(--black);
}

.title-deco {
  width: 8px;
  height: 40px;
  background: var(--red);
  clip-path: polygon(0 10%, 100% 0%, 100% 90%, 0% 100%);
}

.title-main {
  font-size: 2rem;
  letter-spacing: -1px;
}

.title-sub {
  font-size: 0.9rem;
  opacity: 0.6;
  font-family: var(--mono);
  font-weight: 400;
}

.mode-toggle {
  display: flex;
  gap: 0;
  border: 2px solid var(--black);
  overflow: hidden;
  clip-path: polygon(5% 0%, 100% 0%, 95% 100%, 0% 100%);
}

.toggle-btn {
  background: #fff;
  border: none;
  padding: 10px 20px;
  font-family: var(--mono);
  font-weight: 700;
  font-size: 0.85rem;
  cursor: pointer;
  transition: 0.2s;
  border-right: 1px solid var(--black);
  color: var(--black);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 3px;
}

.toggle-btn:last-child {
  border-right: none;
}

.toggle-btn .btn-main {
  font-size: 0.95rem;
}

.toggle-btn .btn-sub {
  font-size: 0.65rem;
  opacity: 0.6;
}

.toggle-btn.active {
  background: var(--black);
  color: var(--off-white);
}

.toggle-btn.active .btn-sub {
  opacity: 0.8;
}

.toggle-btn:hover:not(.active) {
  background: var(--gray);
}

/* 跑马灯 */
.tech-strip {
  background: var(--black);
  color: var(--off-white);
  padding: 8px 0;
  overflow: hidden;
  white-space: nowrap;
  font-family: var(--mono);
  font-size: 0.75rem;
  font-weight: 700;
  border-top: 2px solid var(--black);
  border-bottom: 2px solid var(--black);
}

.strip-content {
  display: inline-block;
  animation: marquee 30s linear infinite;
}

@keyframes marquee {
  0% { transform: translateX(0); }
  100% { transform: translateX(-50%); }
}

/* 主网格布局 */
.profile-main-grid {
  display: flex;
  padding: var(--gap);
  gap: var(--gap);
  max-width: 2560px;
  margin: 0 auto;
  width: 100%;
  box-sizing: border-box;
  align-items: flex-start;
  background: var(--off-white);
}

.profile-left-column {
  flex: 0 0 320px;
  display: flex;
  flex-direction: column;
  gap: var(--gap);
}

.profile-mid-column {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: var(--gap);
}

.profile-right-column {
  flex: 0 0 360px;
  display: flex;
  flex-direction: column;
  gap: var(--gap);
}

/* 通用卡片 */
.cyber-card {
  background: #fff;
  border: 3px solid var(--black);
  box-shadow: 5px 5px 0 rgba(0, 0, 0, 0.15);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  position: relative;
  transition: all 0.3s ease;
}

.cyber-card:hover {
  box-shadow: 8px 8px 0 rgba(0, 0, 0, 0.2);
  transform: translateY(-2px);
}

.card-label-strip {
  background: var(--black);
  color: var(--off-white);
  padding: 10px 15px;
  font-family: var(--mono);
  font-size: 0.8rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: 8px;
  clip-path: polygon(0 0, calc(100% - 15px) 0, 100% 100%, 0 100%);
}

.strip-main {
  font-size: 0.95rem;
}

.strip-sub {
  font-size: 0.65rem;
  opacity: 0.7;
}

.strip-count {
  font-size: 0.75rem;
  opacity: 0.8;
}

.card-deco-corner {
  position: absolute;
  top: -3px;
  right: -3px;
  width: 70px;
  height: 70px;
  background: var(--red);
  clip-path: polygon(100% 0, 100% 100%, 0 0);
  z-index: 1;
  opacity: 0.9;
}

/* 用户名片 */
.user-card {
  padding: 35px;
  position: relative;
  clip-path: polygon(0 0, 100% 0, 100% calc(100% - 20px), calc(100% - 20px) 100%, 0 100%);
}

.taichu-logo-badge {
  position: absolute;
  top: 18px;
  right: 18px;
  background: var(--black);
  color: var(--off-white);
  font-family: var(--heading);
  font-size: 0.75rem;
  padding: 6px 14px;
  letter-spacing: 2px;
  z-index: 2;
  transform: rotate(-2deg);
  box-shadow: 3px 3px 0 var(--red);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  clip-path: polygon(10% 0%, 100% 0%, 90% 100%, 0% 100%);
}

.badge-main {
  font-size: 0.85rem;
}

.badge-sub {
  font-size: 0.55rem;
  opacity: 0.7;
}

.avatar-section {
  display: flex;
  justify-content: center;
  margin-bottom: 25px;
  position: relative;
}

.avatar-wrapper {
  width: 150px;
  height: 150px;
  border: 4px solid var(--black);
  position: relative;
  overflow: hidden;
  transition: 0.3s;
  clip-path: polygon(15% 0%, 100% 0%, 100% 85%, 85% 100%, 0% 100%, 0% 15%);
  box-shadow: 0 0 20px rgba(0, 0, 0, 0.1);
}

.avatar-wrapper img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  filter: grayscale(30%);
  transition: 0.3s;
}

.avatar-wrapper:hover img {
  filter: grayscale(0);
  transform: scale(1.1);
}

.avatar-wrapper.edit-hover:hover {
  transform: scale(1.05) rotate(2deg);
  border-color: var(--red);
  box-shadow: 0 0 30px rgba(217, 35, 35, 0.4);
}

.avatar-overlay {
  position: absolute;
  inset: 0;
  background: rgba(217, 35, 35, 0.9);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: 0.3s;
  cursor: pointer;
  color: var(--off-white);
  font-family: var(--mono);
  font-size: 0.85rem;
  gap: 8px;
}

.overlay-main {
  font-size: 0.95rem;
  font-weight: 700;
}

.overlay-sub {
  font-size: 0.7rem;
  opacity: 0.8;
}

.avatar-wrapper.edit-hover:hover .avatar-overlay {
  opacity: 1;
}

.level-badge {
  position: absolute;
  bottom: -12px;
  left: 50%;
  transform: translateX(-50%);
  background: var(--black);
  color: var(--off-white);
  font-family: var(--mono);
  font-weight: 700;
  padding: 6px 16px;
  font-size: 0.85rem;
  border: 3px solid var(--red);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  clip-path: polygon(10% 0%, 90% 0%, 100% 50%, 90% 100%, 10% 100%, 0% 50%);
  box-shadow: 0 4px 0 rgba(0, 0, 0, 0.3);
}

.badge-level {
  font-size: 0.95rem;
}

.badge-level-sub {
  font-size: 0.6rem;
  opacity: 0.7;
}

.user-info-block {
  text-align: center;
}

.username {
  font-family: var(--heading);
  font-size: 2.2rem;
  margin: 0 0 8px 0;
  text-transform: uppercase;
  letter-spacing: -1px;
  color: var(--black);
}

.username-input {
  font-family: var(--heading);
  font-size: 2rem;
  text-align: center;
  text-transform: uppercase;
  background: #fff;
  color: var(--black);
  border: 2px solid var(--black);
}

.user-id {
  font-family: var(--mono);
  font-size: 0.85rem;
  color: #666;
  margin-bottom: 18px;
  opacity: 0.8;
}

.bio-section {
  margin: 18px 0;
  padding: 18px;
  background: var(--gray);
  border-left: 4px solid var(--black);
  clip-path: polygon(0 0, 100% 0, 100% calc(100% - 8px), calc(100% - 8px) 100%, 0 100%);
}

.bio-text {
  font-size: 0.95rem;
  line-height: 1.6;
  margin: 0;
  font-family: var(--mono);
  color: var(--black);
}

.edit-input,
.edit-textarea {
  width: 100%;
  border: 2px solid var(--black);
  padding: 12px;
  font-family: var(--body);
  font-size: 0.95rem;
  outline: none;
  transition: 0.2s;
  box-sizing: border-box;
  margin: 12px 0;
  background: #fff;
  color: var(--black);
}

.edit-textarea {
  resize: vertical;
  font-family: var(--mono);
  line-height: 1.6;
}

.edit-input:focus,
.edit-textarea:focus {
  border-color: var(--red);
  background: #fafafa;
  box-shadow: 0 0 10px rgba(217, 35, 35, 0.2);
}

.stats-mini-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 18px;
  margin-top: 25px;
  padding-top: 25px;
  border-top: 3px solid var(--black);
}

.stat-mini {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 12px;
  background: var(--gray);
  clip-path: polygon(10% 0%, 100% 0%, 90% 100%, 0% 100%);
  transition: 0.3s;
}

.stat-mini:hover {
  background: var(--black);
  transform: translateY(-3px);
}

.stat-mini .num {
  font-family: var(--heading);
  font-size: 2rem;
  color: var(--red);
}

.stat-mini:hover .num {
  color: var(--off-white);
}

.stat-mini .label {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 3px;
}

.label-main {
  font-family: var(--mono);
  font-size: 0.75rem;
  text-transform: uppercase;
  color: var(--black);
  font-weight: 700;
}

.stat-mini:hover .label-main {
  color: var(--off-white);
}

.label-sub {
  font-size: 0.6rem;
  opacity: 0.7;
  color: #666;
}

.stat-mini:hover .label-sub {
  color: var(--gray);
}

.save-profile-btn {
  width: 100%;
  background: var(--red);
  color: var(--off-white);
  border: 3px solid var(--black);
  padding: 18px;
  font-family: var(--mono);
  font-weight: 700;
  font-size: 1.05rem;
  cursor: pointer;
  margin-top: 25px;
  transition: 0.3s;
  box-shadow: 0 5px 0 var(--red-dark);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  clip-path: polygon(0 0, 100% 0, 100% calc(100% - 12px), calc(100% - 12px) 100%, 0 100%);
}

.save-profile-btn:hover {
  transform: translateY(3px);
  box-shadow: 0 2px 0 var(--red-dark);
  background: var(--black);
  color: var(--off-white);
}

.save-profile-btn:active {
  transform: translateY(5px);
  box-shadow: none;
}

/* 成就栏 */
.achievements-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
  padding: 18px;
  background: var(--gray-light);
}

.achievement-item {
  aspect-ratio: 1;
  border: 2px solid #ccc;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 6px;
  cursor: pointer;
  transition: 0.3s;
  background: #f5f5f5;
  filter: grayscale(100%);
  opacity: 0.4;
  clip-path: polygon(15% 0%, 100% 0%, 100% 85%, 85% 100%, 0% 100%, 0% 15%);
}

.achievement-item.unlocked {
  filter: grayscale(0);
  opacity: 1;
  border-color: var(--black);
  background: #fff;
}

.achievement-item.unlocked:hover {
  transform: translateY(-5px) rotate(5deg);
  box-shadow: 0 8px 0 var(--red);
  background: var(--red);
}

.achievement-item i {
  font-size: 1.8rem;
  color: var(--red);
}

.achievement-item.unlocked:hover i {
  color: var(--off-white);
}

.ach-name {
  font-family: var(--mono);
  font-size: 0.7rem;
  text-align: center;
  font-weight: 700;
  color: var(--black);
}

.achievement-item.unlocked:hover .ach-name {
  color: var(--off-white);
}

/* 仓库入口 */
.repo-list {
  padding: 12px;
  background: var(--gray-light);
}

.repo-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px;
  margin-bottom: 10px;
  border: 2px solid #ddd;
  cursor: pointer;
  transition: 0.3s;
  background: #fff;
  clip-path: polygon(0 0, calc(100% - 10px) 0, 100% 50%, calc(100% - 10px) 100%, 0 100%);
}

.repo-item:hover {
  border-color: var(--black);
  transform: translateX(8px);
  background: var(--gray);
  box-shadow: -5px 0 0 var(--red);
}

.repo-item i {
  font-size: 1.3rem;
  color: var(--red);
}

.repo-name {
  flex: 1;
  font-weight: 700;
  font-family: var(--mono);
  font-size: 0.95rem;
  color: var(--black);
}

.repo-count {
  font-family: var(--mono);
  font-size: 0.85rem;
  color: var(--off-white);
  background: var(--black);
  padding: 4px 12px;
  border-radius: 0;
  clip-path: polygon(10% 0%, 100% 0%, 90% 100%, 0% 100%);
}

/* Tab切换 */
.content-tabs {
  display: flex;
  gap: 0;
  border: 3px solid var(--black);
  background: #fff;
  overflow: hidden;
}

.tab-btn {
  flex: 1;
  background: #fff;
  border: none;
  border-right: 2px solid var(--black);
  padding: 16px 12px;
  font-family: var(--mono);
  font-weight: 700;
  font-size: 0.85rem;
  cursor: pointer;
  transition: 0.3s;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 5px;
  color: var(--black);
  position: relative;
}

.tab-btn:last-child {
  border-right: none;
}

.tab-main {
  font-size: 0.95rem;
}

.tab-count {
  font-size: 0.75rem;
  opacity: 0.8;
}

.tab-sub {
  font-size: 0.65rem;
  opacity: 0.6;
}

.tab-btn.active {
  background: var(--black);
  color: var(--off-white);
}

.tab-btn.active .tab-sub {
  opacity: 0.9;
}

.tab-btn:hover:not(.active) {
  background: var(--gray);
}

.tab-btn::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 0;
  background: var(--red);
  transition: 0.3s;
}

.tab-btn.active::after {
  height: 4px;
}

/* 作品瀑布流 */
.works-waterfall {
  min-height: 600px;
}

.loading-state,
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 20px;
  font-family: var(--mono);
  color: #888;
  gap: 25px;
  background: #fff;
  border: 3px solid var(--black);
}

.loading-state i {
  font-size: 2.5rem;
  color: var(--red);
}

.upload-btn {
  background: var(--red);
  color: var(--off-white);
  border: 3px solid var(--black);
  padding: 14px 28px;
  font-family: var(--mono);
  font-weight: 700;
  cursor: pointer;
  transition: 0.3s;
  box-shadow: 0 5px 0 var(--red-dark);
  display: flex;
  align-items: center;
  gap: 10px;
  clip-path: polygon(5% 0%, 100% 0%, 95% 100%, 0% 100%);
}

.upload-btn:hover {
  transform: translateY(3px);
  box-shadow: 0 2px 0 var(--red-dark);
  background: var(--black);
  color: var(--off-white);
}

.waterfall-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 20px;
  padding: 20px;
  background: var(--gray);
  border: 3px solid var(--black);
}

.artwork-card {
  background: #fff;
  border: 2px solid var(--black);
  overflow: hidden;
  cursor: pointer;
  transition: 0.3s;
  animation: fadeInUp 0.5s ease-out;
  clip-path: polygon(0 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%);
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.artwork-card:hover {
  transform: translateY(-10px) rotate(2deg);
  box-shadow: 0 10px 0 rgba(0, 0, 0, 0.2);
  border-color: var(--red);
}

.artwork-img-wrapper {
  position: relative;
  aspect-ratio: 3/4;
  overflow: hidden;
}

.artwork-img-wrapper img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: 0.3s;
  filter: grayscale(20%);
}

.artwork-card:hover img {
  transform: scale(1.15);
  filter: grayscale(0);
}

.artwork-overlay {
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: 0.3s;
}

.artwork-card:hover .artwork-overlay {
  opacity: 1;
}

.overlay-stats {
  display: flex;
  gap: 25px;
  color: var(--off-white);
  font-family: var(--mono);
  font-size: 0.95rem;
  font-weight: 700;
}

.artwork-info {
  padding: 14px;
  border-top: 2px solid var(--black);
  background: #fff;
}

.artwork-title {
  font-weight: 700;
  font-size: 0.95rem;
  margin: 0 0 6px 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: var(--black);
}

.artwork-date {
  font-family: var(--mono);
  font-size: 0.7rem;
  color: #666;
}

/* 博客列表 */
.blogs-list {
  min-height: 600px;
}

.blog-list-container {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 20px;
  background: var(--gray);
  border: 3px solid var(--black);
}

.blog-item-card {
  background: #fff;
  border: 2px solid var(--black);
  display: flex;
  gap: 20px;
  cursor: pointer;
  transition: 0.3s;
  overflow: hidden;
  clip-path: polygon(0 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%);
}

.blog-item-card:hover {
  transform: translateX(10px);
  box-shadow: -10px 10px 0 rgba(0, 0, 0, 0.15);
  border-color: var(--red);
}

.blog-cover {
  flex: 0 0 200px;
  overflow: hidden;
}

.blog-cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  filter: grayscale(50%);
  transition: 0.3s;
}

.blog-item-card:hover .blog-cover img {
  filter: grayscale(0);
  transform: scale(1.15);
}

.blog-content-section {
  flex: 1;
  padding: 22px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.blog-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  font-family: var(--mono);
  font-size: 0.75rem;
}

.blog-tag {
  background: var(--red);
  color: var(--off-white);
  padding: 4px 12px;
  font-weight: 700;
  clip-path: polygon(8% 0%, 100% 0%, 92% 100%, 0% 100%);
}

.blog-date {
  color: #666;
}

.blog-title {
  font-family: var(--heading);
  font-size: 1.6rem;
  margin: 0 0 12px 0;
  text-transform: uppercase;
  color: var(--black);
  letter-spacing: -0.5px;
}

.blog-excerpt {
  color: #555;
  line-height: 1.7;
  margin: 0 0 18px 0;
  flex: 1;
}

.blog-stats {
  display: flex;
  gap: 25px;
  font-family: var(--mono);
  font-size: 0.85rem;
  color: #666;
}

/* 活动时间线 */
.activity-timeline {
  min-height: 600px;
}

.timeline-container {
  padding: 20px 20px 20px 50px;
  background: var(--gray);
  border: 3px solid var(--black);
  position: relative;
}

.timeline-container::before {
  content: '';
  position: absolute;
  left: 32px;
  top: 20px;
  bottom: 20px;
  width: 3px;
  background: var(--black);
}

.activity-item {
  position: relative;
  margin-bottom: 30px;
  padding-left: 35px;
}

.activity-dot {
  position: absolute;
  left: -10px;
  top: 8px;
  width: 14px;
  height: 14px;
  background: var(--red);
  border: 3px solid var(--black);
  border-radius: 50%;
  box-shadow: 0 0 10px rgba(217, 35, 35, 0.4);
}

.activity-content {
  background: #fff;
  border: 2px solid var(--black);
  padding: 18px;
  clip-path: polygon(0 0, calc(100% - 12px) 0, 100% 50%, calc(100% - 12px) 100%, 0 100%);
  transition: 0.3s;
}

.activity-content:hover {
  background: var(--gray-light);
  transform: translateX(5px);
}

.activity-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.activity-type {
  font-family: var(--mono);
  font-weight: 700;
  font-size: 0.85rem;
  padding: 4px 12px;
  background: var(--black);
  color: var(--off-white);
  display: inline-flex;
  align-items: center;
  gap: 6px;
  clip-path: polygon(8% 0%, 100% 0%, 92% 100%, 0% 100%);
}

.activity-type.post { background: #2196F3; }
.activity-type.blog { background: #4CAF50; }
.activity-type.artwork { background: #FF9800; }
.activity-type.comment { background: #9C27B0; }
.activity-type.like { background: var(--red); }
.activity-type.follow { background: #009688; }

.activity-time {
  font-family: var(--mono);
  font-size: 0.75rem;
  color: #666;
}

.activity-desc {
  margin: 0;
  font-size: 0.95rem;
  color: var(--black);
  line-height: 1.5;
}

/* 留言板 */
.guestbook-card {
  height: 600px;
  display: flex;
  flex-direction: column;
  clip-path: polygon(0 0, 100% 0, 100% calc(100% - 25px), calc(100% - 25px) 100%, 0 100%);
}

.guestbook-messages {
  flex: 1;
  overflow-y: auto;
  padding: 18px;
  background: var(--gray-light);
}

.empty-guestbook {
  text-align: center;
  padding: 60px 20px;
  font-family: var(--mono);
  color: #888;
}

.guestbook-msg {
  background: #fff;
  border: 2px solid var(--black);
  padding: 14px;
  margin-bottom: 14px;
  animation: slideInRight 0.4s ease-out;
  clip-path: polygon(0 0, calc(100% - 10px) 0, 100% 50%, calc(100% - 10px) 100%, 0 100%);
  transition: 0.3s;
}

.guestbook-msg:hover {
  background: var(--gray);
  transform: translateX(5px);
}

@keyframes slideInRight {
  from {
    opacity: 0;
    transform: translateX(30px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

.msg-header {
  display: flex;
  gap: 12px;
  margin-bottom: 10px;
}

.msg-avatar {
  width: 36px;
  height: 36px;
  border: 2px solid var(--black);
  clip-path: polygon(15% 0%, 100% 0%, 100% 85%, 85% 100%, 0% 100%, 0% 15%);
}

.msg-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.msg-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.msg-author {
  font-family: var(--mono);
  font-weight: 700;
  font-size: 0.9rem;
  color: var(--black);
}

.msg-time {
  font-family: var(--mono);
  font-size: 0.7rem;
  color: #666;
}

.msg-content {
  margin: 0;
  line-height: 1.6;
  font-size: 0.95rem;
  padding-left: 48px;
  color: var(--black);
}

.guestbook-input-area {
  padding: 18px;
  border-top: 3px solid var(--black);
  background: #fff;
  display: flex;
  gap: 12px;
}

.msg-input {
  flex: 1;
  border: 2px solid var(--black);
  padding: 12px;
  font-family: var(--body);
  font-size: 0.95rem;
  resize: none;
  outline: none;
  background: #fff;
  color: var(--black);
  transition: 0.3s;
}

.msg-input:focus {
  border-color: var(--red);
  box-shadow: 0 0 10px rgba(217, 35, 35, 0.2);
}

.msg-send-btn {
  background: var(--black);
  color: var(--off-white);
  border: 2px solid var(--black);
  padding: 0 24px;
  font-family: var(--mono);
  font-weight: 700;
  cursor: pointer;
  transition: 0.3s;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 3px;
  clip-path: polygon(15% 0%, 100% 0%, 85% 100%, 0% 100%);
}

.btn-send-main {
  font-size: 0.95rem;
}

.btn-send-sub {
  font-size: 0.65rem;
  opacity: 0.7;
}

.msg-send-btn:hover:not(:disabled) {
  background: var(--red);
  border-color: var(--red);
  transform: scale(1.05);
}

.msg-send-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

/* 设置卡片 */
.settings-list {
  padding: 22px;
  display: flex;
  flex-direction: column;
  gap: 22px;
  background: var(--gray-light);
}

.setting-item {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.setting-item.toggle-row {
  flex-direction: row;
  justify-content: space-between;
  align-items: center;
}

.setting-label {
  font-family: var(--mono);
  font-weight: 700;
  font-size: 0.9rem;
  display: flex;
  align-items: center;
  gap: 10px;
  color: var(--black);
}

.setting-label i {
  color: var(--red);
  font-size: 1.1rem;
}

.label-main {
  font-size: 0.95rem;
}

.label-sub {
  font-size: 0.7rem;
  opacity: 0.7;
  margin-left: 8px;
}

.setting-input {
  border: 2px solid var(--black);
  padding: 12px;
  font-family: var(--body);
  font-size: 0.95rem;
  outline: none;
  transition: 0.3s;
  background: #fff;
  color: var(--black);
}

.setting-input:focus {
  border-color: var(--red);
  box-shadow: 0 0 10px rgba(217, 35, 35, 0.2);
}

.color-picker-group {
  display: flex;
  gap: 12px;
}

.color-option {
  width: 40px;
  height: 40px;
  border: 3px solid #ddd;
  cursor: pointer;
  transition: 0.3s;
  clip-path: polygon(15% 0%, 100% 0%, 100% 85%, 85% 100%, 0% 100%, 0% 15%);
}

.color-option:hover {
  transform: scale(1.15) rotate(5deg);
}

.color-option.selected {
  border-color: var(--black);
  transform: scale(1.2);
  box-shadow: 0 0 15px rgba(217, 35, 35, 0.4);
}

.cyber-switch {
  position: relative;
  display: inline-block;
  width: 55px;
  height: 28px;
}

.cyber-switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.switch-slider {
  position: absolute;
  cursor: pointer;
  inset: 0;
  background: #ccc;
  border: 2px solid var(--black);
  transition: 0.3s;
}

.switch-slider::before {
  position: absolute;
  content: '';
  height: 18px;
  width: 18px;
  left: 3px;
  bottom: 3px;
  background: var(--black);
  transition: 0.3s;
}

.input:checked + .switch-slider {
  background: var(--red);
}

input:checked + .switch-slider::before {
  transform: translateX(27px);
  background: #fff;
}

.danger-btn {
  background: transparent;
  color: var(--red);
  border: 3px solid var(--red);
  padding: 14px;
  font-family: var(--mono);
  font-weight: 700;
  cursor: pointer;
  transition: 0.3s;
  margin-top: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  clip-path: polygon(0 0, 100% 0, 100% calc(100% - 10px), calc(100% - 10px) 100%, 0 100%);
}

.danger-btn:hover {
  background: var(--red);
  color: var(--off-white);
  transform: scale(1.02);
}

/* 社交链接 */
.social-links {
  padding: 18px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  background: var(--gray-light);
}

.social-link {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  border: 2px solid var(--black);
  text-decoration: none;
  color: var(--black);
  font-family: var(--mono);
  font-weight: 700;
  transition: 0.3s;
  background: #fff;
  clip-path: polygon(0 0, calc(100% - 10px) 0, 100% 50%, calc(100% - 10px) 100%, 0 100%);
}

.social-link:hover {
  background: var(--black);
  color: var(--off-white);
  transform: translateX(8px);
  box-shadow: -5px 0 0 var(--red);
}

.social-link i {
  font-size: 1.3rem;
}

/* 删除确认弹窗 */
.cyber-modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.85);
  z-index: 9999;
  display: flex;
  justify-content: center;
  align-items: center;
  backdrop-filter: blur(5px);
}

.confirm-modal {
  background: #fff;
  border: 4px solid var(--black);
  padding: 35px;
  max-width: 450px;
  box-shadow: 15px 15px 0 rgba(0, 0, 0, 0.3);
  clip-path: polygon(0 0, 100% 0, 100% calc(100% - 20px), calc(100% - 20px) 100%, 0 100%);
}

.modal-header {
  display: flex;
  align-items: center;
  gap: 18px;
  margin-bottom: 25px;
}

.warning-icon {
  font-size: 2.5rem;
  color: var(--red);
}

.modal-header h3 {
  font-family: var(--heading);
  font-size: 1.8rem;
  margin: 0;
  color: var(--black);
}

.modal-text {
  font-size: 1.05rem;
  line-height: 1.7;
  margin-bottom: 30px;
  color: #555;
}

.modal-actions {
  display: flex;
  gap: 18px;
}

.cancel-btn,
.confirm-btn {
  flex: 1;
  padding: 14px;
  border: 3px solid var(--black);
  font-family: var(--mono);
  font-weight: 700;
  cursor: pointer;
  transition: 0.3s;
  font-size: 0.95rem;
}

.cancel-btn {
  background: #fff;
  color: var(--black);
  clip-path: polygon(8% 0%, 100% 0%, 92% 100%, 0% 100%);
}

.cancel-btn:hover {
  background: var(--gray);
  transform: translateY(-3px);
}

.confirm-btn {
  background: var(--red);
  color: var(--off-white);
  border-color: var(--red);
  clip-path: polygon(0 0, 92% 0%, 100% 100%, 8% 100%);
}

.confirm-btn:hover {
  background: var(--black);
  border-color: var(--black);
  transform: translateY(-3px);
}

/* 过渡动画 */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.4s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.modal-scale-enter-active,
.modal-scale-leave-active {
  transition: all 0.4s cubic-bezier(0.68, -0.55, 0.265, 1.55);
}

.modal-scale-enter-from,
.modal-scale-leave-to {
  opacity: 0;
  transform: scale(0.8) rotate(-5deg);
}

/* 响应式 */
@media screen and (max-width: 1400px) {
  .profile-left-column {
    flex: 0 0 280px;
  }
  .profile-right-column {
    flex: 0 0 320px;
  }
}

@media screen and (max-width: 1920px) {
  .cyber-profile {
    zoom: 0.8;
  }
}

/* 滚动条样式 */
.custom-scroll::-webkit-scrollbar {
  width: 10px;
  height: 10px;
}

.custom-scroll::-webkit-scrollbar-track {
  background: var(--gray);
}

.custom-scroll::-webkit-scrollbar-thumb {
  background: var(--black);
  border: 2px solid var(--gray);
}

.custom-scroll::-webkit-scrollbar-thumb:hover {
  background: var(--red);
}
</style>
