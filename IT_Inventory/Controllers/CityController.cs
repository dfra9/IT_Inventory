using System;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModel;

namespace DBIT_Inventory.Controllers
{
    public class CityController : Controller
    {
        private readonly DBInventory db = new DBInventory();
        public ActionResult Index()
        {
            var cities = db.City.Where(c => c.Is_Deleted != true).ToList();
            return View(cities);
        }

        public ActionResult Editor(string mode, int? id)
        {
            ViewBag.Mode = mode;
            if (mode == "Create")
            {
                return View(new CityViewModel { Is_Deleted = false });
            }
            var city = id.HasValue ? db.City.FirstOrDefault(c => c.City_Id == id.Value) : null;
            if (city == null)
            {
                TempData["Error"] = "City not found.";
                return RedirectToAction("Index");
            }

            var cityViewModel = new CityViewModel
            {
                City_Id = city.City_Id,
                City_Name = city.City_Name,
                City_Description = city.City_Description,
                Is_Deleted = city.Is_Deleted,
                Create_By = city.Create_By,
                Create_Date = city.Create_Date,
                Edit_By = city.Edit_By,
                Edit_Date = city.Edit_Date,
                Delete_By = city.Delete_By,
                Delete_Date = city.Delete_Date
            };
            return View(cityViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(CityViewModel cityViewModel, string mode)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (mode == "Create")
                    {
                        var existCity = db.City.FirstOrDefault(c => c.City_Name == cityViewModel.City_Name && c.Is_Deleted != true);
                        if (existCity != null)
                        {
                            TempData["ErrorMessage"] = "City name already exists.";
                            ViewBag.Mode = mode;
                            return View(cityViewModel);
                        }

                        var deletedCity = db.City.FirstOrDefault(c => c.City_Name == cityViewModel.City_Name && c.Is_Deleted == true);
                        if (deletedCity != null)
                        {
                            deletedCity.City_Description = cityViewModel.City_Description;
                            deletedCity.Is_Deleted = false;
                            deletedCity.Create_By = User.Identity.Name;
                            deletedCity.Create_Date = DateTime.Now;
                            deletedCity.Edit_By = User.Identity.Name;
                            deletedCity.Edit_Date = DateTime.Now;
                            deletedCity.Delete_By = User.Identity.Name;
                            deletedCity.Delete_Date = DateTime.Now;
                        }
                        else
                        {
                            var newCity = new City
                            {
                                City_Name = cityViewModel.City_Name,
                                City_Description = cityViewModel.City_Description,
                                Is_Deleted = false,
                                Create_By = User.Identity.Name,
                                Create_Date = DateTime.Now
                            };
                            db.City.Add(newCity);
                        }
                        db.SaveChanges();
                        TempData["SuccessMessage"] = "City created successfully.";
                        return RedirectToAction("Index");
                    }
                    else if (mode == "Edit")
                    {
                        var existCity = db.City.Find(cityViewModel.City_Id);
                        if (existCity != null)
                        {
                            existCity.City_Name = cityViewModel.City_Name;
                            existCity.Edit_By = User.Identity.Name;
                            existCity.Edit_Date = DateTime.Now;


                            var duplicateCity = db.City.FirstOrDefault(c =>
                                    c.City_Name == cityViewModel.City_Name &&
                                    c.City_Id != cityViewModel.City_Id &&
                                    c.Is_Deleted != true);

                            if (duplicateCity != null)
                            {
                                TempData["ErrorMessage"] = "Another city with this name already exists.";
                                ViewBag.Mode = mode;
                                return View(cityViewModel);
                            }
                            existCity.City_Name = cityViewModel.City_Name;
                            existCity.City_Description = cityViewModel.City_Description;
                            existCity.Edit_By = User.Identity.Name;
                            existCity.Edit_Date = DateTime.Now;

                            db.SaveChanges();
                            TempData["SuccessMessage"] = "City created successfully.";
                            return RedirectToAction("Index");
                        }
                    }
                    else if (mode == "Delete")
                    {
                        var existCity = db.City.Find(cityViewModel.City_Id);
                        if (existCity != null)
                        {
                            existCity.Delete_By = User.Identity.Name;
                            existCity.Delete_Date = DateTime.Now;
                            existCity.Is_Deleted = true;
                        }
                        db.SaveChanges();
                        TempData["Success"] = "City" + mode + "Succesfully";
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while saving the city: " + ex.Message;
            }
            ViewBag.Mode = mode;
            return View(cityViewModel);
        }
    }
}