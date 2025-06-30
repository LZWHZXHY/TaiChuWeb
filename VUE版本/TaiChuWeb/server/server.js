// server/server.js
const express = require('express');
const fs = require('fs');
const cors = require('cors');
const path = require('path');

const app = express();
const configPath = path.join(__dirname, 'config.json');

app.use(cors());
app.use(express.json());

// 获取配置
app.get('/api/config', (req, res) => {
  const config = JSON.parse(fs.readFileSync(configPath, 'utf-8'));
  res.json(config);
});

// 修改配置
app.post('/api/config', (req, res) => {
  const newConfig = req.body;
  fs.writeFileSync(configPath, JSON.stringify(newConfig, null, 2));
  res.json({ success: true });
});

app.listen(3001, () => {
  console.log('后台服务已启动：http://localhost:3001');
});
