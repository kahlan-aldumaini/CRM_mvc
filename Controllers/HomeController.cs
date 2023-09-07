using CRM_mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CRM_mvc.Context;
using CRM_mvc.Models.Entities;
using CRM_mvc.Models.Views.Charts;
using CRM_mvc.Utilities.Enumerations;

namespace CRM_mvc.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        public int PerDayAverage(PerDay perDay)
        {
            var sum = perDay.Fri + perDay.Mon + perDay.Sat + perDay.Sun + perDay.Thu + perDay.Tue + perDay.Wed;
            return sum > 7 ? sum / 7 : sum;
        }

        public List<int> HourlyCalls(CallTypeEnum callType)
        {
            var calls = _context.Calls.Where(x => x.CreatedAt.Date == DateTime.Now.Date);
            var hourlyCalls = new List<int>();
            for (var i = 0; i < 24; i++)
            {
                hourlyCalls.Add(calls.Count(x => x.Start.Hour == i && x.Type.Name == callType.ToString()));
            }
            return hourlyCalls;
        }

        public PerDay PerDayMethod(IQueryable<IBaseModel> perDay)
        {
            return new PerDay
            {
                Sat = perDay.Count(x => x.CreatedAt.DayOfWeek == DayOfWeek.Saturday),
                Sun = perDay.Count(x => x.CreatedAt.DayOfWeek == DayOfWeek.Sunday),
                Mon = perDay.Count(x => x.CreatedAt.DayOfWeek == DayOfWeek.Monday),
                Tue = perDay.Count(x => x.CreatedAt.DayOfWeek == DayOfWeek.Tuesday),
                Wed = perDay.Count(x => x.CreatedAt.DayOfWeek == DayOfWeek.Wednesday),
                Thu = perDay.Count(x => x.CreatedAt.DayOfWeek == DayOfWeek.Thursday),
                Fri = perDay.Count(x => x.CreatedAt.DayOfWeek == DayOfWeek.Friday)
            };
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel() { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}