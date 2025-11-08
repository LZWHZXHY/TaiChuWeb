<template>
    <div class="notification-container">
        <!-- 页面标题 -->
        <div class="notification-header">
            <h2>我的通知</h2>
            <div class="notification-actions">
                <button class="action-btn mark-all-read" @click="markAllAsRead">全部标记为已读</button>
                <button class="action-btn clear-all" @click="clearAllNotifications">清空所有通知</button>
            </div>
        </div>

        <!-- 通知筛选标签 -->
        <div class="notification-tabs">
            <button 
                v-for="tab in tabs" 
                :key="tab.id"
                class="tab" 
                :class="{ active: activeTab === tab.id }"
                @click="switchTab(tab.id)"
            >
                {{ tab.name }}
                <span v-if="tab.id === 'unread'" class="badge">{{ unreadCount }}</span>
            </button>
        </div>

        <!-- 通知统计 -->
        <div class="notification-stats" v-if="activeTab === 'all'">
            <div class="stat-item">
                <span class="stat-label">未读通知:</span>
                <span class="stat-value">{{ unreadCount }}</span>
            </div>
            <div class="stat-item">
                <span class="stat-label">总计通知:</span>
                <span class="stat-value">{{ totalCount }}</span>
            </div>
        </div>

        <!-- 空状态提示 -->
        <div v-if="filteredNotifications.length === 0" class="empty-state">
            <div class="empty-icon">📭</div>
            <p>暂无通知</p>
        </div>

        <!-- 通知列表 -->
        <div v-else class="notification-list">
            <!-- 通知项 -->
            <div 
                v-for="notification in filteredNotifications" 
                :key="notification.id"
                class="notification-item" 
                :class="{ unread: !notification.isRead, expired: notification.isExpired }"
                @click="viewNotificationDetail(notification)"
            >
                <div class="notification-icon" :class="getNotificationTypeClass(notification.type)">
                    <span>{{ getTypeIcon(notification.type) }}</span>
                </div>
                <div class="notification-content">
                    <div class="notification-header">
                        <h3 class="notification-title">{{ notification.title }}</h3>
                        <span class="notification-badge" :class="getTypeBadgeClass(notification.type)">
                            {{ getTypeName(notification.type) }}
                        </span>
                    </div>
                    <p class="notification-text">{{ notification.content }}</p>
                    
                    <!-- 好友请求操作按钮 -->
                    <div v-if="notification.type === 3" class="friend-request-actions">
                        <button class="accept-btn" @click.stop="handleFriendRequest(notification, 'accept')">
                            <i class="fas fa-check"></i> 同意
                        </button>
                        <button class="reject-btn" @click.stop="handleFriendRequest(notification, 'reject')">
                            <i class="fas fa-times"></i> 拒绝
                        </button>
                    </div>

                    <!-- 通知元信息 -->
                    <div class="notification-meta">
                        <span class="notification-time">{{ formatTime(notification.createTime) }}</span>
                        <span v-if="notification.senderName && notification.senderId !== 1" class="notification-sender">
                            来自: {{ notification.senderName }}
                        </span>
                        <span v-if="!notification.isRead" class="unread-dot"></span>
                    </div>
                </div>
                <div class="notification-actions">
                    <button 
                        v-if="!notification.isRead" 
                        class="read-btn" 
                        @click.stop="markAsRead(notification.id)"
                        title="标记为已读"
                    >
                        <i class="fas fa-check"></i>
                    </button>
                    <button class="delete-btn" @click.stop="deleteNotification(notification.id)" title="删除">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
        </div>

        <!-- 分页控件 -->
        <div v-if="totalPages > 1" class="notification-pagination">
            <button 
                class="page-btn" 
                :class="{ disabled: currentPage === 1 }"
                @click="goToPage(currentPage - 1)"
            >
                <i class="fas fa-chevron-left"></i> 上一页
            </button>
            
            <div class="page-info">
                第 {{ currentPage }} 页，共 {{ totalPages }} 页
            </div>
            
            <button 
                class="page-btn" 
                :class="{ disabled: currentPage === totalPages }"
                @click="goToPage(currentPage + 1)"
            >
                下一页 <i class="fas fa-chevron-right"></i>
            </button>
        </div>

        <!-- 通知详情模态框 -->
        <div v-if="selectedNotification" class="notification-modal" @click="closeModal">
            <div class="modal-content" @click.stop>
                <div class="modal-header">
                    <h3>{{ selectedNotification.title }}</h3>
                    <button class="close-btn" @click="closeModal">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="modal-body">
                    <div class="notification-detail">
                        <div class="detail-icon" :class="getNotificationTypeClass(selectedNotification.type)">
                            <span>{{ getTypeIcon(selectedNotification.type) }}</span>
                        </div>
                        <div class="detail-content">
                            <p>{{ selectedNotification.content }}</p>
                            <div class="detail-meta">
                                <div class="meta-item">
                                    <i class="fas fa-clock"></i>
                                    <span>发送时间: {{ formatTime(selectedNotification.createTime) }}</span>
                                </div>
                                <div v-if="selectedNotification.senderName && selectedNotification.senderId !== 1" class="meta-item">
                                    <i class="fas fa-user"></i>
                                    <span>发送者: {{ selectedNotification.senderName }}</span>
                                </div>
                                <div v-if="selectedNotification.readTime" class="meta-item">
                                    <i class="fas fa-check"></i>
                                    <span>阅读时间: {{ formatTime(selectedNotification.readTime) }}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button v-if="!selectedNotification.isRead" class="action-btn primary" @click="markAsRead(selectedNotification.id); closeModal()">
                        标记为已读
                    </button>
                    <button class="action-btn" @click="closeModal">关闭</button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import request from '../../../server/UserInfoApi';

