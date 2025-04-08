using System;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;

namespace IT_Inventory.Controllers
{
    public class DepartementController : Controller
    {
        private IT_Inventory db = new IT_Inventory();

        public ActionResult Index()
        {
            var departements = db.Departement.Where(d => d.Is_Deleted != true).ToList();
            return View(departements);
        }

        public ActionResult Editor(string mode, string id)
        {
            ViewBag.Mode = mode;
            if (mode == "Create")
            {
                return View(new Departement { Is_Deleted = false });
            }
            var departement = db.Departement.Find(id);
            if (departement == null)
            {
                TempData["Error"] = "Departement not found.";
                return RedirectToAction("Index");
            }
            return View(departement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(Departement departement, string mode)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (mode == "Create")
                    {
                        departement.Create_By = User.Identity.Name;
                        departement.Create_Date = DateTime.Now;
                        db.Departement.Add(departement);
                    }
                    else if (mode == "Edit")
                    {
                        var existDepartement = db.Departement.Find(departement.Departement_Code);
                        if (existDepartement != null)
                        {
                            existDepartement.Departement_Name = departement.Departement_Name;
                            existDepartement.Edit_By = User.Identity.Name;
                            existDepartement.Edit_Date = DateTime.Now;
                        }
                    }
                    else if (mode == "Delete")
                    {
                        var existDepartement = db.Departement.Find(departement.Departement_Code);
                        if (existDepartement != null)
                        {
                            existDepartement.Is_Deleted = true;
                            existDepartement.Delete_By = User.Identity.Name;
                            existDepartement.Delete_Date = DateTime.Now;
                        }
                    }
                    db.SaveChanges();
                    TempData["Success"] = "Departement saved successfully.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while saving the departement: " + ex.Message;
            }
            ViewBag.Mode = mode;
            return View(departement);
        }
    }
}