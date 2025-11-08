<template>
    <div class = "BigContainer">

        <div class="container">
            <h1 class="title">【柴圈社团大全】</h1>
            <div v-if="shetuans.length > 0" class="scrollable-content" >
                <ul class="shetuan-list">
                    <!-- 循环显示每个社团 -->
                    <li v-for="shetuan in verifiedShetuans" :key="shetuan.id" class="shetuan-item">
                        <div class="shetuan-header">
                            <h2 class="shetuan-name">{{ shetuan.name }}</h2>
                            <p class="shetuan-leader">社长: {{ shetuan.leader }}</p>
                        </div>
                        <div class="shetuan-info">
                            <!-- 根据 type 显示不同的群类型 -->
                            <p><strong>群类型:</strong>
                                <span v-if="shetuan.type === 1">交流群</span>
                                <span v-else-if="shetuan.type === 2">
                                    
                                    
                                    
                                    联合群</span>
                                <span v-else-if="shetuan.type === 3">内部群</span>
                                <span v-else>未知类型</span>
                            </p>
                            <p><strong>是否需要考核:</strong> {{ shetuan.test === 1 ? '需要' : '不需要' }}</p>
                            <!-- 只有当 test 是 1 时才显示考核难度 -->
                            <p v-if="shetuan.test === 1"><strong>考核难度:</strong> T{{ shetuan.testlevel }}</p>
                            <!-- 根据 size 显示规模分类 -->
                            <p><strong>社团规模:</strong>
                                <span v-if="shetuan.size === 1">小型</span>
                                <span v-else-if="shetuan.size === 2">中型</span>
                                <span v-else-if="shetuan.size === 3">大型</span>
                                <span v-else-if="shetuan.size === 4">超大型</span>
                                <span v-else>未知规模</span>
                            </p>
                        </div>
                        <div class="shetuan-desc">
                            <p>{{ shetuan.desc }}</p>
                        </div>
                        <a :href="shetuan.url" class="join-link">加入社团</a>
                    </li>
                </ul>


            </div>
            <div v-else>
                <p class="no-data">暂无已认证的社团数据</p>
            </div>
            
        </div>


        





        <div class="function-panel">
            <ul class="actions-list">
                <li class="action-item" @click="handleApply">申请</li>
                <li class="action-item" @click="handleFeedback">反馈</li>
                <li class="action-item" @click="handleReport">举报</li>
            </ul>
        </div>






        <!-- 申请社团的弹窗 -->
    <div v-if="showApplyModal" class="modal">
        <h2>申请社团</h2>
        <form @submit.prevent="submitApplication">
            <label for="name">社团名称(QQ群名称):</label>
            <input type="text" id="name" v-model="applicationForm.name" required />
            
            <label for="leader">社长:</label>
            <input type="text" id="leader" v-model="applicationForm.leader" required />
            
            <label for="type">群类型:</label>
            <select id="type" v-model="applicationForm.type" required>
                <option value="1">交流群</option>
                <option value="2">联合群</option>
                <option value="3">内部群</option>
            </select>
            
            <label for="test">是否需要考核:</label>
            <select id="test" v-model="applicationForm.test" required>
                <option value="1">需要</option>
                <option value="0">不需要</option>
            </select>
            
            <div v-if="applicationForm.test === '1'">
                <label for="testlevel">考核难度:</label>
                <select id="testlevel" v-model="applicationForm.testlevel" required>
                    <option value="0">T0</option>
                    <option value="1">T1</option>
                    <option value="2">T2</option>
                    <option value="3">T3</option>
                </select>
            </div>
            
            <label for="url">加群链接:</label>
            <input type="url" id="url" v-model="applicationForm.url" required placeholder="https://..." />
            
            <label for="size">社团规模:</label>
            <select id="size" v-model="applicationForm.size" required>
                <option value="1">小型 (1-100人)</option>
                <option value="2">中型 (101-200人)</option>
                <option value="3">大型 (201-300人)</option>
                <option value="4">超大型 (300+人)</option>
            </select>
            
            <label for="desc">社团描述(群聊简介):</label>
            <textarea id="desc" v-model="applicationForm.desc" required></textarea>
            
            <div class="form-buttons">
                <button type="submit">提交申请</button>
                <button type="button" @click="closeModal">取消</button>
            </div>
        </form>
    </div>



    </div>
</template>

<script>
import axios from 'axios';
import {useUserStore} from '../../store/user'



