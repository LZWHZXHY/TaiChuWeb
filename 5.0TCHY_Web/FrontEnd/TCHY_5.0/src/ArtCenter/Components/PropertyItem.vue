<template>
  <div class="property-item">
    <div class="prop-row">
      <span class="toggle-icon" @click="isOpen = !isOpen" :style="{ visibility: isFolder ? 'visible' : 'hidden' }">
        {{ isOpen ? '▼' : '▶' }}
      </span>
      
      <input :value="modelValue.key" @input="updateKey" placeholder="KEY" class="prop-input key" />
      <span class="colon">:</span>
      
      <template v-if="!isFolder">
        <input :value="modelValue.value" @input="updateValue" placeholder="VALUE" class="prop-input val" />
        <button class="icon-btn" @click="convertToFolder" title="Group">📁</button>
      </template>
      <template v-else>
        <span class="folder-label">[ GROUP: {{ modelValue.children.length }} ITEMS ]</span>
        <button class="add-child-btn" @click="addChild">+ ITEM</button>
      </template>

      <button class="delete-btn" @click="$emit('delete')">×</button>
    </div>

    <div v-if="isFolder && isOpen" class="children-block">
      <PropertyItem 
        v-for="(child, index) in modelValue.children" 
        :key="index" 
        :modelValue="child" 
        @update:modelValue="updateChild(index, $event)" 
        @delete="deleteChild(index)" 
      />
    </div>
  </div>
</template>

<script>
export default {
  name: 'PropertyItem',
  props: { modelValue: { type: Object, required: true } },
  emits: ['update:modelValue', 'delete'],
  data() { return { isOpen: true }; },
  computed: { isFolder() { return Array.isArray(this.modelValue.children); } },
  methods: {
    updateKey(e) { this.$emit('update:modelValue', { ...this.modelValue, key: e.target.value }); },
    updateValue(e) { this.$emit('update:modelValue', { ...this.modelValue, value: e.target.value }); },
    convertToFolder() { const newItem = { ...this.modelValue, children: [] }; delete newItem.value; this.$emit('update:modelValue', newItem); this.isOpen = true; },
    addChild() { const currentChildren = this.modelValue.children ? [...this.modelValue.children] : []; currentChildren.push({ key: '', value: '' }); this.$emit('update:modelValue', { ...this.modelValue, children: currentChildren }); this.isOpen = true; },
    updateChild(index, newChildData) { const newChildren = [...this.modelValue.children]; newChildren[index] = newChildData; this.$emit('update:modelValue', { ...this.modelValue, children: newChildren }); },
    deleteChild(index) { const newChildren = [...this.modelValue.children]; newChildren.splice(index, 1); this.$emit('update:modelValue', { ...this.modelValue, children: newChildren }); }
  }
};
</script>

<style scoped>
/* 适配新主题的 CSS */
.property-item { display: flex; flex-direction: column; width: 100%; font-family: 'JetBrains Mono', monospace; }
.prop-row { display: flex; align-items: center; margin-bottom: 5px; width: 100%; }
.children-block { margin-left: 15px; padding-left: 10px; border-left: 1px dashed #999; margin-top: 5px; }

.prop-input { 
  background: #fff; border: 1px solid #ccc; padding: 4px 8px; font-size: 0.85rem; 
  outline: none; font-family: inherit; color: #333;
}
.prop-input:focus { border-color: #000; background: #fffef0; }
.prop-input.key { width: 120px; font-weight: bold; }
.prop-input.val { flex: 1; min-width: 100px; }

.colon { margin: 0 5px; font-weight: bold; }
.toggle-icon { cursor: pointer; width: 20px; text-align: center; color: #666; user-select: none; font-size: 10px; }
.folder-label { color: #888; font-size: 0.75rem; margin: 0 10px; flex: 1; font-style: italic; }

.icon-btn, .delete-btn, .add-child-btn { 
  cursor: pointer; margin-left: 5px; border: 1px solid #ccc; background: #eee; 
  padding: 2px 8px; font-size: 0.75rem; font-family: inherit;
}
.icon-btn:hover, .add-child-btn:hover { background: #ddd; color: #000; border-color: #000; }
.delete-btn { color: #999; border-color: transparent; background: transparent; font-size: 1.2rem; line-height: 1; }
.delete-btn:hover { color: #D92323; }
</style>