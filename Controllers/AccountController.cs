using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    private readonly InMemoryUserStore _userStore;

    public AccountController(InMemoryUserStore userStore)
    {
        _userStore = userStore;
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction("Dashboard");

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(User user)
    {
        if (!ModelState.IsValid)
            return View(user);

        var validUser = _userStore.ValidateUser(user.Username, user.Password);
        if (validUser != null)
        {
            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Username) }, CookieAuthenticationDefaults.AuthenticationScheme))
            );
            return RedirectToAction("Dashboard");
        }

        ViewBag.Error = "Invalid credentials";
        return View(user);
    }

    [Authorize]
    public IActionResult Dashboard() => View();

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }
}