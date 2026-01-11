<template>
  <div class="doc-item">
    <div class="node-card">
      <div class="card-main">
        <div class="header-row">
          <div class="title-group">
            <span class="expand-toggle" 
                  v-if="hasChildren" 
                  @click="isExpanded = !isExpanded"
            >
              {{ isExpanded ? '▼' : '▶' }}
            </span>
            <span class="placeholder-toggle" v-else>•</span>
            
            <h3 class="node-name">{{ node.name }}</h3>
            <span class="node-type">{{ node.type }}</span>

            <span class="node-author" v-if="node.author">
              <span class="author-icon">✍️</span> {{ node.author }}
            </span>
          </div>
          <span class="update-time" v-if="node.updated_at">{{ formatDate(node.updated_at) }}</span>
        </div>

        <div class="node-desc" v-if="node.description">
          {{ node.description }}
        </div>

        <div class="node-props" v-if="parsedProps">
          <div v-for="(val, key) in parsedProps" :key="key" class="prop-entry">
            <span class="k">{{ key }}:</span>

            <div v-if="isObject(val)" class="v-nested">
              <div v-for="(subVal, subKey) in val" :key="subKey" class="sub-prop">
                <span class="sub-k">{{ subKey }}:</span>
                <span class="sub-v">{{ subVal }}</span>
              </div>
            </div>

            <span v-else class="v">{{ val }}</span>
          </div>
        </div>

        <div class="node-images" v-if="parsedImages.length > 0">
          <div v-for="(img, idx) in parsedImages" :key="idx" class="mini-img" @click.stop="$emit('preview', img)">
            <img :src="img" loading="lazy" />
          </div>
        </div>
      </div>
    </div>

    <div class="children-container" v-if="hasChildren && isExpanded">
      <DocItem 
        v-for="child in node.children" 
        :key="child.id" 
        :node="child"
        @preview="$emit('preview', $event)" 
      />
    </div>
  </div>
</template>

<script>
export default {
  name: 'DocItem',
  props: {
    node: { type: Object, required: true }
  },
  data() {
    return {
      isExpanded: true
    };
  },
  computed: {
    hasChildren() {
      return this.node.children && this.node.children.length > 0;
    },
    parsedProps() {
      if (!this.node.meta_data_json) return null;
      try {
        const json = typeof this.node.meta_data_json === 'string' 
          ? JSON.parse(this.node.meta_data_json) 
          : this.node.meta_data_json;
        return Object.keys(json).length > 0 ? json : null;
      } catch { return null; }
    },
    parsedImages() {
      const raw = this.node.image_url;
      if (!raw) return [];
      try {
        const res = JSON.parse(raw);
        return Array.isArray(res) ? res : [raw];
      } catch { return [raw]; }
    }
  },
  methods: {
    formatDate(str) {
      if(!str) return '';
      return new Date(str).toLocaleDateString();
    },
    isObject(val) {
      return val !== null && typeof val === 'object' && !Array.isArray(val);
    }
  }
};
</script>

<style scoped>
/* 基础容器 */
.doc-item {
  margin-bottom: 15px;
  position: relative;
}

/* 递归子节点连接线 */
.children-container {
  margin-left: 24px; 
  padding-left: 18px;
  border-left: 2px solid #e8e8e8; 
  margin-top: 12px;
}

/* 核心卡片样式 */
.node-card {
  background: white;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  border: 1px solid #f0f0f0;
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}

.node-card:hover {
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.12);
  border-color: #d9d9d9;
  transform: translateY(-2px);
}

.card-main {
  padding: 16px;
}

/* 标题行布局 */
.header-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 12px;
}

.title-group {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap; /* 适配手机端，防止名字太长挤出去 */
}

.expand-toggle {
  cursor: pointer;
  color: #bfbfbf;
  font-size: 0.8rem;
  width: 18px;
  text-align: center;
  user-select: none;
  transition: color 0.3s;
}

.expand-toggle:hover {
  color: #1890ff;
}

.placeholder-toggle {
  color: #e8e8e8;
  width: 18px;
  text-align: center;
}

.node-name {
  margin: 0;
  font-size: 1.15rem;
  color: #1f1f1f;
  font-weight: 600;
}

.node-type {
  font-size: 0.75rem;
  background: #e6f7ff;
  color: #1890ff;
  padding: 2px 8px;
  border-radius: 4px;
  border: 1px solid #91d5ff;
  font-weight: 500;
}

/* 🔥 作者标签样式 */
.node-author {
  font-size: 0.8rem;
  color: #8c8c8c;
  margin-left: 5px;
  font-style: italic;
  display: flex;
  align-items: center;
}
.author-icon {
  margin-right: 4px;
  font-style: normal;
  font-size: 0.85rem;
}

.update-time {
  font-size: 0.8rem;
  color: #bfbfbf;
}

/* 描述文本 */
.node-desc {
  font-size: 0.95rem;
  color: #595959;
  margin-bottom: 14px;
  line-height: 1.6;
  white-space: pre-wrap;
  background: #fbfbfb;
  padding: 10px;
  border-radius: 6px;
  border-left: 3px solid #eee;
}

/* 详细设定 面板样式 */
.node-props {
  display: flex;
  flex-direction: column;
  gap: 8px;
  background: #fcfcfc;
  padding: 12px;
  border-radius: 8px;
  border: 1px solid #f0f0f0;
  margin-bottom: 12px;
}

.prop-entry {
  font-size: 0.9rem;
  display: flex;
  flex-wrap: wrap;
  align-items: flex-start;
}

.prop-entry .k {
  color: #8c8c8c;
  margin-right: 10px;
  font-weight: 600;
  min-width: 60px;
}

.prop-entry .v {
  color: #262626;
  flex: 1;
}

/* 嵌套参数块 */
.v-nested {
  flex: 1;
  min-width: 100%; 
  margin-top: 8px;
  margin-left: 8px;
  padding: 10px 14px;
  background: #f0f2f5;
  border-radius: 6px;
  border-left: 4px solid #1890ff;
  box-sizing: border-box;
}

.sub-prop {
  font-size: 0.85rem;
  margin-bottom: 4px;
  display: flex;
}

.sub-prop .sub-k {
  color: #595959;
  margin-right: 8px;
  white-space: nowrap;
}

.sub-prop .sub-v {
  color: #000;
  font-weight: 500;
  word-break: break-all;
}

/* 图集展示 */
.node-images {
  display: flex;
  gap: 10px;
  overflow-x: auto;
  padding: 4px 0;
}

.mini-img {
  width: 90px;
  height: 90px;
  flex-shrink: 0;
  border-radius: 6px;
  overflow: hidden;
  cursor: zoom-in;
  border: 1px solid #f0f0f0;
  background: #eee;
}

.mini-img img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.4s;
}

.mini-img:hover img {
  transform: scale(1.15);
}

.node-images::-webkit-scrollbar {
  height: 4px;
}
.node-images::-webkit-scrollbar-thumb {
  background: #ddd;
  border-radius: 2px;
}
</style>