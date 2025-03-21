using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using IT_Inventory.Services;
using IT_Inventory.ViewModel;

namespace IT_Inventory.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IT_Inventory _db;
        public AuthController()
        {
            _db = new IT_Inventory();
            _userService = new UserService(_db);

        }
        public AuthController(IUserService userService, IT_Inventory db)
        {
            _userService = userService;
            _db = db;
        }

        // GET: Auth
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _userService.InitializeAdmin();

            var user = _userService.AuthenticateUser(model.Username, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            System.Diagnostics.Debug.WriteLine($"Authentication successful for user: {user.Username}");

            user.Last_Login = DateTime.Now;
            _db.SaveChanges();

            FormsAuthentication.SetAuthCookie(user.Username, model.RememberMe);
            Session["User"] = user;
            Session["Username"] = user.Username;

            Session["Departement"] = user.Departement;
            Session["City"] = user.City;
            Session["Location"] = user.Location;
            Session["IsAdmin"] = user.Is_Admin;


            if (user.Is_Admin == true)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();

            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie);
            return RedirectToAction("Login", "Auth");
        }


        public ActionResult ForgotPassword()
        {

            return View();
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }


    }
}