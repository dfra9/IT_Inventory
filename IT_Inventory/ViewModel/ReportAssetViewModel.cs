using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace DBIT_Inventory.ViewModel
{
    public class ReportAssetViewModel
    {

        public ReportAssetViewModel() { }

        public string No_asset { get; set; }
        public string Company_Code { get; set; }
        public string Material_Group { get; set; }
        public string Material_Code { get; set; }
        public string Acquisition_Value { get; set; }
        public List<AssetReportItem> ReportItems { get; set; }

        public SelectList AssetNoList { get; set; }
        public SelectList CompanyList { get; set; }
        public SelectList MaterialGroupList { get; set; }
        public SelectList MaterialCodeList { get; set; }
        public SelectList AcquisitionValueList { get; set; }

        public class AssetReportItem
        {
            public string No_asset { get; set; }
            public string Company_Code { get; set; }
            public string Company_Name { get; set; }
            public string Material_Group { get; set; }
            public string Material_Code { get; set; }
            public string Material_Description { get; set; }
            public string Serial_Number { get; set; }
            public int? Quantity { get; set; }
            public string UoM { get; set; }
            public DateTime? Acquisition_Date { get; set; }
            public string Acquisition_Value { get; set; }
            public string No_Asset_PGA { get; set; }
            public string No_Asset_Accounting { get; set; }
            public string No_PO { get; set; }
            public string Device_Id { get; set; }
            public DateTime? Last_Check_Date { get; set; }
            public string Status { get; set; }
            public string Condition { get; set; }
            public string Locations { get; set; }
            public string Dept { get; set; }
            public string Role { get; set; }
            public string Cities { get; set; }
            public string Company_User { get; set; }
            public string City_Name { get; set; }
            public string Latest_User { get; set; }
            public string PIC { get; set; }
            public DateTime? Create_Date { get; set; }
            public DateTime? Edit_Date { get; set; }
        }
    }
}