using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCserver_Backend.Data;
using TCserver_Backend.Models.Game;
using System.Security.Claims;

namespace TCserver_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TournamentsController : ControllerBase
    {
        private readonly FunctionDbContext _context;

        public TournamentsController(FunctionDbContext context)
        {
            _context = context;
        }

        // GET: api/tournaments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tournaments>>> GetTournaments()
        {
            var tournaments = await _context.Tournaments.ToListAsync();
            return Ok(tournaments);
        }

        // GET: api/tournaments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Tournaments>> GetTournament(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
                return NotFound();
            return Ok(tournament);
        }

        // GET: api/champions?tournamentId=xxx
        [HttpGet("/api/champions")]
        public async Task<ActionResult<IEnumerable<Champions>>> GetChampions([FromQuery] int tournamentId)
        {
            var champions = await _context.Champions
                .Where(c => c.tournamentId == tournamentId)
                .ToListAsync();
            return Ok(champions);
        }

        // ====== 新增或补充报名相关接口 ======

        // 假设有一个报名表 TournamentSignups: id, tournamentId, userId

        // GET: api/tournaments/{id}/isSignedUp
        [HttpGet("{id}/isSignedUp")]
        public async Task<ActionResult<object>> IsSignedUp(int id)
        {
            // 假设通过 token 获取 userId
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            var exists = await _context.TournamentSignups.AnyAsync(s => s.tournamentId == id && s.userId == userId);
            return Ok(new { isSignedUp = exists });
        }

        // POST: api/tournaments/{id}/signup
        [HttpPost("{id}/signup")]
        public async Task<IActionResult> SignUp(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var exists = await _context.TournamentSignups.AnyAsync(s => s.tournamentId == id && s.userId == userId);
            if (exists) return BadRequest("已报名");

            // 判断报名截止
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null) return NotFound();
            if (DateTime.UtcNow > tournament.registrationDate) return BadRequest("报名已截止");

            _context.TournamentSignups.Add(new TournamentSignups { tournamentId = id, userId = userId.Value });
            tournament.joinAmount = (tournament.joinAmount ?? 0) + 1;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST: api/tournaments/{id}/cancel-signup
        [HttpPost("{id}/cancel-signup")]
        public async Task<IActionResult> CancelSignUp(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var signup = await _context.TournamentSignups.FirstOrDefaultAsync(s => s.tournamentId == id && s.userId == userId);
            if (signup == null) return BadRequest("未报名");

            _context.TournamentSignups.Remove(signup);
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament != null && tournament.joinAmount > 0)
                tournament.joinAmount -= 1;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // 工具方法：获取当前登录用户id（你需根据你的认证实现修改）
        private int? GetUserId()
        {
            // 你用什么身份认证，这里就怎么取
            // 比如JWT:  return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            // 这里举例用NameIdentifier
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idStr, out int uid)) return uid;
            return null;
        }

        // GET: api/tournaments/{id}/signups
        [HttpGet("{id}/signups")]
        public async Task<ActionResult<IEnumerable<object>>> GetTournamentSignups(int id)
        {
            var users = await _context.TournamentSignups
                .Where(s => s.tournamentId == id)
                .Join(_context.userdata,
                    signup => signup.userId,
                    udata => udata.id,
                    (signup, udata) => new { signup, udata })
                .Join(_context.useraccount,
                    temp => temp.signup.userId,
                    uacc => uacc.Id,
                    (temp, uacc) => new {
                        id = temp.udata.id,
                        logo = temp.udata.logo,
                        username = uacc.username
                    })
                .ToListAsync();

            return Ok(users);
        }














    }
}