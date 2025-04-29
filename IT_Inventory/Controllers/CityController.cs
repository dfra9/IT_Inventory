using System;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;

namespace IT_Inventory.Controllers
{
    public class CityController : Controller
    {
        private readonly IT_Inventory db = new IT_Inventory();
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
                return View(new City { Is_Deleted = false });
            }
            var city = id.HasValue ? db.City.FirstOrDefault(c => c.City_Id == id.Value) : null;
            if (city == null)
            {
                TempData["Error"] = "City not found.";
                return RedirectToAction("Index");
            }
            return View(city);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(City city, string mode)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (mode == "Create")
                    {
                        var existCity = db.City.FirstOrDefault(c => c.City_Name == city.City_Name && c.Is_Deleted != true);
                        if (existCity != null)
                        {
                            TempData["ErrorMessage"] = "City name already exists.";
                            ViewBag.Mode = mode;
                            return View(city);
                        }

                        var deletedCity = db.City.FirstOrDefault(c => c.City_Name == city.City_Name && c.Is_Deleted == true);
                        if (deletedCity != null)
                        {
                            deletedCity.City_Description = city.City_Description;
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
                            city.Create_By = User.Identity.Name;
                            city.Create_Date = DateTime.Now;
                            db.City.Add(city);
                        }
                        db.SaveChanges();
                        TempData["SuccessMessage"] = "City created successfully.";
                        return RedirectToAction("Index");
                    }
                    else if (mode == "Edit")
                    {
                        var existCity = db.City.Find(city.City_Id);
                        if (existCity != null)
                        {
                            existCity.City_Name = city.City_Name;
                            existCity.Edit_By = User.Identity.Name;
                            existCity.Edit_Date = DateTime.Now;


                            var duplicateCity = db.City.FirstOrDefault(c =>
                                    c.City_Name == city.City_Name &&
                                    c.City_Id != city.City_Id &&
                                    c.Is_Deleted != true);

                            if (duplicateCity != null)
                            {
                                TempData["ErrorMessage"] = "Another city with this name already exists.";
                                ViewBag.Mode = mode;
                                return View(city);
                            }
                            existCity.City_Name = city.City_Name;
                            existCity.City_Description = city.City_Description;
                            existCity.Edit_By = User.Identity.Name;
                            existCity.Edit_Date = DateTime.Now;

                            db.SaveChanges();
                            TempData["SuccessMessage"] = "City created successfully.";
                            return RedirectToAction("Index");
                        }
                    }
                    else if (mode == "Delete")
                    {
                        var existCity = db.City.Find(city.City_Id);
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
            return View(city);
        }
    }
}