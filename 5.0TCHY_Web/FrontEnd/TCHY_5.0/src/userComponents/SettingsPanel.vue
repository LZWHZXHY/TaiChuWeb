<template>
  <div class="settings-panel">
    <h3>账户设置</h3>

    <div v-if="isMe" class="setting-content">
      <a-spin :spinning="loading">
        <a-form layout="vertical" :model="formState" class="user-form">
          
          <div class="avatar-setting">
            <a-upload
              name="file"
              class="avatar-uploader"
              :show-upload-list="false"
              :before-upload="beforeAvatarUpload"
              :custom-request="handleUploadAvatar"
              accept="image/png,image/jpeg,image/gif,image/webp"
            >
              <div class="avatar-wrapper">
                <a-avatar :size="80" :src="getAvatarUrl(formState.avatar)">
                  {{ formState.username?.[0]?.toUpperCase() }}
                </a-avatar>
                
                <div class="upload-mask">
                  <div v-if="uploading">
                    <loading-outlined />
                  </div>
                  <div v-else>
                    <CameraOutlined /><span>更换</span>
                  </div>
                </div>
              </div>
            </a-upload>
          </div>

          <div class="section-title">基础信息</div>
          <a-row :gutter="16">
            <a-col :span="12">
              <a-form-item label="昵称" tooltip="修改昵称需要消耗改名卡">
                <div class="input-with-action">
                  <a-input v-model:value="formState.username" disabled />
                  <a-button type="dashed" @click="handleRenameModal"><EditOutlined /> 改名</a-button>
                </div>
              </a-form-item>
            </a-col>
            <a-col :span="12">
              <a-form-item label="性别">
                <a-select v-model:value="formState.gender">
                  <a-select-option :value="1">♂ 男</a-select-option>
                  <a-select-option :value="2">♀ 女</a-select-option>
                  <a-select-option :value="0">👽 保密</a-select-option>
                </a-select>
              </a-form-item>
            </a-col>
          </a-row>

          <a-form-item label="联系方式">
            <div class="privacy-input-group">
              <a-input v-model:value="formState.contact" placeholder="填写您的邮箱或联系方式" />
              <a-tooltip :title="formState.isContactPublic ? '当前：公开可见' : '当前：仅自己可见'">
                <a-button 
                  :type="formState.isContactPublic ? 'primary' : 'default'" 
                  shape="circle" 
                  class="privacy-btn"
                  @click="formState.isContactPublic = !formState.isContactPublic"
                >
                  <EyeOutlined v-if="formState.isContactPublic" />
                  <EyeInvisibleOutlined v-else />
                </a-button>
              </a-tooltip>
            </div>
          </a-form-item>

          <div class="section-title">详细资料</div>
          
          <a-form-item label="出生日期" help="自动计算星座与生肖">
            <div class="privacy-input-group">
              <a-date-picker 
                v-model:value="formState.birthday" 
                style="width: 100%" 
                format="YYYY-MM-DD"
                :disabled-date="disabledDate"
                @change="handleBirthdayChange"
              />
              <a-tooltip :title="formState.isBirthdayPublic ? '生日：公开可见' : '生日：仅自己可见'">
                <a-button 
                  :type="formState.isBirthdayPublic ? 'primary' : 'default'" 
                  shape="circle" 
                  class="privacy-btn"
                  @click="formState.isBirthdayPublic = !formState.isBirthdayPublic"
                >
                  <EyeOutlined v-if="formState.isBirthdayPublic" />
                  <EyeInvisibleOutlined v-else />
                </a-button>
              </a-tooltip>
            </div>
          </a-form-item>

          <a-row :gutter="16">
            <a-col :span="8">
              <a-form-item label="年龄"><a-input v-model:value="formState.age" readonly class="readonly-field" /></a-form-item>
            </a-col>
            <a-col :span="8">
              <a-form-item label="星座"><a-input v-model:value="formState.zodiac" readonly class="readonly-field" /></a-form-item>
            </a-col>
            <a-col :span="8">
              <a-form-item label="生肖"><a-input v-model:value="formState.chineseZodiac" readonly class="readonly-field" /></a-form-item>
            </a-col>
          </a-row>

          <div class="section-title">状态设置</div>
          
          <a-row :gutter="16">
            <a-col :span="24">
              <a-form-item label="当前状态与心情">
                <div class="privacy-input-group">
                  <a-select v-model:value="formState.lifeStatus" placeholder="状态" style="width: 140px">
                    <a-select-option value="learning">📚 学习中</a-select-option>
                    <a-select-option value="working">💼 工作中</a-select-option>
                    <a-select-option value="fishing">🐟 摸鱼中</a-select-option>
                    <a-select-option value="emo">🌧️ EMO了</a-select-option>
                    <a-select-option value="happy">🎉 开心</a-select-option>
                  </a-select>
                  <a-input v-model:value="formState.mood" placeholder="一句话描述心情..." style="flex: 1" />
                  
                  <a-tooltip :title="formState.isStatusPublic ? '状态：公开可见' : '状态：仅自己可见'">
                    <a-button 
                      :type="formState.isStatusPublic ? 'primary' : 'default'" 
                      shape="circle" 
                      class="privacy-btn"
                      @click="formState.isStatusPublic = !formState.isStatusPublic"
                    >
                      <EyeOutlined v-if="formState.isStatusPublic" />
                      <EyeInvisibleOutlined v-else />
                    </a-button>
                  </a-tooltip>
                </div>
              </a-form-item>
            </a-col>
          </a-row>

          <a-form-item label="个性签名">
            <a-textarea 
              v-model:value="formState.title" 
              placeholder="介绍一下你自己..." 
              :rows="3" show-count :maxlength="200" 
            />
          </a-form-item>

          <div class="form-actions">
            <a-button type="primary" size="large" @click="saveChanges" :loading="saving">保存配置</a-button>
          </div>
        </a-form>
      </a-spin>
    </div>
    <div v-else class="not-allowed"><a-empty description="无权访问" /></div>
  </div>
