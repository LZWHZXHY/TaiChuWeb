<template>
    <div class="big-container">
        <!-- 优化后的导航栏 -->
        <div class="top-bar">
            <RouterLink to="/lianhe" class="top-bar-option">
                <i class="fas fa-cubes"></i>
                <span>联合平台</span>
            </RouterLink>

            <RouterLink to="/activity" class="top-bar-option">
                <i class="fa fa-calendar"></i>
                <span>打卡活动</span>
            </RouterLink>

            <RouterLink to="/shetuan" class="top-bar-option">
                <i class="fas fa-users"></i>
                <span>柴圈社团</span>
            </RouterLink>
            
            <RouterLink to="/kaohe" class="top-bar-option">
                <i class="fas fa-palette"></i>
                <span>画师等级考核</span>
            </RouterLink>
            
            <RouterLink to="/rank" class="top-bar-option">
                <i class="fas fa-trophy"></i>
                <span>排行榜</span>
            </RouterLink>

            <RouterLink to="/huati" class="top-bar-option">
                <i class="fas fa-comment-alt"></i>
                <span>话题发布</span>
            </RouterLink>
        </div>

        <!-- 内容区域保持不变 -->
        <main class="chai-view">
            <RouterView></RouterView>
        </main>
    </div>
</template>

<script setup>
// 引入必要的Vue功能
import { RouterLink, RouterView } from 'vue-router'
</script>

<style scoped>
/* 基础重置 - 保持您的原有重置 */
a {
    text-decoration: none;
    color: inherit;
    background: none;
    outline: none;
    box-shadow: none;
    border: none;
}

/* 容器优化 */
.big-container {
    background-color: #0e0e0e00;
    width: 100%;
    height: 100%;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    position: relative;
}
/* 自定义滚动条 */
::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}

::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 10px;
}

::-webkit-scrollbar-thumb {
  background: rgba(90, 90, 57, 0.5);
  border-radius: 10px;
}

::-webkit-scrollbar-thumb:hover {
  background: rgba(90, 90, 57, 0.7);
}

/* 导航栏优化 - 重点改进部分 */
.top-bar {
    background-color: #0e68f000;
    width: 100%;
    height: 80px; /* 优化高度 */
    display: flex;
    justify-content: flex-start; /* 修改为左对齐 */
    align-items: center; /* 垂直居中 */
    padding-left: 2rem; /* 添加左侧内边距，避免选项紧贴屏幕边缘 */
    
    position: relative;
    z-index: 100; /* 确保在内容上方 */
}

.top-bar-option {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    color: rgba(255, 255, 255, 0.9);
    font-weight: 500;
    padding: 0 1.8rem;
    height: 100%;
    text-align: center;
    position: relative;
    transition: all 0.3s ease;
    text-decoration: none;
    cursor: pointer;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    font-size: 0.95rem;
    margin-left: 30px;
}

.top-bar-option i {
    font-size: 1.2rem;
    margin-bottom: 0.4rem;
    transition: transform 0.3s ease;
    color: rgba(255, 255, 255, 0.9);
}

.top-bar-option:hover {
    color: #fff;
    background: rgba(255, 255, 255, 0.15);
}

.top-bar-option:hover i {
    transform: translateY(-5px);
    color: #fff;
}

/* Vue路由激活状态特殊样式 */
.top-bar-option.router-link-exact-active {
    background: rgba(255, 255, 255, 0.2);
    color: #fff;
    font-weight: 600;
}

.top-bar-option.router-link-exact-active::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 50%;
    transform: translateX(-50%);
    width: 80%;
    height: 4px;
    background: linear-gradient(to right, #ff9a9e, #fad0c4);
    border-radius: 2px;
}

.top-bar-option.router-link-exact-active i {
    color: #fff;
    filter: drop-shadow(0 0 2px rgba(255, 255, 255, 0.6));
}

/* 内容区域优化 */
.chai-view {
    background-color: #0e0e0e00;
    flex: 1; /* 占据剩余空间 */
    overflow-y: hidden;
    position: relative;
    padding: 20px;
    overflow-y: scroll;
}

/* 响应式设计 */
@media (max-width: 900px) {
    .top-bar {
        height: auto;
        padding: 0.5rem 0;
    }
    
    .top-bar-option {
        padding: 0 1rem;
        font-size: 0.85rem;
    }
    
    .top-bar-option i {
        font-size: 1rem;
    }
}

@media (max-width: 600px) {
    .top-bar {
        flex-wrap: wrap;
        height: auto;
        padding: 0.5rem 0;
    }
    
    .top-bar-option {
        width: 50%;
        padding: 0.8rem 0.5rem;
        font-size: 0.8rem;
    }
}

@media (max-width: 400px) {
    .top-bar-option {
        width: 100%;
        padding: 0.8rem;
    }
}
</style>