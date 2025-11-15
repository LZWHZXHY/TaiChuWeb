namespace THCY_BE.Requests
{
    
    
        public class RegisterRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string VerificationCode { get; set; } = string.Empty;
        }

        public class SendCodeRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        public class VerifyCodeRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
        }
    
}
