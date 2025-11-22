// 导航配置常量
export const NAV_ITEMS = [
  { 
    name: '首页', 
    path: '/',
    type: 'link'
  },
  { 
    name: '社区', 
    type: 'dropdown',
    children: [
      { name: '绘画社区', path: '/drawing' },
      { name: '编程技术', path: '/community/programming' },
      { name: '火柴人板块', path: '/chai' },
      { name: '游戏交流', path: '/community/gaming' },
      { name: '音乐分享', path: '/community/music' },
      { name: '资源板块', path: '/资源' },
      { name: '文学创作', path: '/community/writing' }
    ]
  },{
    name:'太初产品',
    path:'/product',
    type:'link',


  },
  { 
    name: '管理', 
    path: '/admin',
    type: 'link'
  },
  { 
    name: '意见箱', 
    path: '/suggest',
    type: 'link'
  }
]

export const NAV_CONFIG = {
  height: 70,
  scrolledThreshold: 20,
  animationDuration: 300,
  dropdown: {
    openDelay: 200,
    closeDelay: 300,
    animationDuration: 200
  }
}