// 响应式数据
const activeTab = ref('all');
const currentPage = ref(1);
const itemsPerPage = ref(10);
const notifications = ref([]);
const selectedNotification = ref(null);
const unreadCount = ref(0);
const totalCount = ref(0);
const totalPages = ref(0);

// 标签页配置
const tabs = ref([
    { id: 'all', name: '全部' },
    { id: 'unread', name: '未读' },
    { id: 'friend', name: '好友请求' },
    { id: 'system', name: '系统通知' },
    { id: 'interaction', name: '互动通知' }
]);

// 通知类型映射
const notificationTypes = {
    1: { name: '系统通知', icon: '🔔', class: 'system' },
    2: { name: '活动通知', icon: '🎯', class: 'activity' },
    3: { name: '好友请求', icon: '👥', class: 'friend-request' },
    4: { name: '私信通知', icon: '💬', class: 'message' },
    5: { name: '帖子回复', icon: '📝', class: 'post-reply' },
    6: { name: '评论回复', icon: '↩️', class: 'comment-reply' },
    7: { name: '帖子点赞', icon: '👍', class: 'post-like' },
    8: { name: '评论点赞', icon: '❤️', class: 'comment-like' },
    9: { name: '关注通知', icon: '➕', class: 'follow' },
    10: { name: '积分变动', icon: '💰', class: 'points' },
    11: { name: '等级提升', icon: '⭐', class: 'level-up' },
    12: { name: '奖励发放', icon: '🎁', class: 'award' },
    13: { name: '举报结果', icon: '📋', class: 'report' },
    14: { name: '好友请求', icon: '👥', class: 'friend-request' },
    15: { name: '请求通过', icon: '✅', class: 'friend-accepted' },
    16: { name: '请求拒绝', icon: '❌', class: 'friend-rejected' }
};

// 计算属性
const filteredNotifications = computed(() => {
    let filtered = notifications.value;
    
    // 根据标签筛选
    switch (activeTab.value) {
        case 'unread':
            filtered = filtered.filter(n => !n.isRead);
            break;
        case 'friend':
            filtered = filtered.filter(n => [3, 14, 15, 16].includes(n.type));
            break;
        case 'system':
            filtered = filtered.filter(n => n.type === 1);
            break;
        case 'interaction':
            filtered = filtered.filter(n => [4, 5, 6, 7, 8, 9].includes(n.type));
            break;
    }
    
    // 分页处理
    const startIndex = (currentPage.value - 1) * itemsPerPage.value;
    return filtered.slice(startIndex, startIndex + itemsPerPage.value);
});

