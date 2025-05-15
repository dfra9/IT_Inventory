using System.Collections.Generic;
using System.Linq;
using IT_Inventory.Models;
using static IT_Inventory.ViewModel.AssetManagementViewModel;
using static IT_Inventory.ViewModel.ReportAssetViewModel;

namespace IT_Inventory.Services
{

    public interface IAssetService
    {
        DashboardCountsModel GetDashboardCounts(string cityName = "");
    }
    public class AssetService : IAssetService
    {
        private readonly IT_Inventory _db;

        public AssetService(IT_Inventory db)
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
                    .Where(a => a.Status == "Service")
                    .Count()
            };
        }

        public Asset GetAssetDetails(string assetId)
        {

            if (string.IsNullOrEmpty(assetId))
                return null;
            return _db.Asset
                .Where(a => a.No_asset == assetId && a.Is_Deleted != true)
                .OrderByDescending(a => a.Transaction_Date)
                .FirstOrDefault();

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