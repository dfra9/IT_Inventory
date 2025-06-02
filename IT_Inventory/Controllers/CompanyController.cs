using System;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory;

namespace DBIT_Inventory.Controllers
{
    public class CompanyController : Controller
    {
        private readonly DBIT_Inventory db = new DBIT_Inventory();

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
                return View(new Company { Is_Deleted = false });

            }

            var company = db.Company.Find(id);
            if (company == null)
            {
                TempData["Error"] = "Company not found.";
                return RedirectToAction("Index");
            }
            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(Company company, string mode)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (mode == "Create")
                    {
                        company.Create_By = User.Identity.Name;
                        company.Create_Date = DateTime.Now;
                        db.Company.Add(company);
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