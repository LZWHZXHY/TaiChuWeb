<template>
  <div class="tc-dashboard">
    <header class="sys-info-bar">
      <span class="pulse">●</span>
      <span>TAICHU_ART_HALL // VISUAL_ARCHIVE // {{ currentTime }}</span>
    </header>

    <div class="main-bridge">
      
      <aside class="left-column">
        <div class="section-label">CHANNELS // 频道</div>
        <div class="nav-menu">
          <div 
            class="nav-item" 
            :class="{ active: currentChannel === 'gallery' }"
            @click="currentChannel = 'gallery'"
          >
            <i class="fas fa-palette"></i> 寰宇画廊 (Gallery)
            <span class="count-badge">Live</span>
          </div>

          <div 
            class="nav-item" 
            :class="{ active: currentChannel === 'joint' }"
            @click="currentChannel = 'joint'"
          >
            <i class="fas fa-handshake"></i> 柴圈联合 (Joint)
            <span class="count-badge">New</span>
          </div>

          <div 
            class="nav-item" 
            :class="{ active: currentChannel === 'certification' }"
            @click="currentChannel = 'certification'"
          >
            <i class="fas fa-certificate"></i> 创作认证 (Cert)
            <span class="count-badge">System</span>
          </div>

          <div 
            class="nav-item" 
            :class="{ active: currentChannel === 'battlefield' }" 
            @click="currentChannel = 'battlefield'"
          >
            <i class="fas fa-khanda"></i> 太初约战场 (Battle)
            <span class="count-badge">Alpha</span>
          </div>

          <div 
            class="nav-item" 
            :class="{ active: currentChannel === 'society' }" 
            @click="currentChannel = 'society'"
          >
            <i class="fas fa-users"></i> 柴圈社团 (Society)
            <span class="count-badge">Beta</span>
          </div>
        </div>

        <div class="section-label" style="margin-top: 30px;">MY_ARTWORK</div>
        
        <div class="user-stat-box">
          <div class="stat" @click="currentChannel = 'gallery'" title="跳转至寰宇画廊">
            <span class="num">{{artAmount}}</span>
            <span class="txt">画廊作品</span>
          </div>
          
          <div class="stat" @click="currentChannel = 'joint'" title="跳转至柴圈联合">
            <span class="num">{{JointAmount}}</span>
            <span class="txt">已举行联合</span>
          </div>
          
          <div class="stat" @click="currentChannel = 'battlefield'" title="跳转至太初约战场">
            <span class="num">{{OCAmount}}</span>
            <span class="txt">已收录人设</span>
          </div>
          
          <div class="stat" @click="currentChannel = 'society'" title="跳转至柴圈社团">
            <span class="num">{{SocietyAmount}}</span>
            <span class="txt">已收录群聊</span>
          </div>
        </div>

      </aside>

      <main class="mid-column" :class="{ 'full-width': ['society', 'battlefield', 'certification'].includes(currentChannel) }">
        
        <div v-if="currentChannel === 'gallery'" class="channel-view">
          <div class="content-header">
            <div class="header-group-left">
              <div class="header-title">
                <h2>COMMUNITY_ARTWORKS</h2>
                <span class="subtitle">探索社区创造的无限可能</span>
              </div>

              <div class="segment-button-group">
                <div
                  class="button-segment"
                  :class="{ active: selectedSegment === 1, hoverable: true }"
                  @click="handleSegmentClick(1)"
                >
                  动态 
                </div>
                <div
                  class="button-segment"
                  :class="{ active: selectedSegment === 2, hoverable: true }"
                  @click="handleSegmentClick(2)"
                >
                  综合 
                </div>
                <div
                  class="button-segment"
                  :class="{ active: selectedSegment === 3, hoverable: true }"
                  @click="handleSegmentClick(3)"
                >
                  静态 
                </div>
              </div>
            </div>

            <button class="upload-btn" @click="showUploadModal = true">
              <i class="fas fa-cloud-upload-alt"></i> 投稿
            </button>
          </div>

          <div class="scroll-wrapper custom-scroll" ref="mainScroll" @scroll="handleScroll">
            <div class="gallery-container">
              <div v-if="loading && artworks.length === 0" class="loading-state">
                <i class="fas fa-circle-notch fa-spin"></i> 读取神经元数据...
              </div>

              <div v-else class="gallery-waterfall">
                <div v-for="img in filteredArtworks" :key="img.id" class="art-card" @click="openLightbox(img)">
                  <div class="img-wrapper">
                    <img :src="upgradeUrlToHttps(img.imageUrlFull || img.url)" @error="handleImgError" loading="lazy" />
                  </div>
                  <div class="art-overlay">
                    <h3 class="art-title">{{ img.title }}</h3>
                    <div class="art-meta">
                      <span><i class="fas fa-user"></i> {{ img.authorName || img.author }}</span>
                      <span><i class="fas fa-heart"></i> {{ img.likes }}</span>
                    </div>
                  </div>
                </div>
              </div>

              <div class="list-footer">
                <span v-if="loading && artworks.length > 0">正在加载更多资源...</span>
                <span v-if="!hasMore && artworks.length > 0" class="end-text">--- 已显示全部作品 ---</span>
                <span v-if="!loading && artworks.length === 0" class="end-text">暂无作品，快来发布第一个吧！</span>
                <span v-if="!loading && artworks.length > 0 && filteredArtworks.length === 0" class="end-text">该分类下暂无作品</span>
              </div>
            </div>
          </div>
        </div>

        <JointBoard v-else-if="currentChannel === 'joint'" />
        <Battlefield v-else-if="currentChannel === 'battlefield'" />
        <SocietyPanel v-else-if="currentChannel === 'society'" />
        <CertificationPanel v-else-if="currentChannel === 'certification'" />

      </main>

      <aside class="right-column" v-if="!['society', 'battlefield', 'certification'].includes(currentChannel)">
        <div class="tags-panel">
          <div class="section-label">TRENDING_TAGS</div>
          <div class="tag-cloud">
            <span class="tag">#太初设定集</span>
            <span class="tag">#二创插画</span>
            <span class="tag">#火柴人</span>
            <span class="tag">#概念设计</span>
            <span class="tag">#壁纸分享</span>
          </div>
        </div>

        <div class="top-contributors">
          <div class="section-label">TOP_ARTISTS // 社区名人堂</div>
          
          <div v-for="(user, idx) in leaderboard" :key="user.UploaderId" class="artist-row">
            <span class="rank-num" :class="'rank-' + (idx + 1)">{{ idx + 1 }}</span>
            <div class="artist-info">
              <span class="artist-name">{{ user.Name }}</span>
              <span class="artist-stats">
                <i class="fas fa-heart" style="font-size:10px; margin-right:2px;"></i> {{ user.TotalLikes }}
              </span>
            </div>
          </div>
          <div v-if="leaderboard.length === 0" class="empty-rank">
            暂无排行数据
          </div>
        </div>
      </aside>

    </div>

    <Teleport to="body">
       <Transition name="fade">
        <div v-if="showUploadModal" class="modal-overlay" @click.self="showUploadModal = false">
             <div class="modal-container">
               <div class="modal-header">
                 <h3>UPLOAD_ARTWORK // 投稿</h3>
                 <button class="close-btn" @click="showUploadModal = false"><i class="fas fa-times"></i></button>
               </div>
               <div class="modal-body">
                  <div class="upload-area" @click="triggerFileSelect" :class="{ 'has-file': uploadForm.previewUrl }">
                    <input 
                      type="file" 
                      ref="fileInput" 
                      accept="image/png, image/jpeg, image/gif, image/webp" 
                      @change="handleFileChange" 
                      style="display: none"
                    />
                    <div v-if="uploadForm.previewUrl" class="preview-box">
                      <img :src="uploadForm.previewUrl" />
                      <div class="re-upload-tip">点击更换图片</div>
                    </div>
                    <div v-else class="placeholder-box">
                      <i class="fas fa-cloud-upload-alt"></i>
                      <p>点击上传图片 (Max 20MB)</p>
                      <span class="sub-text">支持 JPG, PNG, GIF, WEBP</span>
                    </div>
                  </div>
                  <div class="form-group">
                    <label>作品标题 (必填)</label>
                    <input type="text" v-model="uploadForm.title" placeholder="给作品起个名字..." maxlength="50" />
                  </div>
                  <div class="form-group">
                    <label>作品描述 (必填)</label>
                    <textarea v-model="uploadForm.desc" placeholder="讲述这张图片背后的故事..." rows="3"></textarea>
                  </div>
                  <div class="form-group">
                    <label>作者署名 (必填)</label>
                    <input type="text" v-model="uploadForm.authorName" placeholder="默认使用当前用户名" maxlength="20" />
                  </div>
                  <button class="submit-btn" @click="submitArtwork" :disabled="isUploading">
                    <span v-if="isUploading"><i class="fas fa-spinner fa-spin"></i> 上传中...</span>
                    <span v-else>确认发布</span>
                  </button>
               </div>
             </div>
        </div>
      </Transition>
    </Teleport>

    <Teleport to="body">
      <Transition name="fade">
        <div v-if="lightboxImg" class="lightbox-overlay" @click="lightboxImg = null">
            <img :src="upgradeUrlToHttps(lightboxImg.imageUrlFull || lightboxImg.url)" class="lightbox-img" @click.stop />
            <div class="lightbox-caption" @click.stop>
               <h3>{{ lightboxImg.title }}</h3>
               <p class="desc" v-if="lightboxImg.desc">{{ lightboxImg.desc }}</p>
               <div class="action-bar">
                  <button class="like-action-btn" :class="{ 'is-liked': lightboxImg.isLiked, 'is-animating': lightboxImg.isAnimating }" @click="handleLike(lightboxImg)">
                    <i class="fas fa-heart"></i>
                    <span>{{ lightboxImg.likes || 0 }}</span>
                  </button>
               </div>
               <div class="meta">
                 <span>By @{{ lightboxImg.authorName || lightboxImg.author }}</span>
                 <span>{{ formatTime(lightboxImg.uploadAt) }}</span>
               </div>
            </div>
            <button class="lightbox-close"><i class="fas fa-times"></i></button>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted, computed } from 'vue'; 
