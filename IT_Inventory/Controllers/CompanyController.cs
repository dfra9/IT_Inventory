using System;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModel;

namespace DBIT_Inventory.Controllers
{
    public class CompanyController : Controller
    {
        private readonly DBInventory db = new DBInventory();

        public ActionResult Index()
        {
            var companies = db.Company.Where(c => c.Is_Deleted != true).ToList();
            return View(companies);

        }

        public ActionResult Editor(string mode, string id)
        {
            ViewBag.Mode = mode;
            if (mode == "Create")
            {
                return View(new CompanyViewModel { Is_Deleted = false });

            }

            var company = db.Company.Find(id);
            if (company == null)
            {
                TempData["Error"] = "Company not found.";
                return RedirectToAction("Index");
            }

            var companyViewModel = new CompanyViewModel
            {
                Company_Code = company.Company_Code,
                Company_Name = company.Company_Name,
                Is_Deleted = company.Is_Deleted,
                Create_By = company.Create_By,
                Create_Date = company.Create_Date,
                Edit_By = company.Edit_By,
                Edit_Date = company.Edit_Date,
                Delete_By = company.Delete_By,
                Delete_Date = company.Delete_Date
            };

            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(CompanyViewModel company, string mode)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (mode == "Create")
                    {
                        var newCompany = new Company
                        {
                            Company_Code = company.Company_Code,
                            Company_Name = company.Company_Name,
                            Is_Deleted = company.Is_Deleted,
                            Create_By = User.Identity.Name,
                            Create_Date = DateTime.Now
                        };
                        db.Company.Add(newCompany);
                    }
                    else if (mode == "Edit")
                    {
                        var existCompany = db.Company.Find(company.Company_Code);
                        if (existCompany != null)
                        {
                            existCompany.Company_Name = company.Company_Name;
                            existCompany.Edit_By = User.Identity.Name;
                            existCompany.Edit_Date = DateTime.Now;
                        }
                    }
                    else if (mode == "Delete")
                    {
                        var existCompany = db.Company.Find(company.Company_Code);
                        if (existCompany != null)
                        {
                            existCompany.Is_Deleted = true;
                            existCompany.Delete_By = User.Identity.Name;
                            existCompany.Delete_Date = DateTime.Now;
                        }
                    }
                    db.SaveChanges();
                    TempData["Success"] = "Company saved successfully.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing your request: " + ex.Message;
            }
            ViewBag.Mode = mode;
            return View(company);
        }
    }
}