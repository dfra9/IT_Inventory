using System;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModel;

namespace IT_Inventory.Controllers
{
    public class MaterialCodeController : Controller
    {
        private readonly IT_Inventory db = new IT_Inventory();

        public ActionResult Index()
        {
            var materialCode = db.Material_Code
                   .Where(m => m.Is_Deleted != true)
                   .Select(m => new MaterialCodeViewModel
                   {
                       Material_CodeId = m.Material_CodeId,
                       Material_Code1 = m.Material_Code1,
                       Material_Description = m.Material_Description,
                       Material_Group = m.Material_Group,
                       No_Asset_PGA = m.No_Asset_PGA,
                       No_Asset_Accounting = m.No_Asset_Accounting,
                       No_PO = m.No_PO,
                       EncodedId = m.Material_CodeId.ToString()
                   }).ToList();
            return View(materialCode);
        }

        [HttpGet]
        public ActionResult Editor(string id, string mode = "Create")
        {
            ViewBag.MaterialGroup = new SelectList(db.Material_Group
                .Where(m => m.Is_Deleted != true),
                "Material_Group1", "Material_Group1");

            ViewBag.Mode = mode;

            if (mode == "Create")
            {
                return View(new MaterialCodeViewModel());
            }

            if (!string.IsNullOrEmpty(id))
            {
                int materialCodeId;
                if (int.TryParse(id, out materialCodeId))
                {

                    var materialCode = db.Material_Code.Find(materialCodeId);
                    if (materialCode != null)
                    {
                        var materialCodeViewModel = new MaterialCodeViewModel
                        {
                            Material_CodeId = materialCode.Material_CodeId,
                            Material_Code1 = materialCode.Material_Code1,
                            Material_Description = materialCode.Material_Description,
                            Material_Group = materialCode.Material_Group,
                            No_Asset_PGA = materialCode.No_Asset_PGA,
                            No_Asset_Accounting = materialCode.No_Asset_Accounting,
                            No_PO = materialCode.No_PO,
                            EncodedId = materialCode.Material_CodeId.ToString()
                        };
                        return View(materialCodeViewModel);


                    }
                }
            }
            TempData["Error"] = "Material Code not found.";
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(MaterialCodeViewModel model)
        {
            string mode = Request.Form["mode"] ?? "Create";


            ViewBag.MaterialGroup = new SelectList(db.Material_Group
                .Where(m => m.Is_Deleted != true),
                "Material_Group1", "Material_Group1", model.Material_Group);
            if (ModelState.IsValid)
            {
                try
                {
                    switch (mode)
                    {
                        case "Create":
                            var newMaterialCode = new Material_Code
                            {
                                Material_Code1 = model.Material_Code1,
                                Material_Description = model.Material_Description,
                                Material_Group = model.Material_Group,
                                No_Asset_PGA = model.No_Asset_PGA,
                                No_Asset_Accounting = model.No_Asset_Accounting,
                                No_PO = model.No_PO,
                                Create_By = User.Identity.Name,
                                Create_Date = DateTime.Now,
                                Is_Deleted = false
                            };
                            db.Material_Code.Add(newMaterialCode);
                            db.SaveChanges();
                            TempData["Success"] = "Material Code created successfully.";
                            break;
                        case "Edit":
                            var materialCodeUpdate = db.Material_Code.Find(model.Material_CodeId);
                            if (materialCodeUpdate != null)
                            {
                                materialCodeUpdate.Material_Code1 = model.Material_Code1;
                                materialCodeUpdate.Material_Description = model.Material_Description;
                                materialCodeUpdate.Material_Group = model.Material_Group;
                                materialCodeUpdate.No_Asset_PGA = model.No_Asset_PGA;
                                materialCodeUpdate.No_Asset_Accounting = model.No_Asset_Accounting;
                                materialCodeUpdate.No_PO = model.No_PO;
                                materialCodeUpdate.Edit_By = User.Identity.Name;
                                materialCodeUpdate.Edit_Date = DateTime.Now;
                                db.SaveChanges();
                                TempData["Success"] = "Material Code updated successfully.";
                            }
                            else
                            {
                                TempData["Error"] = "Material Code not found.";
                            }
                            break;
                        case "Delete":
                            var materialCodeDelete = db.Material_Code.Find(model.Material_CodeId);
                            if (materialCodeDelete != null)
                            {
                                materialCodeDelete.Is_Deleted = true;
                                materialCodeDelete.Delete_By = User.Identity.Name;
                                materialCodeDelete.Delete_Date = DateTime.Now;
                                db.SaveChanges();

                                TempData["Success"] = "Material Code deleted successfully.";
                            }
                            else
                            {
                                TempData["Error"] = "Material Code not found.";
                            }
                            break;
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error: " + ex.Message;
                }
            }
            return View(model);
        }
    }
}


