using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModel;


namespace IT_Inventory.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            var user = Session["User"] as Users;
            return View(user);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Login()
        {
            return View("~/Views/Auth/Login.cshtml", new LoginViewModel());
        }


    }
}