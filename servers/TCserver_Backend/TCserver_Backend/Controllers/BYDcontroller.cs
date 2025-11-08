using Microsoft.AspNetCore.Mvc;
using TCserver_Backend.Data;

namespace TCserver_Backend.Controllers
{
    [ApiController]
    [Route("api/byd/setting")]
    public class BYDcontroller : ControllerBase
    {
        private readonly BydSettingDbContext _context;
        public BYDcontroller(BydSettingDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult GetSettings([FromQuery] string? type, [FromQuery] int? parent_id)
        {
            var query = _context.Settings.AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(s => s.type == type);

            if (parent_id.HasValue)
                query = query.Where(s => s.parent_id == parent_id.Value);

            var settings = query.ToList();
            return Ok(settings);
        }





        [HttpGet("{id}")]
        public IActionResult GetSetting(int id)
        {
            var setting = _context.Settings.FirstOrDefault(s => s.id == id);
            if (setting == null) return NotFound();
            return Ok(setting);
        }
    }
}
