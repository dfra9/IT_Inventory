using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using DBIT_Inventory.Services;
using DBIT_Inventory.ViewModel;
using IT_Inventory;

namespace DBIT_Inventory.Controllers
{
    public class MaterialGroupController : Controller
    {
        private readonly DBIT_Inventory db;


        public MaterialGroupController(DBIT_Inventory db, IUserService userService)
        {
            this.db = db ?? throw new ArgumentNullException(nameof(db));

        }

        private string EncodeId(string id)
        {
            if (string.IsNullOrEmpty(id))
                return string.Empty;

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(id);
            return Convert.ToBase64String(bytes);
        }

        private string DecodeId(string encodedId)
        {
            if (string.IsNullOrEmpty(encodedId))
                return string.Empty;

            try
            {
                byte[] bytes = Convert.FromBase64String(encodedId);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return encodedId;
            }
        }

        public ActionResult Index()
        {
            var materialGroups = db.Material_Group.Where(u => u.Is_Deleted != true).AsEnumerable().Select(u =>
            {
                var model = new MaterialGroupViewModel
                {
                    MaterialGroup = u.Material_Group1,

                    MaterialDescription = u.Material_Description,
                    AgeAccountingAsset = FormatAgeAccountingAsset(u.Age_Accounting_Asset),
                    Quantity = u.Quantity ?? 0,
                    LastCheckDate = u.Last_Check_Date.HasValue ? u.Last_Check_Date.Value.Date.ToString("yyyy-MM-dd") : null,
                    MaxWarrantyDate = u.Max_Warranty_Date.HasValue ? u.Max_Warranty_Date.Value.Date.ToString("yyyy-MM-dd") : null

                };
                return model;
            }).ToList();
            return View(materialGroups);
        }


        //GET: MaterialGroup/Editor
        [ValidateInput(false)]
        public ActionResult Editor(string mode = "Create", string id = null)
        {
            ViewBag.Mode = mode;
            ViewBag.MaterialGroup = db.Material_Group
                .Where(mg => mg.Is_Deleted != true)
                .Select(mg => mg.Material_Group1)
                .ToList();

            if (mode == "Create")
            {
                return View(new Material_Group());
            }
            if (string.IsNullOrEmpty(id))
            {
                TempData["Message"] = "Material Group not found";
                return RedirectToAction("Index");
            }
            string decodedId = DecodeId(id);
            var materialGroup = db.Material_Group.FirstOrDefault(u => u.Material_Group1 == decodedId && u.Is_Deleted != true);
            if (materialGroup == null)
            {
                TempData["Message"] = "Material Group not found";
                return RedirectToAction("Index");
            }

            ViewBag.EncodedId = id;
            return View(materialGroup);

        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(string id, Material_Group material_Group, string mode = "Create", string Year_Value = "0", string Month_Value = "0")
        {
            ModelState.Clear();

            var username = Session["Username"]?.ToString();
            if (string.IsNullOrEmpty(username))
            {
                TempData["Message"] = "User not logged in";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                if (string.IsNullOrEmpty(material_Group.Material_Group1))
                {
                    TempData["ErrorMessage"] = "Material Group ID is required";
                    ViewBag.Mode = mode;
                    return View(material_Group);
                }

                if (!string.IsNullOrEmpty(Year_Value))
                {
                    if (string.IsNullOrEmpty(Month_Value) || Month_Value == "0")
                    {
                        material_Group.Age_Accounting_Asset = $"{Year_Value} Years";
                    }
                    else
                    {
                        material_Group.Age_Accounting_Asset = $"{Year_Value} Years, {Month_Value} Month";
                    }
                }

                material_Group.Is_Deleted = false;
                if (mode == "Create")
                {

                    var isExist = db.Material_Group.FirstOrDefault(u =>
                        u.Material_Group1 == material_Group.Material_Group1 &&
                        u.Material_Description == material_Group.Material_Description &&
                        u.Is_Deleted != true);

                    if (isExist != null)
                    {
                        TempData["ErrorMessage"] = "Material Group with the same description already exists in database";
                        ViewBag.Mode = mode;
                        return View(material_Group);
                    }

                    material_Group.Create_By = username;
                    material_Group.Create_Date = DateTime.Now;
                    material_Group.Is_Deleted = false;
                    db.Material_Group.Add(material_Group);
                }
                else if (mode == "Edit")
                {
                    string decodedId = id != null ? DecodeId(id) : material_Group.Material_Group1;
                    var existingGroup = db.Material_Group.FirstOrDefault(u => u.Material_Group1 == decodedId && u.Is_Deleted != true);

                    if (existingGroup == null)
                    {
                        TempData["Message"] = "Material Group not found";
                        return RedirectToAction("Index");
                    }


                    existingGroup.Material_Description = material_Group.Material_Description;
                    existingGroup.Age_Accounting_Asset = material_Group.Age_Accounting_Asset;
                    existingGroup.Quantity = material_Group.Quantity;
                    existingGroup.Last_Check_Date = material_Group.Last_Check_Date;
                    existingGroup.Max_Warranty_Date = material_Group.Max_Warranty_Date;
                    existingGroup.Edit_By = username;
                    existingGroup.Edit_Date = DateTime.Now;

                    db.Entry(existingGroup).State = EntityState.Modified;
                }
                else if (mode == "Delete")
                {
                    string decodedId = id != null ? DecodeId(id) : material_Group.Material_Group1;
                    var isExist = db.Material_Group.FirstOrDefault(u => u.Material_Group1 == decodedId && u.Is_Deleted != true);
                    if (isExist == null)
                    {
                        TempData["Message"] = "Material Group not found";
                        return RedirectToAction("Index");
                    }

                    isExist.Is_Deleted = true;
                    isExist.Delete_By = username;
                    isExist.Delete_Date = DateTime.Now;
                    db.Entry(isExist).State = EntityState.Modified;
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = $"Material Group successfully {(mode == "Create" ? "created" : mode == "Edit" ? "updated" : "deleted")}";
                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                    }
                }
                TempData["ErrorMessage"] = "Validation error occurred";
                ViewBag.Mode = mode;
                return View(material_Group);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while saving Material Group: {ex.Message}";
                ViewBag.Mode = mode;
                return View(material_Group);
            }
        }

