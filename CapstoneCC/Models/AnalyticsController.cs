using CapstoneCC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CapstoneCC.Models
{
    [Route("AnalyticsApi")]
    public class AnalyticsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("IncrementWalkInCount")]
        public IActionResult IncrementWalkInCount()
        {
            try
            {
                var analytics = _context.Analytics.FirstOrDefault();
                if (analytics != null)
                {
                    analytics.WalkInCount += 1;
                    _context.SaveChanges();
                    return Json(new { success = true, walkInCount = analytics.WalkInCount });
                }

                return Json(new { success = false, message = "Analytics data not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class WalkInCountIncrementRequest
    {
        public int Increment { get; set; }
    }
    public class WalkInRequest
    {
        public int WalkInCount { get; set; }
    }
}