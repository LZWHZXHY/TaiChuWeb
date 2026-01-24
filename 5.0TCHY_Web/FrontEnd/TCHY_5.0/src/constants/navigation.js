// navigation.js

export const NAV_ITEMS = [
  { 
    name: 'nav.home', // 原来是 '首页'
    path: '/Intro',
    type: 'link'
  },
  { 
    name: 'nav.community', // 原来是 '寰宇社区'
    type: 'dropdown',
    children: [
      { name: 'nav.push_center', path: '/MainPush'},
      { name: 'nav.data_center', path: '/DataCenter', mode: 'TERMINAL' }, // 原来是 '交流中枢'
      { name: 'nav.work_center', path: '/WorkCenter', mode: 'GALLERY' },    // 原来是 '艺术大厅'
      { name: 'nav.entertainment', path: '/EntertainmentArea'},        // 原来是 '娱乐区'
      { name: 'nav.missionCenter', path:'/MissionCenter'},
      { name: 'nav.RankCenter', path:'/RankCenter'}
    ]
  },
  {
    name: 'nav.product', // 原来是 '太初产品'
    path: '/TCHYproduct',
    type: 'link',
  },
  {
    name: 'nav.ExperimentalArea', 
    path: '/ExperimentalArea',
    type: 'link',
  },
  { 
    name: 'nav.manage', // 原来是 '管理'
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