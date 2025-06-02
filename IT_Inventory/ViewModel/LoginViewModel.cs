using System.ComponentModel.DataAnnotations;

namespace DBIT_Inventory.ViewModel
{
    public class LoginViewModel
    {


        [Required(ErrorMessage = "Username Is Required")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password Is Required")]

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }






    }


}