using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace TrainTracking.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null && email.ToLower() == "admin@kuwgo.com")
                {
                    // Sticky Admin: Double check roles on the fly to bypass potential seeding race conditions
                    if (!await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        Console.WriteLine($"[KuwGo] PRODUCTION ADMIN DETECTED WITHOUT ROLE. Fixing...");
                        await _userManager.AddToRoleAsync(user, "Admin");
                        // Re-sign in to ensure the user has the "Admin" claim in their cookie
                        await _signInManager.SignInAsync(user, isPersistent: rememberMe);
                    }
                    
                    Console.WriteLine($"[KuwGo] Admin Login SUCCESS: {email}. Redirecting to Dashboard.");
                    return RedirectToAction("Index", "Admin");
                }
                
                return RedirectToAction("Index", "Home");
            }

            // Diagnostic Logging
            Console.WriteLine($"[KuwGo] Login FAILED for user: {email}. Result: {result}");
            ModelState.AddModelError(string.Empty, "محاولة تسجيل دخول غير ناجحة. تأكد من البريد الإلكتروني وكلمة المرور.");
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "كلمة المرور وتأكيدها غير متطابقين.");
                return View();
            }

            var user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
