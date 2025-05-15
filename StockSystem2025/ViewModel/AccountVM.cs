using StockSystem2025.Models.AccountModels;
using System.ComponentModel.DataAnnotations;

namespace StockSystem2025.ViewModel
{
    public class RegisterViewModel
    {
        [Key]
        [Required(ErrorMessage = "الرجاء ادخال الايميل")]
        [EmailAddress(ErrorMessage = "الرجاء ادخال الايميل بالشكل الصحيح")]
        public string Email { get; set; } = "";
        [Required(ErrorMessage = "الرجاء ادخال كلمة المرور")]
        public string password { get; set; }
        [Required(ErrorMessage = "الرجاء ادخال كلمة المرور")]
        [Compare("password", ErrorMessage = "كلمة المرور غير متطابقة")]
        [Display(Name = "conform password")]
        public string ConformPassword { get; set; } = "";
        public string rolename { get; set; }
        [Required(ErrorMessage = "الرجاء ادخال اسم المستخدم")]
        public string fullname { get; set; }
        public bool IsActive { get; set; }


    }
    public class ChangePasswordViewModel
    {

        [Required(ErrorMessage = "الرجاء ادخال كلمة المرور")]
        public string password { get; set; }
        [Required(ErrorMessage = "الرجاء ادخال كلمة المرور")]
        [Compare("password", ErrorMessage = "كلمة المرور غير متطابقة")]
        [Display(Name = "conform password")]
        public string ConformPassword { get; set; } = "";

        public string Id { get; set; }

        public string rolename { get; set; }
        [Required(ErrorMessage = "الرجاء ادخال اسم المستخدم")]
        public string fullname { get; set; }
        public bool IsActive { get; set; }
        public string Email { get; set; } = "";
    }


    public class RegisterIndexViewModel
    {
        [Key]
        public List<ApplicationUser> Users { get; set; }

    }




    public class RegisterEditViewModel
    {
        [Key]
        public string Id { get; set; }
        [Required(ErrorMessage = "الرجاء ادخال الايميل")]
        public string Email { get; set; } = "";
        [Required(ErrorMessage = "الرجاء ادخال كلمة المرور")]
        public string password { get; set; }
        [Required(ErrorMessage = "الرجاء ادخال كلمة المرور")]
        [Compare("password", ErrorMessage = "كلمة المرور غير متطابقة")]
        [Display(Name = "conform password")]
        public string ConformPassword { get; set; }
        [Required(ErrorMessage = "الرجاء ادخال اسم المستخدم")]
        public string fullname { get; set; }
        public bool IsActive { get; set; }
        public string rolename { get; set; }
    }

    public class RegisterDeleteViewModel
    {
        [Key]
        public string Id { get; set; }
        public string Email { get; set; } = "";
        [Required]
        public string password { get; set; }

    }


    public class RegisterLoginViewModel
    {
        [Key]
        [Required(ErrorMessage = "الرجاء ادخال الايميل")]
        public string Email { get; set; } = "";
        [Required(ErrorMessage = "الرجاء ادخال كلمة المرور")]
        public string password { get; set; }

    }

}
