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
                    Cities = db.City.Where(c => c.Is_Deleted != true).ToList(),
                    MaterialGroup = db.Material_Group.Where(c => c.Is_Deleted != true).ToList(),
                    Material_Code1 = db.Material_Code.Where(c => c.Is_Deleted != true).ToList(),
                    UoMList = db.UoM.Where(c => c.Is_Deleted != true).ToList(),
                    AssetHistory = db.Asset.Where(a => a.Is_Deleted != true).OrderByDescending(a => a.Transaction_Date).Take(100).ToList(),

                };

                var defaultCity = viewModel.Cities.FirstOrDefault()?.City_Name;
                viewModel.Locations = db.Location
                    .Where(l => l.Is_Deleted != true && l.City_Name == defaultCity)
                    .ToList();
                viewModel.Companies = viewModel.Companies ?? new List<Company>();
                viewModel.Dept = viewModel.Dept ?? new List<Departement>();
                viewModel.Locations = viewModel.Locations ?? new List<Location>();
                viewModel.Cities = viewModel.Cities ?? new List<City>();
                viewModel.MaterialGroup = viewModel.MaterialGroup ?? new List<Material_Group>();
                viewModel.Material_Code1 = viewModel.Material_Code1 ?? new List<Material_Code>();
                viewModel.UoMList = viewModel.UoMList ?? new List<UoM>();
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
                    ModelState.AddModelError("No_asset", "No Asset is required.");
                }
                if (string.IsNullOrEmpty(viewModel.Status))
                {
                    ModelState.AddModelError("Status", "Status is required.");
                }
                if (string.IsNullOrEmpty(viewModel.PIC))
                {
                    ModelState.AddModelError("PIC", "PIC is required.");
                }
                if (!viewModel.Transaction_Date.HasValue)
                {
                    ModelState.AddModelError("Transaction_Date", "Transaction Date is required.");
                }

                if (ModelState.IsValid)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Validation failed. Please check all required fields." });
                    }
                    LoadDropdownData();

                    viewModel.AssetHistory = db.Asset.Where(a => a.Is_Deleted != true)
                        .OrderByDescending(a => a.Transaction_Date)
                        .Take(100)
                        .ToList();

                    viewModel.Companies = db.Company.Where(c => c.Is_Deleted != true).ToList();
                    viewModel.Dept = db.Departement.Where(c => c.Is_Deleted != true).ToList();
                    viewModel.Locations = db.Location.Where(c => c.Is_Deleted != true).ToList();
                    viewModel.Cities = db.City.Where(c => c.Is_Deleted != true).ToList();
                    viewModel.MaterialGroup = db.Material_Group.Where(c => c.Is_Deleted != true).ToList();
                    viewModel.Material_Code1 = db.Material_Code.Where(c => c.Is_Deleted != true).ToList();
                    viewModel.UoMList = db.UoM.Where(c => c.Is_Deleted != true).ToList();
                    return View(viewModel);
                }

                var assets = db.Asset.FirstOrDefault(a => a.No_asset == viewModel.No_asset);
                if (assets != null)
                {
                    assets.Status = viewModel.Status;
                    assets.PIC = viewModel.PIC;
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
                        Company_Name = viewModel.Company_Name,
                        Material_Group = viewModel.Material_Group,
                        Material_Code = viewModel.Material_Code,
                        Material_Description = viewModel.Material_Description,
                        Quantity = viewModel.Quantity,
                        UoM = viewModel.UoM,
                        Serial_Number = viewModel.Serial_Number,
                        Device_Id = viewModel.Device_Id,
                        Acquisition_Date = viewModel.Acquisition_Date,
                        Acquisition_value = viewModel.Acquisition_value,
                        No_Asset_PGA = viewModel.No_Asset_PGA,
                        No_Asset_Accounting = viewModel.No_Asset_Accounting,
                        No_PO = viewModel.No_PO,
                        Latest_User = viewModel.Latest_User,
                        Departement = viewModel.Departement_Name,
                        Location = viewModel.Location_Name,
                        City = viewModel.City_Id.ToString(),
                        Last_Check_Date = viewModel.Last_Check_Date,
                        Condition = viewModel.Condition,
                        Status = viewModel.Status,
                        PIC = viewModel.PIC,
                        Transaction_Date = viewModel.Transaction_Date,
                        Create_By = User.Identity.Name ?? "System",
                        Create_Date = DateTime.Now,
                        Is_Deleted = false
                    };
                    db.Asset.Add(assets);
                }
                db.SaveChanges();

                var dashboardCounts = GetDashboardCounts();
                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = true,
                        message = "Transaction successful",
                        assetData = new
                        {
                            assets.No_asset,
                            assets.PIC,
                            Transaction_Date = assets.Transaction_Date.HasValue ? assets.Transaction_Date.Value.ToString("yyyy-MM-dd") : "",
                            assets.Status,
                            Submit_Date = assets.Create_Date.HasValue ? assets.Create_Date.Value.ToString("yyyy-MM-dd") : ""
                        }
                    });
                }
                else
                {
                    TempData["SuccessMessage"] = "Transaction successful";
                    return RedirectToAction("Transaction");
                }
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "An error occurred while processing your request: " + ex.Message });
                }
                ModelState.AddModelError("", "An error occurred while processing your request: " + ex.Message);
                viewModel.Companies = db.Company.Where(c => c.Is_Deleted != true).ToList();
                viewModel.Dept = db.Departement.Where(c => c.Is_Deleted != true).ToList();
                viewModel.Locations = db.Location.Where(c => c.Is_Deleted != true).ToList();
                viewModel.Cities = db.City.Where(c => c.Is_Deleted != true).ToList();
                viewModel.MaterialGroup = db.Material_Group.Where(c => c.Is_Deleted != true).ToList();
                viewModel.Material_Code1 = db.Material_Code.Where(c => c.Is_Deleted != true).ToList();
                viewModel.UoMList = db.UoM.Where(c => c.Is_Deleted != true).ToList();

                viewModel.Companies = viewModel.Companies ?? new List<Company>();
                viewModel.Dept = viewModel.Dept ?? new List<Departement>();
                viewModel.Locations = viewModel.Locations ?? new List<Location>();
                viewModel.Cities = viewModel.Cities ?? new List<City>();
                viewModel.MaterialGroup = viewModel.MaterialGroup ?? new List<Material_Group>();
                viewModel.Material_Code1 = viewModel.Material_Code1 ?? new List<Material_Code>();
                viewModel.UoMList = viewModel.UoMList ?? new List<UoM>();
                viewModel.AssetHistory = viewModel.AssetHistory ?? new List<Asset>();

                LoadDropdownData();
                return View(viewModel);
            }
        }

        private object GetDashboardCounts()
        {
            return new
            {
                TotalAssets = db.Asset.Count(a => a.Is_Deleted != true),
                AvailableAssets = db.Asset.Count(a => a.Is_Deleted != true && a.Status == "Ready"),
                AssetInUse = db.Asset.Count(a => a.Is_Deleted != true && a.Status == "Borrowing"),
                AssetInService = db.Asset.Count(a => a.Is_Deleted != true && a.Status == "Service")
            };
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

        public ActionResult GetAssetDetails(string assetId)
        {
            try
            {
                if (string.IsNullOrEmpty(assetId))
                {
                    return Json(new { success = false, message = "Asset ID is required." }, JsonRequestBehavior.AllowGet);
                }

                var asset = db.Asset.FirstOrDefault(a => a.No_asset == assetId && a.Is_Deleted != true);
                if (asset == null)
                {
                    return Json(new { success = false, message = "Asset not found." }, JsonRequestBehavior.AllowGet);
                }

                var assetDetails = new
                {
                    asset.No_asset,
                    asset.Company_Code,
                    asset.Company_Name,
                    asset.Material_Group,
                    asset.Material_Code,
                    asset.Material_Description,
                    asset.Quantity,
                    asset.UoM,
                    asset.Serial_Number,
                    asset.Device_Id,
                    Acquisition_Date = asset.Acquisition_Date.HasValue ? asset.Acquisition_Date.Value.ToString("yyyy-MM-dd") : "",
                    asset.Acquisition_value,
                    asset.No_Asset_PGA,
                    asset.No_Asset_Accounting,
                    asset.No_PO,
                    asset.Departement,
                    asset.Location,
                    City = GetCompanyName(asset.City),
                    Last_Check_Date = asset.Last_Check_Date.HasValue ? asset.Last_Check_Date.Value.ToString("yyyy-MM-dd") : "",
                    asset.Condition,
                    asset.Status,
                    asset.PIC
                };
                return Json(assetDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while retrieving asset details: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAsset(string No_asset, string Status, string PIC, DateTime? Transaction_Date)
        {
            try
            {
                if (string.IsNullOrEmpty(No_asset))
                {
                    return Json(new { success = false, message = "Asset number is required" });
                }

                if (string.IsNullOrEmpty(Status))
                {
                    return Json(new { success = false, message = "Status is required" });
                }

                if (string.IsNullOrEmpty(PIC))
                {
                    return Json(new { success = false, message = "PIC is required" });
                }

                if (!Transaction_Date.HasValue)
                {
                    return Json(new { success = false, message = "Transaction Date is required" });
                }

                var asset = db.Asset.FirstOrDefault(a => a.No_asset == No_asset && a.Is_Deleted != true);
                if (asset == null)
                {
                    return Json(new { success = false, message = "Asset not found" });
                }

                asset.Status = Status;
                asset.PIC = PIC;
                asset.Transaction_Date = Transaction_Date;
                asset.Edit_By = User.Identity.Name ?? "System";
                asset.Edit_Date = DateTime.Now;

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Asset updated successfully",
                    assetData = new
                    {
                        asset.No_asset,
                        asset.PIC,
                        Transaction_Date = asset.Transaction_Date.HasValue ? asset.Transaction_Date.Value.ToString("yyyy-MM-dd") : "",
                        asset.Status,
                        Submit_Date = asset.Create_Date.HasValue ? asset.Create_Date.Value.ToString("yyyy-MM-dd") : ""
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the asset: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAssetData(string search)
        {
            var assetData = db.Asset.Where(a => a.Is_Deleted != true);
            if (!string.IsNullOrEmpty(search))
            {
                assetData = assetData.Where(a => a.No_asset.Contains(search) || a.PIC.Contains(search) || a.Status.Contains(search));
            }
            var assets = assetData.OrderByDescending(a => a.Transaction_Date).Select(a => new
            {
                a.No_asset,
                a.PIC,
                a.Transaction_Date,
                a.Status,
                Submit_Date = a.Create_Date
            }).Take(100)
              .ToList();
            return Json(assets, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchAssetHistory(string search)
        {
            var assetData = db.Asset.Where(a => a.Is_Deleted != true);
            if (!string.IsNullOrEmpty(search))
            {
                assetData = assetData.Where(a => a.No_asset.Contains(search) || a.PIC.Contains(search) || a.Status.Contains(search) || (a.Transaction_Date.HasValue && a.Transaction_Date.Value.ToString().Contains(search)));
            }
            var assets = assetData.OrderByDescending(a => a.Transaction_Date).Select(a => new
            {
                a.No_asset,
                a.PIC,
                a.Transaction_Date,
                a.Status,
                Submit_Date = a.Create_Date
            }).Take(100)
              .ToList();
            return Json(assets, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLocationByCity(string cityId)
        {
            var locations = db.Location
                .Where(l => l.City_Name == cityId && l.Is_Deleted != true)
                .Select(l => new
                {
                    locationCode = l.Location_Code,
                    locationName = l.Location_Name
                })
                .ToList();
            return Json(locations, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        private void LoadDropdownData()
        {
            ViewBag.Company = db.Company.Where(c => c.Is_Deleted != true).ToList();
            ViewBag.Departement = db.Departement.Where(c => c.Is_Deleted != true).ToList();
            ViewBag.Location = db.Location.Where(c => c.Is_Deleted != true).ToList();
            ViewBag.City = db.City.Where(c => c.Is_Deleted != true).ToList();
            ViewBag.MaterialGroup = db.Material_Group.Where(c => c.Is_Deleted != true).ToList();
            ViewBag.Material_Code1 = db.Material_Code.Where(c => c.Is_Deleted != true).ToList();
            ViewBag.UoMList = db.UoM.Where(c => c.Is_Deleted != true).ToList();

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