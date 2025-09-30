<template>
    <div>
        <div> 
            <h1>补天规则</h1>
        </div>

    </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import axios from "axios";

const baseApiUrl = import.meta.env.DEV
  ? "https://localhost:44321"
  : "https://bianyuzhou.com";


</script>