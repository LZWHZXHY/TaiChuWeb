<script setup>
    import {ref} from 'vue'
    const str = ref('123123')
</script>

<template>
    <input class ="input" type="test">
    <div class="box"></div>
</template>

<style scoped>
    .input{
        width: 200px;
        height:30px;
        margin:200px 0 0 400px;
    }
    .box{
        margin: 0 0 0 400px;
        font-size: 20px;
    }
</style>