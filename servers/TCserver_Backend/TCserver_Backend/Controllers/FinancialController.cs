// Controllers/FinancialController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCserver_Backend.Data;
using TCserver_Backend.Models;

namespace TCserver_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialController : ControllerBase
    {
        private readonly MySQLContext _context;

        public FinancialController(MySQLContext context)
        {
            _context = context;
        }


        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "API 工作正常", time = DateTime.Now });
        }

        // GET: api/financial
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Financial>>> GetAll()
        {
            return await _context.Financials
                .OrderByDescending(f => f.TransactionDate)
                .ToListAsync();
        }

        // GET: api/financial/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Financial>> GetById(int id)
        {
            var record = await _context.Financials.FindAsync(id);

            if (record == null)
            {
                return NotFound();
            }

            return record;
        }

        // GET: api/financial/summary
        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetSummary()
        {
            var records = await _context.Financials.ToListAsync();

            var income = records
                .Where(r => r.TransactionType == TransactionType.Income)
                .Sum(r => r.ActualAmount);

            var expenditure = records
                .Where(r => r.TransactionType == TransactionType.Expenditure)
                .Sum(r => r.ActualAmount);

            var net = income + expenditure; // expenditure是负数，所以这里是实际余额

            return new
            {
                TotalIncome = income,
                TotalExpenditure = expenditure,
                NetBalance = net,
                RecordCount = records.Count
            };
        }

        [HttpGet("financialtable")]
        public async Task<ActionResult> GetFinancialStats()
        {
            try
            {
                // 计算总收入
                var totalIncome = await _context.Financials
                    .Where(f => f.TransactionType == TransactionType.Income)
                    .SumAsync(f => f.TransactionAmount);

                // 计算总支出
                var totalExpenditure = await _context.Financials
                    .Where(f => f.TransactionType == TransactionType.Expenditure)
                    .SumAsync(f => f.TransactionAmount);

                // 获取最后更新时间
                var lastUpdate = await _context.Financials
                    .OrderByDescending(f => f.TransactionDate)
                    .Select(f => f.TransactionDate)
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    total_income = totalIncome,
                    total_expense = totalExpenditure,
                    update_time = lastUpdate.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return BadRequest(
                    $"数据库连接失败: {ex.Message}\n" +
                    $"StackTrace: {ex.StackTrace}\n" +
                    $"Inner: {ex.InnerException?.Message}\n" +
                    $"Inner2: {ex.InnerException?.InnerException?.Message}"
                );
            }
        }


    }
}