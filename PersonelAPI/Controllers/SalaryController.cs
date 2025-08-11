using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PersonelAPI.Controllers
{
    [Authorize]
    public class SalaryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