// 监听标签切换
watch(activeTab, () => {
    currentPage.value = 1;
    loadNotifications();
});

// 工具方法
const getTypeName = (type) => {
    return notificationTypes[type]?.name || '未知类型';
};

const getTypeIcon = (type) => {
    return notificationTypes[type]?.icon || '📄';
};

const getNotificationTypeClass = (type) => {
    return notificationTypes[type]?.class || 'default';
};

const getTypeBadgeClass = (type) => {
    return notificationTypes[type]?.class || 'default';
};

const formatTime = (timeString) => {
    if (!timeString) return '';
    const date = new Date(timeString);
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    
    // 小于1分钟
    if (diff < 60000) {
        return '刚刚';
    }
    // 小于1小时
    if (diff < 3600000) {
        return Math.floor(diff / 60000) + '分钟前';
    }
    // 小于1天
    if (diff < 86400000) {
        return Math.floor(diff / 3600000) + '小时前';
    }
    // 小于7天
    if (diff < 604800000) {
        return Math.floor(diff / 86400000) + '天前';
    }
    
    // 返回完整时间
    return date.toLocaleDateString('zh-CN') + ' ' + date.toLocaleTimeString('zh-CN', {
        hour: '2-digit',
        minute: '2-digit'
    });
};

// API方法
const loadNotifications = async () => {
    try {
        const response = await request.get('/api/Notification', {
            params: {
                page: currentPage.value,
                pageSize: itemsPerPage.value,
                unreadOnly: activeTab.value === 'unread'
            }
        });
        
        if (response.data) {
            notifications.value = response.data.notifications || [];
            totalCount.value = response.data.totalCount || 0;
            totalPages.value = response.data.totalPages || 0;
        }
    } catch (error) {
        console.error('加载通知失败:', error);
        notifications.value = [];
        ElMessage.error('加载通知失败');
    }
};

const loadUnreadCount = async () => {
    try {
        const response = await request.get('/api/Notification/unread-count');
        if (response.data) {
            unreadCount.value = response.data.count || 0;
        }
    } catch (error) {
        console.error('加载未读数量失败:', error);
    }
};

const markAsRead = async (notificationId) => {
    try {
        await request.put(`/api/Notification/${notificationId}/read`);
        
        // 更新本地状态
        const notification = notifications.value.find(n => n.id === notificationId);
        if (notification) {
            notification.isRead = true;
            notification.readTime = new Date().toISOString();
        }
        
        await loadUnreadCount();
        ElMessage.success('标记为已读成功');
    } catch (error) {
        console.error('标记为已读失败:', error);
        ElMessage.error('标记为已读失败');
    }
};

const markAllAsRead = async () => {
    try {
        await ElMessageBox.confirm(
            '确定要将所有通知标记为已读吗？',
            '确认操作',
            {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            }
        );

        await request.put('/api/Notification/mark-all-read');
        
        // 更新所有通知状态
        notifications.value.forEach(notification => {
            notification.isRead = true;
            notification.readTime = new Date().toISOString();
        });
        
        await loadUnreadCount();
        ElMessage.success('全部标记为已读成功');
    } catch (error) {
        if (error === 'cancel') return;
        console.error('标记全部为已读失败:', error);
        ElMessage.error('标记全部为已读失败');
    }
};

const deleteNotification = async (notificationId) => {
    try {
        await ElMessageBox.confirm(
            '确定要删除这条通知吗？',
            '确认删除',
            {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            }
        );

        await request.delete(`/api/Notification/${notificationId}`);
        
        // 从本地列表中移除
        notifications.value = notifications.value.filter(n => n.id !== notificationId);
        totalCount.value -= 1;
        
        ElMessage.success('删除通知成功');
    } catch (error) {
        if (error === 'cancel') return;
        console.error('删除通知失败:', error);
        ElMessage.error('删除通知失败');
    }
};

