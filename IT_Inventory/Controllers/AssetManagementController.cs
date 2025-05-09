using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.Services;
using IT_Inventory.Utilities;
using IT_Inventory.ViewModel;

namespace IT_Inventory.Controllers
{
    public class AssetManagementController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly IT_Inventory db = new IT_Inventory();

        public AssetManagementController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        private void LoadDropdownData()
        {
            DropdownLoaderUtility.LoadAllDropdownData(db, ViewBag);
        }


        // GET: AssetManagement
        public ActionResult Index()
        {
            var viewModel = new AssetManagementViewModel
            {
                Companies = db.Company.Where(c => c.Is_Deleted != true).ToList(),
                Dept = db.Departement.Where(c => c.Is_Deleted != true).ToList(),
                Cities = db.City.Where(c => c.Is_Deleted != true).ToList(),
                MaterialGroup = db.Material_Group.Where(c => c.Is_Deleted != true).ToList(),
                Material_Code1 = db.Material_Code.Where(c => c.Is_Deleted != true).ToList(),
                UoMList = db.UoM.Where(c => c.Is_Deleted != true).ToList(),
                AssetList = db.Asset
                    .Where(a => a.Is_Deleted != true)
                    .GroupBy(a => a.No_asset)
                    .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault())
                    .OrderByDescending(a => a.Transaction_Date)
                    .ToList(),
                DashboardCounts = _assetService.GetDashboardCounts()
            };

            var defaultCity = viewModel.Cities.FirstOrDefault()?.City_Name;
            viewModel.LocationsList = db.Location
                .Where(l => l.Is_Deleted != true && l.City_Name == defaultCity)
                .ToList();
            viewModel.Companies = viewModel.Companies ?? new List<Company>();
            viewModel.Dept = viewModel.Dept ?? new List<Departement>();
            viewModel.LocationsList = viewModel.LocationsList ?? new List<Location>();
            viewModel.Cities = viewModel.Cities ?? new List<City>();
            viewModel.MaterialGroup = viewModel.MaterialGroup ?? new List<Material_Group>();
            viewModel.Material_Code1 = viewModel.Material_Code1 ?? new List<Material_Code>();
            viewModel.UoMList = viewModel.UoMList ?? new List<UoM>();
            viewModel.AssetHistory = viewModel.AssetHistory ?? new List<Asset_History>();
            viewModel.Roles = viewModel.Roles ?? new List<Departement>();

