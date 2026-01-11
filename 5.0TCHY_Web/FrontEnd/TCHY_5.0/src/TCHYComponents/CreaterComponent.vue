<template>
  <div class="dashboard-container">
    
    <IpList 
      v-if="currentMode === 'list'" 
      @go-create="currentMode = 'create'" 
      @enter-world="handleEnterWorld"
    />

    <div v-else-if="currentMode === 'create'">
      <button @click="currentMode = 'list'" style="margin: 20px;">⬅️ 返回列表</button>
      <CreateIpForm @create-success="currentMode = 'list'" />
    </div>

    <div v-else-if="currentMode === 'editor'">
      <button @click="currentMode = 'list'" style="margin: 20px;">⬅️ 返回列表</button>
      <WorldEditor :ip-id="selectedIpId" />
    </div>

  </div>
</template>

<script>
// 注意：请确认你的文件夹名字是 Creater 还是 Creator，路径要对
import IpList from './Creater/IpList.vue';
import CreateIpForm from './Creater/CreateIpForm.vue'; 
import WorldEditor from './Creater/WorldEditor.vue';

export default {
  components: { IpList, CreateIpForm, WorldEditor },
  data() {
    return {
      currentMode: 'list', // 可能的值: 'list', 'create', 'editor'
      selectedIpId: null   // 用来存当前点击了哪个宇宙 ID
    };
  },
  methods: {
    // 处理从 IpList 组件传出来的事件
    handleEnterWorld(id) {
      this.selectedIpId = id;   // 1. 记下 ID
      this.currentMode = 'editor'; // 2. 切换到编辑器模式
    }
  }
};
</script>