        [HttpGet]
        public ActionResult Delete(string decodedid)
        {
            ;
            if (string.IsNullOrEmpty(decodedid))
            {
                TempData["Message"] = "Material Group not found";
                return RedirectToAction("Index");
            }
            decodedid = DecodeId(decodedid);
            var materialGroup = db.Material_Group.FirstOrDefault(u => u.Material_Group1 == decodedid && u.Is_Deleted != true);
            if (materialGroup == null)
            {
                TempData["Message"] = "Material Group not found";
                return RedirectToAction("Index");
            }
            return View(materialGroup);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirm(string id)
        {
            id = DecodeId(id);
            var materialGroup = db.Material_Group.FirstOrDefault(u => u.Material_Group1 == id && u.Is_Deleted != true);
            if (string.IsNullOrEmpty(id))
            {
                TempData["Message"] = "Material Group not found";
                return RedirectToAction("Index");
            }
            materialGroup.Is_Deleted = true;
            materialGroup.Delete_By = Session["Username"].ToString();
            materialGroup.Delete_Date = DateTime.Now;

            db.SaveChanges();
            TempData["Message"] = "Material Group deleted successfully";
            return RedirectToAction("Index");

        }
        [ValidateInput(false)]
        public ActionResult Details(string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                TempData["Message"] = "Material Group not found";
                return RedirectToAction("Index");
            }

            id = DecodeId(id);
            var materialGroup = db.Material_Group.FirstOrDefault(u => u.Material_Group1 == id && u.Is_Deleted != true);
            if (materialGroup == null)
            {
                TempData["Message"] = "Material Group not found";
                return RedirectToAction("Index");
            }
            return View(materialGroup);
        }

        private string FormatAgeAccountingAsset(string ageAsset)
        {
            if (string.IsNullOrEmpty(ageAsset))
                return string.Empty;

            if (ageAsset.EndsWith("Years, 0 Month") || ageAsset.EndsWith("Years,  Month"))
            {
                return ageAsset.Split(new string[] { ", " }, StringSplitOptions.None)[0];
            }

            return ageAsset;
        }
    }
}