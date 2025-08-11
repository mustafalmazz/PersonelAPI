using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PersonelAPI.Controllers
{
    [Authorize]
    public class ExtraPaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
