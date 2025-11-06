using System;
using System.Threading.Tasks;
using TCserver_Backend.Data;
using TCserver_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace TCserver_Backend.Services
{
    public class RefreshTokenService
    {
        private readonly RegisterContext _context;

        public RefreshTokenService(RegisterContext context)
        {
            _context = context;
        }

        // 保存刷新令牌
        public async Task SaveRefreshToken(int userId, string refreshToken, int validityDays)
        {
            // 删除用户所有旧令牌
            var oldTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(oldTokens);

            // 添加新令牌
            var token = new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                Expiry = DateTime.UtcNow.AddDays(validityDays),
                Created = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
        }

        // 验证刷新令牌
        public async Task<bool> ValidateRefreshToken(int userId, string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == refreshToken);

            if (token == null || token.Expiry < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        // 更新刷新令牌
        public async Task UpdateRefreshToken(int userId, string oldToken, string newToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == oldToken);

            if (token != null)
            {
                token.Token = newToken;
                token.Expiry = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();
            }
        }

        // 撤销刷新令牌
        public async Task RevokeRefreshToken(int userId, string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == refreshToken);

            if (token != null)
            {
                _context.RefreshTokens.Remove(token);
                await _context.SaveChangesAsync();
            }
        }
    }
}