import apiClient from '@/utils/api'; 

import JointBoard from '@/ArtCenter/UnionPanel.vue' 
import SocietyPanel from '@/ArtCenter/SocietyPanel.vue' 
import Battlefield from '@/ArtCenter/Battlefield.vue' 
import CertificationPanel from '@/ArtCenter/CertificationPanel.vue' 

const currentTime = ref(new Date().toLocaleTimeString());
let clockTimer = null;

const currentChannel = ref('gallery');
const selectedSegment = ref(2); 

const handleSegmentClick = (segment) => {
  selectedSegment.value = segment;
  if (mainScroll.value) {
    mainScroll.value.scrollTop = 0;
  }
};

const artworks = ref([]);
const loading = ref(false);
const nextCursor = ref(null); 
const hasMore = ref(true);
const mainScroll = ref(null); 

const filteredArtworks = computed(() => {
  if (selectedSegment.value === 2) {
    return artworks.value; 
  }
  return artworks.value.filter(img => {
    const url = (img.imageUrlFull || img.url || '').toLowerCase();
    const isGif = url.endsWith('.gif') || url.includes('.gif?'); 
    if (selectedSegment.value === 1) return isGif; 
    else if (selectedSegment.value === 3) return !isGif; 
    return true;
  });
});

const showUploadModal = ref(false);
const isUploading = ref(false);
const fileInput = ref(null);
const uploadForm = reactive({ title: '', desc: '', authorName: '', file: null, previewUrl: '' });

