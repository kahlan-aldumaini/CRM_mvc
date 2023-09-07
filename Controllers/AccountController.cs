using System.Net.Mail;
using CRM_mvc.Context;
using CRM_mvc.Models.Entities;
using CRM_mvc.Models.Views.Account;
using CRM_mvc.Models.Views.User;
using CRM_mvc.Utilities;
using CRM_mvc.Utilities.Config;
using CRM_mvc.Utilities.Constants;
using CRM_mvc.Utilities.Enumerations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CRM_mvc.Controllers;

public class AccountController : Controller
{
    #region Declration

    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly ApplicationDbContext context;

    #endregion

    #region Constractor

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.context = context;
    }

    #endregion

    #region Login

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                const string message = "المستخدم غير موجود";
                const string title = "خطأ";
                Helper.SendNotification(context: HttpContext, message: message, title: title, type: NotifyType.Error);
                return View();
            }

            var Result = await signInManager.PasswordSignInAsync(user, model.Password, false, false);
            ;
            if (Result.Succeeded)
            {
                // stor extentionNumber in session if user Has 
                if (user.Extentions != null)
                    HttpContext.Session.SetString(SessionKeys.ExtensionNumber,
                        user.Extentions.Last().ExtensionNumber.ToString());

                if (model.ReturnUrl != null)
                    return LocalRedirect(model.ReturnUrl);

                const string message = "تم تسجيل الدخول بنجاح";
                const string title = "نجاح";
                Helper.SendNotification(context: HttpContext, message: message, title: title, type: NotifyType.Success);
                return RedirectToAction("Index", "Call");
            }
            else
            {
                const string message = "كلمة المرور غير صحيحة";
                const string title = "خطأ";
                Helper.SendNotification(context: HttpContext, message: message, title: title, type: NotifyType.Error);
            }
        }

        const string message2 = "الرجاء التأكد من البيانات المدخلة";
        const string title2 = "خطأ";
        Helper.SendNotification(context: HttpContext, message: message2, title: title2, type: NotifyType.Error);
        return View();
    }

    [HttpGet]
    public IActionResult AccessDenied(AccessDeniedViewModel model)
    {
        model.Code = 403;
        return View(model);
    }

    #endregion

    #region Register

    [HttpPost("/Account/Register", Name = "Register")]
    public async Task<IActionResult> Register(NewRegisterViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Name = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            LockoutEnabled = false
        };
        var roleRes = await roleManager.FindByNameAsync(model.Role);
        if (roleRes == null)
        {
            var role = new IdentityRole
            {
                Name = model.Role
            };
            var roleResult = await roleManager.CreateAsync(role);
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }
        }

        var response = new IdentityResult();
        try
        {
            response = await userManager.CreateAsync(user, model.Password);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

        if (response.Succeeded)
        {
            await userManager.AddToRoleAsync(user, model.Role);
            return Ok();
        }
        else
            return BadRequest(response.Errors);
    }

    #endregion

    #region ForgetPassword

    [HttpGet]
    public IActionResult ForgetPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                const string message1 = "المستخدم غير موجود";
                const string title = "خطأ";
                Helper.SendNotification(context: HttpContext, message: message1, title: title, type: NotifyType.Error);
                return View();
            }

            try
            {
                var client = Mail.setClint();
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = user.Email },
                    protocol: HttpContext.Request.Scheme);
                var message = new MailMessage
                {
                    From = new MailAddress("crm@crm.crm", "CRM"),
                    Subject = "استعادة كلمة المرور",
                    Body = $"الرجاء الضغط على الرابط التالي لاستعادة كلمة المرور <a href='{callbackUrl}'>link</a>",
                    IsBodyHtml = true
                };
                message.To.Add(new MailAddress(user.Email));
                client.Send(message);
            }
            catch (Exception e)
            {
                var message1 = e.Message;
                const string title = "خطأ";
                Helper.SendNotification(context: HttpContext, message: message1, title: title, type: NotifyType.Error);
                return View();
            }

            const string message2 = "تم ارسال رابط استعادة كلمة المرور الى بريدك الالكتروني";
            const string title2 = "نجاح";
            Helper.SendNotification(context: HttpContext, message: message2, title: title2, type: NotifyType.Success);
            return RedirectToAction("Login", "Account");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        if (token == null || email == null)
        {
            const string message = "الرابط غير صحيح";
            const string title = "خطأ";
            Helper.SendNotification(context: HttpContext, message: message, title: title, type: NotifyType.Error);
            return RedirectToAction("Login", "Account");
        }

        var model = new ResetPasswordViewModel { Token = token, Email = email };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                const string message = "المستخدم غير موجود";
                const string title = "خطأ";
                Helper.SendNotification(context: HttpContext, message: message, title: title, type: NotifyType.Error);
                return View();
            }

            if (model.Password != model.ConfirmPassword)
            {
                const string message = "كلمة المرور غير متطابقة";
                const string title = "خطأ";
                Helper.SendNotification(context: HttpContext, message: message, title: title, type: NotifyType.Error);
                return View(model);
            }

            var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                const string message = "تم تغيير كلمة المرور بنجاح";
                const string title = "نجاح";
                Helper.SendNotification(context: HttpContext, message: message, title: title, type: NotifyType.Success);
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        return View(model);
    }

    #endregion


    #region Logout

    [HttpGet]
    [Route("/Account/logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    #endregion
}