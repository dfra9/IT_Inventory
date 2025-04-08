using System;

namespace IT_Inventory.ViewModel
{
    public class MaterialViewModel
    {
        public int MaterialCode { get; set; }

        public string MaterialGroup { get; set; }
        public string MaterialDescription { get; set; }
        public string AgeAccountingAsset { get; set; }
        public int Quantity { get; set; }
        public string LastCheckDate { get; set; }
        public string MaxWarrantyDate { get; set; }


        public string EncodedId
        {
            get
            {
                if (string.IsNullOrEmpty(MaterialGroup))
                    return string.Empty;

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(MaterialGroup);
                return Convert.ToBase64String(bytes);
            }
        }
    }
}