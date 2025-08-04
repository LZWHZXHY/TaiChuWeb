using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models
{
    [Table("financialtable")] // 明确指定表名
    public class Financial
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("index")]
        public int Index { get; set; }

        [Column("ZhiChu")]
        public decimal? Expenditure { get; set; }

        [Column("ZhiChuXiangMu")]
        [MaxLength(255)]
        public string? ExpenditureItem { get; set; }

        [Required]
        [Column("date")]
        public DateTime TransactionDate { get; set; }

        [Column("ShouKuan")]
        public decimal? Income { get; set; }

        [Required]
        [Column("Amount", TypeName = "decimal(18,2)")]
        public decimal TransactionAmount { get; set; }

        [Required]
        [Column("PayRecive")]
        public TransactionType TransactionType { get; set; }

        // 计算属性：直观显示交易类型文字描述
        public string TransactionTypeDisplay => TransactionType == TransactionType.Expenditure ? "支出" : "收入";

        // 计算属性：根据类型返回实际金额（支出为负，收入为正）
        public decimal ActualAmount => TransactionType == TransactionType.Expenditure
            ? -(Expenditure ?? TransactionAmount)
            : (Income ?? TransactionAmount);
    }

    public enum TransactionType
    {
        Income = 0,    // 收入
        Expenditure = 1 // 支出
    }
}