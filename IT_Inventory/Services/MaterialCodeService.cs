using System;
using System.Collections.Generic;
using System.Linq;
using IT_Inventory.Models;
using IT_Inventory.ViewModel;

namespace IT_Inventory.Services
{

    public class MaterialCodeService
    {
        private readonly DBIT_Inventory db;

        public MaterialCodeService(DBIT_Inventory db)
        {
            this.db = db;
        }

        public List<MaterialCodeViewModel> GetAllMaterialCodes()
        {
            return db.Material_Code.Where(m => m.Is_Deleted != true).Select(m => new MaterialCodeViewModel
            {
                Material_CodeId = m.Material_CodeId,
                Material_Code1 = m.Material_Code1,
                Material_Description = m.Material_Description,
                Material_Group = m.Material_Group,
                Create_By = m.Create_By,
                Create_Date = m.Create_Date,
                Edit_By = m.Edit_By,
                Edit_Date = m.Edit_Date,
                Delete_By = m.Delete_By,
                Delete_Date = m.Delete_Date,
                Is_Deleted = m.Is_Deleted
            }).ToList();
        }

        public Material_Code GetMaterialCodeById(int id)
        {
            return db.Material_Code.FirstOrDefault(m => m.Material_CodeId == id);
        }

        public void CreateMaterialCode(MaterialCodeViewModel materialCodeViewModel)
        {
            var materialCode = new Material_Code
            {
                Material_Code1 = materialCodeViewModel.Material_Code1,
                Material_Description = materialCodeViewModel.Material_Description,
                Material_Group = materialCodeViewModel.Material_Group,
                Create_By = materialCodeViewModel.Create_By,
                Create_Date = DateTime.Now,
                Is_Deleted = false
            };
            db.Material_Code.Add(materialCode);
            db.SaveChanges();
        }

        public void UpdateMaterialCode(MaterialCodeViewModel materialCodeViewModel)
        {
            var materialCode = db.Material_Code.FirstOrDefault(m => m.Material_CodeId == materialCodeViewModel.Material_CodeId);
            if (materialCode == null) return;
            materialCode.Material_Code1 = materialCodeViewModel.Material_Code1;
            materialCode.Material_Description = materialCodeViewModel.Material_Description;
            materialCode.Material_Group = materialCodeViewModel.Material_Group;
            materialCode.Edit_By = materialCodeViewModel.Edit_By;
            materialCode.Edit_Date = DateTime.Now;
            db.SaveChanges();
        }

        public void DeleteMaterialCode(int id)
        {
            var materialCode = db.Material_Code.FirstOrDefault(m => m.Material_CodeId == id);
            if (materialCode == null)
            {
                materialCode.Is_Deleted = true;
                materialCode.Delete_By = "Admin";
                materialCode.Delete_Date = DateTime.Now;
                db.SaveChanges();

            }
        }
    }
}