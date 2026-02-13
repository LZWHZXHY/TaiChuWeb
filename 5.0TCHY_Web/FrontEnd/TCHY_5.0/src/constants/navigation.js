// navigation.js

export const NAV_ITEMS = [
  // 1. 首页
  { 
    name: 'nav.home', 
    path: '/Intro', 
    type: 'link'
  },
  
  // ============================================
  // ✨ 新增：超级菜单 (显示为 COMMUNITY // 社区)
  // ============================================
  { 
    // 这个 name 主要用于 v-for 的 key，实际显示的文字在 CyberMegaMenu.vue 组件里修改
    name: 'nav.mega_menu_entry', 
    type: 'mega' 
  },

  // ============================================
  // 📦 保留：旧的下拉菜单 (为了兼容老页面)
  // ============================================
  { 
    // 建议在你的 zh.js / en.js 里加一个 'nav.legacy' : '旧版功能'
    // 或者暂时借用 'nav.resourse' 这个 key
    name: 'nav.community', // 这里依然显示 "社区" (或者你可以改成 nav.manage 等其他key区分)
    type: 'dropdown',
    children: [
      { name: 'nav.push_center', path: '/MainPush'},
      { name: 'nav.data_center', path: '/DataCenter', mode: 'TERMINAL' }, // 交流中枢
      { name: 'nav.work_center', path: '/WorkCenter', mode: 'GALLERY' }, // 作品大厅
      { name: 'nav.entertainment', path: '/EntertainmentArea'},
      { name: 'nav.missionCenter', path:'/MissionCenter'},
      { name: 'nav.resourse', path:'/Resource'},
      { name: 'nav.RankCenter', path:'/RankCenter'}
    ]
  },

  // 3. 实验区
  {
    name: 'nav.ExperimentalArea', 
    path: '/ExperimentalArea',
    type: 'link',
  },

  // 4. 管理后台
  { 
    name: 'nav.manage', 
    path: '/admin',
    type: 'link',
    minRole: 1 
  }
]

export const NAV_CONFIG = {
  height: 72,
  scrolledThreshold: 20,
  animationDuration: 300,
  dropdown: {
    openDelay: 200,
    closeDelay: 300,
    animationDuration: 200
  }
}