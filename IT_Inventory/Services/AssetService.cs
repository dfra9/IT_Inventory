using System.Linq;
using IT_Inventory.Models;
using static IT_Inventory.ViewModel.AssetManagementViewModel;

namespace IT_Inventory.Services
{

    public interface IAssetService
    {
        DashboardCountsModel GetDashboardCounts();
    }
    public class AssetService : IAssetService
    {
        private readonly IT_Inventory _db;

        public AssetService(IT_Inventory db)
        {
            _db = db;
        }


        public DashboardCountsModel GetDashboardCounts()
        {

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
    }
}