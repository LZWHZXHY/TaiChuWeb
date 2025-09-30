using Microsoft.AspNetCore.Mvc;
using TCserver_Backend.Data;
using TCserver_Backend.Models;

namespace TCserver_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShetuanController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ShetuanController(ApplicationDbContext context)
        {
            _context = context;
        }



        //操作内容

        [HttpGet]
        public IActionResult GetAll()
        {

            Console.WriteLine("社团页面测试");

            var data = _context.Shetuans.ToList();
            return Ok(data);
        }
    



    //用户申请新的社团
        [HttpPost("ApplyShetuan")]
        public IActionResult ApplyShetuan([FromBody] shetuan shetuan)
        {
            _context.Shetuans.Add(shetuan);
            _context.SaveChanges();

            return Ok(new { message = "申请成功，等待审核", data = shetuan });
        }
    }
}
    
