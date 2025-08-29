using System;
using System.Linq;
using System.Web.Mvc;
using DBIT_Inventory.ViewModel;
using IT_Inventory.Models;

namespace DBIT_Inventory.Controllers
{
    public class LocationController : Controller
    {
        private readonly DBInventory db = new DBInventory();

        // GET: Location
        public ActionResult Index()
        {
            var locations = db.Location.Where(l => l.Is_Deleted != true).ToList();
            return View(locations);
        }

        public ActionResult Editor(string id, string mode)
        {
            ViewBag.Mode = mode;

            var cities = db.City
                .Where(c => c.Is_Deleted != true)
                .OrderBy(c => c.City_Name)
                .ToList();

            var viewModel = new LocationViewModel();



            if (mode == "Create")
            {
                viewModel.Cities = viewModel.GetCityListItem(cities);
                return View(viewModel);
            }
            var location = db.Location.Find(id);
            if (location == null)
            {
                TempData["Error"] = "Location not found.";
                return RedirectToAction("Index");
            }

            var locViewModel = new LocationViewModel
            {
                Location_Code = location.Location_Code,
                Location_Name = location.Location_Name,
                Location_Description = location.Location_Description,
                City_Name = location.City_Name,
                Create_By = location.Create_By,
                Create_Date = location.Create_Date,
                Is_Deleted = location.Is_Deleted ?? false,
                Edit_By = location.Edit_By,
                Edit_Date = location.Edit_Date,
                Delete_By = location.Delete_By,
                Delete_Date = location.Delete_Date
            };

            locViewModel.Cities = locViewModel.GetCityListItem(cities);
            return View(locViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(LocationViewModel viewModel, string mode)
        {
            try
            {
                var cities = db.City
                   .Where(c => c.Is_Deleted != true)
                   .OrderBy(c => c.City_Name)
                   .ToList();

                if (ModelState.IsValid)
                {
                    if (mode == "Create")
                    {
                        var existingLocation = db.Location
                            .FirstOrDefault(l => l.Location_Code == viewModel.Location_Code);

                        if (existingLocation != null && existingLocation.Is_Deleted != true)
                        {
                            ModelState.AddModelError("Location_Code", "Location Code already exists.");
                            viewModel.Cities = viewModel.GetCityListItem(cities);
                            return View(viewModel);
                        }


                        if (existingLocation != null && existingLocation.Is_Deleted == true)
                        {
                            existingLocation.Location_Name = viewModel.Location_Name;
                            existingLocation.Location_Description = viewModel.Location_Description;
                            existingLocation.City_Name = viewModel.City_Name;
                            existingLocation.Is_Deleted = false;
                            existingLocation.Create_By = User.Identity.Name;
                            existingLocation.Create_Date = DateTime.Now;
                            existingLocation.Delete_By = null;
                            existingLocation.Delete_Date = null;

                            db.SaveChanges();
                            TempData["Success"] = "Location recreated successfully.";
                            return RedirectToAction("Index");
                        }
                        var location = new Location
                        {
                            Location_Code = viewModel.Location_Code,
                            Location_Name = viewModel.Location_Name,
                            Location_Description = viewModel.Location_Description,
                            City_Name = viewModel.City_Name,
                            Is_Deleted = false,
                            Create_By = User.Identity.Name,
                            Create_Date = DateTime.Now
                        };
                        db.Location.Add(location);
                        db.SaveChanges();
                        TempData["Success"] = "Location saved successfully.";
                        return RedirectToAction("Index");
                    }
                    else if (mode == "Edit")
                    {
                        var existLocation = db.Location.Find(viewModel.Location_Code);
                        if (existLocation != null)
                        {
                            existLocation.Location_Name = viewModel.Location_Name;
                            existLocation.Location_Description = viewModel.Location_Description;
                            existLocation.City_Name = viewModel.City_Name;
                            existLocation.Edit_By = User.Identity.Name;
                            existLocation.Edit_Date = DateTime.Now;

                            db.SaveChanges();
                            TempData["Success"] = "Location saved successfully.";
                            return RedirectToAction("Index");
                        }
                    }
                    else if (mode == "Delete")
                    {
                        var existLocation = db.Location.Find(viewModel.Location_Code);
                        if (existLocation != null)
                        {
                            existLocation.Delete_By = User.Identity.Name;
                            existLocation.Delete_Date = DateTime.Now;
                            existLocation.Is_Deleted = true;

                            db.SaveChanges();
                            TempData["Success"] = "Location saved successfully.";
                            return RedirectToAction("Index");
                        }
                    }
                }


                viewModel.Cities = new SelectList(cities, "City_Name", "City_Name", viewModel.City_Name);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
            }

            ViewBag.Mode = mode;
            return View(viewModel);
        }


        [HttpGet]
        public JsonResult GetLocationsByCity(string cityName)
        {
            var locations = db.Location
                .Where(l => l.City_Name == cityName && l.Is_Deleted != true)
                .Select(l => new
                {
                    locationCode = l.Location_Code,
                    locationName = l.Location_Name
                })
                .ToList();

            return Json(locations, JsonRequestBehavior.AllowGet);
        }
    }
}