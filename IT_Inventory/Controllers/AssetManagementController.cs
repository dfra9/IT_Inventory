using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModel;

namespace IT_Inventory.Controllers
{
    public class AssetManagementController : Controller
    {
        private readonly IT_Inventory db = new IT_Inventory();
        // GET: AssetManagement
        public ActionResult Index()
        {
            return RedirectToAction("Transaction");
        }

        public ActionResult Transaction()
        {
            {
                var viewModel = new AssetManagementViewModel
                {
                    Companies = db.Company.Where(c => c.Is_Deleted != true).ToList(),
                    Dept = db.Departement.Where(c => c.Is_Deleted != true).ToList(),
                    Locations = db.Location.Where(c => c.Is_Deleted != true).ToList(),
                    Cities = db.City.Where(c => c.Is_Deleted != true).ToList(),
                    AssetHistory = db.Asset.Where(a => a.Is_Deleted != true).OrderByDescending(a => a.Transaction_Date).Take(100).ToList(),
                };
                viewModel.Companies = viewModel.Companies ?? new List<Company>();
                viewModel.Dept = viewModel.Dept ?? new List<Departement>();
                viewModel.Locations = viewModel.Locations ?? new List<Location>();
                viewModel.Cities = viewModel.Cities ?? new List<City>();
                viewModel.AssetHistory = viewModel.AssetHistory ?? new List<Asset>();

                LoadDropdownData();
                return View(viewModel);

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Transaction(AssetManagementViewModel viewModel)
        {
            try
            {
                if (string.IsNullOrEmpty(viewModel.No_asset))
                {
                    ModelState.AddModelError("No_Asset", "No Asset is required.");
                }
                if (string.IsNullOrEmpty(viewModel.Status))
                {
                    ModelState.AddModelError("Status", "Status is required.");
                }
                if (!ModelState.IsValid)
                {
                    LoadDropdownData();
                    ViewBag.Company = db.Company.Where(c => c.Is_Deleted != true).DefaultIfEmpty(new Company()).ToList();
                    ViewBag.Departement = db.Departement.Where(c => c.Is_Deleted != true).DefaultIfEmpty(new Departement()).ToList();
                    ViewBag.Location = db.Location.Where(c => c.Is_Deleted != true).DefaultIfEmpty(new Location()).ToList();
                    ViewBag.City = db.City.Where(c => c.Is_Deleted != true).DefaultIfEmpty(new City()).ToList();
                    viewModel.AssetHistory = db.Asset.Where(a => a.Is_Deleted != true)
                        .OrderByDescending(a => a.Transaction_Date)
                        .Take(100)
                        .ToList();
                    return View(viewModel);
                }
                var assets = db.Asset.FirstOrDefault(a => a.No_asset == viewModel.No_asset);
                if (assets != null)
                {
                    assets.Status = viewModel.Status;
                    assets.PIC = viewModel.PIC;
                    assets.Vendor = viewModel.Vendor;
                    assets.Transaction_Date = viewModel.Transaction_Date;
                    assets.Edit_By = User.Identity.Name ?? "System";
                    assets.Edit_Date = DateTime.Now;


                }
                else
                {
                    assets = new Asset
                    {
                        No_asset = viewModel.No_asset,
                        Company_Code = viewModel.Company_Code,
                        Material_Group_Code = viewModel.Material_Group_Code,
                        Material_Group = viewModel.Material_Group,
                        Material_Description = viewModel.Material_Description,
                        Quantity = viewModel.Quantity,
                        Unit = viewModel.Unit,
                        Acquisition_Date = viewModel.Acquisition_Date,
                        Acquisition_value = viewModel.Acquisition_value,
                        No_Asset_PGA = viewModel.No_Asset_PGA,
                        No_Asset_Accounting = viewModel.No_Asset_Accounting,
                        No_PO = viewModel.No_PO,
                        Latest_User = viewModel.Latest_User,
                        Departement = viewModel.Departement_Code,
                        Location = viewModel.Location_Code,
                        City = viewModel.City_Id.ToString(),
                        Last_Check_Date = viewModel.Last_Check_Date,
                        Condition = viewModel.Condition,
                        Status = viewModel.Status,
                        PIC = viewModel.PIC,
                        Vendor = viewModel.Vendor,
                        Transaction_Date = viewModel.Transaction_Date,
                        Create_By = User.Identity.Name ?? "System",
                        Create_Date = DateTime.Now,
                        Is_Deleted = false

                    };
                    db.Asset.Add(assets);
                }
                db.SaveChanges();
                TempData["Success"] = "Transaction successful.";
                return RedirectToAction("Transaction");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your request: " + ex.Message);
                viewModel.Companies = db.Company.Where(c => c.Is_Deleted != true).DefaultIfEmpty(new Company()).ToList();
                viewModel.Dept = db.Departement.Where(c => c.Is_Deleted != true).DefaultIfEmpty(new Departement()).ToList();
                viewModel.Locations = db.Location.Where(c => c.Is_Deleted != true).DefaultIfEmpty(new Location()).ToList();
                viewModel.Cities = db.City.Where(c => c.Is_Deleted != true).DefaultIfEmpty(new City()).ToList();
                return View(viewModel);
            }
        }

        private string GetTransactionDateNameByStatus(string status)
        {
            switch (status)
            {
                case "Return":
                    return "Return Date";
                case "Borrowing":
                    return "Borrowing Date";
                case "Service":
                    return "Service Date";
                case "Ready":
                    return "Ready Date";
                case "Assign":
                    return "Assign Date";
                case "Write Off":
                    return "Write Off Date";
                default:
                    return "Transaction Date";
            }
        }


        public string GetCompanyName(string companyCode)
        {
            var company = db.Company.FirstOrDefault(c => c.Company_Code == companyCode);
            return company?.Company_Name ?? string.Empty;
        }
        public JsonResult GetCompanyNameByCode(string companyCode)
        {
            try
            {

                if (string.IsNullOrEmpty(companyCode))
                {
                    return Json(string.Empty, JsonRequestBehavior.AllowGet);
                }

                var company = db.Company.FirstOrDefault(c => c.Company_Code == companyCode && c.Is_Deleted != true);
                string companyName = company?.Company_Name ?? string.Empty;


                return Json(companyName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return Json(string.Empty, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult GetAssetData(string search)
        {
            var assetData = db.Asset.Where(a => a.Is_Deleted != true);
            if (!string.IsNullOrEmpty(search))
            {
                assetData = assetData.Where(a => a.No_asset.Contains(search) || a.PIC.Contains(search) || a.Vendor.Contains(search) || a.Status.Contains(search));
            }
            var assets = assetData.OrderByDescending(a => a.Transaction_Date).Select(a => new
            {
                a.No_asset,
                a.PIC,
                a.Vendor,
                a.Transaction_Date,
                a.Status,
                Submit_Date = a.Create_Date
            }).Take(100)
              .ToList();
            return Json(assets, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        private void LoadDropdownData()
        {
            ViewBag.Company = db.Company.Where(c => c.Is_Deleted != true).ToList();
            ViewBag.Departement = db.Departement.Where(c => c.Is_Deleted != true).ToList();
            ViewBag.Location = db.Location.Where(c => c.Is_Deleted != true).ToList();
            ViewBag.City = db.City.Where(c => c.Is_Deleted != true).ToList();

            ViewBag.Status = new List<SelectListItem>
            {
                new SelectListItem { Text = "Select Status", Value = "" },
                new SelectListItem { Text = "Return", Value = "Return" },
                new SelectListItem { Text = "Borrowing", Value = "Borrowing" },
                new SelectListItem { Text = "Service", Value = "Service" },
                new SelectListItem { Text = "Ready", Value = "Ready" },
                new SelectListItem { Text = "Assign", Value = "Assign" },
                new SelectListItem { Text = "Write Off", Value = "Write Off" }
            };


        }
    }
}