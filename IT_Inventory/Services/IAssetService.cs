using System;
using System.Collections.Generic;
using DBIT_Inventory.ViewModel;
using IT_Inventory;
using static DBIT_Inventory.Services.AssetService;
using static DBIT_Inventory.ViewModel.AssetManagementViewModel;
using static DBIT_Inventory.ViewModel.ReportAssetViewModel;

namespace DBIT_Inventory.Services
{
    public interface IAssetService
    {
        DashboardCountsModel GetDashboardCounts(string cityName = "");
        AssetManagementViewModel GetAssetManagementViewModel();
        AssetManagementViewModel GetAssetForEditing(string id, string mode);
        AssetManagementViewModel GetEditorViewModel(string id, string mode);
        AssetOperationResult SaveAsset(AssetManagementViewModel viewModel, string userName, bool statusChange);
        AssetOperationResult DeleteAsset(string assetId, string userName);
        AssetOperationResult UpdateAssetStatus(string assetId, string status, string pic,
            DateTime transactionDate, string role, string companyUser, string userName);
        dynamic GetAssetDetails(string assetId);
        List<AssetReportItem> GetAssetReports(string noAsset, string companyCode, string materialGroup, string materialCode, string acquisitionValue);
        AssetFilterData GetFilterDataByNoAsset(string noAsset);
        Asset GetAssetByNo(string noAsset);
        List<string> GetDistinctAssetNumbers();
        List<Company> GetActiveCompanies();
        List<string> GetDistinctMaterialGroups();
        List<string> GetDistinctMaterialCodes();
        List<string> GetDistinctAcquisitionValues();
        string GetCompanyNameByCode(string companyCode);
        List<object> GetMaterialCodesByGroup(string materialGroup);
        string GetMaterialDescription(string materialCode);
        bool IsSerialNumberUnique(string serialNumber, string assetNo);
        List<object> SearchAssets(string search);
        List<object> SearchAssetHistory(string search, string noAsset);
        List<object> GetLocationsByCity(string cityId);
    }
    public class AssetOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool StatusChanged { get; set; }
        public DashboardCountsModel DashboardCounts { get; set; }
        public object AssetData { get; set; }
    }
}