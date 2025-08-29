using System;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModel;

namespace DBIT_Inventory.Controllers
{
    public class DepartementController : Controller
    {
        private readonly DBInventory db = new DBInventory();

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

            var departementViewModel = new DepartementViewModel
            {
                Departement_Code = departement.Departement_Code,
                Role = departement.Role,
                Is_Deleted = departement.Is_Deleted,
                Create_By = departement.Create_By,
                Create_Date = departement.Create_Date,
                Edit_By = departement.Edit_By,
                Edit_Date = departement.Edit_Date,
                Delete_By = departement.Delete_By,
                Delete_Date = departement.Delete_Date
            };
            return View(departementViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(DepartementViewModel departementViewModel, string mode)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (mode == "Create")
                    {
                        var existDepartement = db.Departement.FirstOrDefault(d => d.Departement_Code == departementViewModel.Departement_Code);

                        if (existDepartement != null && existDepartement.Is_Deleted == true)
                        {
                            existDepartement.Role = departementViewModel.Role;
                            existDepartement.Is_Deleted = false;
                            existDepartement.Create_By = departementViewModel.Create_By;
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
                            return View(departementViewModel);
                        }
                        else
                        {
                            var newDepartement = new Departement
                            {
                                Departement_Code = departementViewModel.Departement_Code,
                                Role = departementViewModel.Role,
                                Is_Deleted = false,
                                Create_By = User.Identity.Name,
                                Create_Date = DateTime.Now
                            };
                            db.Departement.Add(newDepartement);
                        }
                    }
                    else if (mode == "Edit")
                    {
                        var existDepartement = db.Departement.Find(departementViewModel.Departement_Code);
                        if (existDepartement != null)
                        {
                            existDepartement.Role = departementViewModel.Role;
                            existDepartement.Edit_By = User.Identity.Name;
                            existDepartement.Edit_Date = DateTime.Now;
                        }
                    }
                    else if (mode == "Delete")
                    {
                        var existDepartement = db.Departement.Find(departementViewModel.Departement_Code);
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
            return View(departementViewModel);
        }
    }
}