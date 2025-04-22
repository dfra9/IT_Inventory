using System;
using System.Linq;
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            try
            {
                if (!string.IsNullOrEmpty(oldPassword) || !string.IsNullOrEmpty(newPassword) || !string.IsNullOrEmpty(confirmPassword))
                {
                    TempData["Message"] = "Please fill in all fields";
                    return RedirectToAction("Index", "Home");
                }
                if (newPassword != confirmPassword)
                {
                    TempData["Message"] = "New password and confirmation do not match";
                    return RedirectToAction("Index", "Home");
                }
                string currentUsername = Session["Username"]?.ToString();


                var users = _db.Users.FirstOrDefault(u => u.Username == currentUsername && u.Is_Deleted != true);
                if (users == null)
                {
                    TempData["Message"] = "User not found";
                    return RedirectToAction("Index", "Home");
                }

                if (!_userService.VerifyPassword(users.Password, oldPassword))
                {
                    TempData["Message"] = "Current is incorrect";
                    return RedirectToAction("Index", "Home");
                }
                users.Password = _userService.HashPassword(newPassword);
                users.Last_Login = DateTime.Now;
                _db.SaveChanges();
                TempData["Message"] = "Password changed successfully";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "An error occurred while changing the password: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }

        }
    }
}
