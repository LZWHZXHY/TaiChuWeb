
import termsText from './legal/terms.zh.js'
import privacyText from './legal/privacy.zh.js'

export default{
    HeaderNav: {
    login: '登录',
    register: '注册',
    logout: '退出登录',
    site_name: '太初寰宇', 
    profile: '个人资料',
    notify:"通知中心",
    members:'注册成员'
  },
    nav:{
    home: '首页',
    community: '寰宇社区',
    data_center: '交流中枢',
    art_center: '艺术大厅',
    entertainment: '娱乐区',
    product: '太初产品',
    manage: '管理'
  },
  login:{
    LoginAccount:"登录太初寰宇",
    Welcome:'欢迎回来',
    apiTest:'测试API连接',
    usernameOrEmail:"用户名或者邮箱",
    password:'密码',
    rememberMe:'记住我',
    forgetPassword:'忘记密码？',
    logIn:'登录',
    Account:'还没有账号？',
    Register:'立即注册'
  },
  Register: {
    terms_title: '太初寰宇服务条款',
    privacy_title: '隐私政策声明',
    terms_content: termsText,
    privacy_content: privacyText,
    registerAccount: '注册太初寰宇',
    createAccount: '创建您的账户，开始探索社区',
    username: '用户名',
    emailAddress: '邮箱地址', 
    emailVerifyCode: '邮箱验证码',
    password: '密码',
    confirmPassword: '确认密码',

    placeholder_username: '请输入用户名（3-20个字符）',
    placeholder_email: '请输入邮箱地址',
    placeholder_code: '请输入6位验证码',
    placeholder_password: '至少6位字符',
    placeholder_confirm: '请再次输入密码',
    send_code: '发送验证码',
    resend_code: '秒后重发',
    submit_btn: '注册账户',
    submit_loading: '注册中...',
    sending: '发送中...',
    agree_prefix: '我已阅读并同意',
    terms: '服务条款',
    privacy: '隐私政策',
    have_account: '已有账户？',
    login_now: '立即登录',
    err_username_empty: '请输入用户名',
    err_username_len: '用户名长度需在3-20个字符之间',
    err_email_empty: '请输入邮箱地址',
    err_email_invalid: '请输入有效的邮箱地址',
    err_code_empty: '请输入验证码',
    err_code_len: '验证码必须是6位数字',
    err_password_empty: '请输入密码',
    err_password_len: '密码长度至少6位',
    err_confirm_empty: '请确认密码',
    err_confirm_match: '两次输入的密码不一致',
    err_terms: '请同意服务条款和隐私政策',
    msg_code_sent: '验证码已发送，请查收',
    msg_code_fail: '发送失败，请稍后重试',
    msg_reg_fail: '注册失败，请稍后重试',
    msg_reg_success: '注册成功！欢迎加入',
    modal_close:'关闭'
  }
}