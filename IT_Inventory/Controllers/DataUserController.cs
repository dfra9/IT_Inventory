using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DBIT_Inventory.Services;
using IT_Inventory.Models;
using IT_Inventory.ViewModel;

namespace DBIT_Inventory.Controllers
{
    public class DataUserController : Controller
    {
        private readonly DBInventory db;
        private readonly IUserService userService;

        public DataUserController(DBInventory db, IUserService userService)
        {
            this.db = new DBInventory();
            this.userService = userService;
        }

        public ActionResult Index()
        {

            var users = db.Users.Where(u => u.Is_Deleted != true).Select(u => new DataUserViewModel
            {
                User_Id = u.User_Id,
                Username = u.Username,
                Long_Name = u.Long_Name,
                Departement = u.Departement,
                City = u.City,
                Location = u.Location,
                LastLogin = u.Last_Login,
                IsAdmin = u.Is_Admin ?? false,
                IsDeleted = (bool)u.Is_Deleted

            }).ToList();

            return View(users);
        }

        public ActionResult Editor(int? id, string mode = "Create")
        {
            //var model = new DataUserViewModel();
            if (mode == "Create")
            {
                DropdownList();
                return View(new DataUserViewModel());
            }

            Users user = new Users();
            user = db.Users.Find(id.Value);
            if (user == null || user.Is_Deleted == true)
            {
                TempData["Message"] = "User not found";
                return RedirectToAction("Index");
            }

            var viewModel = new DataUserViewModel
            {
                User_Id = user.User_Id,
                Username = user.Username,
                Long_Name = user.Long_Name,
                Departement = user.Departement,
                City = user.City,
                Location = user.Location,
                LastLogin = user.Last_Login,
                IsAdmin = user.Is_Admin ?? false,
                IsDeleted = user.Is_Deleted ?? false,
                Password = string.Empty
            };

            ViewBag.Mode = mode;
            DropdownList();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(DataUserViewModel userViewModel, string mode)
        {
            try
            {
                switch (mode)
                {
                    case "Create":
                        if (db.Users.Any(u => u.Username == userViewModel.Username))
                        {
                            TempData["Message"] = "Username already exist";
                            DropdownList();
                            return View(userViewModel);
                        }


                        var newUser = new Users
                        {
                            Username = userViewModel.Username,
                            Long_Name = userViewModel.Long_Name,
                            Password = userService.HashPassword(userViewModel.Password),
                            Departement = userViewModel.Departement,
                            City = userViewModel.City,
                            Location = userViewModel.Location,
                            Is_Admin = userViewModel.IsAdmin,
                            Create_Date = DateTime.Now,
                            Create_By = Session["Username"]?.ToString() ?? "Admin",
                            Is_Deleted = false
                        };
                        db.Users.Add(newUser);
                        db.SaveChanges();
                        TempData["Message"] = "User Created Succesfully";
                        break;
                    case "Edit":
                        var userEdit = db.Users.Find(userViewModel.User_Id);
                        if (userEdit == null || userEdit.Is_Deleted == true)
                        {
                            TempData["Message"] = "User not found";
                            return RedirectToAction("Index");
                        }

                        userEdit.Is_Admin = userViewModel.IsAdmin;
                        if (!string.IsNullOrEmpty(userViewModel.Password))
                        {
                            userEdit.Password = userService.HashPassword(userViewModel.Password);
                        }
                        userEdit.Long_Name = userViewModel.Long_Name;
                        userEdit.Departement = userViewModel.Departement;
                        userEdit.City = userViewModel.City;
                        userEdit.Location = userViewModel.Location;
                        userEdit.Edit_By = Session["Username"]?.ToString() ?? "Admin";
                        userEdit.Edit_Date = DateTime.Now;

                        db.SaveChanges();
                        TempData["Message"] = "User Edited Succesfully";
                        break;
                    case "Delete":
                        var userDelete = db.Users.Find(userViewModel.User_Id);
                        if (userDelete == null || userDelete.Is_Deleted == true)
                        {
                            TempData["Message"] = "User not found";
                            return RedirectToAction("Index");
                        }
                        userDelete.Delete_Date = DateTime.Now;
                        userDelete.Delete_By = Session["Username"]?.ToString() ?? "Admin";
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
                return View(userViewModel);
            }
        }

        [HttpPost]
        public JsonResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            try
            {

                if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
                {
                    return Json(new { success = false, message = "All fields are required" });
                }

                if (newPassword != confirmPassword)
                {
                    return Json(new { success = false, message = "New password and confirmation do not match" });
                }

                string currentUsername = Session["Username"]?.ToString();
                if (string.IsNullOrEmpty(currentUsername))
                {
                    return Json(new { success = false, message = "You must be logged in to change your password" });
                }

                var user = db.Users.FirstOrDefault(u => u.Username == currentUsername && u.Is_Deleted != true);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                if (!userService.VerifyPassword(oldPassword, user.Password))
                {
                    return Json(new { success = false, message = "Current password is incorrect" });
                }


                user.Password = userService.HashPassword(newPassword);
                user.Edit_Date = DateTime.Now;
                user.Edit_By = currentUsername;

                db.SaveChanges();

                return Json(new { success = true, message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }



        [HttpGet]
        public JsonResult GetLocationByCity(string cityId)
        {
            try
            {
                if (string.IsNullOrEmpty(cityId))
                {
                    return Json(new object[0], JsonRequestBehavior.AllowGet);
                }

                var locations = db.Location
                    .Where(l => l.City_Name == cityId && l.Is_Deleted != true)
                    .Select(l => new
                    {
                        locationCode = l.Location_Code,
                        locationName = l.Location_Name
                    })
                    .ToList();

                return Json(locations, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        private void DropdownList()
        {
            try
            {
                ViewBag.Departement = db.Departement.Where(c => c.Is_Deleted != true).ToList();
                ViewBag.Location = db.Location.Where(c => c.Is_Deleted != true).ToList();
                ViewBag.City = db.City.Where(c => c.Is_Deleted != true).ToList();
            }
            catch (Exception ex)
            {
                ViewBag.Departement = new List<object>();
                ViewBag.Location = new List<object>();
                ViewBag.City = new List<object>();
            }
        }
    }
}