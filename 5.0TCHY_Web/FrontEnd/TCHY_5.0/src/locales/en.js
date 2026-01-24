
import termsText from './legal/terms.en.js'
import privacyText from './legal/privacy.en.js'


export default {
  HeaderNav: {
    login: 'Login',
    register: 'Register',
    logout: 'Logout',
    site_name: 'TCHY', // 或者保持你之前的 'TCHY'
    profile: 'Profile',
    notify: 'Notification Center',
    members: 'Members'
  },
  nav: {
    home: 'Intro', // 如果想叫首页，建议用 'Home'
    community: 'TC Community',
    data_center: 'Communication Center',
    work_center: 'product Hall',
    entertainment: 'Entertainment',
    product: 'Products',
    manage: 'Admin'
  },
  login: {
    LoginAccount: "Login to TCHY",
    Welcome: 'Welcome Back',
    apiTest: 'Test API Connection',
    usernameOrEmail: "Username or Email",
    password: 'Password',
    rememberMe: 'Remember Me',
    forgetPassword: 'Forgot Password?',
    logIn: 'Log In',
    Account: "Don't have an account?", // 对应“还没有账号？”
    Register: 'Register Now' // 对应“立即注册”
  },
  Register: {

    terms_title: 'Terms of Service',
    privacy_title: 'Privacy Policy',
    
    terms_content: termsText,
    privacy_content: privacyText,

    registerAccount: 'Register Account',
    createAccount: 'Create your account to explore',
    username: 'Username',
    emailAddress: 'Email Address',
    emailVerifyCode: 'Verification Code',
    password: 'Password',
    confirmPassword: 'Confirm Password',
    // --- 新增补全 ---
    placeholder_username: '3-20 characters',
    placeholder_email: 'Enter your email',
    placeholder_code: '6-digit code',
    placeholder_password: 'At least 6 characters',
    placeholder_confirm: 'Re-enter password',
    send_code: 'Send Code',
    resend_code: 's resend',
    submit_btn: 'Sign Up',
    submit_loading: 'Registering...',
    sending: 'Sending...',
    agree_prefix: 'I have read and agree to',
    terms: 'Terms',
    privacy: 'Privacy',
    have_account: 'Already have an account?',
    login_now: 'Log in',
    // --- 错误提示 ---
    err_username_empty: 'Username is required',
    err_username_len: 'Username must be 3-20 chars',
    err_email_empty: 'Email is required',
    err_email_invalid: 'Invalid email address',
    err_code_empty: 'Code is required',
    err_code_len: 'Code must be 6 digits',
    err_password_empty: 'Password is required',
    err_password_len: 'Password must be at least 6 chars',
    err_confirm_empty: 'Please confirm password',
    err_confirm_match: 'Passwords do not match',
    err_terms: 'Please agree to the terms',
    msg_code_sent: 'Code sent, please check email',
    msg_code_fail: 'Failed to send code',
    msg_reg_fail: 'Registration failed',
    msg_reg_success: 'Registration successful! Welcome'
  },
  legal:{
      system_title: "法律协议终端", // 页面大标题
      terms_title: "服务条款 (TOS)",
      privacy_title: "隐私政策 (PRIVACY)",
      terms_content: termsText,       // 这里直接把那个长字符串挂载上来
      privacy_content: privacyText    // 同理
    }
}