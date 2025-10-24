<template>
    <div class="Big-container">
        <div class="Topbar">
            <div class="Username">{{ username }}</div>
        </div>
        <div class="bodyContainer">
            <div class="infoPanel">
                <ul>
                    <li>
                        <label>
                            <div class="userLogo" @click="openAvatarModal">
                                <img 
                                    v-if="avatarUrl" 
                                    :src="avatarUrl" 
                                    alt="用户头像" 
                                    class="avatar-image" 
                                    @error="handleAvatarError" 
                                />
                                <span v-else>头像</span>
                            </div>
                        </label>
                    </li>
                    <li><label>{{ username }}</label></li>
                    <li><label>等级：{{ userLevel }}</label></li>
                    <li><label>当前经验: {{ userExp }} / {{ nextLevel }}</label></li>
                    <li><label>头衔：{{ userTitle }}</label></li>
                    <li><label>活跃排行：无</label></li>
                    <li><label>间隙粒子量：{{ userPoints }}</label></li>
                    <li><label>账户ID：{{ userID }}</label></li>
                    
                         
                </ul>
            </div>

            <div class="view_window">
                <router-view></router-view>
            </div>

            <div class="functionPanel">
                <ul>
                    <li><button><router-link to='/daka'>签到</router-link></button></li>
                    <li><button><router-link to='/myPost'>我的帖子</router-link></button></li>
                    <li><button><router-link to='/inventory'>仓库</router-link></button></li>
                    <li><button>活动</button></li>
                    <li><button>私信</button></li>
                    <li><button><router-link to='/myFriends'>好友</router-link></button></li>
                    <li><button>黑名单</button></li>
                    <li><button>设置</button></li>
                    <li>
                      <button @click="logout">登出</button>
                    </li>
                </ul>
            </div>
        </div>
        
        <!-- 头像上传模态框 -->
        <div v-if="isAvatarModalOpen" class="modal-overlay" @click="closeAvatarModal">
            <div class="modal-container" @click.stop>
                <div class="modal-header">
                    <h3>更换头像</h3>
                    <button class="close-button" @click="closeAvatarModal">&times;</button>
                </div>
                <div class="modal-body">
                    <div class="avatar-preview">
                        <img v-if="previewUrl" :src="previewUrl" alt="头像预览" />
                        <div v-else class="placeholder">预览区域</div>
                    </div>
                    <div class="upload-section">
                        <label for="avatar-upload" class="upload-button">
                            选择图片
                            <input 
                                type="file" 
                                id="avatar-upload" 
                                accept="image/*" 
                                @change="handleFileChange" 
                            />
                        </label>
                        <p class="hint">支持 JPG、PNG 格式，文件大小不超过 2MB</p>
                    </div>
                    <!-- 错误信息显示 -->
                    <div v-if="uploadError" class="error-message">
                        {{ uploadError }}
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="cancel-button" @click="closeAvatarModal">取消</button>
                    <button class="confirm-button" @click="handleUpload" :disabled="!fileSelected || isLoading">
                        {{ isLoading ? '上传中...' : '保存' }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { computed, ref, onMounted } from 'vue';
import { useUserStore } from '../../store/user';
import axios from 'axios';
import { useRouter } from 'vue-router';

const router = useRouter();

// 根据环境设置基础URL
const baseApiUrl = import.meta.env.DEV 
  ? 'https://localhost:44321' 
  : 'https://bianyuzhou.com';

const userStore = useUserStore();
const username = computed(() => userStore.user ? userStore.user.username : '');
const userLevel = computed(() => userStore.user ? userStore.user.level : 0);
const userExp = computed(() => userStore.user ? userStore.user.exp : 0);
const nextLevel = computed(() => userStore.nextLevelExp);
const userTitle = computed(() => userStore.user ? userStore.user.title : '暂未拥有头衔');
const userPoints = computed(() => userStore.user ? userStore.user.points : 0);
const userID = computed(() => userStore.user ? userStore.user.id : 0);

const avatarUrl = ref(''); 
const isAvatarModalOpen = ref(false);
const previewUrl = ref('');
const selectedFile = ref(null);
const fileSelected = ref(false);
const isLoading = ref(false);
const uploadError = ref('');

const openAvatarModal = () => {
  isAvatarModalOpen.value = true;
  uploadError.value = '';
};

const closeAvatarModal = () => {
  isAvatarModalOpen.value = false;
  previewUrl.value = '';
  selectedFile.value = null;
  fileSelected.value = false;
  uploadError.value = '';
};

const handleFileChange = (event) => {
  const file = event.target.files[0];
  if (file) {
    const validTypes = ['image/jpeg', 'image/png', 'image/jpg', 'image/webp'];
    const maxSize = 2 * 1024 * 1024; // 2MB限制

    if (!validTypes.includes(file.type)) {
      uploadError.value = '请上传 JPG, PNG 或 WEBP 格式的图片';
      return;
    }
    if (file.size > maxSize) {
      uploadError.value = '图片大小不能超过 2MB';
      return;
    }

    selectedFile.value = file;
    fileSelected.value = true;
    uploadError.value = '';

    const reader = new FileReader();
    reader.onload = (e) => {
      previewUrl.value = e.target.result;
    };
    reader.readAsDataURL(file);
  }
};

const handleUpload = async () => {
  if (!selectedFile.value) {
    uploadError.value = '请选择要上传的头像文件';
    return;
  }
  
  try {
    isLoading.value = true;
    uploadError.value = '';
    
    const formData = new FormData();
    formData.append('file', selectedFile.value);

    const userId = userStore.user?.id;
    if (!userId) {
      uploadError.value = '无法获取用户ID，请重新登录';
      return;
    }

    console.log('开始上传头像:', selectedFile.value.name, selectedFile.value.size);
    
    const response = await axios.post(
      `${baseApiUrl}/api/UserInfo/UploadAvatar?userId=${userId}`, 
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
          'Authorization': `Bearer ${localStorage.getItem('token') || ''}`
        },
        timeout: 30000 // 增加超时时间
      }
    );

    console.log('上传响应:', response);
    
    const result = response.data;
    if (result.FileUrl) {
      // 添加时间戳避免缓存
      const timestamp = new Date().getTime();
      avatarUrl.value = `${baseApiUrl}${result.FileUrl}&t=${timestamp}`;
      
      // 更新用户状态
      if (userStore.updateUser) {
        userStore.updateUser({ 
          ...userStore.user, 
          logo: result.FileUrl 
        });
      } else {
        userStore.user.logo = result.FileUrl;
      }
      
      // 关闭模态框
      closeAvatarModal();
    } else {
      uploadError.value = result.message || '头像上传失败';
    }
  } catch (error) {
    console.error('上传错误:', error);
    
    if (error.response) {
      uploadError.value = error.response.data?.message || 
                         `服务器错误: ${error.response.status} ${error.response.statusText}`;
    } else if (error.request) {
      uploadError.value = '网络连接错误，请检查您的网络';
    } else {
      uploadError.value = error.message || '头像上传过程中发生未知错误';
    }
  } finally {
    isLoading.value = false;
  }
};

