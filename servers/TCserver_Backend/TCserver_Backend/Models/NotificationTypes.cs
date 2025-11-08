public static class NotificationTypes
{
    // 系统通知
    public const int System = 1;           // 系统通知
    public const int Activity = 2;         // 活动通知
    public const int FriendRequest = 3;    // 好友请求
    public const int PrivateMessage = 4;   // 私信通知
    public const int PostReply = 5;        // 帖子回复通知
    public const int CommentReply = 6;     // 评论回复通知
    public const int PostLike = 7;        // 帖子点赞通知
    public const int CommentLike = 8;      // 评论点赞通知
    public const int Follow = 9;           // 关注通知
    public const int PointsChange = 10;    // 积分变动通知
    public const int LevelUp = 11;         // 等级提升通知
    public const int Award = 12;           // 奖励发放通知
    public const int ReportResult = 13;     // 举报结果通知
    public const int FriendRequestReceived = 14;    // 收到好友请求
    public const int FriendRequestAccepted = 15;     // 好友请求被接受
    public const int FriendRequestRejected = 16;     // 好友请求被拒绝

    // 获取类型名称
    public static string GetTypeName(int type)
    {
        return type switch
        {
            System => "系统通知",
            Activity => "活动通知",
            FriendRequest => "好友请求",
            PrivateMessage => "私信通知",
            PostReply => "帖子回复",
            CommentReply => "评论回复",
            PostLike => "帖子点赞",
            CommentLike => "评论点赞",
            Follow => "关注通知",
            PointsChange => "积分变动",
            LevelUp => "等级提升",
            Award => "奖励发放",
            ReportResult => "举报结果",
            FriendRequestReceived => "好友请求",
            FriendRequestAccepted => "好友请求通过",
            FriendRequestRejected => "好友请求拒绝",
            _ => "未知类型"
        };
    }

    // 获取类型图标
    public static string GetTypeIcon(int type)
    {
        return type switch
        {
            System => "🔔",
            Activity => "🎯",
            FriendRequest => "👥",
            PrivateMessage => "💬",
            PostReply => "📝",
            CommentReply => "↩️",
            PostLike => "👍",
            CommentLike => "❤️",
            Follow => "➕",
            PointsChange => "💰",
            LevelUp => "⭐",
            Award => "🎁",
            ReportResult => "📋",
            FriendRequestReceived => "👥",
            FriendRequestAccepted => "✅",
            FriendRequestRejected => "❌",
            _ => "📄"
        };
    }

    // 获取类型颜色
    public static string GetTypeColor(int type)
    {
        return type switch
        {
            System => "#ff4d4f",      // 红色
            Activity => "#52c41a",    // 绿色
            FriendRequest => "#1890ff", // 蓝色
            PrivateMessage => "#722ed1", // 紫色
            PostReply => "#faad14",   // 橙色
            CommentReply => "#13c2c2", // 青色
            PostLike => "#eb2f96",    // 粉色
            CommentLike => "#f5222d", // 深红
            Follow => "#52c41a",      // 绿色
            PointsChange => "#faad14", // 橙色
            LevelUp => "#1890ff",     // 蓝色
            Award => "#eb2f96",       // 粉色
            ReportResult => "#722ed1", // 紫色
            _ => "#8c8c8c"            // 灰色
        };
    }
}