export default {
    name: 'SheTuanList',
    data() {
        return {
            shetuans: [], // 保存从后端获取的社团数据
            verifiedShetuans: [],
            userRank: 0,  // 用来保存用户等级
            showApplyModal: false, // 控制申请弹窗显示
            applicationForm: {
                name: "",
                leader: "",
                type: "1", // 默认选择交流群
                test: "0", // 默认不需要考核
                testlevel: "0", // 默认考核难度
                url: "",
                size: "1", // 默认小型
                desc: ""
            }
        };
    },
    mounted() {
        const userStore = useUserStore(); //获取用户Store
        this.userRank = userStore.userRank;

        console.log('User Rank:', this.userRank);
        
        // 页面加载时从后端获取数据
        axios.get('https://bianyuzhou.com/api/Shetuan')
            .then(response => {
                this.shetuans = response.data; // 将后端返回的数据保存到 shetuans
                this.verifiedShetuans = this.shetuans.filter(
                    shetuan=>shetuan.verify === 1
                );
                
            })
            .catch(error => {
                console.error("获取社团数据失败:", error);
            });
    },
    methods:{
        handleApply(){
            console.log("弹窗成功")
            if(this.userRank >= 0){
                this.showApplyModal = true;
            }
            else{
                alert("你的等级不够，暂时不能申请社团收录")
            }
        },
        handleFeedback() {
            // 模拟反馈操作
            alert("反馈功能正在开发中！");
        },
        handleReport() {
            // 模拟举报操作
            alert("举报功能正在开发中！");
        },
        submitApplication() {
            // 提交社团申请
            axios.post("https://bianyuzhou.com/api/Shetuan/ApplyShetuan", this.applicationForm)
                .then(response => {
                    alert("申请成功！等待审核中。");
                    this.closeModal();
                })
                .catch(error => {
                    console.error("提交申请失败:", error);
                    alert("提交失败，请稍后重试！");
                });
        },
        closeModal() {
            this.showApplyModal = false;
            // 重置所有表单字段
            this.applicationForm = {
                name: "",
                leader: "",
                type: "1",
                test: "0",
                testlevel: "0",
                url: "",
                size: "1",
                desc: ""
            };
        },
    }
};




</script>

<style scoped>

/* 在样式部分添加无数据显示的样式 */
.no-data {
    text-align: center;
    padding: 20px;
    color: #666;
    font-style: italic;
}

.BigContainer{
    background-color: #211a1a00;
    position: absolute;
    width: 90vw;
    height: 100vh;
    overflow-y: scroll;
    
}
.container {
    position: absolute;
    left: 0;
    max-width: 60vw;   /* 由原来的20vw改为60vw */
    width: 60vw;   
    margin: auto;
    padding: 20px;
    font-family: Arial, sans-serif;
    background-color: #f9f9f93d;
    border-radius: 8px;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);

    
}

.title {
    text-align: center;
    font-size: 24px;
    color: #333;
    margin-bottom: 20px;
}

/* 添加滚动功能 */
.scrollable-content {
    max-height: 80vh; /* 设置最大高度 */
    overflow-y: auto; /* 超出内容时添加垂直滚动条 */
    padding-right: 10px; /* 滚动条与内容的间距 */
}

.shetuan-list {
    list-style-type: none;
    padding: 0;
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(260px, 1fr)); /* 关键 */
    gap: 18px;
}

.shetuan-item {
    background-color: #ffffffc8;
    margin-bottom: 15px;
    padding: 15px;
    border: 1px solid #ddddddb0;
    border-radius: 8px;
    transition: box-shadow 0.3s ease;
}

.shetuan-item:hover {
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.425);
}

.shetuan-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 10px;
}

.shetuan-name {
    font-size: 18px;
    color: #000000c4;
    margin: 0;
}

.shetuan-leader {
    font-size: 14px;
    color: #272525;
}

.shetuan-info {
    margin-bottom: 10px;
}

.shetuan-info p {
    margin: 5px 0;
    font-size: 14px;
    color: #333;
}

.shetuan-desc {
    margin-bottom: 10px;
    font-size: 14px;
    color: #666;
}

.join-link {
    display: inline-block;
    text-decoration: none;
    font-size: 14px;
    color: #fff;
    background-color: #4c47365f;
    padding: 8px 12px;
    border-radius: 4px;
    transition: background-color 0.3s ease;
}

.join-link:hover {
    background-color: #86b94f;
}

.function-panel {
    padding: 1rem;
    background-color: #f9f9f922;
    border: 1px solid #ddd;
    border-radius: 5px;
    width: 300px;
    position: absolute;
    right: 2vw;
    
}

.actions-list {
    list-style: none;
    margin: 0;
    padding: 0;
}

.action-item {
    padding: 0.5rem 1rem;
    margin: 0.5rem 0;
    background-color: #e6e6e665;
    border-radius: 3px;
    cursor: pointer;
    text-align: center;
    font-family: Arial, sans-serif;
    font-size: 14px;
    transition: background-color 0.3s;
}

.action-item:hover {
    background-color: #d0d690b2;
}









/* 新增弹窗样式 */
.modal {
    position: fixed;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    background-color: #d8d8d35e;
    padding: 20px;
    border-radius: 8px;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    z-index: 1000;
}

/* 添加表单样式 */
.modal form {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 10px;
}

.modal form label {
    grid-column: 1 / 2;
    margin-top: 10px;
}

.modal form input,
.modal form select,
.modal form textarea {
    grid-column: 2 / 3;
    margin-top: 5px;
    padding: 8px;
    font-size: 14px;
    border-radius: 4px;
    border: 1px solid #ddd;
}

.modal form .form-buttons {
    grid-column: 1 / 3;
    display: flex;
    justify-content: space-between;
    margin-top: 15px;
}

.modal form button {
    padding: 8px 15px;
    font-size: 14px;
    border-radius: 4px;
    border: none;
    cursor: pointer;
}

.modal form button[type="submit"] {
    background-color: #007BFF;
    color: white;
}

.modal form button[type="button"] {
    background-color: #6c757d;
    color: white;
}
</style>