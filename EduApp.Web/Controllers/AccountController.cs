using EduApp.Web.Constants;
using EduApp.Web.Enums;
using EduApp.Web.Models.Identity;
using EduApp.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduApp.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }
    
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        string normalizedEmail = model.Email.Trim();

        ApplicationUser? existingUser = await _userManager.FindByEmailAsync(normalizedEmail);

        if (existingUser is not null)
        {
            ModelState.AddModelError(nameof(model.Email), "Bu e-posta adresi zaten kayıtlı.");

            return View(model);
        }

        string roleName = model.Role switch
        {
            RegistrationRole.Student => ApplicationRoles.Student,
            RegistrationRole.Instructor => ApplicationRoles.Instructor,
            _ => ""
        };

        if (string.IsNullOrEmpty(roleName))
        {
            ModelState.AddModelError(nameof(model.Role), "Geçersiz hesap türü seçildi.");

            return View(model);
        }

        ApplicationUser user = new()
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = normalizedEmail,
            UserName = normalizedEmail,
            CreatedAt = DateTime.UtcNow
        };

        IdentityResult createResult = await _userManager.CreateAsync(user, model.Password);

        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);

            return View(model);
        }

        IdentityResult roleResult = await _userManager.AddToRoleAsync(user, roleName);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            AddIdentityErrors(roleResult);

            return View(model);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Index", "Home");
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, TranslateIdentityError(error));
        }
    }

    private static string TranslateIdentityError(IdentityError error)
    {
        return error.Code switch
        {
            "PasswordTooShort" => "Parola en az 8 karakter olmalıdır.",
            "PasswordRequiresDigit" => "Parola en az bir rakam içermelidir.",
            "PasswordRequiresLower" => "Parola en az bir küçük harf içermelidir.",
            "PasswordRequiresUpper" => "Parola en az bir büyük harf içermelidir.",
            "DuplicateEmail" => "Bu e-posta adresi zaten kayıtlı.",
            "DuplicateUserName" => "Bu e-posta adresi zaten kayıtlı.",
            "InvalidEmail" => "Geçerli bir e-posta adresi giriniz.",
            _ => error.Description
        };
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        LoginViewModel model = new()
        {
            ReturnUrl = returnUrl
        };

        return View(model);
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email.Trim());

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");

            return View(model);
        }

        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Hesabınız geçici olarak kilitlenmiştir. Lütfen daha sonra tekrar deneyiniz.");

            return View(model);
        }

        if (result.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty, "Bu hesabın giriş yapmasına izin verilmiyor.");

            return View(model);
        }

        ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;

        return View();
    }
}