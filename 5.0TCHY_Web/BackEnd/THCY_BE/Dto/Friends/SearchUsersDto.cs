namespace THCY_BE.Dto.Friends
{
    public class SearchUsersResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public SearchUsersDataDto? Data { get; set; }
    }

    public class SearchUsersDataDto
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<UserSearchResultDto> Users { get; set; } = new List<UserSearchResultDto>();
    }

    public class UserSearchResultDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public int Level { get; set; }
        public int Points { get; set; }
        public int Exp { get; set; }
        public string? Title { get; set; }
        public string? Logo { get; set; }
        public int Rank { get; set; }
        public int State { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? LastActiveTime { get; set; }
        public int Likes { get; set; }
        public int Byd { get; set; }
        public int Creater { get; set; }

        // 关系状态
        public bool IsFriend { get; set; }
        public bool HasPendingRequest { get; set; }
        public bool HasReceivedRequest { get; set; }

        // 计算属性 - 是否在线（最后活跃时间在5分钟内）
        public bool IsOnline => LastActiveTime.HasValue &&
                              (DateTime.Now - LastActiveTime.Value).TotalMinutes < 5;
    }
}
