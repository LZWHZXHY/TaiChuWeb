using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCserver_Backend.Data;
using TCserver_Backend.Models;

[ApiController]
[Route("api/[controller]")]
public class FinancialController : ControllerBase
{
    private readonly FunctionDbContext _context;

    public FinancialController(FunctionDbContext context)
    {
        _context = context;
    }

    // 获取全部
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<Financial>>> GetAll()
    {
        return await _context.Financials.ToListAsync();
    }

    // 检索（模糊查询、时间段、类型等）
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Financial>>> Search(
        string? zhiChu, string? zhiChuXiangMu, DateTime? dateFrom, DateTime? dateTo, int? payReceive)
    {
        var query = _context.Financials.AsQueryable();

        if (!string.IsNullOrEmpty(zhiChu))
            query = query.Where(f => f.ZhiChu.Contains(zhiChu));
        if (!string.IsNullOrEmpty(zhiChuXiangMu))
            query = query.Where(f => f.ZhiChuXiangMu.Contains(zhiChuXiangMu));
        if (dateFrom.HasValue)
            query = query.Where(f => f.date >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(f => f.date <= dateTo.Value);
        if (payReceive.HasValue)
            query = query.Where(f => f.PayReceive == payReceive);

        return await query.ToListAsync();
    }
}