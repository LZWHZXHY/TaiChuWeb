<template>
  <section class="module-records">
    <div class="cyber-table-wrapper">
      <table class="cyber-table">
        <thead>
          <tr><th>DATE</th><th>OPERATION</th><th>INITIATOR</th><th>DEFENDER</th><th>OUTCOME</th></tr>
        </thead>
        <tbody>
          <tr><td colspan="5" class="empty-row">[ NO_COMBAT_LOGS_RECORDED ]</td></tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<script setup>
</script>

<style scoped>
.module-records { width: 100%; }
.cyber-table-wrapper { border: 2px solid var(--black); background: var(--white); width: 100%; }
.cyber-table { width: 100%; border-collapse: collapse; text-align: left; }
.cyber-table th { background: #eee; border-bottom: 2px solid var(--black); padding: 10px; font-weight: bold; }
.cyber-table td { padding: 10px; border-bottom: 1px solid #ccc; font-size: 0.9rem; }
.empty-row { text-align: center; padding: 40px; color: #888; }
</style>