const handleAvatarError = (event) => {
  console.error('头像加载失败:', event.target.src);
  
  // 使用默认头像并添加时间戳避免缓存
  const timestamp = new Date().getTime();
  avatarUrl.value = `${baseApiUrl}/api/UserInfo/imageproxy?path=default-avatar&t=${timestamp}`;
};

onMounted(() => {
  // 从后端获取用户信息
  if (userID.value) {
    axios.get(`${baseApiUrl}/api/UserInfo?userId=${userID.value}`)
      .then(response => {
        const userData = response.data;
        const timestamp = new Date().getTime();
        
        if (userData.logo) {
          // 处理头像URL并添加时间戳
          avatarUrl.value = `${baseApiUrl}${userData.logo}&t=${timestamp}`;
        } else {
          // 使用默认头像
          avatarUrl.value = `${baseApiUrl}/api/UserInfo/imageproxy?path=default-avatar&t=${timestamp}`;
        }
      })
      .catch(() => {
        const timestamp = new Date().getTime();
        avatarUrl.value = `${baseApiUrl}/api/UserInfo/imageproxy?path=default-avatar&t=${timestamp}`;
      });
  } else {
    const timestamp = new Date().getTime();
    avatarUrl.value = `${baseApiUrl}/api/UserInfo/imageproxy?path=default-avatar&t=${timestamp}`;
  }
});



const logout = () => {
  localStorage.removeItem('token');
  sessionStorage.removeItem('token');
  if (userStore.resetUser) {
    userStore.resetUser();
  } else {
    userStore.user = null;
  }
  // 跳转到登录页（index.html 或 login.html）
  window.location.href = '/index.html'; // 或 '/login.html'
};



</script>

<style scoped>
@import url('/static/css/user.css');
</style>