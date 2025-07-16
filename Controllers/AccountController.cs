using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using SafeVaultApp.Helpers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;

    public AccountController(ApplicationDbContext db)
    {
        _db = db;
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

        var hashedPassword = PasswordHelper.HashPassword(user.Password);
        var validUser = _db.Users.FirstOrDefault(u => u.Username == user.Username && u.Password == hashedPassword);
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

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(User user)
    {
        if (!ModelState.IsValid)
            return View(user);

        if (_db.Users.Any(u => u.Username == user.Username))
        {
            ViewBag.Error = "Username already exists.";
            return View(user);
        }

        user.Password = PasswordHelper.HashPassword(user.Password);
        _db.Users.Add(user);
        _db.SaveChanges();
        return RedirectToAction("Login");
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