const artAmount = ref(0);
const OCAmount = ref(0);
const JointAmount = ref(0);
const SocietyAmount = ref(0);

const lightboxImg = ref(null);
const leaderboard = ref([]);

const fetchLeaderboard = async () => {
  try {
    const res = await apiClient.get('/Drawing/leaderboard?limit=10');
    if (res.data.success) leaderboard.value = res.data.data;
  } catch (error) { console.error("加载排行榜失败", error); }
};

const fetchTotalCount = async () => {
  try {
    const [galleryRes, jointRes, ocRes, societyRes] = await Promise.all([
      apiClient.get('/Drawing/AllArtWork'),
      apiClient.get('/ChaiLianHe/verified-count'),
      apiClient.get('/Chai/stats/OCcount'),
      apiClient.get('/ChaiSheTuan/verified-count')
    ]);
    if (galleryRes.status === 200) artAmount.value = galleryRes.data;
    if (jointRes.status === 200) JointAmount.value = jointRes.data;
    if (ocRes.status === 200) OCAmount.value = ocRes.data;
    if (societyRes.status === 200) SocietyAmount.value = societyRes.data;
  } catch (error) { console.error("获取统计数据失败", error); }
};

const upgradeUrlToHttps = (url) => {
  if (!url) return '';
  if (url.startsWith('/')) return url;
  return url.replace('http://', 'https://');
};

