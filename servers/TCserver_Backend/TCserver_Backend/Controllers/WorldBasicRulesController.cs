using Microsoft.AspNetCore.Mvc;
using TCserver_Backend.Data;

namespace TCserver_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorldBasicRulesController : ControllerBase
    {
        private readonly BydSettingDbContext _context;
        public WorldBasicRulesController(BydSettingDbContext context)
        {
            _context = context;

        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var rules = _context.WorldBasicRules.OrderBy(r => r.id).ToList();
            return Ok(rules);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var rule = _context.WorldBasicRules.FirstOrDefault(r => r.id == id);
            if (rule == null) return NotFound();
            return Ok(rule);
        }

    }
}
