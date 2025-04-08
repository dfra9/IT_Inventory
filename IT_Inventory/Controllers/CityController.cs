using System;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;

namespace IT_Inventory.Controllers
{
    public class CityController : Controller
    {
        private IT_Inventory db = new IT_Inventory();
        public ActionResult Index()
        {
            var cities = db.City.Where(c => c.Is_Deleted != true).ToList();
            return View(cities);
        }

        public ActionResult Editor(string mode, string id)
        {
            ViewBag.Mode = mode;
            if (mode == "Create")
            {
                return View(new City { Is_Deleted = false });
            }
            var city = db.City.Find(id);
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
                        city.Create_By = User.Identity.Name;
                        city.Create_Date = DateTime.Now;
                        db.City.Add(city);
                    }
                    else if (mode == "Edit")
                    {
                        var existCity = db.City.Find(city.City_Id);
                        if (existCity != null)
                        {
                            existCity.City_Name = city.City_Name;
                            existCity.Edit_By = User.Identity.Name;
                            existCity.Edit_Date = DateTime.Now;
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
                        TempData["Success"] = "City deleted successfully.";
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while saving the city: " + ex.Message;
            }
            ViewBag.mode = mode;
            return View(city);
        }
    }
}