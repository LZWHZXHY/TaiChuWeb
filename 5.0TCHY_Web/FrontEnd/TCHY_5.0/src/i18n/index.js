import { createI18n } from 'vue-i18n'
import zh from '../locales/zh'
import en from '../locales/en'

// 1. 定义默认语言变量
const DEFAULT_LANG = 'zh' 

// 2. 获取本地存储。
// 逻辑：如果有缓存用缓存，没有缓存就用上面定义的 DEFAULT_LANG (zh)
const savedLocale = localStorage.getItem('app_language') || DEFAULT_LANG

const i18n = createI18n({
  legacy: false, 
  globalInjection: true,
  locale: savedLocale, // <--- 这里决定了初始语言
  fallbackLocale: DEFAULT_LANG, // <--- 这里决定了缺省时回退的语言（设为 zh）
  messages: {
    zh,
    en
  }
})

export default i18n