const clearAllNotifications = async () => {
    try {
        await ElMessageBox.confirm(
            '确定要清空所有通知吗？此操作不可恢复。',
            '确认清空',
            {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            }
        );

        await request.delete('/api/Notification');
        
        notifications.value = [];
        totalCount.value = 0;
        unreadCount.value = 0;
        
        ElMessage.success('清空通知成功');
    } catch (error) {
        if (error === 'cancel') return;
        console.error('清空通知失败:', error);
        ElMessage.error('清空通知失败');
    }
};

// 好友请求处理
const handleFriendRequest = async (notification, action) => {
    try {
        // 这里需要根据实际API调整
        // 假设通知中包含好友请求的ID信息
        const requestId = notification.relatedId;
        
        if (action === 'accept') {
            await ElMessageBox.confirm(
                '确定要同意这个好友请求吗？',
                '同意好友请求',
                {
                    confirmButtonText: '确定',
                    cancelButtonText: '取消',
                    type: 'warning'
                }
            );

            // 调用同意好友请求的API
            const response = await request.post(`/api/Friends/request/${requestId}/process`, {
                action: 1
            });
            
            ElMessage.success('好友请求已同意');
        } else {
            await ElMessageBox.confirm(
                '确定要拒绝这个好友请求吗？',
                '拒绝好友请求',
                {
                    confirmButtonText: '确定',
                    cancelButtonText: '取消',
                    type: 'warning'
                }
            );

            // 调用拒绝好友请求的API
            const response = await request.post(`/api/Friends/request/${requestId}/process`, {
                action: 2
            });
            
            ElMessage.success('好友请求已拒绝');
        }
        
        // 标记通知为已读
        await markAsRead(notification.id);
        
    } catch (error) {
        if (error === 'cancel') return;
        console.error('处理好友请求失败:', error);
        ElMessage.error('处理好友请求失败');
    }
};

// 界面交互方法
const switchTab = (tabId) => {
    activeTab.value = tabId;
};

const goToPage = (page) => {
    if (page >= 1 && page <= totalPages.value) {
        currentPage.value = page;
        loadNotifications();
    }
};

const viewNotificationDetail = (notification) => {
    selectedNotification.value = notification;
    if (!notification.isRead) {
        markAsRead(notification.id);
    }
};

const closeModal = () => {
    selectedNotification.value = null;
};

// 生命周期
onMounted(async () => {
    await loadNotifications();
    await loadUnreadCount();
});
</script>

<style scoped>
.notification-container {
    max-width: 800px;
    margin: 0 auto;
    padding: 20px;
    background-color: #fff;
    border-radius: 12px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
}

.notification-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
    padding-bottom: 15px;
    border-bottom: 1px solid #eaeaea;
}

.notification-header h2 {
    margin: 0;
    color: #333;
    font-size: 24px;
    font-weight: 600;
}

.notification-actions {
    display: flex;
    gap: 10px;
}

.action-btn {
    padding: 8px 15px;
    border: 1px solid #ddd;
    background-color: white;
    border-radius: 6px;
    cursor: pointer;
    font-size: 14px;
    transition: all 0.2s;
    font-weight: 500;
}

.action-btn:hover {
    background-color: #f5f5f5;
}

.action-btn.primary {
    background-color: #1890ff;
    color: white;
    border-color: #1890ff;
}

.action-btn.primary:hover {
    background-color: #40a9ff;
}

.mark-all-read {
    color: #1890ff;
    border-color: #1890ff;
}

.mark-all-read:hover {
    background-color: #e6f7ff;
}

.clear-all {
    color: #ff4d4f;
    border-color: #ff4d4f;
}

.clear-all:hover {
    background-color: #fff2f0;
}

.notification-tabs {
    display: flex;
    margin-bottom: 20px;
    border-bottom: 1px solid #eaeaea;
    flex-wrap: wrap;
    gap: 5px;
}

.tab {
    position: relative;
    padding: 12px 20px;
    background: none;
    border: none;
    cursor: pointer;
    font-size: 14px;
    color: #666;
    border-bottom: 2px solid transparent;
    transition: all 0.2s;
    white-space: nowrap;
    font-weight: 500;
}

