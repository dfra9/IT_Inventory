using System;
using System.Collections.Generic;
using System.Linq;
using DBIT_Inventory.ViewModel;
using IT_Inventory;
using IT_Inventory.Models;
using static DBIT_Inventory.ViewModel.AssetManagementViewModel;
using static DBIT_Inventory.ViewModel.ReportAssetViewModel;

namespace DBIT_Inventory.Services
{
    public class AssetService : IAssetService
    {
        private readonly DBInventory _db;

        public AssetService(DBInventory db)
        {
            _db = db;
        }

        public DashboardCountsModel GetDashboardCounts(string cityName = "")
        {
            var assetQuery = _db.City.Where(a => a.Is_Deleted != true);

            if (!string.IsNullOrEmpty(cityName))
            {
                assetQuery = assetQuery.Where(a => a.City_Name == cityName);
            }

            var latestAssets = _db.Asset
                .Where(a => a.Is_Deleted != true)
                .GroupBy(a => a.No_asset)
                .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault())
                .ToList();

            return new DashboardCountsModel
            {
                TotalAssets = latestAssets
                    .Where(a => a.Status != "Write Off")
                    .Count(),

                AvailableAssets = latestAssets
                    .Where(a => a.Status == "Ready" || a.Status == "Return")
                    .Count(),

                AssetsInUse = latestAssets
                    .Where(a => a.Status == "Borrowing" || a.Status == "Assign")
                    .Count(),

                AssetsInMaintenance = latestAssets
                    .Where(a => a.Status == "Service" || a.Status == "Damage")
                    .Count()
            };
        }

        public AssetManagementViewModel GetAssetManagementViewModel()
        {
            var viewModel = new AssetManagementViewModel
            {
                Companies = _db.Company.Where(c => c.Is_Deleted != true).ToList(),
                Dept = _db.Departement.Where(c => c.Is_Deleted != true).ToList(),
                Cities = _db.City.Where(c => c.Is_Deleted != true).ToList(),
                MaterialGroup = _db.Material_Group.Where(c => c.Is_Deleted != true).ToList(),
                Material_Code1 = _db.Material_Code.Where(c => c.Is_Deleted != true).ToList(),
                UoMList = _db.UoM.Where(c => c.Is_Deleted != true).ToList(),
                AssetList = _db.Asset
                    .Where(a => a.Is_Deleted != true)
                    .GroupBy(a => a.No_asset)
                    .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault())
                    .OrderByDescending(a => a.Transaction_Date)
                    .ToList(),
                DashboardCounts = GetDashboardCounts()
            };

            var defaultCity = viewModel.Cities.FirstOrDefault()?.City_Name;
            viewModel.LocationsList = _db.Location
                .Where(l => l.Is_Deleted != true && l.City_Name == defaultCity)
                .ToList();


            viewModel.Companies = viewModel.Companies != null ? viewModel.Companies : new List<Company>();
            viewModel.Dept = viewModel.Dept != null ? viewModel.Dept : new List<Departement>();
            viewModel.LocationsList = viewModel.LocationsList != null ? viewModel.LocationsList : new List<Location>();
            viewModel.Cities = viewModel.Cities != null ? viewModel.Cities : new List<City>();
            viewModel.Companies = viewModel.Companies != null ? viewModel.Companies : new List<Company>();
            viewModel.MaterialGroup = viewModel.MaterialGroup != null ? viewModel.MaterialGroup : new List<Material_Group>();
            viewModel.Material_Code1 = viewModel.Material_Code1 != null ? viewModel.Material_Code1 : new List<Material_Code>();
            viewModel.UoMList = viewModel.UoMList != null ? viewModel.UoMList : new List<UoM>();
            viewModel.AssetHistory = viewModel.AssetHistory != null ? viewModel.AssetHistory : new List<Asset_History>();
            viewModel.Roles = viewModel.Roles != null ? viewModel.Roles : new List<Departement>();

