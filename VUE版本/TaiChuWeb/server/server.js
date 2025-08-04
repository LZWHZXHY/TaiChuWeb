// server/server.js
const express = require('express');
const fs = require('fs');
const cors = require('cors');
const path = require('path');
const jwt = require('jsonwebtoken');
const pool = require('./db'); // 使用安全连接

const app = express();
const configPath = path.join(__dirname, 'config.json');

// 安全中间件
app.use(cors({
  origin: process.env.NODE_ENV === 'production' 
    ? 'https://your-domain.com' 
    : 'http://localhost:8080'
}));
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// 身份验证中间件
const authenticate = (req, res, next) => {
  const token = req.headers.authorization?.split(' ')[1];
  
  if (!token) {
    return res.status(401).json({ error: '未提供认证令牌' });
  }

  try {
    const decoded = jwt.verify(token, process.env.JWT_SECRET);
    req.user = decoded;
    next();
  } catch (err) {
    res.status(401).json({ error: '无效令牌' });
  }
};

// 登录获取令牌
app.post('/api/login', async (req, res) => {
  const { username, password } = req.body;
  
  try {
    const [users] = await pool.query(
      'SELECT id FROM users WHERE username = ? AND password = ?',
      [username, password] // 实际应用中密码应加密存储
    );
    
    if (users.length === 0) {
      return res.status(401).json({ error: '用户名或密码错误' });
    }
    
    const token = jwt.sign(
      { userId: users[0].id },
      process.env.JWT_SECRET,
      { expiresIn: '1h' }
    );
    
    res.json({ token });
  } catch (err) {
    console.error('登录错误:', err);
    res.status(500).json({ error: '服务器错误' });
  }
});

// 获取配置（公开）
app.get('/api/config', (req, res) => {
  try {
    const config = JSON.parse(fs.readFileSync(configPath, 'utf-8'));
    res.json(config);
  } catch (err) {
    console.error('配置读取错误:', err);
    res.status(500).json({ error: '无法读取配置' });
  }
});

// 修改配置（需要认证）
app.post('/api/config', authenticate, (req, res) => {
  try {
    fs.writeFileSync(configPath, JSON.stringify(req.body, null, 2));
    res.json({ success: true });
  } catch (err) {
    console.error('配置写入错误:', err);
    res.status(500).json({ error: '无法保存配置' });
  }
});

// 获取数据库数据（需要认证）
app.get('/api/db-data', authenticate, async (req, res) => {
  try {
    const [rows] = await pool.query('SELECT * FROM your_table');
    res.json(rows);
  } catch (err) {
    console.error('数据库查询错误:', err);
    res.status(500).json({ error: '数据库查询失败' });
  }
});

// 添加数据（需要认证）
app.post('/api/db-data', authenticate, async (req, res) => {
  const { name, value } = req.body;
  
  try {
    const [result] = await pool.query(
      'INSERT INTO your_table (name, value) VALUES (?, ?)',
      [name, value]
    );
    res.json({ success: true, id: result.insertId });
  } catch (err) {
    console.error('数据库插入错误:', err);
    res.status(500).json({ error: '数据插入失败' });
  }
});

// 错误处理中间件
app.use((err, req, res, next) => {
  console.error('服务器错误:', err.stack);
  res.status(500).json({ error: '内部服务器错误' });
});

const PORT = process.env.PORT || 3001;
app.listen(PORT, () => {
  console.log(`后台服务已启动：http://localhost:${PORT}`);
  console.log(`环境模式：${process.env.NODE_ENV || 'development'}`);
});