<template>
  <div class="modal-backdrop" @click.self="$emit('cancel')">
    <div class="modal">
      <h3 class>确认购买</h3>
      <div class="content">
        <div class="left">
          <div class="thumb">{{ item.title }}</div>
        </div>
        <div class="right">
          <div class="row"><label>单价：</label><div>{{ item.price }} 金币</div></div>
          <div class="row">
            <label>数量：</label>
            <input type="number" v-model.number="qty" :min="1" :max="maxQuantity" />
            <div class="hint">最多 {{ maxQuantity }} 个</div>
          </div>
          <div class="row"><label>总计：</label><div>{{ total }} 金币</div></div>
          <div class="actions">
            <button class="confirm" @click="confirm">确认支付</button>
            <button class="cancel" @click="$emit('cancel')">取消</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
const props = defineProps({
  item: { type: Object, required: true },
  maxQuantity: { type: Number, default: 99 },
})
const emit = defineEmits(['confirm', 'cancel'])

const qty = ref(1)

watch(() => props.item, () => { qty.value = 1 })

const total = computed(() => (props.item.price * qty.value))

function confirm() {
  emit('confirm', { item: props.item, qty: qty.value, total: total.value })
}
</script>

<style scoped>
.modal-backdrop {
  position:fixed; inset:0; display:flex; align-items:center; justify-content:center;
  background: rgba(16,24,40,0.35); z-index:50;
}
.modal {
  width:680px; max-width:95%; background: #ffffff; border-radius:12px; padding:18px; color:var(--text);
  border:1px solid #e6e9ef;
  box-shadow: 0 24px 70px rgba(16,24,40,0.08);
}
.content { display:flex; gap:16px; }
.left .thumb { width:160px; height:110px; border-radius:10px; background: linear-gradient(180deg,#fbfdff,#f7fbfd); display:flex; align-items:center; justify-content:center; color:#6b7280; font-weight:700; }
.right { flex:1; display:flex; flex-direction:column; gap:12px; }
.row { display:flex; align-items:center; gap:8px; justify-content:space-between; }
.row label { color:#6b7280; width:80px; }
input[type="number"] {
  padding:8px 10px; border-radius:8px; border:1px solid #e6e9ef; background: #fff; color:#111827; width:110px;
}
.actions { display:flex; gap:8px; justify-content:flex-end; }
.confirm { background: var(--accent); border:none; padding:10px 14px; border-radius:8px; color:#000000; font-weight:700; }
.cancel { background:transparent; border:1px solid #e6e9ef; padding:10px 14px; border-radius:8px; color:#374151; }
.hint { color:#6b7280; font-size:12px; }
</style>