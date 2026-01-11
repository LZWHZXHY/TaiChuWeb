<template>
  <div class="admin-panel" style="padding:32px;">
    <el-button type="primary" @click="openAddDialog" style="margin-bottom: 24px;">新增规则</el-button>
    <!-- 规则树 -->
    <el-tree
      class="rules-tree"
      :data="rules"
      node-key="id"
      :props="treeProps"
      highlight-current
      :expand-on-click-node="false"
      :default-expand-all="true"
      empty-text="暂无规则"
    >
      <template #default="{ node, data }">
        <div class="tree-node">
          <div class="tree-node__main">
            <div class="tn-title-row">
              <span class="tn-title">{{ data.title }}</span>
              <span v-if="data.ruleNumber" class="tn-meta">[{{ data.ruleNumber }}]</span>
              <span v-if="!data.enabled" class="tn-disable">已禁用</span>
            </div>
            <div class="tn-meta-row">
              <span v-if="data.penalty" class="tn-penalty">处罚：{{ data.penalty }}</span>
              <span class="tn-version">版本: {{ data.version }}</span>
              <span class="tn-order">排序: {{ data.orderNum }}</span>
            </div>
            <div v-if="data.content" class="tn-content">
              {{ data.content }}
            </div>
            <div class="tn-time">
              创建时间：{{ formatDate(data.createAt) }}
            </div>
          </div>
          <div class="tree-node__actions">
            <el-button size="small" @click.stop="openEditDialog(data)">编辑</el-button>
            <el-button size="small" type="danger" @click.stop="openDeleteDialog(data)">删除</el-button>
          </div>
        </div>
      </template>
    </el-tree>

    <!-- 新增/编辑/删除通用弹窗 -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="500px"
      @closed="resetDialogForm"
      destroy-on-close
    >
      <!-- 新增/编辑表单 -->
      <template v-if="dialogMode !== 'delete'">
        <el-form ref="formRef" :model="form" :rules="formRules" label-width="100px" status-icon>
          <el-form-item label="规则标题" prop="title">
            <el-input v-model="form.title" placeholder="请输入规则标题" clearable maxlength="250" />
          </el-form-item>
          <el-form-item label="规则内容" prop="content">
            <el-input v-model="form.content" type="textarea" placeholder="请输入详细规则内容" :rows="4" maxlength="2000" show-word-limit />
          </el-form-item>
          <el-form-item label="规则编号" prop="ruleNumber">
            <el-input v-model="form.ruleNumber" placeholder="如: RULE-001" maxlength="32" />
          </el-form-item>
          <el-form-item label="排序号" prop="orderNum">
            <el-input-number v-model="form.orderNum" :min="0" :max="999" />
          </el-form-item>
          <el-form-item label="处罚说明" prop="penalty">
            <el-input v-model="form.penalty" placeholder="可选：违规处罚说明" maxlength="255" />
          </el-form-item>
          <el-form-item label="版本号" prop="version">
            <el-input v-model="form.version" placeholder="如: 1.0" maxlength="20" />
          </el-form-item>
          <el-form-item label="启用">
            <el-switch v-model="form.enabled" active-text="启用" inactive-text="禁用" />
          </el-form-item>
          <el-form-item label="父级规则">
            <el-select v-model="form.parentId" clearable placeholder="可为空(根规则)">
              <el-option label="无父级" :value="''" />
              <el-option
                v-for="item in flatOptions"
                :key="item.id"
                :label="item.title"
                :value="item.id"
                :disabled="item.id === form.id"
              />
            </el-select>
          </el-form-item>
        </el-form>
      </template>
      <!-- 删除确认 -->
      <template v-else>
        <div style="padding:36px 0;text-align:center;">
          确定要删除规则「{{ form.title }}」吗？
          <div style="margin-top:10px; color:#888; font-size:13px;">该操作不可恢复，子规则（如有）会一并删除。</div>
        </div>
      </template>
      <template #footer>
        <el-button @click="dialogVisible = false" :disabled="submitting">取消</el-button>
        <el-button v-if="dialogMode === 'add'" type="primary" :loading="submitting" @click="submitForm">添加规则</el-button>
        <el-button v-if="dialogMode === 'edit'" type="primary" :loading="submitting" @click="submitForm">保存修改</el-button>
        <el-button v-if="dialogMode === 'delete'" type="danger" :loading="submitting" @click="deleteRule">确认删除</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import apiClient from '@/utils/api'

