using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSystem2025.Models.AccountModels;
using StockSystem2025.ViewModel;

namespace StockSystem2025.Controllers
{
    public class AccountCsontroller : Controller
    {
        //UserManager من النظام ويستخدم لعملية انشاء يوسر   لارسال  باسورد مشفر 
        //SignInManager  يستخدم لامر تسجيل الخروج وتسجيل الدخول
        // تم عمل     وذالك لان سوف نستخدمه في ما بعد constructer
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private RoleManager<IdentityRole> _roleManager;

        public ApplicationDbContext _context { get; }

        public AccountCsontroller(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> SignInManager, ApplicationDbContext Context)
        {
            _userManager = userManager;
            _signInManager = SignInManager;
            _context = Context;
            _roleManager = roleManager;
        }

        public IActionResult MainIndex()
        {
            return View();
        }

        [Authorize(Roles = "مسؤول النظام")]
        public async Task<IActionResult> index()

        {

            var UserList = new RegisterIndexViewModel();
            var users = await _userManager.Users.ToListAsync();

            foreach (var user in users)
            {
                user.Roles = await _userManager.GetRolesAsync(user);
            }

            UserList.Users = users;

            return View(UserList);
        }




        //[Authorize(Roles = "مسؤول النظام")]
        public IActionResult Regester()
        {
            //لعرض الرول ب dropdownlist
            ViewBag.roles = _roleManager.Roles.ToList();
            return View();
        }

        //[Authorize(Roles = "مسؤول النظام")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Regester(RegisterViewModel model)

        {


            if (ModelState.IsValid)
            {
                ViewBag.roles = _roleManager.Roles.ToList();
                // للتحقق من عدم وجود نفس الايميل 
                var existUser = _userManager.Users.FirstOrDefault(x => x.Email == model.Email);
                if (existUser != null) { ModelState.AddModelError(string.Empty, "يوجد مستخدم بهاذا الاسم"); }

                else
                {

                    //هنا تم تعريف ابجكت من الكلاس الذي يحتوي على اسم المستخدم والايميل والباسورد والذي ياتي من النظام للتعامل مع ال   identity 
                    // سوف يتم حفظ بيانات المستخدم الجديد
                    var user = new ApplicationUser
                    {
                        Email = model.Email,
                        UserName = model.Email,
                        //ضرورية للغاية وتتغني ان الحساب غير مغلق 
                        LockoutEnabled = false,
                        //ضرورية للغاية وتتغني ان الاينيل تم توثيقه
                        EmailConfirmed = true,
                        FullName = model.fullname,
                        IsActive = model.IsActive
                    };
                    // يستخدم هاذا الامر لارسال البيانات الى الداتا بيس مع باسورد مشفروتم الوصول لهاذا الامر عن طريق عمل ابجكت من يوسر منجر بالكنستركتر
                    //تم وضع الامر بمتغير لاستخدام المتغير للتحقق من الحفظ
                    var rusult = await _userManager.CreateAsync(user, model.password);




                    if (rusult.Succeeded)
                    {
                        //يستخدم هذا الامر لاعطاء صلاحية للمستخدم الذي ضفناه
                        var roleResult = await _userManager.AddToRoleAsync(user, model.rolename);

                        // في حال نجاح العملية يتم اعادة التوجيه للصفحة الرئيسية 
                        //isPersistent: false  تعني المستخدم الي رح اعمللو تسجيل دخول في حال طلع من المتصفح ورجع دخل خليه يرجع يسجل دخول من جديد 
                        // isPersistent: trou  لو طلع ورجع دخل يضل عامل تسجيل دخول
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("index", "home");
                    }

                    //foreach  تم انشائها لاظهار الاخطاء التي حدثت مثل ادخال كلمة سر بدون احرف كبيرة حيث سو يتم طباعتها كلها بالصفخة تحت ال فور في الجزئ المخصص بالفاليديشن
                    foreach (var erorr in rusult.Errors)
                    {
                        //ModelState.AddModelError  الجزئية المسؤولة عن عرض الاخطاء بالصفحة 
                        //erorr.Description  هنا نص الخطء الذي سوف يظهر بحيث انها ستعرض وصف الخطء الذي حدث 
                        //تعديل الشروط للباس ورد يتم في ملف بروقرام سي اس
                        ModelState.AddModelError(string.Empty, erorr.Description);
                    }
                    ;
                }
            }
            return View();
        }