.tab.active {
    color: #1890ff;
    border-bottom-color: #1890ff;
}

.tab:hover {
    color: #1890ff;
}

.badge {
    position: absolute;
    top: -5px;
    right: 5px;
    background-color: #ff4d4f;
    color: white;
    border-radius: 10px;
    padding: 2px 6px;
    font-size: 12px;
    min-width: 18px;
    text-align: center;
    font-weight: 600;
}

.notification-stats {
    display: flex;
    gap: 20px;
    margin-bottom: 20px;
    padding: 15px;
    background-color: #f8f9fa;
    border-radius: 8px;
    border-left: 4px solid #1890ff;
}

.stat-item {
    display: flex;
    align-items: center;
    gap: 8px;
}

.stat-label {
    color: #666;
    font-size: 14px;
    font-weight: 500;
}

.stat-value {
    color: #1890ff;
    font-weight: 600;
    font-size: 16px;
}

.empty-state {
    text-align: center;
    padding: 60px 20px;
    color: #999;
}

.empty-icon {
    font-size: 48px;
    margin-bottom: 15px;
    opacity: 0.5;
}

.empty-state p {
    margin: 0;
    font-size: 16px;
    font-weight: 500;
}

.notification-list {
    margin-bottom: 20px;
}

.notification-item {
    display: flex;
    align-items: flex-start;
    padding: 16px;
    margin-bottom: 12px;
    background-color: white;
    border-radius: 8px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
    transition: all 0.2s;
    border-left: 4px solid transparent;
    cursor: pointer;
    border: 1px solid #f0f0f0;
}

.notification-item.unread {
    border-left-color: #1890ff;
    background-color: #f0f8ff;
    border-color: #e6f7ff;
}

.notification-item.expired {
    opacity: 0.6;
    background-color: #fafafa;
}

.notification-item:hover {
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    transform: translateY(-1px);
    border-color: #e0e0e0;
}