const rules = ref([])
const loading = ref(false)
const dialogVisible = ref(false)
const dialogMode = ref('add')
const submitting = ref(false)
const dialogTitle = computed(() =>
  dialogMode.value === "add" ? "新增规则" :
    dialogMode.value === "edit" ? "修改规则" : "删除规则"
)
const formRef = ref()
const form = reactive({
  id: '',
  title: '',
  content: '',
  ruleNumber: '',
  parentId: '',
  orderNum: 0,
  penalty: '',
  version: '1.0',
  enabled: true
})

const treeProps = { children: 'children', label: 'title' }

const normalizeData = (node) => {
  return {
    id: node.Id,
    title: node.Title,
    content: node.Content,
    ruleNumber: node.RuleNumber,
    parentId: node.ParentId,
    orderNum: node.OrderNum,
    penalty: node.Penalty,
    version: node.Version,
    enabled: node.Enabled,
    createAt: node.CreateAt,
    updateAt: node.UpdateAt,
    // 递归处理子节点，如果 Children 存在则继续映射
    children: node.Children && node.Children.length > 0 
      ? node.Children.map(normalizeData) 
      : []
  }
}


const flatOptions = computed(() => {
  const arr = []
  function walk(nodes) {
    for (const n of nodes) {
      arr.push({ id: n.id, title: n.title })
      if (n.children && n.children.length) walk(n.children)
    }
  }
  walk(rules.value)
  return arr
})

function formatDate(val) {
  if (!val) return ''
  try {
    return new Date(val).toLocaleString()
  } catch {
    return val
  }
}

async function fetchRules() {
  loading.value = true
  try {
    const res = await apiClient.get('/rules/tree')
    
    // 获取原始数据
    const rawData = Array.isArray(res.data?.data) ? res.data.data : []
    
    // 🔥 关键步骤：将原始的大写数据转换为小写数据
    rules.value = rawData.map(normalizeData)
    
  } catch (err) {
    console.error(err) // 方便调试看报错
    rules.value = []
    ElMessage.error('加载规则失败')
  } finally {
    loading.value = false
  }
}

function openAddDialog() {
  dialogMode.value = 'add'
  dialogVisible.value = true
  Object.assign(form, {
    id: '',
    title: '',
    content: '',
    ruleNumber: '',
    parentId: '',
    orderNum: 0,
    penalty: '',
    version: '1.0',
    enabled: true
  })
}

function openEditDialog(rule) {
  dialogMode.value = 'edit'
  dialogVisible.value = true
  Object.assign(form, {
    id: rule.id,
    title: rule.title,
    content: rule.content,
    ruleNumber: rule.ruleNumber,
    parentId: rule.parentId == null ? '' : rule.parentId,
    orderNum: rule.orderNum ?? 0,
    penalty: rule.penalty,
    version: rule.version,
    enabled: rule.enabled,
  })
}

function openDeleteDialog(rule) {
  dialogMode.value = 'delete'
  dialogVisible.value = true
  Object.assign(form, {
    id: rule.id,
    title: rule.title
  })
}

function resetDialogForm() {
  if (formRef.value) formRef.value.resetFields()
  Object.assign(form, {
    id: '',
    title: '',
    content: '',
    ruleNumber: '',
    parentId: '',
    orderNum: 0,
    penalty: '',
    version: '1.0',
    enabled: true
  })
}

const formRules = {
  title: [
    { required: true, message: '请输入规则标题', trigger: 'blur' },
    { min: 1, max: 250, message: '标题长度在1~250个字符', trigger: 'blur' }
  ],
  content: [
    { required: true, message: '请输入规则内容', trigger: 'blur' }
  ]
}

