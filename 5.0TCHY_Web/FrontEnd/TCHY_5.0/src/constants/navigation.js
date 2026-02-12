// navigation.js

export const NAV_ITEMS = [
  { 
    name: 'nav.home', 
    path: '/Intro',
    type: 'link'
  },
  { 
    name: 'nav.community', 
    type: 'dropdown',
    children: [
      { name: 'nav.push_center', path: '/MainPush'},
      { name: 'nav.data_center', path: '/DataCenter', mode: 'TERMINAL' },
      { name: 'nav.work_center', path: '/WorkCenter', mode: 'GALLERY' },
      { name: 'nav.entertainment', path: '/EntertainmentArea'},
      { name: 'nav.missionCenter', path:'/MissionCenter'},
      { name: 'nav.resourse', path:'/Resource'},
      { name: 'nav.RankCenter', path:'/RankCenter'}
    ]
  },
  {
    name: 'nav.ExperimentalArea', 
    path: '/ExperimentalArea',
    type: 'link',
  },
  { 
    name: 'nav.manage', 
    path: '/admin',
    type: 'link',
    // ✅ 【新增】添加这一行，对应 HeaderNav.vue 里的 item.minRole 判断
    minRole: 1  
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