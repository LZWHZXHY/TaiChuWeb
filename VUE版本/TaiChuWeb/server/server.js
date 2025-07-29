// server/server.js
const express = require('express');
const fs = require('fs');
const cors = require('cors');
const path = require('path');
const mysql = require('mysql2/promise'); // 添加MySQL驱动




const app = express();
const configPath = path.join(__dirname, 'config.json');


// 加载数据库配置
const dbConfig = {
  host: 'bj-cynosdbmysql-grp-j3ff1zck.sql.tencentcdb.com',
  port: 20379,
  user: 'dev_admin',
  password: 'your_password', // 替换为实际密码
  database: 'your_database',
  waitForConnections: true,
  connectionLimit: 10
};


// 创建数据库连接池
const pool = mysql.createPool(dbConfig);







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







// 新增：获取数据库数据
app.get('/api/db-data', async (req, res) => {
  try {
    const [rows] = await pool.query('SELECT * FROM your_table');
    res.json(rows);
  } catch (err) {
    console.error('数据库查询错误:', err);
    res.status(500).json({ error: '数据库查询失败' });
  }
});

// 新增：插入数据到数据库
app.post('/api/db-data', async (req, res) => {
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






app.listen(3001, () => {
  console.log('后台服务已启动：http://localhost:3001');
  console.log('数据库连接池已创建');
});