async function submitForm() {
  if (!formRef.value) return
  await formRef.value.validate()
  submitting.value = true
  try {
    const data = {
      title: form.title,
      content: form.content,
      ruleNumber: form.ruleNumber,
      parentId: form.parentId === '' ? null : form.parentId,
      orderNum: form.orderNum,
      penalty: form.penalty,
      version: form.version,
      enabled: form.enabled
    }
    if (dialogMode.value === 'add') {
      await apiClient.post('/rules/create', data)
      ElMessage.success('添加成功')
    } else if (dialogMode.value === 'edit') {
      await apiClient.put(`/rules/${form.id}`, data)
      ElMessage.success('修改成功')
    }
    dialogVisible.value = false
    await fetchRules()
  } catch (e) {
    ElMessage.error('操作失败')
  } finally {
    submitting.value = false
  }
}

async function deleteRule() {
  submitting.value = true
  try {
    await apiClient.delete(`/rules/${form.id}`)
    ElMessage.success('已删除')
    dialogVisible.value = false
    await fetchRules()
  } catch (e) {
    ElMessage.error('删除失败')
  } finally {
    submitting.value = false
  }
}

onMounted(fetchRules)
</script>

<style scoped>
.admin-panel {
  width: 100%;
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #e4e7ed 100%);
}
.rules-tree {
  max-width: 700px;
  margin: 0 auto 32px auto;
  background: #fff;
  padding: 24px;
  border-radius: 10px;
  box-shadow: 0 2px 16px 0 rgba(0,0,0,0.06);
  width: 100%;
}

/* 重点，取消 el-tree 默认单行高度限制，让节点内容高度自适应 */
:global(.el-tree) {
  --el-tree-node-content-height: auto;
}
:global(.el-tree-node__content) {
  height: auto !important;
  align-items: stretch;
  min-height: 48px;
}

/* 卡片式节点样式 */
.tree-node {
  display: flex;
  align-items: stretch;
  border-radius: 10px;
  background: #fff;
  margin-bottom: 16px;
  border: 1px solid #eff0f2;
  padding: 18px 16px 16px 18px;
  width: 100%;
  box-sizing: border-box;
  transition: box-shadow .18s, border-color .18s;
}
.tree-node__main {
  flex: 1;
  width: 100%;
  min-width: 0;
  box-sizing: border-box;
}
.tn-title-row {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 18px;
  font-weight: 600;
  color: #222;
  margin-bottom: 4px;
}
.tn-meta {
  font-size: 13px;
  color: #888;
}
.tn-disable {
  color: #f56c6c;
  font-size: 13px;
  margin-left: 6px;
  font-weight: 500;
}
.tn-meta-row {
  display: flex;
  align-items: center;
  gap: 16px;
  font-size: 13px;
  color: #636363;
  margin-bottom: 7px;
  margin-top: 2px;
}
.tn-penalty {
  color: #f56c6c;
  font-weight: 500;
}
.tn-content {
  color: #374151;
  font-size: 15px;
  line-height: 1.74;
  background: #f4f8fb;
  border-radius: 4px;
  padding: 10px 13px 10px 13px;
  margin: 9px 0 9px 0;
  word-break: break-word;
  white-space: pre-line;
  width: 100%;
  box-sizing: border-box;
}
.tn-time {
  font-size: 12px;
  color: #aaa;
}
.tree-node__actions {
  display: flex;
  flex-direction: column;
  justify-content: flex-start;
  gap: 14px;
  align-items: flex-end;
  min-width: 70px;
  margin-left: 28px;
  margin-top: 6px;
}
.tree-node:hover {
  border-color: #90caf9;
  box-shadow: 0 2px 12px 0 rgba(30,144,255,.08);
  background: #f4f8fb;
}

@media (max-width: 600px) {
  .rules-tree {
    max-width: 100vw;
    padding: 7vw 2vw;
  }
  .tree-node {
    flex-direction: column;
    padding: 10px 6px 10px 8px;
  }
  .tree-node__main { margin-bottom: 12px; }
  .tree-node__actions { flex-direction: row; gap: 10px; min-width: 0; margin-left: 0; }
}
</style>