</template>

<script setup>
import { reactive, ref, watch } from 'vue';
import { CameraOutlined, EditOutlined, EyeOutlined, EyeInvisibleOutlined, LoadingOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import apiClient from '@/utils/api';
import dayjs from 'dayjs';

const props = defineProps({
  user: { type: Object, default: () => ({}) },
  isMe: Boolean
});

const loading = ref(false);
const saving = ref(false);
const uploading = ref(false);
const defaultAvatar = '/default-avatar.png'; 
const BASE_URL = 'https://bianyuzhou.com'; 

const formState = reactive({
  username: '', 
  avatar: '',   
  gender: 0,    
  title: '',    
  contact: '',  
  isContactPublic: false,
  birthday: null, 
  isBirthdayPublic: false,
  age: '',        
  zodiac: '',     
  chineseZodiac: '', 
  lifeStatus: undefined, 
  mood: '',       
  isStatusPublic: true,
});

// --- 初始化数据 ---
watch(() => props.user, (newUser) => {
  if (newUser) {
    formState.username = newUser.username || '';
    formState.avatar = newUser.logo || ''; 
    formState.gender = newUser.gender || 0;
    formState.title = newUser.title || '';
    formState.contact = newUser.contact || '';
    formState.isContactPublic = !!newUser.isContactPublic;
    formState.lifeStatus = newUser.lifeStatus || undefined;
    formState.mood = newUser.mood || '';
    formState.isStatusPublic = newUser.isStatusPublic !== false;

    if (newUser.birthday) {
      formState.birthday = dayjs(newUser.birthday);
      formState.isBirthdayPublic = !!newUser.isBirthdayPublic;
      handleBirthdayChange(formState.birthday);
    }
  }
}, { immediate: true, deep: true });

// --- Improved getAvatarUrl ---
const getAvatarUrl = (path) => {
  if (!path) return defaultAvatar;
  
  if (path.startsWith('http')) return path;
  
  let cleanPath = path.startsWith('/') ? path.substring(1) : path;
  
  // Ensure it starts with uploads/ if it's a relative path from the backend logic
  if (!cleanPath.startsWith('uploads/')) {
    cleanPath = 'uploads/' + cleanPath;
  }
  
  // Add timestamp to defeat browser caching
  return `${BASE_URL}/${cleanPath}?t=${new Date().getTime()}`;
};

// --- Utils ---
const disabledDate = (current) => current && current > dayjs().endOf('day');
const getZodiac = (month, day) => {
  const signs = ["摩羯座", "水瓶座", "双鱼座", "白羊座", "金牛座", "双子座", "巨蟹座", "狮子座", "处女座", "天秤座", "天蝎座", "射手座"];
  const startDay = [20, 19, 21, 20, 21, 22, 23, 23, 23, 24, 23, 22];
  return day < startDay[month - 1] ? signs[month - 1] : signs[month];
};
const getChineseZodiac = (year) => {
  const animals = ["猴", "鸡", "狗", "猪", "鼠", "牛", "虎", "兔", "龙", "蛇", "马", "羊"];
  return animals[year % 12];
};
const handleBirthdayChange = (date) => {
  if (!date) {
    formState.age = ''; formState.zodiac = ''; formState.chineseZodiac = ''; return;
  }
  const d = dayjs(date);
  formState.age = dayjs().diff(d, 'year');
  formState.zodiac = getZodiac(d.month() + 1, d.date());
  formState.chineseZodiac = getChineseZodiac(d.year());
};

// --- Upload Logic ---
const beforeAvatarUpload = (file) => {
  const isImage = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'].includes(file.type);
  if (!isImage) {
    message.error('只能上传 JPG/PNG/GIF/WEBP 格式的图片!');
    return false;
  }
  const isLt5M = file.size / 1024 / 1024 < 5;
  if (!isLt5M) {
    message.error('图片大小不能超过 5MB!');
    return false;
  }
  return true;
};

// --- Updated Upload Handler ---
const handleUploadAvatar = async ({ file, onSuccess, onError }) => {
  uploading.value = true;
  const formData = new FormData();
  formData.append('file', file);

  try {
    const res = await apiClient.post('/UserDetail/UploadAvatar', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });

    if (res.data.success) {
      const { relativePath } = res.data.data;
      
      // Update form state immediately so the avatar refreshes in UI
      formState.avatar = relativePath; 
      
      message.success('头像上传成功');
      onSuccess(res.data);
    } else {
      message.error(res.data.message || '上传失败');
      onError();
    }
  } catch (error) {
    console.error(error);
    message.error('上传出错，请稍后重试');
    onError();
  } finally {
    uploading.value = false;
  }
};

