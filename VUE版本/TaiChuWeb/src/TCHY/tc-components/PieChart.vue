<template>
  <Pie :data="chartData" :options="chartOptions" />
</template>
<script setup>
import { Pie } from 'vue-chartjs'
import { computed } from 'vue'

const props = defineProps({
  labels: Array,
  data: Array,
  colors: Array
})

const chartData = computed(() => ({
  labels: props.labels,
  datasets: [
    {
      data: props.data,
      backgroundColor: props.colors
    }
  ]
}))

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: true,
      position: 'bottom'
    },
    tooltip: {
      callbacks: {
        label: context => {
          const label = context.label || ''
          const value = context.parsed
          const sum = props.data.reduce((a, b) => a + b, 0)
          const percent = sum ? ((value / sum) * 100).toFixed(1) : 0
          return `${label}: ${value} (${percent}%)`
        }
      }
    }
  }
}
</script>