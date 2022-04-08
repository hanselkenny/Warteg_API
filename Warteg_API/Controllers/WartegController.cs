using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Warteg_API.Context;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Warteg_API.Controllers
{
    [ApiController]
    [Route("[controller]")] // Route untuk akses controller ini, adalah nama controllernya ( contohnya ; localhost/warteg )
    public class WartegController : ControllerBase
    {
        //Supaya kita bisa akses databasenya
        private readonly WartegContext _context;
        //Kalau ada error bisa muncul di command prompt nya
        private readonly ILogger<WartegController> _logger;
        public WartegController(ILogger<WartegController> logger,WartegContext context)
        {
            _logger = logger;
            this._context = context;
        }
        [Authorize(AuthenticationSchemes ="Bearer")]
        [HttpGet]
        [Route("GetAllFood")] // route nya jadi localhost/warteg/getallfood
        public IActionResult GetAllFood()
        {
            try
            {
                using (var db = _context)
                {
                    var AllFood = (
                            from F in _context.Foods
                            orderby F.Name
                            select F
                        ).ToList();
                    return Ok(AllFood);
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost]
        [Route("InsertFood")] // route nya jadi localhost/warteg/InsertFood
        public IActionResult InsertFood(Food Food)
        {
            try
            {
                using (var db = _context)
                {
                    db.Foods.Add(Food);
                    db.SaveChanges();
                    return Ok(Food);
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
