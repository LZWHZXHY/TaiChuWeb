// Utils/PasswordHasher.cs
using System;
using System.Security.Cryptography;

namespace THCY_BE.Utils
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bit 
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 210000; // OWASP 2023推荐

        public static byte[] HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("密码不能为空", nameof(password));

            // 生成随机盐
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // 使用PBKDF2进行哈希
            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(KeySize);
            }

            // 组合盐和哈希值
            var result = new byte[SaltSize + KeySize];
            Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
            Buffer.BlockCopy(hash, 0, result, SaltSize, KeySize);

            return result;
        }

        public static bool VerifyPassword(string password, byte[] hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || hashedPassword == null || hashedPassword.Length != SaltSize + KeySize)
                return false;

            try
            {
                // 从哈希值中提取盐
                var salt = new byte[SaltSize];
                Buffer.BlockCopy(hashedPassword, 0, salt, 0, SaltSize);

                // 从哈希值中提取原始哈希
                var originalHash = new byte[KeySize];
                Buffer.BlockCopy(hashedPassword, SaltSize, originalHash, 0, KeySize);

                // 使用相同的盐重新计算哈希
                byte[] newHash;
                using (var pbkdf2 = new Rfc2898DeriveBytes(
                    password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    newHash = pbkdf2.GetBytes(KeySize);
                }

                // 使用恒定时间比较
                return CryptographicOperations.FixedTimeEquals(newHash, originalHash);
            }
            catch
            {
                return false;
            }
        }

        // 可选：添加一个方法将哈希转换为字符串（用于调试）
        public static string HashPasswordToString(string password)
        {
            var hashBytes = HashPassword(password);
            return Convert.ToBase64String(hashBytes);
        }

        // 可选：从字符串验证密码
        public static bool VerifyPasswordFromString(string password, string base64HashedPassword)
        {
            try
            {
                var hashedPassword = Convert.FromBase64String(base64HashedPassword);
                return VerifyPassword(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }
    }
}