            return viewModel;
        }

        public AssetManagementViewModel GetAssetForEditing(string id, string mode)
        {
            AssetManagementViewModel viewModel = new AssetManagementViewModel();

            viewModel.Companies = _db.Company.Where(c => c.Is_Deleted != true).ToList();
            viewModel.Dept = _db.Departement.Where(c => c.Is_Deleted != true).ToList();
            viewModel.Cities = _db.City.Where(c => c.Is_Deleted != true).ToList();
            viewModel.MaterialGroup = _db.Material_Group
                .Where(c => c.Is_Deleted != true || c.Is_Deleted == null)
                .ToList();
            viewModel.Material_Code1 = _db.Material_Code.Where(c => c.Is_Deleted != true).ToList();
            viewModel.UoMList = _db.UoM.Where(c => c.Is_Deleted != true).ToList();
            viewModel.Roles = _db.Departement.Where(c => c.Is_Deleted != true).ToList();

            if (id != null && (mode == "Edit" || mode == "View" || mode == "Delete"))
            {
                var asset = _db.Asset
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
                    viewModel.Departement_Code = _db.Departement
                        .Where(d => d.Departement_Code == asset.Departement && d.Is_Deleted != true)
                        .Select(d => d.Departement_Code)
                        .FirstOrDefault();
                    viewModel.Departement_Name = asset.Departement;
                    viewModel.City_Name = asset.City;

                    var location = _db.Location.FirstOrDefault(l =>
                        l.Location_Name == asset.Location &&
                        l.City_Name == asset.City &&
                        l.Is_Deleted != true);

                    viewModel.Locations = location?.Location_Code;
                    viewModel.Location_Name = asset.Location;

                    if (!string.IsNullOrEmpty(asset.City))
                    {
                        viewModel.LocationsList = _db.Location
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
                    viewModel.AssetHistory = _db.Asset_History
                        .Where(a => a.No_asset == id && a.Is_Deleted != true)
                        .OrderByDescending(a => a.ID)
                        .ToList();
                }
            }
            else
            {
                viewModel.AssetHistory = new List<Asset_History>();
            }

            return viewModel;
        }

        public AssetManagementViewModel GetEditorViewModel(string id, string mode)
        {
            return GetAssetForEditing(id, mode);
        }

        public AssetOperationResult SaveAsset(AssetManagementViewModel viewModel, string userName, bool statusChange)
        {
            if (!string.IsNullOrEmpty(viewModel.Serial_Number))
            {
                var existSerial = _db.Asset.Where(a =>
                    a.Serial_Number == viewModel.Serial_Number &&
                    a.Is_Deleted != true &&
                    a.No_asset != viewModel.No_asset).FirstOrDefault();

                if (existSerial != null)
                {
                    return new AssetOperationResult
                    {
                        Success = false,
                        Message = "This Serial Number is already used by another asset"
                    };
                }
            }

            string mode = viewModel.mode ?? "Create";

            string locationName = null;
            if (!string.IsNullOrEmpty(viewModel.Locations))
            {
                var location = _db.Location.FirstOrDefault(l =>
                    l.Location_Code == viewModel.Locations && l.Is_Deleted != true);
                locationName = location?.Location_Name;
            }

            string departmentName = null;
            if (!string.IsNullOrEmpty(viewModel.Departement_Code))
            {
                var department = _db.Departement.FirstOrDefault(d =>
                    d.Departement_Code == viewModel.Departement_Code && d.Is_Deleted != true);
                departmentName = department?.Departement_Code;
            }

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
                Create_By = userName ?? "System",
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
                Create_By = userName ?? "System",
                Create_Date = DateTime.Now,
                Is_Deleted = false
            };

            if (mode == "Create")
            {
                _db.Asset.Add(assetData);
                _db.Asset_History.Add(assetHistory);
            }
            else if (mode == "Edit")
            {
                var existAsset = _db.Asset
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
                    existAsset.Edit_By = userName ?? "System";
                    existAsset.Edit_Date = DateTime.Now;
                    existAsset.Is_Deleted = false;

                    bool realStatusChange = statusChange ||
                        existAsset.Status != viewModel.Status ||
                        existAsset.PIC != viewModel.PIC ||
                        existAsset.Transaction_Date != viewModel.Transaction_Date;

                    if (realStatusChange)
                    {
                        _db.Asset_History.Add(assetHistory);
                    }
                }
            }

            _db.SaveChanges();
            var dashboardCounts = GetDashboardCounts();

            return new AssetOperationResult
            {
                Success = true,
                Message = mode == "Create" ? "Asset created successfully" : "Asset updated successfully",
                DashboardCounts = dashboardCounts,
                AssetData = new
                {
                    No_asset = viewModel.No_asset,
                    PIC = viewModel.PIC,
                    Transaction_Date = viewModel.Transaction_Date.HasValue ? viewModel.Transaction_Date.Value.ToString("yyyy-MM-dd") : "",
                    Status = viewModel.Status,
                    Role = viewModel.Role,
                    Company_User = viewModel.Company_User,
                    Submit_Date = DateTime.Now.ToString("yyyy-MM-dd")
                }
            };
        }

        public AssetOperationResult DeleteAsset(string assetId, string userName)
        {
            if (string.IsNullOrEmpty(assetId))
            {
                return new AssetOperationResult
                {
                    Success = false,
                    Message = "Asset number is required"
                };
            }

            var assetExists = _db.Asset.Any(a => a.No_asset == assetId && a.Is_Deleted != true);
            if (!assetExists)
            {
                return new AssetOperationResult
                {
                    Success = false,
                    Message = "Asset not found"
                };
            }

            var assetRecords = _db.Asset.Where(a => a.No_asset == assetId && a.Is_Deleted != true).ToList();

            foreach (var asset in assetRecords)
            {
                asset.Is_Deleted = true;
                asset.Delete_By = userName ?? "System";
                asset.Delete_Date = DateTime.Now;
            }

            var historyRecords = _db.Asset_History.Where(a => a.No_asset == assetId && a.Is_Deleted != true).ToList();

            foreach (var history in historyRecords)
            {
                history.Is_Deleted = true;
                history.Delete_By = userName ?? "System";
                history.Delete_Date = DateTime.Now;
            }

            var deletedHistory = new Asset_History
            {
                No_asset = assetId,
                Status = "Deleted",
                PIC = userName ?? "System",
                Role = "Deleted",
                Company_User = "Deleted",
                Transaction_Date = DateTime.Now,
                Submit_Date = DateTime.Now,
                Create_By = userName ?? "System",
                Create_Date = DateTime.Now,
                Delete_By = userName ?? "System",
                Delete_Date = DateTime.Now,
                Is_Deleted = true
            };

            _db.SaveChanges();
            var dashboardCounts = GetDashboardCounts();

            return new AssetOperationResult
            {
                Success = true,
                Message = "Asset deleted successfully",
                DashboardCounts = dashboardCounts
            };
        }

        public AssetOperationResult UpdateAssetStatus(string assetId, string status, string pic,
            DateTime transactionDate, string role, string companyUser, string userName)
        {
            var existingAsset = _db.Asset
                .Where(a => a.No_asset == assetId && a.Is_Deleted != true)
                .OrderByDescending(a => a.Transaction_Date)
                .FirstOrDefault();

            if (existingAsset == null)
            {
                return new AssetOperationResult
                {
                    Success = false,
                    Message = "Asset not found"
                };
            }

            bool statusChange = existingAsset.Status != status;
            bool picChanged = existingAsset.PIC != pic;
            bool dateChanged = existingAsset.Transaction_Date != transactionDate;
            bool anyChange = statusChange || picChanged || dateChanged;

            if (!anyChange)
            {
                return new AssetOperationResult
                {
                    Success = false,
                    Message = "No changes detected"
                };
            }

            var assetHistory = new Asset_History
            {
                No_asset = assetId,
                Status = status,
                PIC = pic,
                Role = role,
                Company_User = companyUser,
                Transaction_Date = transactionDate,
                Submit_Date = DateTime.Now,
                Create_By = userName ?? "System",
                Create_Date = DateTime.Now,
                Is_Deleted = false
            };

            _db.Asset_History.Add(assetHistory);
            _db.SaveChanges();

            var dashboardCounts = GetDashboardCounts();

            return new AssetOperationResult
            {
                Success = true,
                Message = "Asset status updated successfully",
                StatusChanged = true,
                DashboardCounts = dashboardCounts,
                AssetData = new
                {
                    No_asset = assetId,
                    PIC = pic,
                    Role = role,
                    Company_User = companyUser,
                    Transaction_Date = transactionDate.ToString("yyyy-MM-dd"),
                    Status = status,
                    Submit_Date = DateTime.Now.ToString("yyyy-MM-dd")
                }
            };
        }
        public dynamic GetAssetDetails(string assetId)
        {
            if (string.IsNullOrEmpty(assetId))
                return null;

            var asset = _db.Asset
                .Where(a => a.No_asset == assetId && a.Is_Deleted != true)
                .OrderByDescending(a => a.Transaction_Date)
                .FirstOrDefault();

            if (asset == null)
                return null;

            return new
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
            };
        }

        public List<AssetReportItem> GetAssetReports(string noAsset, string companyCode, string materialGroup, string materialCode, string acquisitionValue)
        {
            var assetQuery = _db.Asset
                .Where(x => (x.Is_Deleted != true || x.Is_Deleted == null));
            if (!string.IsNullOrEmpty(noAsset))
            {
                assetQuery = assetQuery.Where(x => x.No_asset == noAsset);
            }
            if (!string.IsNullOrEmpty(companyCode))
            {
                assetQuery = assetQuery.Where(x => x.Company_Code == companyCode);
            }
            if (!string.IsNullOrEmpty(materialGroup))
            {
                assetQuery = assetQuery.Where(x => x.Material_Group == materialGroup);
            }
            if (!string.IsNullOrEmpty(materialCode))
            {
                assetQuery = assetQuery.Where(x => x.Material_Code == materialCode);
            }
            if (!string.IsNullOrEmpty(acquisitionValue))
            {
                assetQuery = assetQuery.Where(x => x.Acquisition_value == acquisitionValue);
            }

            var latestAssets = assetQuery
        .GroupBy(a => a.No_asset)
        .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault())
        .ToList();

            var companyList = _db.Company.Where(c => c.Is_Deleted != true).ToList();

            var reportItem = latestAssets.Select(a => new AssetReportItem
            {
                No_asset = a.No_asset,
                Company_Code = a.Company_Code,
                Company_Name = companyList.FirstOrDefault(c => c.Company_Code == a.Company_Code)?.Company_Name,
                Material_Group = a.Material_Group,
                Material_Code = a.Material_Code,
                Material_Description = a.Material_Description,
                Serial_Number = a.Serial_Number,
                Quantity = a.Quantity,
                UoM = a.UoM,
                Acquisition_Date = a.Acquisition_Date,
                Acquisition_Value = a.Acquisition_value,
                No_Asset_PGA = a.No_Asset_PGA,
                No_Asset_Accounting = a.No_Asset_Accounting,
                No_PO = a.No_PO,
                Status = a.Status,
                Condition = a.Condition,
                Locations = a.Location,
                Dept = a.Departement,
                City_Name = a.City_Name,
                Cities = a.City,
                Latest_User = a.Latest_User,
                PIC = a.PIC,
                Role = a.Role,
                Company_User = a.Company_User,
                Create_Date = a.Create_Date,
                Edit_Date = a.Edit_Date
            }).ToList();

            return reportItem;
        }

        public AssetFilterData GetFilterDataByNoAsset(string noAsset)
        {
            var result = new AssetFilterData();

            if (string.IsNullOrEmpty(noAsset))
            {
                result.Companies = GetActiveCompanies();
                result.MaterialGroups = GetDistinctMaterialGroups();
                result.MaterialCodes = GetDistinctMaterialCodes();
                result.AcquisitionValues = GetDistinctAcquisitionValues();
            }
            else
            {
                var assetQuery = _db.Asset
                .Where(x => x.No_asset == noAsset && (x.Is_Deleted != true || x.Is_Deleted == null));

                var companyCodes = assetQuery.Select(x => x.Company_Code).Distinct().ToList();

                result.Companies = _db.Company
                    .Where(x => companyCodes.Contains(x.Company_Code) && (x.Is_Deleted != true || x.Is_Deleted == null))
                    .OrderBy(x => x.Company_Code)
                    .ToList();

                result.MaterialGroups = assetQuery
                .Select(x => x.Material_Group)
                .Distinct()
                .Where(x => !string.IsNullOrEmpty(x))
                .OrderBy(x => x)
                .ToList();

                result.MaterialCodes = assetQuery
               .Select(x => x.Material_Code)
               .Distinct()
               .Where(x => !string.IsNullOrEmpty(x))
               .OrderBy(x => x)
               .ToList();

                result.AcquisitionValues = assetQuery
                    .Select(x => x.Acquisition_value)
                    .Distinct()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .OrderBy(x => x)
                    .ToList();
            }

            return result;
        }
        public Asset GetAssetByNo(string noAsset)
        {
            return _db.Asset.FirstOrDefault(x => x.No_asset == noAsset && (x.Is_Deleted != true || x.Is_Deleted == null));
        }

        public List<string> GetDistinctAssetNumbers()
        {
            return _db.Asset
                .Where(x => x.Is_Deleted != true || x.Is_Deleted == null)
                .Select(x => x.No_asset)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        public List<Company> GetActiveCompanies()
        {
            return _db.Company
                .Where(x => x.Is_Deleted != true || x.Is_Deleted == null)
                .OrderBy(x => x.Company_Code)
                .ToList();
        }

        public List<string> GetDistinctMaterialGroups()
        {
            return _db.Asset
                .Where(x => x.Is_Deleted != true || x.Is_Deleted == null)
                .Select(x => x.Material_Group)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        public List<string> GetDistinctMaterialCodes()
        {
            return _db.Asset
                .Where(x => x.Is_Deleted != true || x.Is_Deleted == null)
                .Select(x => x.Material_Code)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        public List<string> GetDistinctAcquisitionValues()
        {
            return _db.Asset
                .Where(x => (x.Is_Deleted != true || x.Is_Deleted == null) && !string.IsNullOrEmpty(x.Acquisition_value))
                .Select(x => x.Acquisition_value)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        public string GetCompanyNameByCode(string companyCode)
        {
            if (string.IsNullOrEmpty(companyCode))
            {
                return string.Empty;
            }

            var company = _db.Company.FirstOrDefault(c => c.Company_Code == companyCode && c.Is_Deleted != true);
            return company?.Company_Name ?? string.Empty;
        }

        public List<object> GetMaterialCodesByGroup(string materialGroup)
        {
            if (string.IsNullOrEmpty(materialGroup))
            {
                return new List<object>();
            }

            return _db.Material_Code
                .Where(c => c.Material_Group == materialGroup && c.Is_Deleted != true)
                .Select(c => new
                {
                    materialCode = c.Material_Code1
                })
                .Cast<object>()
                .ToList();
        }

        public string GetMaterialDescription(string materialCode)
        {
            if (string.IsNullOrEmpty(materialCode))
            {
                return string.Empty;
            }

            return _db.Material_Code
                .Where(c => c.Material_Code1 == materialCode && c.Is_Deleted != true)
                .Select(c => c.Material_Description)
                .FirstOrDefault() ?? string.Empty;
        }

        public bool IsSerialNumberUnique(string serialNumber, string assetNo)
        {
            if (string.IsNullOrEmpty(serialNumber))
            {
                return true;
            }

            var existingAsset = _db.Asset
                .Where(a => a.Serial_Number == serialNumber &&
                           a.Is_Deleted != true &&
                           a.No_asset != assetNo)
                .FirstOrDefault();

            return existingAsset == null;
        }

        public List<object> SearchAssets(string search)
        {
            var assetData = _db.Asset.Where(a => a.Is_Deleted != true);

            if (!string.IsNullOrEmpty(search))
            {
                assetData = assetData.Where(a =>
                    a.No_asset.Contains(search) ||
                    a.PIC.Contains(search) ||
                    a.Status.Contains(search));
            }

            var latestAssets = assetData
                .GroupBy(a => a.No_asset)
                .Select(g => g.OrderByDescending(a => a.Transaction_Date).FirstOrDefault());

            return latestAssets
                .OrderByDescending(a => a.Transaction_Date)
                .Select(a => new
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
                })
                .Cast<object>()
                .ToList();
        }

        public List<object> SearchAssetHistory(string search, string noAsset)
        {
            var query = _db.Asset_History
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
            }

            return filteredResults
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
                .Cast<object>()
                .ToList();
        }

        public List<object> GetLocationsByCity(string cityId)
        {
            if (string.IsNullOrEmpty(cityId))
            {
                return new List<object>();
            }

            return _db.Location
                .Where(l => l.City_Name == cityId && l.Is_Deleted != true)
                .Select(l => new
                {
                    locationCode = l.Location_Code,
                    locationName = l.Location_Name
                })
                .Cast<object>()
                .ToList();
        }
        public class AssetFilterData
        {
            public List<Company> Companies { get; set; }
            public List<string> MaterialGroups { get; set; }
            public List<string> MaterialCodes { get; set; }
            public List<string> AcquisitionValues { get; set; }

            public AssetFilterData()
            {
                Companies = new List<Company>();
                MaterialGroups = new List<string>();
                MaterialCodes = new List<string>();
                AcquisitionValues = new List<string>();
            }
        }
    }
}