        public IActionResult Login()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Login(RegisterLoginViewModel model)
        {
            if (ModelState.IsValid)
            {

                //lockoutOnFailure: false   يعني أن الحساب لن يتم قفله عند الفشل.
                //isPersistent: false  تعني المستخدم الي رح اعمللو تسجيل دخول في حال طلع من المتصفح ورجع دخل خليه يرجع يسجل دخول من جديد 

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("index", "home");
                }
                ModelState.AddModelError(string.Empty, "الحساب غير معرف");
            }
            return View(model);
        }



        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            return RedirectToAction("Login", "AccountCsontroller");
        }


        [Authorize(Roles = "مسؤول النظام")]
        public async Task<IActionResult> Edet(string? Id)
        {
            ViewBag.roles = _roleManager.Roles.ToList();
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }

            var role = await _userManager.GetRolesAsync(user);
            ViewBag.role = role.FirstOrDefault();

            var UserEdit = new RegisterEditViewModel
            {
                Id = user.Id,
                Email = user.Email,
                fullname = user.FullName,
                IsActive = user.IsActive,


            };

            return View(UserEdit);
        }

        [Authorize(Roles = "مسؤول النظام")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edet(RegisterEditViewModel model)
        {
            ViewBag.roles = _roleManager.Roles.ToList();
            var user = await _userManager.FindByIdAsync(model.Id);
            user.Email = model.Email;
            user.Id = model.Id;
            user.FullName = model.fullname;
            user.IsActive = model.IsActive;

            var rusult = await _userManager.UpdateAsync(user);


            if (rusult.Succeeded)
            {   //احضار رول القديمة
                IEnumerable<string> oldRoleName = await _userManager.GetRolesAsync(user);
                // حذف الرول القديمة
                var removeRoleResult = await _userManager.RemoveFromRolesAsync(user, oldRoleName);

                if (removeRoleResult.Succeeded)
                {
                    //اضافة الرول الجديدة
                    await _userManager.AddToRoleAsync(user, model.rolename);
                }
                return RedirectToAction("Index", "AccountCsontroller");
            }
            foreach (var erorr in rusult.Errors)
            {
                ModelState.AddModelError(string.Empty, erorr.Description);
            }
            ;


            return View(model);
        }




        [Authorize(Roles = "مسؤول النظام")]
        public async Task<IActionResult> Delete(string? Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }
            var UserDelete = new RegisterDeleteViewModel
            {
                Id = user.Id,
                Email = user.Email,
            };

            return View(UserDelete);
        }

        [Authorize(Roles = "مسؤول النظام")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Delete(RegisterDeleteViewModel model)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Id == model.Id);
            var rusult = await _userManager.DeleteAsync(user);
            if (rusult.Succeeded)
            {
                return RedirectToAction("Index", "AccountCsontroller");
            }


            return View(model);
        }


        public async Task<IActionResult> BlockActive(string id, bool state)
        {

            var user = await _userManager.FindByIdAsync(id);
            user.IsActive = state;
            user.EmailConfirmed = state;
            var result = await _userManager.UpdateAsync(user);


            return RedirectToAction("Index");
        }

        //[Authorize(Roles = "مسؤول النظام")]
        public async Task<IActionResult> ChangePassword(string id)
        {

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var UserEdit = new ChangePasswordViewModel
            {
                Id = user.Id,
                fullname = user.FullName,
                Email = user.Email

            };
            return View(UserEdit);
        }
        [Authorize(Roles = "مسؤول النظام")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel ChangePasswordViewModel)
        {



            //string t = HttpContext.User.Claims.FirstOrDefault().Value;
            var user = await _userManager.FindByIdAsync(ChangePasswordViewModel.Id);

            await _userManager.RemovePasswordAsync(user);
            var AddNewPassword = await _userManager.AddPasswordAsync(user, ChangePasswordViewModel.password);


            if (AddNewPassword.Succeeded)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "يجب ان تحتوي كلمة المرور على رموز واحرف كبيرة وصغيرة وارقام");
                return View();
            }



        }












    }
}
