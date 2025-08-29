using System.Linq;
using IT_Inventory.Models;

namespace DBIT_Inventory.Services
{
    public class DropdownService : IDropdownService
    {
        private readonly DBInventory _db;

        public DropdownService(DBInventory db)
        {
            _db = db;
        }

        public void LoadAllDropdownData(dynamic viewBag)
        {

            viewBag.Companies = _db.Company
                .Where(c => c.Is_Deleted != true)
                .OrderBy(c => c.Company_Name)
                .ToList();

            viewBag.Departments = _db.Departement
                .Where(d => d.Is_Deleted != true)
                .OrderBy(d => d.Departement_Name)
                .ToList();

            viewBag.Cities = _db.City
                .Where(c => c.Is_Deleted != true)
                .OrderBy(c => c.City_Name)
                .ToList();

            viewBag.MaterialGroups = _db.Material_Group
                .Where(m => m.Is_Deleted != true)
                .OrderBy(m => m.Material_Group1)
                .ToList();

            viewBag.UoMs = _db.UoM
                .Where(u => u.Is_Deleted != true)
                .OrderBy(u => u.UoM_Id)
                .ToList();

            viewBag.AssetStatuses = new[]
            {
                "Ready",
                "Assign",
                "Borrowing",
                "Service",
                "Return",
                "Write Off",
                "Damage"
            };

            viewBag.AssetConditions = new[]
            {
                "Good",
                "Fair",
                "Poor"
            };
        }
    }
}
