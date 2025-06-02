using System;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory;

namespace DBIT_Inventory.Controllers
{
    public class DepartementController : Controller
    {
        private readonly DBIT_Inventory db = new DBIT_Inventory();

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
                        var existDepartement = db.Departement.FirstOrDefault(d => d.Departement_Code == departement.Departement_Code);

                        if (existDepartement != null && existDepartement.Is_Deleted == true)
                        {
                            existDepartement.Role = departement.Role;
                            existDepartement.Is_Deleted = false;
                            existDepartement.Create_By = departement.Create_By;
                            existDepartement.Create_Date = DateTime.Now;
                            existDepartement.Edit_By = null;
                            existDepartement.Edit_Date = null;
                            existDepartement.Delete_By = null;
                            existDepartement.Delete_Date = null;
                        }
                        else if (existDepartement != null && existDepartement.Is_Deleted != true)
                        {
                            TempData["ErrorMessage"] = "Departement Code already exists.";
                            ViewBag.Mode = mode;
                            return View(departement);
                        }
                        else
                        {
                            departement.Create_By = User.Identity.Name;
                            departement.Create_Date = DateTime.Now;
                            db.Departement.Add(departement);
                        }
                    }
                    else if (mode == "Edit")
                    {
                        var existDepartement = db.Departement.Find(departement.Departement_Code);
                        if (existDepartement != null)
                        {
                            existDepartement.Role = departement.Role;
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