.notification-icon {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-right: 15px;
    color: white;
    font-size: 16px;
    font-weight: bold;
    flex-shrink: 0;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

/* 通知类型图标样式 */
.notification-icon.system { background-color: #52c41a; }
.notification-icon.activity { background-color: #faad14; }
.notification-icon.friend-request { background-color: #1890ff; }
.notification-icon.message { background-color: #722ed1; }
.notification-icon.post-reply { background-color: #13c2c2; }
.notification-icon.comment-reply { background-color: #eb2f96; }
.notification-icon.post-like { background-color: #f5222d; }
.notification-icon.comment-like { background-color: #fa541c; }
.notification-icon.follow { background-color: #52c41a; }
.notification-icon.points { background-color: #faad14; }
.notification-icon.level-up { background-color: #1890ff; }
.notification-icon.award { background-color: #eb2f96; }
.notification-icon.report { background-color: #722ed1; }
.notification-icon.friend-accepted { background-color: #52c41a; }
.notification-icon.friend-rejected { background-color: #f5222d; }
.notification-icon.default { background-color: #8c8c8c; }

.notification-content {
    flex: 1;
    min-width: 0;
}

.notification-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 8px;
    flex-wrap: wrap;
    gap: 10px;
}

.notification-title {
    margin: 0;
    font-size: 16px;
    color: #333;
    font-weight: 600;
    line-height: 1.4;
}

.notification-badge {
    padding: 4px 8px;
    border-radius: 12px;
    font-size: 12px;
    font-weight: 500;
    white-space: nowrap;
}

.notification-badge.system { background-color: #f6ffed; color: #52c41a; border: 1px solid #b7eb8f; }
.notification-badge.activity { background-color: #fff7e6; color: #fa8c16; border: 1px solid #ffd591; }
.notification-badge.friend-request { background-color: #e6f7ff; color: #1890ff; border: 1px solid #91d5ff; }
.notification-badge.message { background-color: #f9f0ff; color: #722ed1; border: 1px solid #d3adf7; }
.notification-badge.post-reply { background-color: #e6fffb; color: #13c2c2; border: 1px solid #87e8de; }
.notification-badge.comment-reply { background-color: #fff0f6; color: #eb2f96; border: 1px solid #ffadd2; }
.notification-badge.post-like { background-color: #fff1f0; color: #f5222d; border: 1px solid #ffa39e; }
.notification-badge.comment-like { background-color: #fff2e8; color: #fa541c; border: 1px solid #ffbb96; }
.notification-badge.follow { background-color: #f6ffed; color: #52c41a; border: 1px solid #b7eb8f; }
.notification-badge.points { background-color: #fff7e6; color: #fa8c16; border: 1px solid #ffd591; }
.notification-badge.level-up { background-color: #e6f7ff; color: #1890ff; border: 1px solid #91d5ff; }
.notification-badge.award { background-color: #fff0f6; color: #eb2f96; border: 1px solid #ffadd2; }
.notification-badge.report { background-color: #f9f0ff; color: #722ed1; border: 1px solid #d3adf7; }
.notification-badge.friend-accepted { background-color: #f6ffed; color: #52c41a; border: 1px solid #b7eb8f; }
.notification-badge.friend-rejected { background-color: #fff1f0; color: #f5222d; border: 1px solid #ffa39e; }
.notification-badge.default { background-color: #f5f5f5; color: #666; border: 1px solid #d9d9d9; }

.notification-text {
    margin: 0 0 12px 0;
    font-size: 14px;
    color: #666;
    line-height: 1.5;
    word-wrap: break-word;
}

.friend-request-actions {
    display: flex;
    gap: 10px;
    margin-bottom: 12px;
}

.accept-btn, .reject-btn {
    padding: 6px 12px;
    border: 1px solid #ddd;
    background-color: white;
    border-radius: 4px;
    cursor: pointer;
    font-size: 12px;
    transition: all 0.2s;
    font-weight: 500;
}

.accept-btn {
    color: #52c41a;
    border-color: #52c41a;
}

.accept-btn:hover {
    background-color: #f6ffed;
    transform: translateY(-1px);
}

.reject-btn {
    color: #ff4d4f;
    border-color: #ff4d4f;
}

.reject-btn:hover {
    background-color: #fff2f0;
    transform: translateY(-1px);
}

.notification-meta {
    display: flex;
    align-items: center;
    gap: 15px;
    font-size: 12px;
    color: #999;
    flex-wrap: wrap;
}

.unread-dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background-color: #1890ff;
    animation: pulse 2s infinite;
}

@keyframes pulse {
    0% { opacity: 1; }
    50% { opacity: 0.5; }
    100% { opacity: 1; }
}

.notification-actions {
    display: flex;
    gap: 8px;
    margin-left: 15px;
    flex-shrink: 0;
}

.read-btn, .delete-btn {
    padding: 6px;
    border: 1px solid #ddd;
    background-color: white;
    border-radius: 4px;
    cursor: pointer;
    font-size: 12px;
    transition: all 0.2s;
    width: 32px;
    height: 32px;
    display: flex;
    align-items: center;
    justify-content: center;
}

.read-btn:hover {
    background-color: #e6f7ff;
    border-color: #1890ff;
    color: #1890ff;
    transform: scale(1.1);
}

.delete-btn:hover {
    background-color: #fff2f0;
    border-color: #ff4d4f;
    color: #ff4d4f;
    transform: scale(1.1);
}

.notification-pagination {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 20px;
    padding: 15px 0;
    border-top: 1px solid #eaeaea;
}

.page-btn {
    padding: 8px 16px;
    border: 1px solid #ddd;
    background-color: white;
    border-radius: 6px;
    cursor: pointer;
    font-size: 14px;
    transition: all 0.2s;
    display: flex;
    align-items: center;
    gap: 5px;
    font-weight: 500;
}

.page-btn:hover:not(.disabled) {
    background-color: #f5f5f5;
    border-color: #1890ff;
    color: #1890ff;
}

.page-btn.disabled {
    color: #ccc;
    cursor: not-allowed;
    background-color: #fafafa;
}

.page-info {
    color: #666;
    font-size: 14px;
    font-weight: 500;
}

/* 模态框样式 */
.notification-modal {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
    animation: fadeIn 0.3s;
}

.modal-content {
    background-color: white;
    border-radius: 12px;
    width: 90%;
    max-width: 500px;
    max-height: 80vh;
    overflow-y: auto;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
    animation: slideIn 0.3s;
}

@keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
}

@keyframes slideIn {
    from { transform: translateY(-50px); opacity: 0; }
    to { transform: translateY(0); opacity: 1; }
}

.modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 20px;
    border-bottom: 1px solid #eaeaea;
}

.modal-header h3 {
    margin: 0;
    color: #333;
    font-size: 18px;
    font-weight: 600;
}

.close-btn {
    background: none;
    border: none;
    font-size: 18px;
    color: #666;
    cursor: pointer;
    padding: 5px;
    border-radius: 4px;
    transition: all 0.2s;
}

.close-btn:hover {
    background-color: #f5f5f5;
    color: #333;
}

.modal-body {
    padding: 20px;
}

.notification-detail {
    display: flex;
    gap: 15px;
}

.detail-icon {
    width: 50px;
    height: 50px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 20px;
    font-weight: bold;
    flex-shrink: 0;
}

.detail-content {
    flex: 1;
}

.detail-content p {
    margin: 0 0 15px 0;
    font-size: 15px;
    line-height: 1.6;
    color: #333;
}

.detail-meta {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.meta-item {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 13px;
    color: #666;
}

.meta-item i {
    width: 16px;
    text-align: center;
}

.modal-footer {
    padding: 15px 20px;
    border-top: 1px solid #eaeaea;
    display: flex;
    justify-content: flex-end;
    gap: 10px;
}

/* 响应式设计 */
@media (max-width: 768px) {
    .notification-container {
        padding: 15px;
        margin: 10px;
    }
    
    .notification-header {
        flex-direction: column;
        gap: 15px;
        align-items: flex-start;
    }
    
    .notification-actions {
        width: 100%;
        justify-content: flex-end;
    }
    
    .notification-tabs {
        overflow-x: auto;
        padding-bottom: 5px;
    }
    
    .notification-item {
        flex-direction: column;
        align-items: flex-start;
    }
    
    .notification-icon {
        margin-right: 0;
        margin-bottom: 10px;
    }
    
    .notification-content {
        width: 100%;
    }
    
    .notification-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 8px;
    }
    
    .notification-actions {
        margin-left: 0;
        margin-top: 10px;
        width: 100%;
        justify-content: flex-end;
    }
    
    .notification-meta {
        gap: 10px;
    }
    
    .modal-content {
        width: 95%;
        margin: 10px;
    }
    
    .notification-detail {
        flex-direction: column;
        text-align: center;
    }
    
    .detail-icon {
        align-self: center;
    }
}

@media (max-width: 480px) {
    .notification-header h2 {
        font-size: 20px;
    }
    
    .action-btn {
        padding: 6px 12px;
        font-size: 13px;
    }
    
    .tab {
        padding: 10px 15px;
        font-size: 13px;
    }
    
    .notification-stats {
        flex-direction: column;
        gap: 10px;
    }
    
    .page-btn {
        padding: 6px 12px;
        font-size: 13px;
    }
    
    .modal-content {
        max-height: 90vh;
    }
}

/* 滚动条样式 */
.notification-list::-webkit-scrollbar {
    width: 6px;
}

.notification-list::-webkit-scrollbar-track {
    background: #f1f1f1;
    border-radius: 3px;
}

.notification-list::-webkit-scrollbar-thumb {
    background: #c1c1c1;
    border-radius: 3px;
}

.notification-list::-webkit-scrollbar-thumb:hover {
    background: #a8a8a8;
}

.modal-content::-webkit-scrollbar {
    width: 8px;
}

.modal-content::-webkit-scrollbar-track {
    background: #f1f1f1;
    border-radius: 4px;
}

.modal-content::-webkit-scrollbar-thumb {
    background: #c1c1c1;
    border-radius: 4px;
}

.modal-content::-webkit-scrollbar-thumb:hover {
    background: #a8a8a8;
}
</style>