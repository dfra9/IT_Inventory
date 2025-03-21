using System;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.Services;
using IT_Inventory.ViewModel;

namespace IT_Inventory.Controllers
{
    public class DataUserController : Controller
    {
        private readonly IT_Inventory db;

        private readonly IUserService userService;

        public DataUserController(IT_Inventory db, IUserService userService)
        {
            this.db = db;
            this.userService = userService;
        }

        public ActionResult Index()
        {

            if (!(Session["IsAdmin"] is bool IsAdmin) || !IsAdmin)
            {
                TempData["Message"] = "You are not authorized to access this page";
                return RedirectToAction("Index", "Home");
            }
            var users = db.Users.Where(u => u.Is_Deleted != true).Select(u => new DataUserViewModel
            {
                User_Id = u.User_Id,
                Username = u.Username,
                Departement = u.Departement,
                City = u.City,

                Location = u.Location,
                LastLogin = u.Last_Login,
                IsAdmin = u.Is_Admin ?? false,

            }).ToList();

            return View(users);
            //return View("~/Views/MasterData/DataUser/Index.cshtml");
        }

        public ActionResult Editor(int? id, string mode = "Create")
        {

            if (mode == "Create")
            {

                return View(new Users());
            }

            Users user = new Users();

            user = db.Users.Find(id);
            if (user == null || user.Is_Deleted == true)
            {
                TempData["Message"] = "User not found";
                return RedirectToAction("Index");
            }

            ViewBag.Mode = mode;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(Users user, string mode)
        {
            try
            {
                switch (mode)
                {
                    case "Create":
                        if (db.Users.Any(u => u.Username == user.Username))
                        {
                            TempData["Message"] = "Username already exist";
                            DropdownList();
                            return View(user);
                        }


                        user.Password = userService.HashPassword(user.Password);




                        user.Create_Date = DateTime.Now;
                        user.Create_By = Session["Username"]?.ToString() ?? "Admin";
                        user.Is_Deleted = false;
                        user.Is_Admin = false;



                        db.Users.Add(user);
                        db.SaveChanges();
                        TempData["Message"] = "User Created Succesfully";
                        break;
                    case "Edit":
                        var userEdit = db.Users.Find(user.User_Id);


                        if (userEdit == null || userEdit.Is_Deleted == true)
                        {
                            TempData["Message"] = "User not found";
                            return RedirectToAction("Index");
                        }



                        if (!string.IsNullOrEmpty(user.Password))
                        {
                            userEdit.Password = userService.HashPassword(user.Password);
                        }

                        userEdit.Departement = user.Departement;
                        userEdit.City = user.City;
                        userEdit.Location = user.Location;

                        userEdit.Edit_By = "Admin";
                        userEdit.Edit_Date = DateTime.Now;

                        db.SaveChanges();
                        TempData["Message"] = "User Edited Succesfully";
                        break;
                    case "Delete":
                        var userDelete = db.Users.Find(user.User_Id);
                        if (userDelete == null || userDelete.Is_Deleted == true)
                        {
                            TempData["Message"] = "User not found";
                            return RedirectToAction("Index");
                        }
                        userDelete.Delete_Date = DateTime.Now;
                        userDelete.Delete_By = "Admin";
                        userDelete.Is_Deleted = true;
                        db.SaveChanges();
                        TempData["Message"] = "User Deleted Succesfully";
                        break;
                }
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                DropdownList();
                return RedirectToAction("Editor", new { id = user.User_Id, mode = mode });
            }
        }

        private void GetCity()
        {
            var city = db.City
                .Where(c => c.Is_Deleted != true)
                .Select(c => new
                {
                    c.City_Id,
                    c.City_Name
                })
                .ToList();
            ViewBag.City = new SelectList(city, "City_Id", "City_Name");
        }

        //[HttpGet]
        //public JsonResult GetLocationByCity(string city)
        //{
        //    var locations = db.Location

        //        .Where(l => l.City_Name == city && l.Is_Deleted)
        //    .Select(l => new
        //    {
        //        l.Location_Id,
        //        l.Location_Name
        //    })
        //        .ToList();
        //    return Json(locations, JsonRequestBehavior.AllowGet);
        //}

        private void DropdownList()
        {
            ViewBag.Departements = db.Departement.ToList();
            ViewBag.City = db.City.ToList();
            ViewBag.Location = db.Location.ToList();
        }



    }
}