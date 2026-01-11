// 导航配置常量
export const NAV_ITEMS = [
  { 
    name: '首页', 
    path: '/',
    type: 'link'
  },
  { 
    name: '寰宇社区', 
    type: 'dropdown',
    children: [
      { name: '交流中枢', path: '/DataCenter', mode: 'TERMINAL' }, 
      { name: '艺术大厅', path: '/ArtCenter', mode: 'GALLERY' },
      { name: '娱乐区', path: '/EntertainmentArea'}
    ]
  },{
    name:'太初产品',
    path:'/TCHYproduct',
    type:'link',
  },
  
  { 
    name: '管理', 
    path: '/admin',
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