<template>
    <div class="operation-panel">
        <div class="search-container">
            <h2>添加好友</h2>
            <div class="search-box">
                <input 
                    v-model="searchId" 
                    type="text" 
                    placeholder="输入用户ID进行搜索" 
                    @keyup.enter="searchUser"
                />
                <button @click="searchUser">
                    <i class="search-icon">🔍</i> 搜索
                </button>
            </div>
            
            <div v-if="searchResult" class="search-result">
                <div class="user-card">
                    <div class="avatar-placeholder"></div>
                    <div class="user-info">
                        <h3>{{ searchResult.username }}</h3>
                        <p>ID: {{ searchResult.id }}</p>
                    </div>
                    <button 
                        class="add-btn" 
                        @click="addFriend"
                        :disabled="isFriend"
                    >
                        {{ isFriend ? '已是好友' : '添加好友' }}
                    </button>
                </div>
            </div>
            
            <div v-if="errorMessage" class="error-message">
                {{ errorMessage }}
            </div>
            
            <div v-if="successMessage" class="success-message">
                {{ successMessage }}
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { useUserStore } from '../../store/user';
import axios from 'axios';
import { useRouter } from 'vue-router';

const userStore = useUserStore();
const router = useRouter();

const baseApiUrl = import.meta.env.DEV
  ? "https://localhost:44321"
  : "https://bianyuzhou.com";

const searchId = ref('');
const searchResult = ref(null);
const errorMessage = ref('');
const successMessage = ref('');

const isFriend = computed(() => {
    if (!searchResult.value) return false;
    return userStore.friends.some(friend => friend.id === searchResult.value.id);
});

const searchUser = async () => {
    if (!searchId.value.trim()) {
        errorMessage.value = '请输入用户ID';
        return;
    }
    
    try {
        const response = await axios.get(`${baseApiUrl}/api/UserInfo/SearchId/${searchId.value}`);
        searchResult.value = response.data;
        errorMessage.value = '';
    } catch (error) {
        searchResult.value = null;
        if (error.response && error.response.status === 404) {
            errorMessage.value = '未找到该用户';
        } else {
            errorMessage.value = '搜索失败，请稍后再试';
        }
    }
};

const addFriend = async () => {
    if (!searchResult.value) return;
    
    try {
        await axios.post(`${baseApiUrl}/api/friends`, {
            friendId: searchResult.value.id
        });
        
        // 更新本地好友列表
        userStore.addFriend(searchResult.value);
        
        successMessage.value = '好友添加成功';
        errorMessage.value = '';
        
        // 3秒后清除成功消息
        setTimeout(() => {
            successMessage.value = '';
        }, 3000);
    } catch (error) {
        successMessage.value = '';
        if (error.response && error.response.status === 400) {
            errorMessage.value = '已经是好友关系';
        } else {
            errorMessage.value = '添加好友失败，请稍后再试';
        }
    }
};
</script>

<style scoped>
.operation-panel {
    max-width: 600px;
    margin: 0 auto;
    padding: 20px;
    background-color: #fff;
    border-radius: 10px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.search-container {
    display: flex;
    flex-direction: column;
    gap: 20px;
}

h2 {
    color: #333;
    text-align: center;
    margin-bottom: 20px;
}

.search-box {
    display: flex;
    gap: 10px;
}

.search-box input {
    flex: 1;
    padding: 10px 15px;
    border: 1px solid #ddd;
    border-radius: 5px;
    font-size: 16px;
}

.search-box button {
    padding: 10px 20px;
    background-color: #4CAF50;
    color: white;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    font-size: 16px;
    transition: background-color 0.3s;
}

.search-box button:hover {
    background-color: #45a049;
}

.search-icon {
    margin-right: 5px;
}

.search-result {
    margin-top: 20px;
}

.user-card {
    display: flex;
    align-items: center;
    padding: 15px;
    background-color: #f9f9f9;
    border-radius: 8px;
    gap: 15px;
}

.avatar-placeholder {
    width: 50px;
    height: 50px;
    background-color: #ddd;
    border-radius: 50%;
}

.user-info {
    flex: 1;
}

.user-info h3 {
    margin: 0;
    color: #333;
}

.user-info p {
    margin: 5px 0 0;
    color: #666;
    font-size: 14px;
}

.add-btn {
    padding: 8px 15px;
    background-color: #2196F3;
    color: white;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    transition: background-color 0.3s;
}

.add-btn:hover:not(:disabled) {
    background-color: #0b7dda;
}

.add-btn:disabled {
    background-color: #cccccc;
    cursor: not-allowed;
}

.error-message {
    color: #f44336;
    padding: 10px;
    background-color: #ffebee;
    border-radius: 5px;
    text-align: center;
}

.success-message {
    color: #4CAF50;
    padding: 10px;
    background-color: #e8f5e9;
    border-radius: 5px;
    text-align: center;
}
</style>