const fetchArtworks = async (isRefresh = false) => {
  if (loading.value) return;
  if (!isRefresh && !hasMore.value) return;
  loading.value = true;
  try {
    const params = { pageSize: 30, sort: 'new', cursor: isRefresh ? null : nextCursor.value };
    const res = await apiClient.get('/Drawing/list', { params });
    if (res.data.success) {
      const { items, nextCursor: newCursor, hasMore: more } = res.data.data;
      if (isRefresh) artworks.value = items;
      else artworks.value = [...artworks.value, ...items];
      nextCursor.value = newCursor;
      hasMore.value = more;
    }
  } catch (error) { console.error("加载画廊失败", error); } 
  finally { loading.value = false; }
};

const handleScroll = (e) => {
  const { scrollTop, scrollHeight, clientHeight } = e.target;
  if (scrollTop + clientHeight >= scrollHeight - 100) {
    fetchArtworks(false);
  }
};

const handleLike = async (img) => {
  if (!img) return;
  const originalLiked = img.isLiked;
  const originalLikes = img.likes || 0;
  img.isLiked = !originalLiked;
  img.likes = originalLiked ? originalLikes - 1 : originalLikes + 1;
  img.isAnimating = true;
  setTimeout(() => img.isAnimating = false, 500); 
  try {
    const res = await apiClient.post(`/Drawing/like/${img.id}`);
    if (res.data.success) {
      img.likes = res.data.likes;
      img.isLiked = res.data.isLiked;
    } else {
      img.isLiked = originalLiked;
      img.likes = originalLikes;
    }
  } catch (error) {
    img.isLiked = originalLiked;
    img.likes = originalLikes;
    if (error.response?.status === 401) alert("请先登录后再点赞");
  }
};

const triggerFileSelect = () => { fileInput.value.click(); };
const handleFileChange = (e) => {
  const file = e.target.files[0];
  if (!file) return;
  if (file.size > 20 * 1024 * 1024) return alert("文件过大");
  uploadForm.file = file;
  uploadForm.previewUrl = URL.createObjectURL(file);
};

const submitArtwork = async () => {
  if (!uploadForm.file) return alert("请选择图片");
  if (!uploadForm.title.trim()) return alert("请输入标题");
  isUploading.value = true;
  try {
    const formData = new FormData();
    formData.append('Image', uploadForm.file);
    formData.append('Title', uploadForm.title);
    if (uploadForm.desc) formData.append('Desc', uploadForm.desc);
    if (uploadForm.authorName) formData.append('AuthorName', uploadForm.authorName);
    const res = await apiClient.post('/Drawing/upload', formData, { headers: { 'Content-Type': 'multipart/form-data' } });
    if (res.data.success) {
      alert("发布成功");
      showUploadModal.value = false;
      uploadForm.title = ''; uploadForm.desc = ''; uploadForm.file = null; uploadForm.previewUrl = '';
      fetchArtworks(true);
      fetchTotalCount();
      fetchLeaderboard(); 
    } else alert(res.data.message);
  } catch (e) { alert("上传错误"); }
  finally { isUploading.value = false; }
};

const handleImgError = (e) => { e.target.src = '/土豆.jpg'; };
const openLightbox = (img) => { lightboxImg.value = img; };
const formatTime = (t) => t ? new Date(t).toLocaleDateString() : '';

onMounted(() => {
  fetchArtworks(true);
  fetchTotalCount();
  fetchLeaderboard(); 
  clockTimer = setInterval(() => { currentTime.value = new Date().toLocaleTimeString(); }, 1000);
});
onUnmounted(() => clearInterval(clockTimer));
</script>

<style scoped>
.channel-view {
  display: flex;
  flex-direction: column;
  height: 100%;
  width: 100%;
  overflow: hidden; 
}

/* 布局基础 */
.tc-dashboard {
  --bg-main: #f8fafc;
  --accent-purple: #8b5cf6;
  --accent-red: #ef4444;
  --accent-dark: #0f172a;
  --text-primary: #1e293b;
  --text-sub: #64748b;
  --border: #e2e8f0;
  width: 100%; height: 100%; background-color: var(--bg-main);
  display: flex; flex-direction: column; overflow: hidden; box-sizing: border-box;
}
* { box-sizing: border-box; }