            LoadDropdownData();
            return View(viewModel);
        }

        public ActionResult Editor(string id = null, string mode = "Create")
        {
            AssetManagementViewModel viewModel = new AssetManagementViewModel();
            viewModel.mode = mode;

            LoadDropdownData();


            viewModel.Companies = db.Company.Where(c => c.Is_Deleted != true).ToList();
            viewModel.Dept = db.Departement.Where(c => c.Is_Deleted != true).ToList();
            viewModel.Cities = db.City.Where(c => c.Is_Deleted != true).ToList();
            viewModel.MaterialGroup = db.Material_Group
            .Where(c => c.Is_Deleted != true || c.Is_Deleted == null)
            .ToList();
            viewModel.Material_Code1 = db.Material_Code.Where(c => c.Is_Deleted != true).ToList();
            viewModel.UoMList = db.UoM.Where(c => c.Is_Deleted != true).ToList();
            viewModel.Roles = db.Departement.Where(c => c.Is_Deleted != true).ToList();

            if (id != null && (mode == "Edit" || mode == "View" || mode == "Delete"))
            {
                var asset = db.Asset
                .Where(a => a.No_asset == id && a.Is_Deleted != true)
                .OrderByDescending(a => a.Transaction_Date)
                .FirstOrDefault();

                if (asset != null)
                {
                    viewModel.No_asset = asset.No_asset;
                    viewModel.Company_Code = asset.Company_Code;
                    viewModel.Company_Name = asset.Company_Name;
                    viewModel.Material_Group = asset.Material_Group;
                    viewModel.Material_Code = asset.Material_Code;
                    viewModel.Material_Description = asset.Material_Description;
                    viewModel.Quantity = asset.Quantity;
                    viewModel.UoM = asset.UoM;
                    viewModel.Serial_Number = asset.Serial_Number;
                    viewModel.Device_Id = asset.Device_Id;
                    viewModel.Acquisition_Date = asset.Acquisition_Date;
                    viewModel.Acquisition_value = asset.Acquisition_value;
                    viewModel.No_Asset_PGA = asset.No_Asset_PGA;
                    viewModel.No_Asset_Accounting = asset.No_Asset_Accounting;
                    viewModel.No_PO = asset.No_PO;
                    viewModel.Latest_User = asset.Latest_User;
                    viewModel.Asset_Image = asset.Asset_Image;
                    viewModel.Departement_Code = db.Departement
                        .Where(d => d.Departement_Code == asset.Departement && d.Is_Deleted != true)
                        .Select(d => d.Departement_Code)
                        .FirstOrDefault();
                    viewModel.Departement_Name = asset.Departement;
                    viewModel.City_Name = asset.City;
                    var location = db.Location.FirstOrDefault(l =>
                    l.Location_Name == asset.Location &&
                    l.City_Name == asset.City &&
                    l.Is_Deleted != true);
                    viewModel.Locations = location?.Location_Code;
                    viewModel.Location_Name = asset.Location;
                    if (!string.IsNullOrEmpty(asset.City))
                    {
                        viewModel.LocationsList = db.Location
                            .Where(l => l.City_Name == asset.City && l.Is_Deleted != true)
                            .ToList();
                    }
                    viewModel.Last_Check_Date = asset.Last_Check_Date;
                    viewModel.Condition = asset.Condition;
                    viewModel.Status = asset.Status;
                    viewModel.PIC = asset.PIC;
                    viewModel.Role = asset.Role;
                    viewModel.Company_User = asset.Company_User;
                    viewModel.Transaction_Date = asset.Transaction_Date.HasValue ? (DateTime)asset.Transaction_Date : DateTime.Now;
                    viewModel.AssetHistory = db.Asset_History
                   .Where(a => a.No_asset == id && a.Is_Deleted != true)
                   .OrderByDescending(a => a.ID)
                   .ToList();

                }
                else
                {
                    TempData["ErrorMessage"] = "Asset not found.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                viewModel.AssetHistory = new List<Asset_History>();
            }

            ViewBag.Mode = mode;
            return View(viewModel);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(AssetManagementViewModel viewModel, List<HttpPostedFileBase> file, bool statusChange = false)
        {
            try
            {
                string mode = viewModel.mode ?? "Create";

                if (mode == "Delete")
                {
                    return DeleteAsset(viewModel.No_asset);
                }

                if (!string.IsNullOrEmpty(viewModel.Serial_Number))
                {
                    var existSerial = db.Asset.Where(a => a.Serial_Number == viewModel.Serial_Number && a.Is_Deleted != true && a.No_asset != viewModel.No_asset).FirstOrDefault();

                    if (existSerial != null)
                    {
                        ModelState.AddModelError("Serial_Number", "This Serial Number is already used by another asset");

                        if (Request.IsAjaxRequest())
                        {
                            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                                .ToDictionary(
                                    kvp => kvp.Key,
                                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                                );
                            return Json(new { success = false, message = "Serial Number already exists", errors = errors });
                        }
                    }
                }



                List<string> imagePaths = new List<string>();

                if (mode == "Edit" && !string.IsNullOrEmpty(viewModel.Asset_Image))
                {
                    imagePaths.AddRange(viewModel.Asset_Image.Split(';').Where(p => !string.IsNullOrWhiteSpace(p)));
                }

                if (file != null && file.Any(f => f != null && f.ContentLength > 0))
                {
                    string uploadPath = Server.MapPath("~/UploadFile/");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    foreach (var uploadedFile in file.Where(f => f != null && f.ContentLength > 0))
                    {
                        string originalFileName = uploadedFile.FileName;
                        string filePath = Path.Combine(uploadPath, originalFileName);

                        uploadedFile.SaveAs(filePath);
                        imagePaths.Add(originalFileName);
                    }
                }

                viewModel.Asset_Image = string.Join(";", imagePaths);

                string locationName = null;
                if (!string.IsNullOrEmpty(viewModel.Locations))
                {
                    var location = db.Location.FirstOrDefault(l => l.Location_Code == viewModel.Locations && l.Is_Deleted != true);
                    locationName = location?.Location_Name;
                }
                string departmentName = null;
                if (!string.IsNullOrEmpty(viewModel.Departement_Code))
                {
                    var department = db.Departement.FirstOrDefault(d => d.Departement_Code == viewModel.Departement_Code && d.Is_Deleted != true);
                    departmentName = department?.Departement_Code;
                }

                if (string.IsNullOrEmpty(viewModel.No_asset))
                {
                    ModelState.AddModelError("No_asset", "No Asset is required");
                }
                if (string.IsNullOrEmpty(viewModel.Material_Code))
                {
                    ModelState.AddModelError("Material_Code", "Material Code is required");
                }
                if (string.IsNullOrEmpty(viewModel.Material_Group))
                {
                    ModelState.AddModelError("Material_Group", "Material Group is required");
                }
                if (string.IsNullOrEmpty(viewModel.Company_Code))
                {
                    ModelState.AddModelError("Company_Code", "Company Code is required");
                }
                if (string.IsNullOrEmpty(viewModel.Departement_Code))
                {
                    ModelState.AddModelError("Departement_Code", "Department is required");
                }
                if (string.IsNullOrEmpty(viewModel.City_Name))
                {
                    ModelState.AddModelError("City_Name", "City is required");
                }
                if (string.IsNullOrEmpty(viewModel.Locations))
                {
                    ModelState.AddModelError("Locations", "Location is required");
                }
                if (string.IsNullOrEmpty(viewModel.Material_Description))
                {
                    ModelState.AddModelError("Material_Description", "Asset Description is required");
                }
                if (string.IsNullOrEmpty(viewModel.Status))
                {
                    ModelState.AddModelError("Status", "Status is required");
                }
                if (string.IsNullOrEmpty(viewModel.PIC))
                {
                    ModelState.AddModelError("PIC", "PIC is required");
                }
                if (!viewModel.Transaction_Date.HasValue)
                {
                    ModelState.AddModelError("Transaction_Date", "Transaction Date is required");
                }
                if (ModelState.IsValid)
                {
                    var assetData = new Asset
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
                        Departement = departmentName,
                        Location = locationName,
                        City = viewModel.City_Name,
                        Last_Check_Date = viewModel.Last_Check_Date,
                        Condition = viewModel.Condition,
                        Status = viewModel.Status,
                        PIC = viewModel.PIC,
                        Role = viewModel.Role,
                        Asset_Image = viewModel.Asset_Image,
                        Company_User = viewModel.Company_User,
                        Transaction_Date = viewModel.Transaction_Date,
                        Create_By = User.Identity.Name ?? "System",
                        Create_Date = DateTime.Now,
                        Is_Deleted = false
                    };

                    var assetHistory = new Asset_History
                    {
                        No_asset = viewModel.No_asset,
                        Status = viewModel.Status,
                        PIC = viewModel.PIC,
                        Role = viewModel.Role,
                        Company_User = viewModel.Company_User,
                        Transaction_Date = viewModel.Transaction_Date,
                        Submit_Date = DateTime.Now,
                        Create_By = User.Identity.Name ?? "System",
                        Create_Date = DateTime.Now,
                        Is_Deleted = false
                    };


                    if (mode == "Create")
                    {
                        db.Asset.Add(assetData);
                        db.Asset_History.Add(assetHistory);
                    }
                    else if (mode == "Edit")
                    {

                        var existAsset = db.Asset
                    .Where(a => a.No_asset == viewModel.No_asset && a.Is_Deleted != true)
                    .OrderByDescending(a => a.Transaction_Date)
                    .FirstOrDefault();

                        if (existAsset != null)
                        {

                            existAsset.No_asset = viewModel.No_asset;
                            existAsset.Company_Code = viewModel.Company_Code;
                            existAsset.Company_Name = viewModel.Company_Name;
                            existAsset.Material_Group = viewModel.Material_Group;
                            existAsset.Material_Code = viewModel.Material_Code;
                            existAsset.Material_Description = viewModel.Material_Description;
                            existAsset.Quantity = viewModel.Quantity;
                            existAsset.UoM = viewModel.UoM;
                            existAsset.Serial_Number = viewModel.Serial_Number;
                            existAsset.Device_Id = viewModel.Device_Id;
                            existAsset.Acquisition_Date = viewModel.Acquisition_Date;
                            existAsset.Acquisition_value = viewModel.Acquisition_value;
                            existAsset.No_Asset_PGA = viewModel.No_Asset_PGA;
                            existAsset.No_Asset_Accounting = viewModel.No_Asset_Accounting;
                            existAsset.No_PO = viewModel.No_PO;
                            existAsset.Latest_User = viewModel.Latest_User;
                            existAsset.Departement = departmentName;
                            existAsset.Location = locationName;
                            existAsset.City = viewModel.City_Name;
                            existAsset.Last_Check_Date = viewModel.Last_Check_Date;
                            existAsset.Asset_Image = viewModel.Asset_Image;
                            existAsset.Condition = viewModel.Condition;
                            existAsset.Status = viewModel.Status;
                            existAsset.PIC = viewModel.PIC;
                            existAsset.Role = viewModel.Role;
                            existAsset.Company_User = viewModel.Company_User;
                            existAsset.Transaction_Date = viewModel.Transaction_Date;
                            existAsset.Edit_By = User.Identity.Name ?? "System";
                            existAsset.Edit_Date = DateTime.Now;
                            existAsset.Is_Deleted = false;
                            bool realStatusChange = statusChange ||
                            existAsset.Status != viewModel.Status ||
                            existAsset.PIC != viewModel.PIC ||
                            existAsset.Transaction_Date != viewModel.Transaction_Date;

                            if (realStatusChange)
                            {
                                db.Asset_History.Add(assetHistory);
                            }
                        }
                    }
                    assetData.Asset_Image = viewModel.Asset_Image;
                    db.SaveChanges();
                    var dashboardCounts = _assetService.GetDashboardCounts();

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new
                        {
                            success = true,
                            message = mode == "Create" ? "Asset created successfully" : "Asset updated successfully",
                            assetData = new
                            {
                                No_asset = viewModel.No_asset,
                                PIC = viewModel.PIC,
                                Transaction_Date = viewModel.Transaction_Date.HasValue ? viewModel.Transaction_Date.Value.ToString("yyyy-MM-dd") : "",
                                Status = viewModel.Status,
                                Role = viewModel.Role,
                                Company_User = viewModel.Company_User,
                                Submit_Date = DateTime.Now.ToString("yyyy-MM-dd")
                            },
                            dashboardCounts = dashboardCounts
                        });
                    }
                    else
                    {
                        TempData["SuccessMessage"] = mode == "Create" ? "Asset created successfully" : "Asset updated successfully";
                        TempData["DashboardCounts"] = dashboardCounts;
                        return RedirectToAction("Index");
                    }
                }
                LoadDropdownData();
                viewModel.DashboardCounts = _assetService.GetDashboardCounts();
                ViewBag.Mode = mode;
                if (Request.IsAjaxRequest())
                {
                    var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    return Json(new { success = false, message = "Validation failed", errors = errors });
                }
                return View(viewModel);
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    LoadDropdownData();
                    return Json(new
                    {
                        success = false,
                        message = "An error occurred while processing your request: " + ex.Message,
                        details = ex.ToString()
                    });
                }
                return View(viewModel);
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

        public ActionResult GetAssetDetails(string assetId)
        {
            try
            {
                if (string.IsNullOrEmpty(assetId))
                {
                    return Json(new { success = false, message = "Asset ID is required." }, JsonRequestBehavior.AllowGet);
                }
                var asset = db.Asset
                    .Where(a => a.No_asset == assetId && a.Is_Deleted != true)
                    .OrderByDescending(a => a.Transaction_Date)
                    .FirstOrDefault();

                return Json(new
                {
                    success = true,
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
                    asset.Acquisition_Date,
                    asset.Acquisition_value,
                    asset.PIC,
                    asset.Status,
                    asset.Role,
                    asset.Company_User,
                    asset.Transaction_Date,
                    asset.Location,
                    asset.Departement,
                    asset.Condition,
                    asset.Asset_Image
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while retrieving asset details: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAsset(string No_asset, string Status, string PIC, DateTime? Transaction_Date, string Role, string Company_User)
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

                var existingAsset = db.Asset
                .Where(a => a.No_asset == No_asset && a.Is_Deleted != true)
                .OrderByDescending(a => a.Transaction_Date)
                .FirstOrDefault();

                bool statusChange = existingAsset.Status != Status;
                bool picChanged = existingAsset.PIC != PIC;
                bool dateChanged = existingAsset.Transaction_Date != Transaction_Date;
                bool roleChanged = existingAsset.Role != Role;
                bool companyChanged = existingAsset.Company_User != Company_User;
                bool anyField = statusChange || picChanged || dateChanged;

                if (anyField)
                {
                    var newAsset = new Asset
                    {
                        No_asset = existingAsset.No_asset,
                        Company_Code = existingAsset.Company_Code,
                        Company_Name = existingAsset.Company_Name,
                        Material_Group = existingAsset.Material_Group,
                        Material_Code = existingAsset.Material_Code,
                        Material_Description = existingAsset.Material_Description,
                        Quantity = existingAsset.Quantity,
                        UoM = existingAsset.UoM,
                        Serial_Number = existingAsset.Serial_Number,
                        Device_Id = existingAsset.Device_Id,
                        Acquisition_Date = existingAsset.Acquisition_Date,
                        Acquisition_value = existingAsset.Acquisition_value,
                        No_Asset_PGA = existingAsset.No_Asset_PGA,
                        No_Asset_Accounting = existingAsset.No_Asset_Accounting,
                        No_PO = existingAsset.No_PO,
                        Latest_User = existingAsset.Latest_User,
                        Departement = existingAsset.Departement,
                        Location = existingAsset.Location,
                        City = existingAsset.City,
                        Last_Check_Date = existingAsset.Last_Check_Date,
                        Asset_Image = existingAsset.Asset_Image,
                        Condition = existingAsset.Condition,
                        Status = Status,
                        PIC = PIC,
                        Role = Role,
                        Company_User = Company_User,
                        Transaction_Date = Transaction_Date,
                        Create_By = User.Identity.Name ?? "System",
                        Create_Date = DateTime.Now,
                        Is_Deleted = false,

                    };


                    var assetHistory = new Asset_History
                    {
                        No_asset = No_asset,
                        Status = Status,
                        PIC = PIC,
                        Role = Role,
                        Company_User = Company_User,
                        Transaction_Date = Transaction_Date,
                        Submit_Date = DateTime.Now,
                        Create_By = User.Identity.Name ?? "System",
                        Create_Date = DateTime.Now,
                        Is_Deleted = false
                    };

                    db.Asset_History.Add(assetHistory);
                    db.SaveChanges();
                    var dashboardCounts = _assetService.GetDashboardCounts();
                    return Json(new
                    {
                        success = true,
                        message = "Asset status updated successfully",
                        statusChange = true,
                        dashboardCounts = dashboardCounts,
                        assetData = new
                        {
                            No_asset = No_asset,
                            PIC = PIC,
                            Role = Role,
                            Company_User = Company_User,
                            Transaction_Date = Transaction_Date.Value.ToString("yyyy-MM-dd"),
                            Status = Status,
                            Submit_Date = DateTime.Now.ToString("yyyy-MM-dd")
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "No changes detected" });
                }
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = "An error occurred while updating the asset",
                    errorDetails = ex.Message,
                });
            }
        }

        public ActionResult DeleteAsset(string No_asset)
        {
            try
            {
                if (string.IsNullOrEmpty(No_asset))
                {
                    if (Request.IsAjaxRequest())

                        return Json(new { success = false, message = "Asset number is required" });

                    TempData["ErrorMessage"] = "Asset number is required";
                    return RedirectToAction("Index");

                }
                var assetExists = db.Asset.Any(a => a.No_asset == No_asset && a.Is_Deleted != true);
                if (!assetExists)
                {
                    if (Request.IsAjaxRequest())
                        return Json(new { success = false, message = "Asset not found" });

                    TempData["ErrorMessage"] = "Asset not found";
                    return RedirectToAction("Index");
                }

                var assetRecords = db.Asset.Where(a => a.No_asset == No_asset && a.Is_Deleted != true).ToList();

                foreach (var asset in assetRecords)
                {
                    asset.Is_Deleted = true;
                    asset.Delete_By = User.Identity.Name ?? "System";
                    asset.Delete_Date = DateTime.Now;
                }

                var historyRecords = db.Asset_History.Where(a => a.No_asset == No_asset && a.Is_Deleted != true).ToList();

                foreach (var history in historyRecords)
                {
                    history.Is_Deleted = true;
                    history.Delete_By = User.Identity.Name ?? "System";
                    history.Delete_Date = DateTime.Now;
                }

                var deletedHistory = new Asset_History
                {
                    No_asset = No_asset,
                    Status = "Deleted",
                    PIC = User.Identity.Name ?? "System",
                    Role = "Deleted",
                    Company_User = "Deleted",
                    Transaction_Date = DateTime.Now,
                    Submit_Date = DateTime.Now,
                    Create_By = User.Identity.Name ?? "System",
                    Create_Date = DateTime.Now,
                    Delete_By = User.Identity.Name ?? "System",
                    Delete_Date = DateTime.Now,
                    Is_Deleted = true
                };

                db.SaveChanges();
                var dashboardCounts = _assetService.GetDashboardCounts();

                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = true,
                        message = "Asset deleted successfully",
                        dashboardCounts = dashboardCounts
                    });
                }

                TempData["SuccessMassage"] = "Asset deleted successfully";
                TempData["DashboardCounts"] = dashboardCounts;
                return RedirectToAction("Index");
            }

            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "An error occurred while deleting the asset: " + ex.Message });
                }

                TempData["ErrorMassage"] = "An error occurred while deleting the asset: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public JsonResult GetMaterialCodesByGroup(string materialGroup)
        {
            try
            {
                if (string.IsNullOrEmpty(materialGroup))
                {
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);
                }

                var materialCodes = db.Material_Code
                    .Where(c => c.Material_Group == materialGroup && c.Is_Deleted != true)
                    .Select(c => new
                    {
                        materialCode = c.Material_Code1
                    })
                    .ToList();

                return Json(materialCodes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting material codes: {ex.Message}");
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetMaterialDescription(string materialCode)
        {
            try
            {
                if (string.IsNullOrEmpty(materialCode))
                {
                    return Json(string.Empty, JsonRequestBehavior.AllowGet);
                }

                var material = db.Material_Code
                    .Where(c => c.Material_Code1 == materialCode && c.Is_Deleted != true)
                    .Select(c => c.Material_Description)
                    .FirstOrDefault();

                return Json(material ?? string.Empty, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting material description: {ex.Message}");
                return Json(string.Empty, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult CheckSerialNumberUnique(string serialNumber, string assetNo)
        {
            try
            {
                if (string.IsNullOrEmpty(serialNumber))
                {
                    return Json(new { isUnique = true }, JsonRequestBehavior.AllowGet);
                }

                var existingAsset = db.Asset
                    .Where(a => a.Serial_Number == serialNumber &&
                               a.Is_Deleted != true &&
                               a.No_asset != assetNo)
                    .FirstOrDefault();

                return Json(new { isUnique = (existingAsset == null) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking serial number uniqueness: {ex.Message}");
                return Json(new { isUnique = false, error = "Error checking serial number uniqueness" }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetAssetData(string search)
        {
            var assetData = db.Asset.Where(a => a.Is_Deleted != true);
            if (!string.IsNullOrEmpty(search))
            {
                assetData = assetData.Where(a => a.No_asset.Contains(search) || a.PIC.Contains(search) || a.Status.Contains(search));
            }

            var latestAssets = assetData
                .GroupBy(a => a.No_asset)
                .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault());
            var assets = latestAssets.OrderByDescending(a => a.Transaction_Date).Select(a => new
            {
                a.No_asset,
                a.Company_Name,
                a.Material_Group,
                a.Material_Code,
                a.Material_Description,
                a.Departement,
                a.Location,
                a.PIC,
                a.Role,
                a.Company_User,
                a.Transaction_Date,
                a.Status,
                Submit_Date = a.Create_Date
            }).ToList();
            return Json(assets, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchAssetHistory(string search, string noAsset)
        {
            try
            {
                if (string.IsNullOrEmpty(noAsset))
                {
                    return Json(new { success = false, message = "No asset selected" }, JsonRequestBehavior.AllowGet);
                }

                var query = db.Asset_History
                .Where(h => h.No_asset == noAsset && (h.Is_Deleted == false || h.Is_Deleted == null))
                .ToList();

                var filteredResults = query;

                if (!string.IsNullOrEmpty(search))
                {
                    string lowercaseSearch = search.Trim().ToLower();

                    filteredResults = query.Where(a =>
                        (a.No_asset != null && a.No_asset.ToLower().Contains(lowercaseSearch)) ||
                        (a.PIC != null && a.PIC.ToLower().Contains(lowercaseSearch)) ||
                        (a.Status != null && a.Status.ToLower().Contains(lowercaseSearch)) ||
                        (a.Role != null && a.Role.ToLower().Contains(lowercaseSearch)) ||
                        (a.Company_User != null && a.Company_User.ToLower().Contains(lowercaseSearch)) ||
                        (a.Create_Date != null && a.Create_Date.Value.ToString("yyyy-MM-dd").Contains(lowercaseSearch)) ||
                        (a.Transaction_Date != null && a.Transaction_Date.Value.ToString("yyyy-MM-dd").Contains(lowercaseSearch))
                    ).ToList();

                    System.Diagnostics.Debug.WriteLine($"Filtered results count: {filteredResults.Count}");
                }

                var assets = filteredResults
                    .OrderByDescending(a => a.Transaction_Date)
                    .Select(a => new
                    {
                        a.No_asset,
                        a.PIC,
                        a.Role,
                        a.Company_User,
                        Transaction_Date = a.Transaction_Date.HasValue ? a.Transaction_Date.Value.ToString("yyyy-MM-dd") : "",
                        a.Status,
                        Submit_Date = a.Create_Date.HasValue ? a.Create_Date.Value.ToString("yyyy-MM-dd") : ""
                    })
                    .ToList();

                return Json(new { success = true, data = assets }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SearchAssetHistory: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while searching asset history: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
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



    }
}