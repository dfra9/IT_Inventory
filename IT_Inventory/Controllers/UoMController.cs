using System;
using System.Linq;
using System.Web.Mvc;
using IT_Inventory.Models;
using IT_Inventory.ViewModel;

namespace IT_Inventory.Controllers
{
    public class UoMController : Controller
    {
        private readonly IT_Inventory db = new IT_Inventory();


        public ActionResult Index()
        {
            var uoms = db.UoM
                 .Where(u => u.Is_Deleted != true)
                 .AsEnumerable()
                 .Select(u => new UoMViewModel
                 {
                     UoM_Id = u.UoM_Id,
                     UoM_Code = u.UoM_Code,
                     UoM_Description = u.UoM_Description,
                     Is_Deleted = u.Is_Deleted,
                     EncodedId = EncodeId(u.UoM_Id.ToString())
                 })
                 .ToList();
            return View(uoms);
        }

        public ActionResult Editor(string id, string mode = "Create")
        {
            UoMViewModel viewModel;

            if (mode != "Create")
            {
                if (!int.TryParse(DecodeId(id), out int decodedId))
                {
                    TempData["ErrorMessage"] = "Invalid Unit of Measure ID.";
                    return RedirectToAction("Index");
                }
                if (decodedId == 0)
                {
                    TempData["ErrorMessage"] = "Invalid Unit of Measure ID.";
                    return RedirectToAction("Index");
                }

                var uom = db.UoM.Find(decodedId);
                if (uom == null)
                {
                    TempData["ErrorMessage"] = "Unit of Measure not found.";
                    return RedirectToAction("Index");
                }

                viewModel = new UoMViewModel
                {
                    UoM_Id = uom.UoM_Id,
                    UoM_Code = uom.UoM_Code,
                    UoM_Description = uom.UoM_Description,
                    Is_Deleted = uom.Is_Deleted,
                    EncodedId = EncodeId(uom.UoM_Id.ToString())
                };
            }
            else
            {
                viewModel = new UoMViewModel();
            }

            ViewBag.Mode = mode;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editor(UoMViewModel viewModel, string mode)
        {
            if (ModelState.IsValid)
            {
                UoM uom;
                switch (mode)
                {
                    case "Create":
                        uom = new UoM
                        {
                            UoM_Code = viewModel.UoM_Code,
                            UoM_Description = viewModel.UoM_Description,
                            Create_By = User.Identity.Name,
                            Create_Date = DateTime.Now
                        };
                        db.UoM.Add(uom);
                        TempData["SuccessMessage"] = "Unit of Measure created successfully.";
                        break;

                    case "Edit":
                        uom = db.UoM.Find(viewModel.UoM_Id);
                        if (uom == null)
                        {
                            TempData["ErrorMessage"] = "Unit of Measure not found.";
                            return RedirectToAction("Index");
                        }
                        uom.UoM_Code = viewModel.UoM_Code;
                        uom.UoM_Description = viewModel.UoM_Description;
                        uom.Edit_By = User.Identity.Name;
                        uom.Edit_Date = DateTime.Now;
                        TempData["SuccessMessage"] = "Unit of Measure updated successfully.";
                        break;

                    case "Delete":
                        uom = db.UoM.Find(viewModel.UoM_Id);
                        if (uom == null)
                        {
                            TempData["ErrorMessage"] = "Unit of Measure not found.";
                            return RedirectToAction("Index");
                        }
                        uom.Is_Deleted = true;
                        uom.Delete_By = User.Identity.Name;
                        uom.Delete_Date = DateTime.Now;
                        TempData["SuccessMessage"] = "Unit of Measure deleted successfully.";
                        break;

                    default:
                        TempData["ErrorMessage"] = "Invalid operation mode.";
                        return RedirectToAction("Index");
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Mode = mode;
            return View(viewModel);
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
    }
}