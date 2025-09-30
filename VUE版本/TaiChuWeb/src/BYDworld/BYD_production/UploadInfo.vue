<template>
  <div class="upload-info">
    <h3>
      上传通道方式说明
    </h3>
    <div class="info-body">
      <p>
        项目资源统一通过
        <a
          href="https://taichuhuanyu.us5.ug.link/desktop/?os=ugospro#"
          target="_blank"
          class="main-link"
        >
          太初寰宇社区网盘
        </a>
        管理。完成内容制作后，请按以下步骤上传：
      </p>
      <ol>
        <li>
          打开
          <a
            href="https://taichuhuanyu.us5.ug.link/desktop/?os=ugospro#"
            target="_blank"
            class="main-link"
          >
            太初寰宇社区网盘
          </a>
        </li>
        <li>使用个人账户登录（如无账户请先注册）</li>
        <li>在指定区域上传你的内容</li>
      </ol>
      <div class="tips">
        如无法上传或未见上传区域，可能是权限不足。请联系管理员或“腾蛇（总监）”分配权限。
      </div>
    </div>
  </div>
</template>

<script setup>
// 可用 props/API 传入说明内容
</script>

<style scoped>
.upload-info {
  background: #f6f9fc;
  border-radius: 12px;
  padding: 18px 24px 14px 24px;
  box-shadow: 0 2px 8px #e0e7ef55;
  color: #333;
  font-size: 16px;
  max-width: 560px;
  margin: 0 auto;
}

.upload-info h3 {
  font-size: 18px;
  font-weight: 700;
  color: #222;
  margin-bottom: 12px;
  letter-spacing: 1px;
}

.info-body {
  font-size: 15px;
  line-height: 1.7;
}

.upload-info a.main-link {
  color: #2563eb;
  text-decoration: none;
  font-weight: 600;
  border-bottom: 2px dotted #2563eb;
  cursor: pointer;
  transition: border-bottom 0.2s;
}
.upload-info a.main-link:hover {
  border-bottom: 2px solid #2563eb;
  background: #eaf1fb;
}

.upload-info ol {
  margin: 12px 0 8px 0;
  padding-left: 22px;
}

.upload-info ol li {
  margin-bottom: 5px;
}

.tips {
  background: #fdf6e3;
  border-radius: 8px;
  padding: 8px 11px;
  margin-top: 9px;
  color: #b58900;
  font-size: 14px;
  font-weight: 500;
  line-height: 1.6;
}
</style>