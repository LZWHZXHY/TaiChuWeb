<template>
    <div class = "container">
        <div class="topBar">

            
            <RouterLink to="/Settings">设定集</RouterLink>
            <RouterLink to="/OClist">OC列表</RouterLink>
           
        </div>



        <div class="leftBar">
            <div class="leftBar_header">



            </div>

            <router-view></router-view>

        </div>



      
    </div>


    
</template> 

<style scoped>
    @import url('/static/css/general.css');
    @import url('/static/css/彼岸宇宙.css');

</style>

<script setup>
    import { RouterLink } from 'vue-router';

</script>