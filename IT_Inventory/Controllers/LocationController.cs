using System;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;

namespace IT_Inventory.Controllers
{
    public class LocationController : Controller
    {
        private IT_Inventory db = new IT_Inventory();

        // GET: Location
        public ActionResult Index()
        {
            var locations = db.Location.ToList();
            return View(locations);
        }

        public ActionResult Editor(string id, string mode)
        {
            ViewBag.Mode = mode;
            if (mode == "Create")
            {
                return View(new Location { Is_Deleted = false });
            }
            var location = db.Location.Find(id);
            if (location == null)
            {
                TempData["Error"] = "Location not found.";
                return RedirectToAction("Index");
            }
            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Editor(Location location, string mode)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (mode == "Create")
                    {
                        location.Create_By = User.Identity.Name;
                        location.Create_Date = DateTime.Now;
                        db.Location.Add(location);
                    }
                    else if (mode == "Edit")
                    {
                        var existLocation = db.Location.Find(location.Location_Code);
                        if (existLocation != null)
                        {
                            existLocation.Location_Name = location.Location_Name;
                            existLocation.Edit_By = User.Identity.Name;
                            existLocation.Edit_Date = DateTime.Now;
                        }
                    }
                    else if (mode == "Delete")
                    {
                        var existLocation = db.Location.Find(location.Location_Code);
                        if (existLocation != null)
                        {
                            existLocation.Delete_By = User.Identity.Name;
                            existLocation.Delete_Date = DateTime.Now;
                            existLocation.Is_Deleted = true;
                        }
                        db.SaveChanges();
                        TempData["Success"] = "Location saved successfully.";
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
            }
            ViewBag.Mode = mode;
            return View(location);
        }
    }
}