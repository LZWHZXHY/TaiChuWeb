<template>
  <div class="property-item">
    <div class="prop-row">
      <span 
        class="toggle-icon" 
        @click="isOpen = !isOpen"
        :style="{ visibility: isFolder ? 'visible' : 'hidden' }"
      >
        {{ isOpen ? '▼' : '▶' }}
      </span>

      <input 
        :value="modelValue.key" 
        @input="updateKey"
        placeholder="属性名" 
        class="prop-key" 
      />
      
      <span class="colon">:</span>

      <template v-if="!isFolder">
        <input 
          :value="modelValue.value"
          @input="updateValue" 
          placeholder="属性值" 
          class="prop-value" 
        />
        <button class="icon-btn" @click="convertToFolder" title="变为参数组">📂</button>
      </template>

      <template v-else>
        <span class="folder-label"> (包含 {{ modelValue.children.length }} 项) </span>
        <button class="add-child-btn" @click="addChild">+ 子项</button>
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
  props: {
    modelValue: { type: Object, required: true }
  },
  emits: ['update:modelValue', 'delete'],
  data() {
    return { isOpen: true };
  },
  computed: {
    isFolder() {
      return Array.isArray(this.modelValue.children);
    }
  },
  methods: {
    updateKey(e) {
      this.$emit('update:modelValue', { ...this.modelValue, key: e.target.value });
    },
    updateValue(e) {
      this.$emit('update:modelValue', { ...this.modelValue, value: e.target.value });
    },
    convertToFolder() {
      const newItem = { ...this.modelValue, children: [] };
      delete newItem.value; 
      this.$emit('update:modelValue', newItem);
      this.isOpen = true;
    },
    addChild() {
      const currentChildren = this.modelValue.children ? [...this.modelValue.children] : [];
      currentChildren.push({ key: '', value: '' });
      this.$emit('update:modelValue', { ...this.modelValue, children: currentChildren });
      this.isOpen = true;
    },
    updateChild(index, newChildData) {
      const newChildren = [...this.modelValue.children];
      newChildren[index] = newChildData;
      this.$emit('update:modelValue', { ...this.modelValue, children: newChildren });
    },
    deleteChild(index) {
      const newChildren = [...this.modelValue.children];
      newChildren.splice(index, 1);
      this.$emit('update:modelValue', { ...this.modelValue, children: newChildren });
    }
  }
};
</script>

<style scoped>
.property-item { display: flex; flex-direction: column; width: 100%; }
.prop-row { display: flex; align-items: center; margin-bottom: 8px; width: 100%; }
.children-block { margin-left: 20px; padding-left: 10px; border-left: 1px dashed #ccc; margin-top: 4px; }
.prop-key { width: 140px; padding: 6px; border: 1px solid #ddd; border-radius: 4px; font-weight: bold; }
.prop-value { flex: 1; padding: 6px; border: 1px solid #ddd; border-radius: 4px; min-width: 100px; }
.colon { margin: 0 8px; font-weight: bold; color: #999; }
.toggle-icon { cursor: pointer; width: 20px; text-align: center; color: #666; user-select: none; font-size: 12px; }
.folder-label { color: #999; font-size: 0.8rem; margin: 0 10px; flex: 1; font-style: italic; }
.icon-btn, .delete-btn, .add-child-btn { cursor: pointer; margin-left: 5px; border: 1px solid #ddd; background: #fff; border-radius: 4px; padding: 4px 10px; font-size: 0.8rem; white-space: nowrap; }
.delete-btn { color: #ff4d4f; border-color: #ffccc7; }
.add-child-btn { color: #52c41a; border-color: #b7eb8f; background: #f6ffed; }
</style>