.sys-info-bar { flex-shrink: 0; height: 36px; background: #fff; border-bottom: 1px solid var(--border); display: flex; align-items: center; padding: 0 25px; font-size: 11px; font-weight: bold; color: var(--text-sub); }
.main-bridge { flex: 1; display: flex; padding: 20px; gap: 20px; min-height: 0; width: 100%; max-width: 1920px; margin: 0 auto; box-sizing: border-box;}

/* 左侧 */
.left-column { width: 18%; display: flex; flex-direction: column; gap: 20px; }
.nav-menu { display: flex; flex-direction: column; gap: 5px; }
.nav-item { padding: 12px 15px; background: #fff; border-radius: 8px; border: 1px solid var(--border); cursor: pointer; display: flex; align-items: center; justify-content: space-between; font-size: 14px; color: var(--text-primary); transition: 0.2s; }
.nav-item:hover, .nav-item.active { background: var(--accent-dark); color: #fff; border-color: var(--accent-dark); }
.count-badge { font-size: 10px; background: #f1f5f9; color: var(--text-sub); padding: 2px 6px; border-radius: 4px; }
.nav-item.active .count-badge { background: rgba(255,255,255,0.2); color: #fff; }

.user-stat-box { display: flex; justify-content: space-around; background: #fff; padding: 15px; border-radius: 8px; border: 1px solid var(--border); }
.stat { display: flex; flex-direction: column; align-items: center; padding: 5px; transition: all 0.2s; border-radius: 6px; cursor: pointer; }
.stat:hover { background-color: #f1f5f9; transform: translateY(-2px); }
.stat:active { transform: translateY(0); }
.stat .num { font-weight: 800; font-size: 16px; color: var(--accent-purple); }
.stat .txt { font-size: 10px; color: var(--text-sub); }

/* 中间 */
.mid-column { width: 60%; display: flex; flex-direction: column; gap: 20px; min-height: 0; transition: width 0.3s ease; overflow: hidden; }
.mid-column.full-width { width: auto; flex: 1; }

.content-header { display: flex; 
  justify-content: space-between; 
  align-items: center; 
  flex-shrink: 0; 
  background: #fff; 
  padding: 15px 20px; 
  border-radius: 10px; 
  border: 1px solid var(--border);
  flex-wrap: wrap;
  gap: 15px; /* 增加Gap防止挤压 */
  min-width: 0;
}
.header-group-left {
  display: flex;
  align-items: center;
  gap: 20px;
  flex: 1;
  min-width: 0;
  flex-wrap: wrap;
}

.header-title h2 { margin: 0; font-size: 18px; font-weight: 900; letter-spacing: 1px; }
.header-title .subtitle { font-size: 12px; color: var(--text-sub); }
.upload-btn { background: var(--accent-purple); color: #fff; border: none; padding: 8px 20px; border-radius: 20px; font-weight: bold; cursor: pointer; display: flex; align-items: center; gap: 8px; transition: 0.2s; height: 36px; white-space: nowrap; flex-shrink: 0;}
.upload-btn:hover { background: #7c3aed; transform: translateY(-2px); box-shadow: 0 4px 12px rgba(139, 92, 246, 0.3); }

/* 滚动容器 */
.scroll-wrapper { flex: 1; overflow-y: auto; padding-right: 5px; }

/* 画廊 - 瀑布流 */
.loading-state, .list-footer { text-align: center; padding: 20px; color: var(--text-sub); font-size: 12px; }
.gallery-waterfall { column-count: 5; column-gap: 15px; padding-bottom: 20px; }
.art-card { break-inside: avoid; margin-bottom: 15px; border-radius: 8px; overflow: hidden; position: relative; cursor: pointer; border: 1px solid var(--border); background: #fff; transition: 0.2s; }
.art-card:hover { transform: translateY(-3px); box-shadow: 0 5px 15px rgba(0,0,0,0.1); }
.img-wrapper { width: 100%; min-height: 100px; background: #f3f4f6; }
.art-card img { width: 100%; display: block; height: auto; }
.art-overlay { padding: 10px; background: #fff; }
.art-title { margin: 0 0 5px 0; font-size: 13px; font-weight: bold; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.art-meta { font-size: 10px; display: flex; justify-content: space-between; color: var(--text-sub); }

/* 右侧 */
.right-column { width: 22%; display: flex; flex-direction: column; gap: 20px; }
.tags-panel, .top-contributors { background: #fff; border: 1px solid var(--border); border-radius: 10px; padding: 15px; }
.tag-cloud { display: flex; flex-wrap: wrap; gap: 8px; }
.tag { font-size: 11px; background: #f3f4f6; padding: 4px 8px; border-radius: 12px; color: var(--text-sub); cursor: pointer; }

/* 排行榜 */
.artist-row { display: flex; align-items: center; margin-bottom: 12px; font-size: 12px; padding-bottom: 8px; border-bottom: 1px dashed #f1f5f9; }
.artist-row:last-child { border-bottom: none; margin-bottom: 0; }
.rank-num { width: 24px; height: 24px; line-height: 24px; text-align: center; font-weight: 800; color: var(--text-sub); background: #f1f5f9; border-radius: 6px; margin-right: 10px; font-family: 'Segoe UI', sans-serif; }
.rank-1 { background: #fee2e2; color: #ef4444; }
.rank-2 { background: #ffedd5; color: #f97316; }
.rank-3 { background: #fef9c3; color: #eab308; }
.artist-info { flex: 1; display: flex; justify-content: space-between; align-items: center; }
.artist-name { font-weight: bold; color: var(--text-primary); max-width: 90px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.artist-stats { color: var(--accent-purple); font-weight: bold; font-size: 11px; background: #f5f3ff; padding: 2px 6px; border-radius: 10px; }
.empty-rank { text-align: center; color: var(--text-sub); font-size: 12px; padding: 20px 0; }

/* Modal & Lightbox */
.modal-overlay, .lightbox-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.8); z-index: 1000; display: flex; justify-content: center; align-items: center; }
.modal-container { background: #fff; width: 450px; border-radius: 12px; overflow: hidden; }
.modal-header { background: var(--accent-dark); color: #fff; padding: 15px; display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { margin: 0; font-size: 14px; letter-spacing: 1px; }
.modal-body { padding: 25px; display: flex; flex-direction: column; gap: 15px; }
.upload-area { border: 2px dashed #e2e8f0; border-radius: 8px; padding: 30px; text-align: center; position: relative; cursor: pointer; transition: 0.2s; min-height: 150px; display: flex; flex-direction: column; justify-content: center; align-items: center; }
.upload-area:hover { border-color: var(--accent-purple); background: #faf5ff; }
.upload-area.has-file { padding: 0; border: none; overflow: hidden; }
.preview-box { width: 100%; height: 200px; position: relative; }
.preview-box img { width: 100%; height: 100%; object-fit: contain; background: #000; }
.re-upload-tip { position: absolute; inset: 0; background: rgba(0,0,0,0.5); color: #fff; display: flex; justify-content: center; align-items: center; opacity: 0; transition: 0.2s; }
.preview-box:hover .re-upload-tip { opacity: 1; }
.placeholder-box i { font-size: 40px; color: #cbd5e1; margin-bottom: 10px; }
.placeholder-box p { margin: 0; font-weight: bold; color: var(--text-primary); }
.placeholder-box .sub-text { font-size: 12px; color: var(--text-sub); }
.form-group label { font-size: 12px; font-weight: bold; display: block; margin-bottom: 6px; color: var(--text-sub); }
.form-group input, .form-group textarea { width: 100%; padding: 10px; border: 1px solid #e2e8f0; border-radius: 6px; font-size: 14px; outline: none; transition: 0.2s; }
.form-group input:focus, .form-group textarea:focus { border-color: var(--accent-purple); }
.submit-btn { width: 100%; background: var(--accent-purple); color: #fff; padding: 12px; border: none; border-radius: 6px; font-weight: bold; cursor: pointer; margin-top: 10px; }
.submit-btn:disabled { background: #d1d5db; cursor: not-allowed; }
.lightbox-img { max-height: 85vh; max-width: 90vw; box-shadow: 0 0 30px rgba(0,0,0,0.5); border-radius: 4px; }
.lightbox-caption { position: absolute; bottom: 30px; left: 50%; transform: translateX(-50%); background: rgba(0,0,0,0.8); color: #fff; padding: 15px 25px; border-radius: 30px; text-align: center; backdrop-filter: blur(10px); min-width: 300px; }
.lightbox-caption h3 { margin: 0 0 5px 0; font-size: 16px; }
.lightbox-caption .desc { font-size: 13px; color: #ddd; margin: 0 0 10px 0; }
.lightbox-caption .meta { font-size: 12px; color: #aaa; display: flex; justify-content: space-between; gap: 20px; border-top: 1px solid rgba(255,255,255,0.1); padding-top: 15px; margin-top: 10px; }
.lightbox-close { position: absolute; top: 30px; right: 30px; background: none; border: none; color: #fff; font-size: 32px; cursor: pointer; filter: drop-shadow(0 0 5px rgba(0,0,0,0.5)); }

/* Like Action */
.action-bar { margin: 15px 0; display: flex; align-items: center; justify-content: center; }
.like-action-btn { background: rgba(255, 255, 255, 0.15); border: 1px solid rgba(255, 255, 255, 0.3); color: #fff; padding: 8px 24px; border-radius: 30px; cursor: pointer; display: flex; align-items: center; gap: 8px; font-size: 16px; font-weight: bold; transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275); backdrop-filter: blur(5px); outline: none; }
.like-action-btn:hover { background: rgba(255, 255, 255, 0.25); transform: scale(1.05); border-color: #fff; }
.like-action-btn.is-liked { background: rgba(239, 68, 68, 0.2); border-color: #ef4444; }
.like-action-btn.is-liked i { color: #ef4444; }
.like-action-btn.is-animating { transform: scale(0.9); }
.like-action-btn.is-animating i { transform: scale(1.4); }

/* Common */
.section-label { font-size: 10px; font-weight: 800; color: #94a3b8; letter-spacing: 1.5px; margin-bottom: 10px; }
.custom-scroll::-webkit-scrollbar { width: 4px; }
.custom-scroll::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 10px; }
.close-btn { background: none; border: none; color: inherit; cursor: pointer; }

/* 优化后的 Segment Group 
  - 移除了固定宽度 650px，改为 flex: 1 配合 max-width
*/
.segment-button-group {
  display: flex;
  align-items: center;
  gap: 0px; 
  height: 40px; 
  flex: 1; /* 占据剩余空间 */
  max-width: 650px; /* 最大限制，防止在大屏过宽 */
  min-width: 250px; /* 最小保证 */
  margin: 0 1px;
}

.button-segment  {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 10%;
  height: 100%;
  font-size: 16px;
  border-radius: 10px;
  color: #333;
  transition: background-color 0.2s ease, width 0.5s ease-out; /* 统一动画属性 */
  cursor: pointer;
  flex-wrap: wrap;
  flex: 1 1 auto;
  min-width: 60px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.button-segment.hoverable:hover {
  background-color: #f3f4f6; 
  width: 25%;
  flex-grow: 2; /* 弹性增长 */
}

.button-segment.active {
  width: 50%;
  flex-grow: 4; /* 弹性增长权重更大 */
  background-color: #1b212e; 
  color: #fff; 
}

.button-segment.active.hoverable:hover {
  width: 55%;
  background-color: #2f313f; 
}

/* --- 1080P / 1K 屏幕专属优化 --- */
@media screen and (max-width: 1920px) {
  .tc-dashboard {
    font-size: 13px;
    zoom: 0.75; /* 稍微调大一点点zoom，靠增加列数来让卡片变小，视觉会更清晰 */
    padding: 15px;
    gap: 15px;
  }

  .content-header {
    padding: 8px 12px;
  }

  .segment-button-group {
    height: 32px; /* 按钮高度再压扁一点 */
    font-size: 13px;
  }
  
  .button-segment {
     font-size: 13px;
  }

  /* 核心修改：将列数从 4 改为 6，卡片瞬间变小变精致 */
  .gallery-waterfall {
    column-count: 6; 
    column-gap: 12px; /* 间隙也调小一点 */
  }

  /* 针对卡片内部的微调，防止卡片变小了文字却很大 */
  .art-card { 
    margin-bottom: 10px; 
    border-radius: 6px; /* 圆角也缩小一点 */
  }
  
  .art-overlay {
    padding: 8px !important; /* 减小卡片内部留白 */
  }

  .art-title {
    font-size: 12px !important; /* 标题字号调小 */
    margin-bottom: 3px !important;
  }

  .art-meta {
    font-size: 9px !important; /* 底部信息字号极小化 */
  }

  /* 侧边栏保持固定宽度，防止被挤压 */
  .left-column { width: 220px; flex-shrink: 0; } 
  .right-column { width: 260px; flex-shrink: 0; } 
  .mid-column { flex: 1; } 
}

</style>