const handleRenameModal = () => { /* Rename logic */ };


// --- Save Changes ---
const saveChanges = async () => {
  saving.value = true;
  try {
    const payload = {
      logo: formState.avatar,
      // 注意：只有你后端 UpdateUserProfileDto 里定义了的字段，并且数据库里有的字段，才会被保存。
      // 如果你刚才只选了"方案一"（不改数据库），那么 gender, birthday 等字段虽然传过去了，但后端会忽略它们。
      gender: formState.gender,
      title: formState.title,
      contact: formState.contact,
      isContactPublic: formState.isContactPublic,
      birthday: formState.birthday ? formState.birthday.format('YYYY-MM-DD') : null,
      isBirthdayPublic: formState.isBirthdayPublic,
      lifeStatus: formState.lifeStatus,
      mood: formState.mood,
      isStatusPublic: formState.isStatusPublic,
      // age, zodiac, chineseZodiac 通常是前端算出来的，后端如果不存这些字段，传了也没用，但传了也不报错。
      age: formState.age, 
      zodiac: formState.zodiac,
      chineseZodiac: formState.chineseZodiac
    };

    // 👇👇👇【核心修改在这里】👇👇👇
    // 把原来的 '/Userinfo/update' 改为 '/UserDetail/update'
    const res = await apiClient.post('/UserDetail/update', payload);
    
    if (res.data.success) {
      message.success('个人资料已更新！');
      // 可以选择在这里刷新一下页面或者重新获取用户信息，确保数据同步
      // location.reload(); 
    }
  } catch (error) {
    console.error(error); // 建议打印错误以便调试
    message.error('保存失败');
  } finally {
    saving.value = false;
  }
};
</script>

<style scoped>
.settings-panel { padding: 24px; background: #fff; border-radius: 12px; }
.setting-content { max-width: 680px; margin: 0 auto; }
.section-title { font-size: 14px; font-weight: 600; color: #1890ff; margin: 24px 0 16px; padding-bottom: 8px; border-bottom: 1px dashed #e8e8e8; }
.privacy-input-group { display: flex; align-items: center; gap: 10px; }
.privacy-btn { flex-shrink: 0; }
.input-with-action { display: flex; gap: 8px; }

/* Avatar Styles */
.avatar-setting { display: flex; flex-direction: column; align-items: center; margin-bottom: 20px; }
.avatar-uploader { display: block; cursor: pointer; }
.avatar-wrapper { position: relative; width: 88px; height: 88px; border-radius: 50%; overflow: hidden; border: 3px solid #f0f0f0; transition: transform 0.2s; }
.avatar-wrapper:hover { transform: scale(1.05); }

/* Upload Mask */
.upload-mask { 
  position: absolute; top: 0; left: 0; width: 100%; height: 100%; 
  background: rgba(0,0,0,0.5); color: white; 
  display: flex; flex-direction: column; justify-content: center; align-items: center; 
  opacity: 0; transition: opacity 0.3s; 
  pointer-events: none; 
}
.avatar-wrapper:hover .upload-mask { opacity: 1; }

.disabled-input { color: #666; background-color: #f9f9f9; cursor: not-allowed; }
.readonly-field { background-color: #fff; color: #333; border-color: #d9d9d9; }
.form-actions { margin-top: 40px; text-align: center; padding-top: 20px; border-top: 1px solid #f0f0f0; }
.not-allowed { padding: 60px 0; text-align: center; }
</style>